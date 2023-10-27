using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {
	/// <summary>
	/// 読み込み専用のツリー構造を提供する。
	/// </summary>
	/// <typeparam name="TNode">各ノードの型</typeparam>
	public interface IReadOnlyTreeNode<TNode> where TNode : IReadOnlyTreeNode<TNode> {
		/// <summary>親ノードを取得する。</summary>
		TNode? Parent { get; }
		/// <summary>子ノードを取得する。</summary>
		IReadOnlyList<TNode> Children { get; }
	}

	/// <summary>ツリー構造をなすノード間の関係を定義する。</summary>
	/// <typeparam name="TNode">ノードの型</typeparam>
	public interface ITreeNode<TNode> : IReadOnlyTreeNode<TNode> where TNode : ITreeNode<TNode> {
		/// <summary>子ノードを追加する。</summary>
		/// <param name="child">子ノード</param>
		TNode AddChild(TNode child);
		/// <summary>指定されたインデックスの位置に子ノードを追加する。</summary>
		/// <param name="index">インデックス</param>
		/// <param name="child">子ノード</param>
		TNode InsertChild(int index, TNode child);
		/// <summary>子ノードを削除する。</summary>
		/// <param name="child">削除する子ノード</param>
		TNode RemoveChild(TNode child);
		/// <summary>子ノードを全て削除する。</summary>
		TNode ClearChildren();
	}
	/// <summary>
	/// 観測可能なツリー構造を表す。
	/// </summary>
	/// <typeparam name="TNode">ノードの型</typeparam>
	public interface IReadOnlyObservableTreeNode<TNode> where TNode : IReadOnlyObservableTreeNode<TNode> {
		/// <summary>観測可能な子ノードのコレクションを取得する。</summary>
		ReadOnlyObservableCollection<TNode> Children { get; }
	}

}
