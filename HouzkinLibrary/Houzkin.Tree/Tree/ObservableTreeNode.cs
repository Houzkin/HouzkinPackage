using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {

	/// <summary>ツリー構造を観測可能なノードを表す。</summary>
	/// <typeparam name="TNode">各ノードの共通基本クラスとなる型</typeparam>
	[Serializable]
	public abstract class ObservableTreeNode<TNode> : TreeNode<TNode>, IReadOnlyObservableTreeNode<TNode>
	where TNode : ObservableTreeNode<TNode> {

		#region Observable
		/// <summary>対象ノードが属するツリー構造に変更があったときに発生する。</summary>
		[field: NonSerialized]
		public event EventHandler<StructureChangedEventArgs<TNode>> StructureChanged;

		void onStructureChanged(StructureChangedEventArgs<TNode> args) {
			if (StructureChanged != null) StructureChanged(this, args);
		}
		ReadOnlyObservableCollection<TNode> _observableChildren;
		ReadOnlyObservableCollection<TNode> IReadOnlyObservableTreeNode<TNode>.Children {
			get {
				if (_observableChildren == null) _observableChildren = new ReadOnlyObservableCollection<TNode>(this.ChildNodes);
				return _observableChildren;
			}
		}
		
		/// <summary>子ノードを取得する。</summary>
		public new ObservableCollection<TNode> Children {
			get { return this.ChildNodes; }
		}
		#endregion

		#region override
		/// <summary>指定したインデックスの位置に子ノードを追加する。</summary>
		protected override void InsertChildNode(int index, TNode child) {
			using (child.EveManager.Entry(self, child.Parent)) {
				base.InsertChildNode(index, child);
			}
		}
		/// <summary>指定したインデックスの位置にある子ノードを置き換える。</summary>
		protected override void SetChildNode(int index, TNode child) {
			using (this.Children[index].EveManager.Entry(null, self))
			using(child.EveManager.Entry(self,child.Parent)){
				base.SetChildNode(index, child);
			}
		}
		/// <summary>指定したインデックスの子ノードを削除する。</summary>
		protected override void RemoveChildNode(int index) {
			using (this.Children[index].EveManager.Entry(null, self)) {
				base.RemoveChildNode(index);
			}
		}
		/// <summary>子ノードを移動する。</summary>
		protected override void MoveChildNode(int oldIndex, int newIndex) {
			//using(this.ChildNodes[newIndex].EveManager.Entry(self,self))
			using(this.Children[oldIndex].EveManager.Entry(self, self)) {
				base.MoveChildNode(oldIndex, newIndex);
			}
		}
		/// <summary>子ノードを全て削除する。</summary>
		protected override void ClearChildNodes() {
			using (new DisposingCollection(this.Children.Select(x => x.EveManager.Entry(null, self)))) {
				base.ClearChildNodes();
			}
		}
		/// <summary>指定したオブジェクトが現在のオブジェクトと同一かどうかを判断する。<para>このメンバーはオーバーライドできません。</para></summary>
		/// <param name="obj">比較するオブジェクト</param>
		public sealed override bool Equals(object obj) {
			return base.Equals(obj);
		}
		/// <summary>ハッシュ関数として機能する。<para>このメンバーはオーバーライドできません。</para></summary>
		public sealed override int GetHashCode() {
			return base.GetHashCode();
		}
		#endregion
		/// <summary>インスタンスが破棄されたときに発生する。</summary>
		[field: NonSerialized]
		public event EventHandler Disposed;

		/// <summary>リソースを破棄する。</summary>
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			if (Disposed != null) Disposed(this, EventArgs.Empty);
		}
		[NonSerialized]
		private EventManager _em;
		private EventManager EveManager {
			get {
				if (_em == null) _em = new EventManager(self);
				return _em;
			}
		}
		private class EventManager {
			readonly TNode _self;
			/// <summary>自身を含まない、移動前の祖先ノード</summary>
			TNode[] preOldAnc;
			int oldIndex;

			public EventManager(TNode self) {
				_self = self;
				initialize();
			}

			int entryCount = 0;
			public IDisposable Entry(TNode newParent, TNode oldParent) {
				entryCount++;
				this.NewParent = newParent;
				this.OldParent = oldParent;

				return new Disposable(() => Exit());
			}
			TNode 〆newParent;
			TNode 〆oldParent;
			TNode NewParent {
				get { return 〆newParent; }
				set { if (〆newParent == null) 〆newParent = value; }
			}
			TNode OldParent {
				get { return 〆oldParent; }
				set {
					if (〆oldParent == null && value != null) {
						〆oldParent = value;
						oldIndex = OldParent.Children.IndexOf(_self);
						preOldAnc = OldParent.Upstream().ToArray();
					}
				}
			}
			TNode PreOldRoot {
				get { return OldParent == null ? _self : preOldAnc.Last(); }
			}
			void Exit() {
				entryCount--;
				if (0 != entryCount) return;
				raiseProcess();
				initialize();
			}
			void initialize() {
				〆newParent = null; 〆oldParent = null; oldIndex = -1; preOldAnc = new TNode[0];
			}
			IDictionary<ChangedDescendantInfo<TNode>, IEnumerable<TNode>> desChangedRange() {
				//祖先宛の子孫変更通知
				//同一ツリー内での移動
				var dic = new Dictionary<ChangedDescendantInfo<TNode>, IEnumerable<TNode>>();
				var dArg = new ChangedDescendantInfo<TNode>(TreeNodeChangedAction.Deviate, _self, OldParent, oldIndex);
				var jArg = new ChangedDescendantInfo<TNode>(TreeNodeChangedAction.Join, _self, OldParent, oldIndex);
				if (object.Equals(PreOldRoot, _self.Upstream().Last())) {
					var mArg = new ChangedDescendantInfo<TNode>(TreeNodeChangedAction.Move, _self, OldParent, oldIndex);
					var deviate = preOldAnc.Except(_self.Upstream().Skip(1));
					var join = _self.Upstream().Skip(1).Except(preOldAnc);
					var move = preOldAnc.Intersect(_self.Upstream().Skip(1));
					if (deviate.Any())
						dic.Add(dArg, deviate);
					if (join.Any())
						dic.Add(jArg, join);
					if (move.Any())
						dic.Add(mArg, move);
				} else {
					if (OldParent != null)
						dic.Add(dArg, OldParent.Upstream());
					if (NewParent != null)
						dic.Add(jArg, NewParent.Upstream());
				}
				return dic;
			}
			Tuple<ChangedAncestorInfo<TNode>, IEnumerable<TNode>> ancChangedRange() {
				bool rootChanged = !object.Equals(PreOldRoot, _self.Upstream().Last());
				var arg = new ChangedAncestorInfo<TNode>(_self, OldParent, oldIndex, rootChanged);
				return Tuple.Create(arg, _self.Levelorder());
			}
			IDictionary<StructureChangedEventArgs<TNode>, IEnumerable<TNode>> strChangeRange() {
				var nr = _self.Upstream().Last();
				Dictionary<StructureChangedEventArgs<TNode>, IEnumerable<TNode>> dic = new Dictionary<StructureChangedEventArgs<TNode>, IEnumerable<TNode>>();
				var dArg = new StructureChangedEventArgs<TNode>(TreeNodeChangedAction.Deviate, _self, OldParent, oldIndex);
				var jArg = new StructureChangedEventArgs<TNode>(TreeNodeChangedAction.Join, _self, OldParent, oldIndex);
				if (object.Equals(PreOldRoot, nr)) {
					var mArg = new StructureChangedEventArgs<TNode>(TreeNodeChangedAction.Move, _self, OldParent, oldIndex);
					if (NewParent != null && OldParent != null && OldParent.Root() != PreOldRoot) {
						dic.Add(dArg, OldParent.Root().Levelorder());//remove
					}
					dic.Add(mArg, nr.Levelorder());//move
				} else {
					//add or remove
					if (OldParent != null) 
						dic.Add(dArg, OldParent.Root().Levelorder());
					if (NewParent != null) {
						dic.Add(jArg, NewParent.Root().Levelorder());
					} else {
						IEnumerable<TNode> v;
						if (dic.TryGetValue(dArg, out v)) {
							dic[dArg] = v.Union(_self.Levelorder());
						} else { 
							dic.Add(dArg, _self.Levelorder());
						}
					}
				}
				return dic;
			}
			void raiseProcess() {
				//子孫変更の通知を発行するノード
				var des = from x in desChangedRange()
						  from y in x.Value
						  select new { DesArg = x.Key, Node = y };
				//祖先変更の通知を発行するノード
				var anc = from x in ancChangedRange().Item2
						  select new { AncArg = ancChangedRange().Item1, Node = x };
				//ツリー構造変更の通知を発行するノード
				var str = from x in strChangeRange()
						  from y in x.Value
						  select new { StrArg = x.Key, Node = y };
				//イベントの発行シーケンス
				var ele = from n in str
						  join d in des on n.Node equals d.Node into dn
						  from da in dn.DefaultIfEmpty(new { DesArg = null as ChangedDescendantInfo<TNode>, Node = n.Node })
						  join a in anc on n.Node equals a.Node into an
						  from aa in an.DefaultIfEmpty(new { AncArg = null as ChangedAncestorInfo<TNode>, Node = n.Node })
						  select new { Node = n.Node, StrArg = n.StrArg, DesArgs = da.DesArg, AncArg = aa.AncArg };
				//発火
				foreach (var nd in ele) {
					nd.StrArg.AncestorInfo = nd.AncArg as ChangedAncestorInfo<TNode>;
					nd.StrArg.DescendantInfo = nd.DesArgs as ChangedDescendantInfo<TNode>;
					nd.Node.onStructureChanged(nd.StrArg);
				}
			}
		}
		private class Disposable : IDisposable {
			Action _disp;
			public Disposable(Action dispose) { _disp = dispose; }
			public void Dispose() {
				if (_disp != null) _disp(); _disp = null;
			}
		}
		private class DisposingCollection : Collection<IDisposable>, IDisposable {
			public DisposingCollection(IEnumerable<IDisposable> collection) 
				: base(collection.ToList()) { }
			public void Dispose() {
				foreach (var dsp in this) dsp.Dispose();
				this.Clear();
			}
		}

	}
}
