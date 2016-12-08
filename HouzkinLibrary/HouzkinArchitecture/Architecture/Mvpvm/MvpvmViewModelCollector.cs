using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Houzkin.Architecture.Mvpvm {

	///// <summary>MVPVM パターンにおいて、参照オブジェクトを観測可能なコレクションとして扱うビューモデルを表す。</summary>
	///// <typeparam name="T">プレゼンターまたはモデルからインデックスによって取得した値のうち、ビューモデルの役割を担う要素として含めるオブジェクトの型</typeparam>
	//public class MvpViewModelCollector<T>  : MvpvmViewModel, INotifyCollectionChanged, IReadOnlyCollection<T> {
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	public MvpViewModelCollector() : base() { }
	//	/// <summary>プレゼンターの変更を実行する。</summary>
	//	internal override void ChangePresenter(IPresenter newPresenter, IPresenter oldPresenter) {
	//		base.ChangePresenter(newPresenter, oldPresenter);
	//		if (oldPresenter != null) oldPresenter.CollectionChanged -= OnModelCollectionChanged;
	//		if (newPresenter != null) newPresenter.CollectionChanged += OnModelCollectionChanged;
	//	}
	//	void OnModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
	//		ThrowExceptionIfDisposed();
	//		if (!object.ReferenceEquals(sender, this.Presenter)) {
	//			//発送元がプレゼンターだった場合、引数としてint型のインデックスを使用するプロパティに
	//			//PreModel属性がついていなかったら無視
	//			if (!IncludedProperties.Any(p => p.GetIndexParameters().Select(pt => pt.ParameterType).SequenceEqual(new Type[] { typeof(int) }))) 
	//				return;
	//		}
	//		this.ChangedEventManager.OnCollectionChanged(this, e);
	//	}
	//	/// <summary>コレクションが変更されたときに発生する。</summary>
	//	public event NotifyCollectionChangedEventHandler CollectionChanged {
	//		add { this.ChangedEventManager.CollectionChanged += value; }
	//		remove { this.ChangedEventManager.CollectionChanged -= value; }
	//	}
	//	/// <summary>モデルとして参照するオブジェクトが設定または変更されたときに、ラッピングしているメンバーの値を設定し直し、コレクション変更通知を発行する。</summary>
	//	internal  override void RefreshWrappingMember() {
	//		base.RefreshWrappingMember();
	//		this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	//		this.OnPropertyChanged(() => this.Count);
	//	}
	//	/// <summary>コレクション変更通知を発行する。</summary>
	//	/// <param name="e">イベント引数</param>
	//	protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
	//		this.ChangedEventManager.OnCollectionChanged(this, e);
	//	}
	//	/// <summary>コレクションのカウントを取得する。</summary>
	//	public int Count {
	//		get { return this.Count(); }
	//	}
	//	/// <summary>コレクションを反復処理する列挙しを返す。 </summary>
	//	public IEnumerator<T> GetEnumerator() {
	//		return (IEnumerator<T>)(this as IEnumerable).GetEnumerator();
	//	}
	//	IEnumerator IEnumerable.GetEnumerator() {
	//		return this.GetEnumerableObject().OfType<T>().GetEnumerator();
	//	}
	//	/// <summary>コレクションとして扱う IEnumerable を取得する。</summary>
	//	IEnumerable GetEnumerableObject() {
	//		if (IncludedProperties.Any(p => p.GetIndexParameters().Select(pt => pt.ParameterType).SequenceEqual(new Type[] { typeof(int) }))) {
	//			return new Enumerator(this);
	//		}
	//		if (this.Model != null) {
	//			var mm = this.Model as IEnumerable;
	//			if (mm != null) return mm;
	//		}
	//		return new object[0];
	//	}
	//	class Enumerator : IEnumerator,IEnumerable {
	//		MvpvmViewModel _mdl;
	//		public Enumerator(MvpvmViewModel mdl) {
	//			this._mdl = mdl;
	//		}
	//		int index = -1;
	//		object IEnumerator.Current {
	//			get { return _mdl.MaybeGetIndex<object>(index); }
	//		}

	//		bool IEnumerator.MoveNext() {
	//			index++;
	//			object dmy;
	//			return _mdl.TryGetIndex<object>(new object[] { index }, out dmy);
	//		}

	//		void IEnumerator.Reset() {
	//			index = -1;
	//		}
	//		IEnumerator IEnumerable.GetEnumerator() {
	//			return this;
	//		}
	//	}
	//}
}
