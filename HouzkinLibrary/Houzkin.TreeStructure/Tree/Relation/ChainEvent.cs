using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {
	///// <summary>連鎖イベントをサポートする。</summary>
	//internal interface IChainEventNode : INodeFaculty<ChainEvent> {
	//	/// <summary>対象オブジェクトの連鎖イベントを発行する。</summary>
	//	void RaiseEvent(ChainEvent ce,ChainEventArgs e);
	//}
	///// <summary>連鎖タイプを示す。</summary>
	//public enum ChainStrategy { 
	//	/// <summary>指定された連鎖イベントはノードを伝う連鎖をしない。</summary>
	//	Direct, 
	//	/// <summary>祖先ノード方向から対象ノードまで順に連鎖する。(接頭辞 Pre)</summary>
	//	Tunnel, 
	//	/// <summary>対象ノードから祖先方向へ順に連鎖する。</summary>
	//	Bubble, 
	//	/// <summary>対象ノードから子孫方向へ連鎖する。(接頭辞 Follow)</summary>
	//	ToPeripheral,
	//	/// <summary>祖先ノードから広がるすべてのノードに連鎖する。</summary>
	//	Entirely,
	//}
	///// <summary>連鎖イベントデータを格納する。</summary>
	//[Serializable]
	//public class ChainEventArgs : EventArgs {
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	public ChainEventArgs() { }
	//	object _sourceNode = null;
	//	ChainEvent _chainEvent = null;
	//	/// <summary>イベントを発生させたオブジェクトの参照を取得する。</summary>
	//	public object SourceNode {
	//		get { return _sourceNode; }
	//		internal set {
	//			if (_sourceNode == null) _sourceNode = value;
	//			else throw new InvalidOperationException("使用済みの連鎖イベント引数が指定されました。");
	//		}
	//	}
	//	/// <summary>このインスタンスが関連付けられている情報を取得する。</summary>
	//	public ChainEvent ChainEvent {
	//		get { return _chainEvent; }
	//		internal set {
	//			if (_chainEvent == null) _chainEvent = value;
	//			else throw new InvalidOperationException("使用済みの連鎖イベント引数が指定されました。");
	//		}
	//	}
	//	/// <summary>連鎖過程イベント処理の現在の状態を示す値を取得または設定する。
	//	/// <para>処理済みとする場合は true を設定。</para>
	//	/// <para>このとき連鎖タイプが Tunnel、Bubble ならば以降の連鎖を中断、ToPeriphera、Entirely ならば対象ノードの子孫方向への連鎖のみを中断する。</para></summary>
	//	public bool Handled { get; set; }
	//}
	///// <summary>再帰構造をなすノードにおいて、連鎖可能なイベントを表す。</summary>
	//[Serializable]
	//public class ChainEvent {
	//	#region static
	//	internal static Dictionary<Tuple<string, Type>, ChainEvent> EventDic = new Dictionary<Tuple<string, Type>, ChainEvent>();
		
	//	/// <summary>連鎖イベントを登録する。
	//	/// <para>要素型クラス内の public static readonly フィールドで使用し、取得した値を公開する必要がある。</para></summary>
	//	/// <typeparam name="TArgs">ChainEventArgs を継承した、連鎖イベントが引数にとるイベント引数型。</typeparam>
	//	/// <param name="name">連鎖するイベント名</param>
	//	/// <param name="strategy">連鎖タイプ</param>
	//	/// <param name="elementType">要素型</param>
	//	/// <returns>連鎖イベント識別子</returns>
	//	public static ChainEvent<TArgs> Register<TArgs>(string name, Type elementType, ChainStrategy strategy) 
	//		where TArgs : ChainEventArgs {
	//		return new ChainEvent<TArgs>(name, elementType, strategy);
	//	}
	//	/// <summary>ソースハンドラーを設定する。
	//	/// <para>実装するノードの static readonly フィールドで公開する。</para></summary>
	//	/// <typeparam name="TArgs">イベント引数の型</typeparam>
	//	/// <param name="chainEvent">連鎖イベント識別子</param>
	//	/// <returns>ソースハンドラー識別子</returns>
	//	public static SourceHandler<TArgs> EstablishSourceHandler<TArgs>(ChainEvent<TArgs> chainEvent)
	//		where TArgs : ChainEventArgs {
	//		return new SourceHandler<TArgs>(chainEvent, null);
	//	}
	//	/// <summary>発生源となったノードのキャストを必須条件とするソースハンドラーを設定する。
	//	/// <para>実装するノードの static readonly フィールドで公開する。</para></summary>
	//	/// <typeparam name="TArgs">イベント引数の型</typeparam>
	//	/// <typeparam name="TNode">発生源となったノードの、指定した型へのキャストを必須条件とする。</typeparam>
	//	/// <param name="chainEvent">連鎖イベント識別子</param>
	//	/// <param name="pred">イベントソースとなったキャスト後のノードを引数にとる、ソースハンドラーの発動条件。</param>
	//	/// <returns>ソースハンドラー識別子</returns>
	//	public static SourceHandler<TArgs> EstablishSourceHandler<TArgs, TNode>(ChainEvent<TArgs> chainEvent,
	//	Predicate<TNode> pred = null)
	//	where TArgs : ChainEventArgs
	//	where TNode : class {
	//		return new SourceHandler<TArgs>(chainEvent, (snd,src) => {
	//			var xx = src as TNode;
	//			return (xx == null) ? false : (pred == null) ? true : pred(xx);
	//		});
	//	}
	//	/// <summary>発生源となったノードのキャストを必須条件とするソースハンドラーを設定する。
	//	/// <para>実装するノードの static readonly フィールドで公開する。</para></summary>
	//	/// <typeparam name="TArgs">イベント引数の型</typeparam>
	//	/// <typeparam name="TNode">発生源となったノードの、指定した型へのキャストを必須条件とする。</typeparam>
	//	/// <param name="chainEvent">連鎖イベント識別子</param>
	//	/// <param name="pred">発送元ノードとキャスト後のイベントソースを引数にとり、条件を設定する。</param>
	//	/// <returns>ソースハンドラー識別子</returns>
	//	public static SourceHandler<TArgs> EstablishSourceHandler<TArgs,TNode>(ChainEvent<TArgs> chainEvent,
	//	Func<object, TNode, bool> pred = null)
	//	where TArgs : ChainEventArgs
	//	where TNode : class {
	//		return new SourceHandler<TArgs>(chainEvent, (snd, src) => {
	//			var sr = src as TNode;
	//			return (sr == null) ? false : (pred == null) ? true : pred(snd, sr);
	//		});
	//	}
	//	#endregion

	//	#region instance
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	internal ChainEvent(string name, ChainStrategy strategy) {
	//		this.Name = name; this.Strategy = strategy;
	//	}
	//	/// <summary>連鎖イベント名を取得する。</summary>
	//	public string Name { get; private set; }
	//	/// <summary>連鎖タイプを取得する。</summary>
	//	public ChainStrategy Strategy { get; private set; }

	//	internal virtual void Raise(object sender, IEnumerable<Delegate> handlers, ChainEventArgs e) { throw new NotSupportedException(); }
	//	/// <summary>指定された型、またはその基本型が要素型として登録されているかどうかを示す。</summary>
	//	/// <param name="forType">要素型またはその派生型</param>
	//	public virtual bool ContainsAsKey(Type forType) {
	//		throw new NotSupportedException();
	//	}
	//	#endregion
	//}
	///// <summary>連鎖イベントの発生源によって発動されるイベントの識別子を表す。</summary>
	//[Serializable]
	//public abstract class SourceHandler : ChainEvent {
	//	ChainEvent _ce;
	//	internal SourceHandler(ChainEvent ce) : base(ce.Name, ChainStrategy.Direct) {
	//		_ce = ce;
	//	}
	//	internal bool IsPostOf(ChainEvent ce) {
	//		return _ce == ce;
	//	}
	//	/// <summary>指定された型、またはその基本型が要素型として登録されているかどうかを示す。</summary>
	//	/// <param name="forType">要素型またはその派生型</param>
	//	public override bool ContainsAsKey(Type forType) {
	//		return _ce.ContainsAsKey(forType); 
	//	}
	//}
	///// <summary>連鎖イベントの発生源となったノードに対する処理を示す。</summary>
	///// <typeparam name="TArgs">イベント引数の型</typeparam>
	//[Serializable]
	//public class SourceHandler<TArgs> : SourceHandler
	//where TArgs : ChainEventArgs {
	//	Func<Object, Object, bool> _ssCnd;
	//	internal SourceHandler(ChainEvent<TArgs> target, Func<object,object,bool> pred)
	//		: base(target) {
	//		_ssCnd = pred;
	//	}
	//	bool MatchCnd(object sender, object source) {
	//		if (_ssCnd == null) return true;
	//		return _ssCnd(sender, source);
	//	}
	//	internal override void Raise(object sender, IEnumerable<Delegate> handlers, ChainEventArgs e) {
	//		if (!this.MatchCnd(sender, e.SourceNode)) return;
	//		var hls = handlers.OfType<EventHandler<TArgs>>();
	//		var arg = (TArgs)e;
	//		foreach (var h in hls) h(sender, arg);
	//	}
	//}
	///// <summary>再帰構造をなすノードにおいて、連鎖可能なイベントを提供する。</summary>
	///// <typeparam name="TArgs">連鎖イベントが引数にとるイベント引数型</typeparam>
	//[Serializable]
	//public sealed class ChainEvent<TArgs> : ChainEvent where TArgs : ChainEventArgs {
	//	internal ChainEvent(string name,Type elementType, ChainStrategy strategy) :base(name, strategy) {
	//		this.AddElementType(elementType);
	//	}
	//	internal override void Raise(object sender, IEnumerable<Delegate> handlers, ChainEventArgs e) {
	//		var hls = handlers.OfType<EventHandler<TArgs>>();
	//		var arg = (TArgs)e;
	//		foreach (var h in hls) h(sender, arg);
	//	}
	//	List<Type> _elementTypeCollection = new List<Type>();
	//	/// <summary>登録済み連鎖イベントの要素型として別の型を追加する。
	//	/// <para>追加する要素型クラス内の public static readonly フィールドで使用し、取得した値を公開する必要がある。</para></summary>
	//	/// <param name="elementType">要素型</param>
	//	public ChainEvent AddElementType(Type elementType) {
	//		if (elementType == null)
	//			throw new ArgumentNullException("elementType");
	//		var key = Tuple.Create(this.Name, elementType);
	//		if (EventDic.ContainsKey(key))
	//			throw new ArgumentException("指定された型で同一名のイベントが既に登録されています。");
	//		if (_elementTypeCollection.Any(x => elementType.IsSubclassOf(x))) {
	//			var tgt = _elementTypeCollection.First(x => elementType.IsSubclassOf(x));
	//			throw new ArgumentException("基本クラス" + tgt.ToString() + "で既に登録済みです。");
	//		}
	//		if (_elementTypeCollection.Any(x => x.IsSubclassOf(elementType))) {
	//			var tgt = _elementTypeCollection.First(x => x.IsSubclassOf(elementType));
	//			throw new ArgumentException("派生クラス"+tgt.ToString() +"で既に登録済みです。");
	//		}
	//		_elementTypeCollection.Add(elementType);
	//		EventDic[key] = this;
	//		return this;
	//	}
	//	/// <summary>指定された型、またはその基本型が要素として登録されているかどうかを示す。</summary>
	//	/// <param name="forType">要素型またはその派生型</param>
	//	public override bool ContainsAsKey(Type forType) {
	//		return _elementTypeCollection.Any(x => x.IsAssignableFrom(forType));
	//	}
	//}
}
