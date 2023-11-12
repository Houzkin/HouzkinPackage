using Houzkin.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {
	// IReadOnlyTreeNodeに対する拡張メソッドを定義する。
	public static partial class TreeNode {

		#region 列挙
		/// <summary>逐次的に列挙する</summary>
		private static bool _evolve<T>(
			Func<T, IEnumerable<T>> evolve,
			Func<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>, IEnumerable<T>> alignment,
			ref IEnumerable<T> seeds, ref IEnumerable<T> his, out T cur) where T : IReadOnlyTreeNode<T> {

			if (seeds.Any()) {
				cur = seeds.First();
				var nxtScd = (evolve(cur) ?? new T[0]);//.Where(x => x != null);
				his = his.Concat(new T[] { cur }).ToArray();
				seeds = alignment(his, seeds.Skip(1), nxtScd).ToArray();
				return true;
			} else {
				cur = default(T);
				return false;
			}
		}
		/// <summary>対象ノードを初期値として、指定された関数によって要素を逐次生成する。</summary>
		/// <typeparam name="T">ノードの型</typeparam>
		/// <param name="self">対象ノード</param>
		/// <param name="evolve">ノードを引数に、追加シーケンスを生成する。</param>
		/// <param name="alignment">処理履歴、未処理要素、新規追加要素の順の引数から未処理要素を更新する。
		/// <para>次のシーケンスは未処理要素の先頭から処理される。</para></param>
		public static IEnumerable<T> Evolve<T>(this  IReadOnlyTreeNode<T> self,
			Func<T, IEnumerable<T>> evolve,
			Func<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>, IEnumerable<T>> alignment)
			where T : IReadOnlyTreeNode<T> {

			IEnumerable<T> df = new T[1] { (T)self };
			T cur;
			IEnumerable<T> his = new T[0];
			while (_evolve(evolve, alignment, ref df, ref his, out cur)) {
				yield return cur;
			}
		}
		/// <summary>対象ノードを初期値として、指定された関数によって要素を逐次生成する。</summary>
		/// <typeparam name="T">ノードの型</typeparam>
		/// <param name="self">対象ノード</param>
		/// <param name="evolve">ノードを引数に、追加シーケンスを生成する。</param>
		/// <param name="alignment">未処理要素、新規追加要素の順の引数から未処理要素を更新する。
		/// <para>一度処理された要素は除外される。</para>
		/// <para>次のシーケンスは未処理要素の先頭から処理される。</para></param>
		public static IEnumerable<T> Evolve<T>(this  IReadOnlyTreeNode<T> self,
			Func<T, IEnumerable<T>> evolve,
			Func<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>> alignment)
			where T : IReadOnlyTreeNode<T> {

			Func<IEnumerable<T>, IEnumerable<T>, IEnumerable<T>, IEnumerable<T>> ali =
				(hst, stc, evl) => alignment(stc, evl).Except(hst);
			return self.Evolve(evolve, ali);
		}
		/// <summary>対象ノードを始点とし、先行順でシーケンスを生成する。</summary>
		public static IEnumerable<T> Preorder<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Evolve(
				x => x.Children,
				(x, y) => y.Concat(x));
		}
		/// <summary>対象ノードを始点とし、後行順でシーケンスを生成する。</summary>
		public static IEnumerable<T> Postorder<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Evolve(
				x => x.Children.Reverse(),
				(x, y) => y.Concat(x))
				.Reverse();
		}
		/// <summary>対象ノードを始点とし、レベル順でシーケンスを生成する。</summary>
		public static IEnumerable<T> Levelorder<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Evolve(
				x => x.Children,
				(x, y) => x.Concat(y));
		}
		/// <summary>対象ノードを始点とし、中間順でシーケンスを生成する。</summary>
		public static IEnumerable<T> Inorder<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			var lcl = self.Evolve(
				x => x.Children,
				(h, x, y) => {
					var pre = y.Take(1);
					var pst = y.Skip(1);
					IEnumerable<T> ad;
					if (2 <= h.Count(hx => object.Equals(hx, h.Last()))) {
						ad = new T[] { h.Last() };
					} else {
						ad = pre.Concat(new T[] { h.Last() }).Concat(pst);
					}
					var ads = ad.Except(x);
					return x.Concat(ads);
				});
			IEnumerable<T> tk;
			var tp = self.Postorder().First();
			var ltc = self.Levelorder().Count();
			do {
				lcl = lcl.SkipWhile(x => !object.Equals(tp, x));
				tk = lcl.Take(ltc);
				lcl = lcl.Skip(1);
			} while (self.Levelorder().Except(tk).Count() != 0);
			return tk;
		}
		/// <summary>対象ノードから祖先方向へシーケンスを生成する。</summary>
		public static IEnumerable<T> Upstream<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self
				.Evolve(
					x => x.Parent != null ? new T[] { x.Parent } : new T[0],
					(a, b) => a.Concat(b));
		}
		/// <summary>現在のノードを含めた兄弟ノードを取得する。</summary>
		public static IEnumerable<T> Siblings<T>(this IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			IEnumerable<T> arr;
			if (self == null) throw new ArgumentNullException("self");
			if (self.Parent == null) arr = new T[] { (T)self };
			else arr = self.Parent.Children;
			return arr;
		}
		/// <summary>現在より前の兄弟ノードを取得する。</summary>
		static IEnumerable<T> previousSiblings<T>(this IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Siblings().TakeWhile(x => !object.Equals(x, self));
		}
		/// <summary>現在より後の兄弟ノードを取得する。</summary>
		static IEnumerable<T> nextSiblings<T>(this IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Siblings().SkipWhile(x => !object.Equals(x, self)).Skip(1);
		}
		#endregion

		#region 移動メソッド
		/// <summary>対象ノードが所属するツリーのルートノードを取得する。</summary>
		public static T Root<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Upstream().Last();
		}
		/// <summary>兄弟ノードの最初のノードへ移動する。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">条件を指定した場合、条件を満たす最初のノードを取得する。</param>
		public static T FirstSibling<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate) where T : IReadOnlyTreeNode<T> {
			predicate = predicate ?? new Predicate<T>(x => true);
			return self.Siblings().First(x => predicate(x));
		}
		/// <summary>兄弟ノードの最初のノードへ移動する。見つからなかった場合は現在のノードを返す。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">条件を指定した場合、条件を満たす最初のノードを取得する。</param>
		public static ResultWithValue<T> FirstSiblingOrSelf<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
			var fst = self.Siblings().FirstOrDefault(x => pred(x));
			if (fst != null) return new ResultWithValue<T>(fst);
			else return new ResultWithValue<T>(false, (T)self);
		}
		/// <summary>兄弟ノードの最後へ移動する。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">条件を指定した場合、条件を満たす最後のノードを取得する。</param>
		public static T LastSibling<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			Predicate<T> pred = predicate ?? new Predicate<T>(x => true);
			return self.Siblings().Last(x => pred(x));
		}
		/// <summary>兄弟ノードの最後へ移動する。見つからなかった場合は現在のノードを返す。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">条件を指定した場合、条件を満たす最後のノードを取得する。</param>
		public static ResultWithValue<T> LastSiblingOrSelf<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
			var lst = self.Siblings().LastOrDefault(x => pred(x));
			if (lst != null) return new ResultWithValue<T>(lst);
			else return new ResultWithValue<T>(false, (T)self);
		}
		/// <summary>次の兄弟ノードへ移動する。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">条件を指定した場合、条件を満たす次のノードを取得する。</param>
		public static T NextSibling<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			Predicate<T> pred = predicate ?? new Predicate<T>(x => true);
			return self.nextSiblings().First(x => pred(x));
		}
		/// <summary>次の兄弟ノードへ移動する。見つからなかった場合は現在のノードを返す。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">条件を指定した場合、条件を満たす次のノードを取得する。</param>
		public static ResultWithValue<T> NextSiblingOrSelf<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
			var nxt = self.nextSiblings().FirstOrDefault(x => pred(x));
			if (nxt != null) return new ResultWithValue<T>(nxt);
			else return new ResultWithValue<T>(false, (T)self);
		}
		/// <summary>前の兄弟ノードへ移動する。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">条件を指定した場合、条件を満たす前のノードを取得する。</param>
		public static T PreviousSibling<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			Predicate<T> pred = predicate ?? new Predicate<T>(x => true);
			return self.previousSiblings().Last(x => pred(x));
		}
		/// <summary>前の兄弟ノードへ移動する。見つからなかった場合は現在のノードを返す。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">条件を指定した場合、条件を満たす前のノードを取得する。</param>
		public static ResultWithValue<T> PreviousSiblingOrSelf<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			Predicate<T> pred = predicate ?? new Predicate<T>(_ => true);
			var prv = self.previousSiblings().LastOrDefault(x => pred(x));
			if (prv != null) return new ResultWithValue<T>(prv);
			else return new ResultWithValue<T>(false, (T)self);
		}
		/// <summary>シーケンス巡回用のインスタンスを生成する。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="seq">現在のノードを含むシーケンス</param>
		/// <returns>列挙子</returns>
		public static IElementScroller<T> ToElementScroller<T>(this IReadOnlyTreeNode<T> self,IEnumerable<T> seq) where T : IReadOnlyTreeNode<T> {
			return new ElementScroller<T>(seq).MoveTo((T)self);
		}
		/// <summary>述語の処理により現在のノードから移動しても、処理前のノードへ戻ってくる。</summary>
		/// <returns>述語の処理を行う前に対象としていたノード</returns>
		public static T Restore<T>(this T self, Action<T> action)//Fork or Pretreat
		where T : IReadOnlyTreeNode<T> {
			if (self == null) throw new ArgumentNullException("self");
			if (action != null) action(self);
			return self;
		}
		#endregion

		#region 判定メソッド
		/// <summary>現在のノードが、指定したノードの子孫ノードかどうかを示す値を取得する。</summary>
		/// <param name="self">対象ノード</param>
		/// <param name="node">指定ノード</param>
		public static bool IsDescendantOf<T>(this  IReadOnlyTreeNode<T> self, T node) where T : IReadOnlyTreeNode<T> {
			return self.Upstream().Skip(1).Contains(node);
		}
		/// <summary>現在のノードが、指定したノードの祖先ノードかどうかを示す値を取得する。</summary>
		/// <param name="self">対象ノード</param>
		/// <param name="node">指定ノード</param>
		public static bool IsAncestorOf<T>(this  IReadOnlyTreeNode<T> self, T node) where T : IReadOnlyTreeNode<T> {
			return node.Upstream().Skip(1).Contains((T)self);
		}
		/// <summary>対象ノードが現在ルートノードかどうかを示す値を取得する。</summary>
		public static bool IsRoot<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Parent == null;
		}
		/// <summary>現在のノードの次に兄弟ノードが存在するかどうかを示す値を取得する。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">関数を指定した場合、指定した条件を満たすノードの存在を判定する。</param>
		public static bool HasNextSibling<T>(this IReadOnlyTreeNode<T> self,Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			predicate = predicate ?? new Predicate<T>(x => true);
			return self.nextSiblings().Any(x => predicate(x));
		}
		/// <summary>現在のノードの前に兄弟ノードが存在するかどうかを示す値を取得する。</summary>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">関数を指定した場合、指定した条件を満たすノードの存在を判定する。</param>
		public static bool HasPrevoiusSibling<T>(this IReadOnlyTreeNode<T> self, Predicate<T> predicate = null) where T : IReadOnlyTreeNode<T> {
			predicate = predicate ?? new Predicate<T>(x => true);
			return self.previousSiblings().Any(x => predicate(x));
		}
		/// <summary>現在のノードが最後の兄弟ノードかどうかを示す値を取得する。</summary>
		public static bool IsLastSibling<T>(this IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return !self.HasNextSibling();
		}
		/// <summary>現在のノードが最初の兄弟ノードかどうかを示す値を取得する。</summary>
		public static bool IsFirstSibling<T>(this IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return !self.HasPrevoiusSibling();
		}
		#endregion

		#region 取得
		/// <summary>対象ノードの位置を示すパスコードを取得する。</summary>
		public static NodeIndex NodeIndex<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			var z = self
				.Upstream()
				.Select(b => b.BranchIndex())
				.TakeWhile(c => 0 <= c)
				.Reverse();
			return new NodeIndex(z);
		}
		/// <summary>ルートから現在のノードまでのパスを生成する。</summary>
		/// <param name="self">対象ノード</param>
		/// <param name="conv">各ノードの固有の値、またはそのノードを示す値を指定する。</param>
		public static NodePath<TPath> NodePath<TPath,T>(this T self,Converter<T,TPath> conv)
		where T : IReadOnlyTreeNode<T> {
			return NodePath<TPath>.Create(self, conv);
		}
		/// <summary>対象ノードにおいて、その子孫ノード最深部からの距離を取得する。</summary>
		public static int Height<T>(this  IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Levelorder().Last().Depth() - self.Depth();
		}
		/// <summary>対象ノードにおいて、Rootからの深さを取得する。。</summary>
		public static int Depth<T>(this IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			return self.Upstream().Count() - 1;
		}
		/// <summary>対象ノードが親ノードによって振り当てられているインデックスを取得する。</summary>
		public static int BranchIndex<T>(this IReadOnlyTreeNode<T> self) where T : IReadOnlyTreeNode<T> {
			if (self.Parent == null) return -1;
			return self.Parent.Children
				.Select((nd, idx) => new { Node = nd, Index = idx })
				.First(x => object.Equals(x.Node, self)).Index;
		}
		#endregion

		#region 変換
		/// <summary>各ノードから生成したデータとそのインデックスをペアとするコレクションを取得する。</summary>
		/// <typeparam name="U">生成するデータの型</typeparam>
		/// <typeparam name="T">各ノードの型</typeparam>
		/// <param name="self">始点となるノード</param>
		/// <param name="conv">変換関数</param>
		public static Dictionary<NodeIndex,U> ToNodeMap<T,U>(this IReadOnlyTreeNode<T> self, Func<T,U> conv)
		where T : IReadOnlyTreeNode<T> {
			return self.Levelorder().ToDictionary(x => x.NodeIndex(), x => conv(x));
		}
		/// <summary>各ノードとそのインデックスをペアとするコレクションを取得する。</summary>
		/// <typeparam name="T">各ノードの型</typeparam>
		/// <param name="self">始点となるノード</param>
		public static Dictionary<NodeIndex, T> ToNodeMap<T>(this IReadOnlyTreeNode<T> self)
		where T : IReadOnlyTreeNode<T> {
			return self.ToNodeMap(x => x);
		}
		/// <summary>文字列で樹形図を生成する。</summary>
		/// <typeparam name="T">ノードの型</typeparam>
		/// <param name="self">対象ノード</param>
		/// <param name="conv">ノードを表す文字列へ変換</param>
		/// <returns>樹形図</returns>
        public static string ToTreeDiagram<T>(this IReadOnlyTreeNode<T> self, Func<T, string> conv) where T : IReadOnlyTreeNode<T> {
            string branch = "├ ";
            string lastBranch = "└ ";
            string through = "│ ";
			string blank = "  ";
			
			Func<IEnumerable<T>, NodeIndex, string> createLine = (nodes, idx) => {
				var line = string.Concat(nodes.Zip(idx).Select(pair => pair.First.IsRoot() ? "" : pair.First.HasNextSibling() ? through : blank));
				return line;
			};
			var strlit = new List<string>();
			foreach(var n in self.Preorder()) {
				var line = createLine(n.Upstream().Reverse(), n.NodeIndex());
				var head = n.IsRoot() ? "": n.HasNextSibling() ? branch : lastBranch;
				line = line + head + conv(n) + Environment.NewLine;
				strlit.Add(line);
			}
			return string.Concat(strlit);
        }
        #endregion
    }
}
