using Houzkin.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {
	// ツリー構造の組み立てを行うメソッドを提供する。
	public static partial class TreeNode {

		#region 変換
		private static U convert<T, U>(T self, Func<T, U> generator, Action<U, U> addAction)
		where T : IReadOnlyTreeNode<T> {
			if (generator == null) throw new ArgumentNullException("generator");
			if (addAction == null) throw new ArgumentNullException("addAction");
			var t = self.Postorder().Select(x => Tuple.Create(x, generator(x)));
			var vst = new ElementScroller<Tuple<T, U>>(t);
			foreach (var tr in vst.GetSequence()) {
				vst.MoveTo(tr)
					.MaybeNext(x => x.Item1.Children.Contains(tr.Item1))
					.TrueOrNot(r => addAction(r.Current.Item2, tr.Item2));
			}
			return vst.Last().Current.Item2;
		}
		/// <summary>対象ノードを始点とするツリーと同じ構造で、各ノードの型を変換した構造を再構築する。</summary>
		/// <typeparam name="T">変換前の型</typeparam>
		/// <typeparam name="U">変換後の型</typeparam>
		/// <param name="self">対象ノード</param>
		/// <param name="generator">各ノードに適用されるノード変換関数</param>
		public static U Convert<T, U>(this T self, Func<T, U> generator)
		where T : IReadOnlyTreeNode<T>
		where U : ITreeNode<U> {
			return convert(self, generator, (p, c) => p.AddChild(c));
		}
		/// <summary>対象ノードを始点とするツリーと同じ構造で、各ノードの型を変換した構造を再構築する。</summary>
		/// <typeparam name="T">変換前の型</typeparam>
		/// <typeparam name="U">変換後の型</typeparam>
		/// <param name="self">対象ノード</param>
		/// <param name="generator">各ノードに適用されるノード変換関数</param>
		/// <param name="addAction">親となるオブジェクトと子となるオブジェクトを引数に取り、その関係を成り立たせる関数</param>
		public static U Convert<T, U>(this T self, Func<T, U> generator, Action<U, U> addAction)
		where T : IReadOnlyTreeNode<T> {
			return convert(self, generator, addAction);
		}
		#endregion

		#region 組み立て
		private static T _assemble<T>(IEnumerable<Tuple<NodeIndex, T>> dic, Action<T, T> addAction) {
			var seq = dic.OrderBy(x => x.Item1, Tree.NodeIndex.GetPostorderComparer());
			var vst = new ElementScroller<Tuple<NodeIndex, T>>(seq);
			foreach (var tr in vst.GetSequence()) {
				vst.MoveTo(tr)
					.MaybeNext(x => tr.Item1.CurrentDepth > x.Item1.CurrentDepth)
					.TrueOrNot(r => addAction(r.Current.Item2, tr.Item2));
			}
			return vst.Last().Current.Item2;
		}
		private static U assemble<U, T>(IDictionary<NodeIndex, T> dictionary, Func<T, U> conv, Action<U, U> addAction) {
			var seq = dictionary.Select(x => Tuple.Create(x.Key, conv(x.Value)));
			return _assemble(seq, addAction);
		}
		/// <summary>各ノードをキーが示すインデックスをもとに組み立てる。</summary>
		/// <typeparam name="T">ノードの型</typeparam>
		public static T AssembleTree<T>(this IDictionary<NodeIndex, T> self) where T : ITreeNode<T> {
			return assemble(self, x => x, (p, c) => p.AddChild(c));
		}
		/// <summary>各データをキーが示すインデックスをもとに組み立てる。</summary>
		/// <typeparam name="T">データの型</typeparam>
		/// <param name="self">現在のオブジェクト</param>
		/// <param name="addAction">追加処理</param>
		public static T AssembleTree<T>(this IDictionary<NodeIndex, T> self, Action<T, T> addAction) {
			return assemble(self, x => x, addAction);
		}
		/// <summary>階層を示すインデックスをもとに、データからノードを生成しつつ組み立てる。</summary>
		/// <typeparam name="T">データの型</typeparam>
		/// <typeparam name="U">ノードの型</typeparam>
		/// <param name="self">現在のオブジェクト</param>
		/// <param name="conv">各データからノードへの変換関数</param>
		public static U AssembleTree<T, U>(this IDictionary<NodeIndex, T> self, Func<T, U> conv)
		where U : ITreeNode<U> {
			return assemble(self, conv, (p, c) => p.AddChild(c));
		}
		/// <summary>階層を示すインデックスをもとに、各データの変換と組み立てを行う。</summary>
		/// <typeparam name="T">データ</typeparam>
		/// <typeparam name="U">変換先の型</typeparam>
		/// <param name="self">現在のオブジェクト</param>
		/// <param name="conv">変換関数</param>
		/// <param name="addAction">追加処理</param>
		public static U AssembleTree<T, U>(this IDictionary<NodeIndex, T> self, Func<T, U> conv, Action<U, U> addAction) {
			return assemble(self, conv, addAction);
		}
		#endregion
	}
}
