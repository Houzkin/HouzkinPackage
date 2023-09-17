using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {

	/// <summary>ツリー構造をなすノードを表す。</summary>
	/// <typeparam name="TNode">各ノードの共通基本クラスとなる型</typeparam>
	[Serializable]
	public abstract class TreeNode<TNode> : ITreeNode<TNode>,IDisposable 
	where TNode : TreeNode<TNode>, ITreeNode<TNode> {

		/// <summary>新規インスタンスを初期化する。</summary>
		protected TreeNode() { }

		ChildNodeCollection<TNode> 〆childNodes = null;
		/// <summary>子ノードのコレクションを取得する。</summary>
		protected ChildNodeCollection<TNode> ChildNodes {
			get {
				if (〆childNodes == null) 〆childNodes = new ChildNodeCollection<TNode>();
				if (〆childNodes.Self == null) 〆childNodes.Self = (TNode)this;
				return 〆childNodes;
			}
		}
		TNode 〆self;
		internal TNode self {
			get { 
				if (〆self == null) 〆self = (TNode)this;
				return 〆self;
			}
		}

		#region TreeNode
		/// <summary>子ノードを取得する。</summary>
		public IList<TNode> Children {
			get { return this.ChildNodes; }
		}
		/// <summary>子ノードを追加する。</summary>
		/// <param name="newChild">子ノード</param>
		/// <returns>現在のノード</returns>
		public TNode AddChild(TNode newChild) {
			ChildNodes.Add(newChild);
			return self;
		}
		/// <summary>子ノードを削除する。</summary>
		/// <param name="child">削除する子ノード</param>
		/// <returns>現在のノード</returns>
		public TNode RemoveChild(TNode child) {
			ChildNodes.Remove(child);
			return self;
		}
		/// <summary>指定したインデックスの位置に子ノードを追加する。</summary>
		/// <param name="index">インデックス</param>
		/// <param name="child">削除する子ノード</param>
		/// <returns>現在のノード</returns>
		public TNode InsertChild(int index, TNode child) {
			ChildNodes.Insert(index, child);
			return self;
		}
		/// <summary>子ノードを全て削除する。</summary>
		/// <returns>現在のノード</returns>
		public TNode ClearChildren() {
			ChildNodes.Clear();
			return self;
		}
		/// <summary>親ノードを取得・設定する。</summary>
		[System.Xml.Serialization.XmlIgnore]
		public TNode Parent {
			get { return _parent; }
			set {
				if (value == null && this.Parent != null) {
					this.Parent.RemoveChild((TNode)this);
				} else {
					if(value != null) value.ChildNodes.Add((TNode)this);
				}
			}
		}

		IReadOnlyList<TNode> IReadOnlyTreeNode<TNode>.Children {
			get { return this.ChildNodes; }
		}
		TNode _parent = null;
		private void SetParentNode(TNode newParent) {
			if (newParent != _parent) {
				var op = _parent;
				_parent = newParent;
				if (op != null && op.Children.Contains((TNode)this)) {
					op.Children.Remove((TNode)this);
				}
			}
		}
		/// <summary>指定された子ノードの追加がツリー構造を崩さない場合に呼び出され、継承先で追加条件を指定する。</summary>
		/// <param name="child">追加する子ノード</param>
		/// <returns>追加可能な場合は true。</returns>
		protected virtual bool CanAddChild(TNode child) {
			return true;
		}
		private bool essentialCanAddChild(TNode child) {
			if (child == null) return false;
			if (this.Upstream().Any(x => object.Equals(x, child))) return false;

			if (this.ChildNodes.Any(x => object.Equals(x, child)) && object.Equals(child.Parent, self)) {
				return false;
			} else if (this.ChildNodes.Any(x => object.Equals(x, child)) ^ object.Equals(child.Parent, self)) {
				if (child.Parent == (TNode)this) {
					Debug.Assert(false,
						"再帰構造が正常に機能していない可能性があります。",
						"現在のノード " + this.ToString() + " に子ノードとして追加される " + child.ToString() + " は、既に現在のノードを親ノードとして設定済みです。");
				} else {
					Debug.Assert(false,
						"再帰構造が正常に機能していない可能性があります。",
						"現在のノード " + this.ToString() + " は指定された子ノード " + child.ToString() + " を既に追加済みです。");
				}
			}
			return true;
		}
		/// <summary>指定したインデックスの位置に子ノードを追加する。</summary>
		protected virtual void InsertChildNode(int index, TNode child) {
			ChildNodes.InsetChild(index, child);
		}
		/// <summary>指定したインデックスの位置にある子ノードを置き換える。</summary>
		protected virtual void SetChildNode(int index, TNode child) {
			ChildNodes.SetChild(index, child);
		}
		/// <summary>指定したインデックスの子ノードを削除する。</summary>
		protected virtual void RemoveChildNode(int index) {
			ChildNodes.RemoveChild(index);
		}
		/// <summary>子ノードを移動する。</summary>
		protected virtual void MoveChildNode(int oldIndex, int newIndex) {
			ChildNodes.MoveChild(oldIndex, newIndex);
		}
		/// <summary>子ノードを全て削除する。</summary>
		protected virtual void ClearChildNodes() {
			ChildNodes.ClearChildren();
		}
		#endregion

		#region Dispose
		[NonSerialized]
		bool 〆isDisposed;
		[NonSerialized]
		bool 〆isDisposing;
		/// <summary>現在のインスタンスが既に破棄されているかどうかを示す値を取得する。</summary>
		protected bool IsDisposed {
			get { return 〆isDisposed; }
			private set { 〆isDisposed = value; }
		}
		/// <summary>インスタンスを破棄する。</summary>
		/// <returns>親ノード</returns>
		public TNode Dispose() {
			if (〆isDisposing) return self.Parent;
			〆isDisposing = true;
			this.Dispose(true);
			GC.SuppressFinalize(this);
			〆isDisposing = false;
			return self.Parent;
		}
		/// <summary>リソースを破棄する。</summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing) {
			if (IsDisposed) return;
			if (disposing) {
				this.Parent = null;
				foreach (var cld in this.Levelorder().Skip(1).ToArray()) {
					cld.Dispose();
				}
			}
			IsDisposed = true;
		}
		void IDisposable.Dispose() {
			this.Dispose();
		}
		/// <summary>既に破棄されたインスタンスの操作を禁止する。</summary>
		protected void ThrowExceptionIfDisposed() {
			if (IsDisposed) throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
		}
		/// <summary>子孫ノードを全て破棄する。</summary>
		/// <returns>現在のノード</returns>
		public TNode DisposeDescendants() {
			foreach (var cld in this.Children.ToArray())
				cld.Dispose();
			return self;
		}
		/// <summary>ファイナライズ</summary>
		~TreeNode() {
			this.Dispose(false);
		}
		#endregion

		/// <summary>子ノードを管理するコレクション。</summary>
		/// <typeparam name="U">ノードの型</typeparam>
		[Serializable]
		protected sealed class ChildNodeCollection<U> : ObservableCollection<U>
		where U : TreeNode<U>, ITreeNode<U> {
			internal ChildNodeCollection() { }
			internal U Self { get; set; }
			//public new bool Add(U child) {
			//	var c = this.Count + 1;
			//	base.Add(child);
			//	return c == this.Count;
			//}
			/// <summary>コレクション内の要素を移動する。</summary>
			public new void Move(int oldIndex, int newIndex) {
				if (oldIndex != newIndex) base.Move(oldIndex, newIndex);
			}
			/// <summary>子ノードの並び替えを行う。</summary>
			/// <param name="comparison">比較演算子</param>
			public void Sort(Comparison<U> comparison) {
				this.Sort(x => x, comparison);
			}
			/// <summary>コレクションの要素をキーに従って並び替えを行う。</summary>
			/// <typeparam name="TKey">キーの型</typeparam>
			/// <param name="keySelector">各要素からキーを取り出す関数。</param>
			/// <param name="comparison">各キーの比較演算子。省略でデフォルトの演算子を使用する。</param>
			public void Sort<TKey>(Func<U, TKey> keySelector, Comparison<TKey> comparison = null) {
				U[] tmp;
				if (comparison == null) tmp = this.OrderBy(keySelector).ToArray();
				else tmp = this.OrderBy(keySelector, Comparer<TKey>.Create(comparison)).ToArray();
				for (int i = 0; i < this.Count; i++) {
					this.Move(this.IndexOf(tmp[i]), i);
				}
			}
			/// <summary>指定したインデックスの位置に子ノードを追加する。</summary>
			protected override void InsertItem(int index, U item) {
				Self.ThrowExceptionIfDisposed();
				if (!Self.essentialCanAddChild(item)) return;
				if (Self.CanAddChild(item)) {
					Self.InsertChildNode(index, item);
				}
			}
			internal void InsetChild(int index, U item) {
				if (item.Parent != Self) {
					item.SetParentNode(Self);
				}
				if (!this.Contains(item)) {
					base.InsertItem(index, item);
				}
			}
			/// <summary>指定した位置にある子ノードを置き換える。</summary>
			protected override void SetItem(int index, U item) {
				Self.ThrowExceptionIfDisposed();
				if (!Self.essentialCanAddChild(item)) return;
				if (Self.CanAddChild(item)) {
					Self.SetChildNode(index, item);
				}
			}
			internal void SetChild(int index, U item) {
				var op = this[index];
				base.SetItem(index, item);
				if (op.Parent == Self)
					op.SetParentNode(null);
				if (item.Parent != Self)
					item.SetParentNode(Self);
			}
			/// <summary>指定したインデックスの位置にある子ノードを削除する。</summary>
			protected override void RemoveItem(int index) {
				Self.ThrowExceptionIfDisposed();
				Self.RemoveChildNode(index);
			}
			internal void RemoveChild(int index) {
				var oc = this[index];
				base.RemoveItem(index);
				if (oc.Parent == Self)
					oc.SetParentNode(null);
			}
			/// <summary>指定したインデックスの位置にある子ノードを移動する。</summary>
			protected override void MoveItem(int oldIndex, int newIndex) {
				Self.ThrowExceptionIfDisposed();
				Self.MoveChildNode(oldIndex, newIndex);
			}
			internal void MoveChild(int oldIndex, int newIndex) {
				base.MoveItem(oldIndex, newIndex);
			}
			/// <summary>全ての子ノードを削除する。</summary>
			protected override void ClearItems() {
				Self.ThrowExceptionIfDisposed();
				Self.ClearChildNodes();
			}
			internal void ClearChildren() {
				var clds = this.ToArray();
				base.ClearItems();
				foreach (var cld in clds) 
					if(cld.Parent == Self)
						cld.SetParentNode(null);
			}
		}

	}
}
