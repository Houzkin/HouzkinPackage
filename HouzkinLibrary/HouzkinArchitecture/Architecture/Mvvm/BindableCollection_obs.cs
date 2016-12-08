using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Houzkin.Architecture {

	///// <summary>
	///// ビューによってバインドされるコレクションの提供と、その生成を補助する。
	///// <para>拡張する場合はジェネリクス型を継承る。</para>
	///// </summary>
	//public abstract class ReadOnlyBindableCollection : BindableObject, INotifyCollectionChanged,IEnumerable {
	//	internal readonly IEnumerable _source;
	//	internal ReadOnlyBindableCollection(IEnumerable model) : base(model) { _source = model; }

	//	/// <summary>コレクションが変更されたときに発生する。</summary>
	//	public event NotifyCollectionChangedEventHandler CollectionChanged {
	//		add { this.ChangedEventManager.CollectionChanged += value; }
	//		remove { this.ChangedEventManager.CollectionChanged -= value; }
	//	}
	//	/// <summary>コレクション変更通知を発行する。</summary>
	//	/// <param name="e">イベント引数</param>
	//	protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
	//		this.ChangedEventManager.OnCollectionChanged(e);
	//	}
	//	/// <summary>観測可能なコレクションを素に、連動するビューモデルのコレクションを生成する。</summary>
	//	/// <typeparam name="TModel">モデルとする要素の型</typeparam>
	//	/// <typeparam name="TViewModel">モデルをもとに、初期化されるビューモデル</typeparam>
	//	/// <param name="source">INotifyCollectionChanged インターフェイスを実装したコレクション</param>
	//	/// <param name="converter">モデルからビューモデルを生成する関数</param>
	//	public static ReadOnlyBindableCollection<TViewModel> Create<TModel, TViewModel>(IEnumerable<TModel> source, Func<TModel, TViewModel> converter) {

	//		Func<object, MVMPair<TModel, TViewModel>> generator = x => {
	//			var m = (TModel)x;
	//			return new MVMPair<TModel, TViewModel>(m, converter(m));
	//		};
	//		return new ReadOnlyBindableCollection<TViewModel>(source, generator);
	//	}
		
	//	internal class MVMPair {
	//		protected MVMPair(object model, object viewModel) {
	//			_model = model; _viewModel = viewModel;
	//		}
	//		object _model;
	//		public object Model { get { return _model; } }
	//		object _viewModel;
	//		public object ViewModel { get { return _viewModel; } }
	//	}
	//	internal class MVMPair<TModel, TViewModel> : MVMPair {
	//		public MVMPair(TModel model, TViewModel viewModel) : base(model, viewModel) { }
	//		public new TModel Model { get { return (TModel)base.Model; } }
	//		public new TViewModel ViewModel { get { return (TViewModel)base.ViewModel; } }
	//	}
	//	IEnumerator IEnumerable.GetEnumerator() {
	//		return _source.GetEnumerator();
	//	}
	//}
	///// <summary>
	///// ビューによってバインドされるコレクションを提供する。
	///// </summary>
	///// <typeparam name="TViewModel">各要素のモデルから生成するビューモデルの型</typeparam>
	//public class ReadOnlyBindableCollection<TViewModel> :　ReadOnlyBindableCollection, IReadOnlyList<TViewModel> {

	//	//readonly IEnumerable _source;
	//	IList<MVMPair> _pairs;
	//	readonly Func<object, MVMPair> _converter;
	//	IDisposable _collectionListener;
	//	private ReadOnlyBindableCollection(IEnumerable source, IList<MVMPair> pairs, Func<object, MVMPair> converter)
	//		: base(source) {
	//		var s = source as INotifyCollectionChanged;
	//		if (s == null) throw new ArgumentException("INotifyCollectionChanged インターフェイスを実装していません。");
	//		_collectionListener = WeakEvent<NotifyCollectionChangedEventArgs>.CreateListener(
	//			h => new NotifyCollectionChangedEventHandler(h),
	//			h => s.CollectionChanged += h,
	//			h => s.CollectionChanged -= h,
	//			CollectionChangedAction);

	//		this._pairs = pairs;
	//		this._converter = converter;
	//	}
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	/// <param name="source">INotifyCollectionChanged インターフェイスを実装したコレクション</param>
	//	/// <param name="converter">モデルからビューモデルを生成する関数</param>
	//	internal ReadOnlyBindableCollection(IEnumerable source, Func<object, MVMPair> converter)
	//		: this(source,source.OfType<object>().Select(x => converter(x)).ToList<MVMPair>(),converter) {
	//	}
	//	/// <summary>観測可能なコレクションを素に連動するビューモデルのコレクションを初期化する。</summary>
	//	/// <param name="source">INotifyCollectionChanged を実装したコレクション</param>
	//	/// <param name="converter">ソースとなるコレクションの要素からビューモデルを生成する関数</param>
	//	protected ReadOnlyBindableCollection(IEnumerable source, Func<object, TViewModel> converter)
	//		: this(source, new Func<object, MVMPair>(src => new MVMPair<object, TViewModel>(src, converter(src)))) { }
		
	//	void CollectionChangedAction(object sender, NotifyCollectionChangedEventArgs e) {
	//		ThrowExceptionIfDisposed();
	//		//新規リストに並べ替えられた要素を格納
	//		IList<MVMPair> newSrc = new List<MVMPair>();
	//		foreach (var s in _source) {
	//			var v = _pairs.FirstOrDefault(x => object.Equals(x.Model, s));
	//			if (v != null) {
	//				newSrc.Add(v);
	//				_pairs.Remove(v);
	//			} else {
	//				newSrc.Add(_converter(s));
	//			}
	//		}
	//		var allItem = newSrc.Concat(_pairs);
	//		IList newVm = e.NewItems == null ? null :
	//			allItem.Where(x => e.NewItems.OfType<object>().Any(y => object.Equals(y, x.Model)))
	//			.Select(x => x.ViewModel).ToArray();
	//		IList oldVm = e.OldItems == null ? null :
	//			allItem.Where(x => e.OldItems.OfType<object>().Any(y => object.Equals(y, x.Model)))
	//			.Select(x => x.ViewModel).ToArray();
			
	//		var trash = _pairs;//削除される要素
	//		_pairs = newSrc;//ソート後の要素に置換

	//		NotifyCollectionChangedEventArgs arg;
	//		switch (e.Action) {
	//		case NotifyCollectionChangedAction.Add:
	//			arg = new NotifyCollectionChangedEventArgs(e.Action, newVm, e.NewStartingIndex);
	//			goto countChange;
	//		case NotifyCollectionChangedAction.Remove:
	//			arg = new NotifyCollectionChangedEventArgs(e.Action, oldVm, e.OldStartingIndex);
	//			goto countChange;
	//		case NotifyCollectionChangedAction.Move:
	//			arg = new NotifyCollectionChangedEventArgs(e.Action, newVm, e.NewStartingIndex, e.OldStartingIndex);
	//			break;
	//		case NotifyCollectionChangedAction.Replace:
	//			arg = new NotifyCollectionChangedEventArgs(e.Action, newVm, oldVm, e.NewStartingIndex);
	//			break;
	//		case NotifyCollectionChangedAction.Reset:
	//			arg = new NotifyCollectionChangedEventArgs(e.Action);
	//			goto countChange;
	//		countChange:
	//			this.OnPropertyChanged(() => this.Count);
	//			break;
	//		default:
	//			throw new ArgumentException();
	//		}
	//		this.OnCollectionChanged(arg);

	//		var ds = trash.Select(x => x.ViewModel).OfType<IDisposable>();
	//		foreach (var d in ds) d.Dispose();
	//	}
	//	/// <summary>コレクションのカウントを返す。</summary>
	//	public int Count {
	//		get { return _pairs.Count; }
	//	}
	//	/// <summary>指定したインデックスが示す位置にある要素を取得する。</summary>
	//	public TViewModel this[int index] {
	//		get { return (TViewModel)this._pairs[index].ViewModel; }
	//	}
	//	IEnumerator<TViewModel> IEnumerable<TViewModel>.GetEnumerator() {
	//		return _pairs.Select(x => x.ViewModel).OfType<TViewModel>().GetEnumerator();
	//	}
	//	/// <summary>
	//	/// インスタンスを破棄する。
	//	/// <para>コレクションに含まれる破棄可能なインスタンスも同時に破棄する。</para>
	//	/// </summary>
	//	protected override void Dispose(bool disposing) {
	//		if (IsDisposed) return;
	//		if (disposing) {
	//			var d = this._pairs.Select(x => x.ViewModel).OfType<IDisposable>();
	//			foreach (var dd in d) dd.Dispose();
	//			if (_collectionListener != null) _collectionListener.Dispose();
	//		}
	//		base.Dispose(disposing);
	//	}
	//}
	///// <summary>
	///// MVVMモデルにおける観測可能なコレクションを参照するビューモデルの提供する。
	///// <para>観測可能なコレクションの各要素をビューモデルに変換し、現在のインスタンス自身をビューモデルとして使用可能なコレクションを生成する。</para> 
	///// </summary>
	///// <typeparam name="TModel">モデルとする各要素の型</typeparam>
	///// <typeparam name="TViewModel">各要素のモデルから生成するビューモデルの型</typeparam>
	//public abstract class ReadOnlyBindableCollection<TModel, TViewModel> : ReadOnlyBindableCollection<TViewModel> {
	//	/// <summary>観測可能なコレクションを素に連動するビューモデルのコレクションを初期化する。</summary>
	//	/// <param name="source">INotifyCollectionChanged を実装したコレクション</param>
	//	/// <param name="converter">ソースとなるコレクションの要素からビューモデルを生成する関数</param>
	//	public ReadOnlyBindableCollection(IEnumerable<TModel> source, Func<TModel,TViewModel> converter):
	//		base(source, new Func<object, TViewModel>((x) => converter((TModel)x))) { }
	//}

}
