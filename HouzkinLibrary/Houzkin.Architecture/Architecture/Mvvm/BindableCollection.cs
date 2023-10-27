using Livet.EventListeners.WeakEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Houzkin.Architecture {
	/// <summary>
	/// ビューによってバインドされるコレクションの提供と、その生成を補助する。
	/// <para>拡張する場合はジェネリクス型を継承る。</para>
	/// </summary>
	public abstract class ReadOnlyBindableCollection : INotifyPropertyChanged, INotifyCollectionChanged, IDisposable {
		internal readonly IEnumerable _source;
		internal ReadOnlyBindableCollection(IEnumerable model) { _source = model; }

		NotifyChangedEventManager _cpm;
		internal NotifyChangedEventManager ChangedEventManager {
			get {
				if (_cpm == null) _cpm = new NotifyChangedEventManager(this);
				return _cpm;
			}
		}
		/// <summary>コレクションが変更されたときに発生する。</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged {
			add { this.ChangedEventManager.CollectionChanged += value; }
			remove { this.ChangedEventManager.CollectionChanged -= value; }
		}
		/// <summary>プロパティが変更されたときに発生する。</summary>
		public event PropertyChangedEventHandler PropertyChanged {
			add { ChangedEventManager.PropertyChanged += value; }
			remove { ChangedEventManager.PropertyChanged -= value; }
		}
		/// <summary>観測可能なコレクションを素に、連動するビューモデルのコレクションを生成する。</summary>
		/// <typeparam name="TModel">モデルとする要素の型</typeparam>
		/// <typeparam name="TViewModel">モデルをもとに、初期化されるビューモデル</typeparam>
		/// <param name="source">INotifyCollectionChanged インターフェイスを実装したコレクション</param>
		/// <param name="converter">モデルからビューモデルを生成する関数</param>
		public static ReadOnlyBindableCollection<TViewModel> Create<TModel, TViewModel>(IEnumerable<TModel> source, Func<TModel, TViewModel> converter) {
			Func<object, MVMPair<TModel, TViewModel>> generator = x => {
				var m = (TModel)x;
				return new MVMPair<TModel, TViewModel>(m, converter(m));
			};
			return new ReadOnlyBindableCollection<TViewModel>(source, generator);
		}
		/// <summary>空のビューモデルコレクションを生成する。</summary>
		/// <typeparam name="TModel">モデルとする要素の型</typeparam>
		/// <typeparam name="TViewModel">ビューモデルの型</typeparam>
		public static ReadOnlyBindableCollection<TViewModel> Empty<TModel,TViewModel>(){
			return new ReadOnlyBindableCollection<TViewModel>(new ObservableCollection<TModel>());
		}

		internal class MVMPair {
			protected MVMPair(object model, object viewModel) {
				_model = model; _viewModel = viewModel;
			}
			object _model;
			public object Model { get { return _model; } }
			object _viewModel;
			public object ViewModel { get { return _viewModel; } }
		}
		internal class MVMPair<TModel, TViewModel> : MVMPair {
			public MVMPair(TModel model, TViewModel viewModel) : base(model, viewModel) { }
			public new TModel Model { get { return (TModel)base.Model; } }
			public new TViewModel ViewModel { get { return (TViewModel)base.ViewModel; } }
		}
		

		#region IDisposable Support
		bool _isDisposed;
		/// <summary>既に破棄されているかどうかを示す値を取得する。</summary>
		protected bool IsDisposed { get { return _isDisposed; } }
		/// <summary>ビューモデルを破棄する。</summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>ビューモデルを破棄する。</summary>
		protected virtual void Dispose(bool disposing) {
			if (IsDisposed) return;
			if (disposing) {
				
			}
			_isDisposed = true;
		}
		/// <summary>既に破棄されているインスタンスの操作を禁止する。</summary>
		protected void ThrowExceptionIfDisposed() {
			if (IsDisposed)
				throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
		}
		#endregion
	}
	/// <summary>
	/// ビューによってバインドされるコレクションを提供する。
	/// </summary>
	/// <typeparam name="TViewModel">各要素のモデルから生成するビューモデルの型</typeparam>
	public class ReadOnlyBindableCollection<TViewModel> : ReadOnlyBindableCollection, IReadOnlyList<TViewModel> {

		IList<MVMPair> _pairs;
		readonly Func<object, MVMPair> _converter;
		IDisposable _collectionListener;
		IDisposable _propListener;

		private ReadOnlyBindableCollection(IEnumerable source, IList<MVMPair> pairs, Func<object, MVMPair> converter)
			: base(source) {

			var s = source as INotifyCollectionChanged;
			if (s == null) throw new ArgumentException("INotifyCollectionChanged インターフェイスを実装していません。");
			//_collectionListener = WeakEvent<NotifyCollectionChangedEventArgs>.CreateListener(

			_collectionListener = new LivetWeakEventListener<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
				h => new NotifyCollectionChangedEventHandler(h),
				h => s.CollectionChanged += h,
				h => s.CollectionChanged -= h,
				CollectionChangedAction);

			var ps = source as INotifyPropertyChanged;
			if(ps != null) {
				_propListener = new LivetWeakEventListener<PropertyChangedEventHandler,PropertyChangedEventArgs>(//WeakEvent<PropertyChangedEventArgs>.CreateListener(
					h => new PropertyChangedEventHandler(h),
					h => ps.PropertyChanged += h,
					h => ps.PropertyChanged -= h,
					(sender, args) => { ChangedEventManager.OnPropertyChanged(args.PropertyName); });
			}
			this._pairs = pairs;
			this._converter = converter;
		}
		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="source">INotifyCollectionChanged インターフェイスを実装したコレクション</param>
		/// <param name="converter">モデルからビューモデルを生成する関数</param>
		internal ReadOnlyBindableCollection(IEnumerable source, Func<object, MVMPair> converter)
			: this(source, source.OfType<object>().Select(x => converter(x)).ToList<MVMPair>(), converter) {
		}
		/// <summary>観測可能なコレクションを素に連動するビューモデルのコレクションを初期化する。</summary>
		/// <param name="source">INotifyCollectionChanged を実装したコレクション</param>
		/// <param name="converter">ソースとなるコレクションの要素からビューモデルを生成する関数</param>
		protected ReadOnlyBindableCollection(IEnumerable source, Func<object, TViewModel> converter)
			: this(source, new Func<object, MVMPair>(src => new MVMPair<object, TViewModel>(src, converter(src)))) { }

		/// <summary>空のコレクションを初期化する。</summary>
		/// <param name="source"></param>
		internal ReadOnlyBindableCollection(IEnumerable source) : base(source){
			_pairs = new List<MVMPair>();
		}
		void CollectionChangedAction(object sender, NotifyCollectionChangedEventArgs e) {
			ThrowExceptionIfDisposed();
			//新規リストに並べ替えられた要素を格納
			IList<MVMPair> newSrc = new List<MVMPair>();
			foreach (var s in _source) {
				var v = _pairs.FirstOrDefault(x => object.Equals(x.Model, s));
				if (v != null) {
					newSrc.Add(v);
					_pairs.Remove(v);
				} else {
					newSrc.Add(_converter(s));
				}
			}
			var allItem = newSrc.Concat(_pairs);
			IList newVm = e.NewItems == null ? Array.Empty<object>() :
				allItem.Where(x => e.NewItems.OfType<object>().Any(y => object.Equals(y, x.Model)))
				.Select(x => x.ViewModel).ToArray();
			IList oldVm = e.OldItems == null ? Array.Empty<object>() :
				allItem.Where(x => e.OldItems.OfType<object>().Any(y => object.Equals(y, x.Model)))
				.Select(x => x.ViewModel).ToArray();

			var trash = _pairs;//削除される要素
			_pairs = newSrc;//ソート後の要素に置換

			NotifyCollectionChangedEventArgs arg;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				arg = new NotifyCollectionChangedEventArgs(e.Action, newVm, e.NewStartingIndex);
				break;
			case NotifyCollectionChangedAction.Remove:
				arg = new NotifyCollectionChangedEventArgs(e.Action, oldVm, e.OldStartingIndex);
				break;
			case NotifyCollectionChangedAction.Move:
				arg = new NotifyCollectionChangedEventArgs(e.Action, newVm, e.NewStartingIndex, e.OldStartingIndex);
				break;
			case NotifyCollectionChangedAction.Replace:
				arg = new NotifyCollectionChangedEventArgs(e.Action, newVm, oldVm, e.NewStartingIndex);
				break;
			case NotifyCollectionChangedAction.Reset:
				arg = new NotifyCollectionChangedEventArgs(e.Action);
				break;
			default:
				throw new ArgumentException();
			}
			base.ChangedEventManager.OnCollectionChanged(arg);

			var ds = trash.Select(x => x.ViewModel).OfType<IDisposable>();
			foreach (var d in ds) d.Dispose();
		}
		/// <summary>コレクションのカウントを返す。</summary>
		public int Count {
			get { return _pairs.Count; }
		}
		/// <summary>指定したインデックスが示す位置にある要素を取得する。</summary>
		public TViewModel this[int index] {
			get { return (TViewModel)this._pairs[index].ViewModel; }
		}
		IEnumerator<TViewModel> IEnumerable<TViewModel>.GetEnumerator() {
			return _pairs.Select(x => x.ViewModel).OfType<TViewModel>().GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return (this as IEnumerable<TViewModel>).GetEnumerator();
		}
		/// <summary>
		/// インスタンスを破棄する。
		/// <para>コレクションに含まれる破棄可能なインスタンスも同時に破棄する。</para>
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed) return;
			if (disposing) {
				var d = this._pairs.Select(x => x.ViewModel).OfType<IDisposable>();
				foreach (var dd in d) dd.Dispose();
				if (_collectionListener != null) _collectionListener.Dispose();
				if (_propListener != null) _propListener.Dispose();
			}
			base.Dispose(disposing);
		}
	}
	/// <summary>
	/// MVVMモデルにおける観測可能なコレクションを参照するビューモデルの提供する。
	/// <para>観測可能なコレクションの各要素をビューモデルに変換し、現在のインスタンス自身をビューモデルとして使用可能なコレクションを生成する。</para> 
	/// </summary>
	/// <typeparam name="TModel">モデルとする各要素の型</typeparam>
	/// <typeparam name="TViewModel">各要素のモデルから生成するビューモデルの型</typeparam>
	public abstract class ReadOnlyBindableCollection<TModel, TViewModel> : ReadOnlyBindableCollection<TViewModel> {
		/// <summary>観測可能なコレクションを素に連動するビューモデルのコレクションを初期化する。</summary>
		/// <param name="source">INotifyCollectionChanged を実装したコレクション</param>
		/// <param name="converter">ソースとなるコレクションの要素からビューモデルを生成する関数</param>
		public ReadOnlyBindableCollection(IEnumerable<TModel> source, Func<TModel, TViewModel> converter) :
			base(source, new Func<object, TViewModel>((x) => converter((TModel)x))) { }
	}
}
