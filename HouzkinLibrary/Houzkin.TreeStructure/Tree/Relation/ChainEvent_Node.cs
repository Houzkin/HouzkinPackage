using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {
	///// <summary>再帰構造において同期を図るノードを表す。</summary>
	///// <typeparam name="TNode">ノードの型</typeparam>
	//[Serializable]
	//public abstract class SympathizeableNode<TNode> : AssociativePropertyNode<TNode>, IChainEventNode
	//	where TNode : SympathizeableNode<TNode> {

	//	private static void ChainRaise(SympathizeableNode<TNode> self, ChainEvent ce, ChainEventArgs arg) {
	//		Action<IEnumerable<TNode>> loop = x => {
	//			foreach (var n in x) {
	//				(n as IChainEventNode).RaiseEvent(ce, arg);
	//				if (arg.Handled) break;
	//			}
	//		};
	//		Action<TNode> withExtend = n => {
	//			n.Evolve(x => {
	//				(x as IChainEventNode).RaiseEvent(ce, arg);
	//				if (arg.Handled) {
	//					arg.Handled = false;
	//					return new TNode[0];
	//				}
	//				return x.Children;
	//			}, (x, y) => x.Concat(y));
	//		};
	//		switch (ce.Strategy) {
	//		case ChainStrategy.Direct:
	//			(self as IChainEventNode).RaiseEvent(ce, arg);
	//			break;
	//		case ChainStrategy.Tunnel:
	//			loop(self.Upstream().Reverse());
	//			break;
	//		case ChainStrategy.Bubble:
	//			loop(self.Upstream());
	//			break;
	//		case ChainStrategy.ToPeripheral:
	//			withExtend((TNode)self);
	//			break;
	//		case ChainStrategy.Entirely:
	//			var r = self.Upstream().LastOrDefault();
	//			if (r != null) 
	//				withExtend(r);
	//			break;
	//		}
	//	}
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	public SympathizeableNode() : this(null) { }
	//	/// <summary>連鎖イベント機構、連想プロパティ機構における所有者型を指定して新規インスタンスを初期化する。</summary>
	//	/// <param name="elementType">要素型</param>
	//	protected SympathizeableNode(Type elementType) : base(elementType) { }

	//	//連鎖なし
	//	void IChainEventNode.RaiseEvent(ChainEvent ce, ChainEventArgs e) {
	//		List<Delegate> dlg;
	//		if (linkMap.TryGetValue(ce, out dlg)) ce.Raise(this, dlg, e);
	//	}
	//	/// <summary>指定された連鎖イベントを発行する。</summary>
	//	/// <typeparam name="TArgs">イベントハンドラが要求するイベント引数の型</typeparam>
	//	/// <param name="ce">連鎖イベント識別子</param>
	//	/// <param name="e">イベント引数</param>
	//	protected void RaiseEvent<TArgs>(ChainEvent<TArgs> ce, TArgs e) where TArgs : ChainEventArgs {
	//		e.SourceNode = this; e.ChainEvent = ce;
	//		ChainRaise(this, ce, e);
	//		var tre = this.Upstream().Last().Levelorder();
	//		foreach (var n in tre) { n.postRaise(ce, e); }
	//	}
	//	/// <summary>連鎖イベント発行後、ソースハンドラを処理する。</summary>
	//	/// <typeparam name="TArgs">イベント引数の型</typeparam>
	//	/// <param name="ce">連鎖イベント識別子</param>
	//	/// <param name="e">イベント引数</param>
	//	void postRaise<TArgs>(ChainEvent<TArgs> ce, ChainEventArgs e) where TArgs : ChainEventArgs {
	//		var ll = linkMap
	//			.Select(x => new { Eve = x.Key as SourceHandler, Hnd = x.Value })
	//			.Where(x => x.Eve != null && x.Eve.IsPostOf(ce));
	//		foreach (var p in ll) {
	//			p.Eve.Raise(this, p.Hnd, e);
	//		}
	//	}
	//	void addHandler(ChainEvent ce, Delegate dlg) {
	//		List<Delegate> dlgs;
	//		if (linkMap.TryGetValue(ce, out dlgs)) {
	//			dlgs.Add(dlg);
	//		} else {
	//			linkMap.Add(ce, new List<Delegate>() { dlg });
	//		}
	//	}
	//	void removeHandler(ChainEvent ce, Delegate dlg) {
	//		List<Delegate> dlgs;
	//		if (linkMap.TryGetValue(ce, out dlgs)) {
	//			dlgs.Remove(dlg);
	//			if (!dlgs.Any()) linkMap.Remove(ce);
	//		}
	//	}
	//	/// <summary>対象ノードが所属するデータ構造において、連鎖イベントの発生源となったノードに対するハンドラーを追加する。
	//	/// <para>このメンバーによって登録されたイベント処理はノードを伝う連鎖イベントの後にレベル順で処理される。</para></summary>
	//	/// <typeparam name="TArgs">イベント引数の型</typeparam>
	//	/// <param name="sh">ソースハンドラー識別子</param>
	//	/// <param name="handler">イベント処理</param>
	//	protected void AddHandler<TArgs>(SourceHandler<TArgs> sh, EventHandler<TArgs> handler) where TArgs : ChainEventArgs {
	//		addHandler(sh, handler);
	//	}
	//	/// <summary>対象ノードが所属するデータ構造において、連鎖イベントの発生源となったノードに対するハンドラーを削除する。</summary>
	//	/// <typeparam name="TArgs">イベント引数の型</typeparam>
	//	/// <param name="sh">ソースハンドラー識別子</param>
	//	/// <param name="handler">イベント処理</param>
	//	protected void RemoveHandler<TArgs>(SourceHandler<TArgs> sh, EventHandler<TArgs> handler) where TArgs : ChainEventArgs {
	//		removeHandler(sh, handler);
	//	}
	//	/// <summary>連鎖イベントを指定して、イベント処理を追加する。</summary>
	//	/// <typeparam name="TArgs">イベントハンドラーが要求するイベント引数型</typeparam>
	//	/// <param name="ce">連鎖イベント識別子</param>
	//	/// <param name="handler">イベント処理</param>
	//	protected void AddHandler<TArgs>(ChainEvent<TArgs> ce, EventHandler<TArgs> handler)
	//		where TArgs : ChainEventArgs {
	//		if(elementCheck(ce)) addHandler(ce, handler);
	//	}
	//	/// <summary>連鎖イベントを指定して、イベント処理を削除する。</summary>
	//	/// <typeparam name="TArgs">イベントハンドラーが要求するイベント引数型</typeparam>
	//	/// <param name="ce">連鎖イベント識別子</param>
	//	/// <param name="handler">イベント処理</param>
	//	protected void RemoveHandler<TArgs>(ChainEvent<TArgs> ce, EventHandler<TArgs> handler)
	//		where TArgs : ChainEventArgs {
	//		if(elementCheck(ce)) removeHandler(ce, handler);
	//	}
	//	bool elementCheck(ChainEvent ce) {
	//		if (ce.ContainsAsKey(ElementType)) return true;
	//		string msg = "現在のノードの要素型は、指定された識別子に要素型として登録されていません。"+
	//			"\n登録時の要素型に対し、コンストラクタ呼出時の型を一致または派生型とする必要があります。";
	//		if (ElementType != this.GetType()) {
	//			msg += "\n操作を行ったノードの要素型 :\n";
	//			msg += ElementType.ToString();
	//		}
	//		Debug.Assert(false, msg);
	//		return false;
	//	}
	//	bool INodeFaculty<ChainEvent>.IsElementOf(ChainEvent id) {
	//		return id.ContainsAsKey(ElementType);
	//	}
	//	Dictionary<ChainEvent, List<Delegate>> linkMap {
	//		get {
	//			if (_lm == null) _lm = new Dictionary<ChainEvent, List<Delegate>>();
	//			return _lm;
	//		}
	//	}
	//	[NonSerialized]
	//	Dictionary<ChainEvent, List<Delegate>> _lm = null;


	//}
}
