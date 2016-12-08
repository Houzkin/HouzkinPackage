using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Collections.ObjectModel {
	///// <summary>追加・削除された要素に対して指定した処理を行う、観測可能なコレクションを表す。</summary>
	///// <typeparam name="T">要素の型</typeparam>
	//public class MarkableCollection<T> : ObservableCollection<T> {
	//	/// <summary>指定したコレクションを追加した、新規インスタンスを初期化する。</summary>
	//	/// <param name="collection">コピーするコレクション</param>
	//	/// <param name="added">追加されたときに実行される処理</param>
	//	/// <param name="removed">削除されたときに実行される処理</param>
	//	public MarkableCollection(IEnumerable<T> collection, Action<T> added, Action<T> removed) : this(added, removed) {
	//		foreach (var itm in collection) this.Add(itm);
	//	}
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	/// <param name="added">追加されたときに実行される処理</param>
	//	/// <param name="removed">削除されたときに実行される処理</param>
	//	public MarkableCollection(Action<T> added, Action<T> removed) : base() {
	//		if (added != null) ItemAdded += added;
	//		if (removed != null) ItemRemoved += removed;
	//	}
	//	event Action<T> ItemAdded;
	//	event Action<T> ItemRemoved;

	//	/// <summary>要素を追加する。</summary>
	//	protected override void InsertItem(int index, T item) {
	//		base.InsertItem(index, item);
	//		if (ItemAdded != null) ItemAdded(item);
	//	}
	//	/// <summary>指定したインデックスが示す位置に要素を設定する。</summary>
	//	protected override void SetItem(int index, T item) {
	//		var old = this[index];
	//		base.SetItem(index, item);
	//		if (ItemRemoved != null) ItemRemoved(old);
	//		if (ItemAdded != null) ItemAdded(item);
	//	}
	//	/// <summary>要素を削除する。</summary>
	//	protected override void RemoveItem(int index) {
	//		var old = this[index];
	//		base.RemoveItem(index);
	//		if (ItemRemoved != null) ItemRemoved(old);
	//	}
	//	/// <summary>要素を全て削除する。</summary>
	//	protected override void ClearItems() {
	//		var colec = this.ToArray();
	//		base.ClearItems();
	//		if(ItemRemoved != null)
	//			foreach (var itm in colec) ItemRemoved(itm);
	//	}
	//}
}
