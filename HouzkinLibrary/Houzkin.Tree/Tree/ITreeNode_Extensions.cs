﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Houzkin;
using Houzkin.Collections;
using System.Collections.ObjectModel;

namespace Houzkin.Tree {
	/// <summary>
	/// ツリー構造を成すオブジェクトに対する拡張メソッドを定義する。
	/// </summary>
	public static partial class TreeNode {

		#region 編集

		/// <summary>子ノードを追加する。</summary>
		/// <returns>正常に追加できたかどうか示す値と現在のノードを返す。</returns>
		public static ResultWithValue<T> TryAddChild<T>(this ITreeNode<T> self, T child) where T : ITreeNode<T> {
			var cash = self.Children.Count;
			var ncash = self.AddChild(child).Children.ToArray();
			if (cash + 1 == ncash.Length && ncash.Contains(child)) return new ResultWithValue<T>((T)self);
			return new ResultWithValue<T>(false, (T)self);
		}
		/// <summary>子ノードを削除する。</summary>
		/// <returns>正常に削除できたかどうか示す値と現在のノードを返す。</returns>
		public static ResultWithValue<T> TryRemoveChild<T>(this ITreeNode<T> self, T child) where T : ITreeNode<T> {
			var cash = self.Children.Count;
			self.RemoveChild(child);
			var ncash = self.Children.Count;
			if (cash - 1 == ncash) return new ResultWithValue<T>((T)self);
			return new ResultWithValue<T>(false, (T)self);
		}
		///// <summary>条件に一致する子ノードを全て削除する。</summary>
		//[Obsolete("このメソッドの使用は非推奨です。RemoveChildrenの使用を検討してください。")]
		//public static T RemoveChild<T>(this ITreeNode<T> self, Predicate<T> predicate) where T : ITreeNode<T> {
		//	if (predicate == null) throw new ArgumentNullException("predicate");
		//	foreach (var cld in self.Children.ToArray()) {
		//		if (predicate(cld)) self.RemoveChild(cld);
		//	}
		//	return (T)self;
		//}
		///// <summary>対象ノードからレベル順に、条件に一致したノードを全て削除する。</summary>
		//[Obsolete("このメソッドの使用は非推奨です。RemoveDescendantsメソッドの使用を検討してください。")]
		//public static T RemoveDescendant<T>(this ITreeNode<T> self, Predicate<T> predicate) where T : ITreeNode<T> {
		//	if (predicate == null) throw new ArgumentNullException("predicate");
		//	self.Evolve(x => x.RemoveChild(predicate).Children, (x, y) => x.Concat(y));
		//	return (T)self;
		//}
        ///// <summary>子孫ノードを全て分解する。</summary>
        //public static T DismantleDescendants<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
        //	foreach (var cld in self.Levelorder().ToArray())
        //		cld.ClearChildren();
        //	return (T)self;
        //}
		
        /// <summary>子孫ノードを全て分解する。</summary>
        public static IReadOnlyList<T> DismantleDescendants<T>(this ITreeNode<T> self) where T : ITreeNode<T>{
			var lst = self.Levelorder().ToArray();
			foreach (var cld in lst) cld.ClearChildren();
			return lst;
		}
		/// <summary>条件に一致する子ノードを全て削除する。</summary>
		/// <typeparam name="T">ノードの型</typeparam>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">削除対象であればtrue</param>
		/// <returns>削除したノード</returns>
        public static IReadOnlyList<T> RemoveChild<T>(this ITreeNode<T> self, Predicate<T> predicate) where T : ITreeNode<T> {
			if(predicate == null) throw new ArgumentNullException(nameof(predicate));
			var lst = new List<T>();
			foreach(var cld in self.Children.ToArray()) {
				if (predicate(cld)) {
					lst.Add(cld);
					self.RemoveChild(cld);
				}
			}
			return lst;
		}
		/// <summary>対象ノードからレベル順に、条件に一致したノードを全て削除する。</summary>
		/// <typeparam name="T">ノードの型</typeparam>
		/// <param name="self">現在のノード</param>
		/// <param name="predicate">削除対象であればtrue</param>
		/// <returns>削除したノード</returns>
		public static IReadOnlyList<T> RemoveDescendant<T>(this ITreeNode<T> self, Predicate<T> predicate) where T : ITreeNode<T> {
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));
			var lst = new List<T>();
			self.Evolve(x => {
				lst.AddRange(x.RemoveChild(predicate));
				return x.Children;
			}, (x, y) => x.Concat(y));
			return lst;
		}
		/// <summary>現在のノードの次に兄弟ノードを追加する。</summary>
		/// <param name="self">対象ノード</param>
		/// <param name="sibling">追加する兄弟ノード</param>
		/// <returns>正常に追加できたかどうか示す値と現在のノードを返す。</returns>
		public static ResultWithValue<T> TryInsertNextSibling<T>(this ITreeNode<T> self,ITreeNode<T> sibling)
		where T : ITreeNode<T> {
			if (self.Parent != null) {
				var cash = self.Children.Count;
				var ncash = self.Parent.InsertChild(self.BranchIndex() + 1, (T)sibling).Children.ToArray();
				if (cash + 1 == ncash.Length && ncash.Contains((T)sibling)) return new ResultWithValue<T>((T)self);
			}
			return new ResultWithValue<T>(false, (T)self);
		}
		/// <summary>現在のノードの前に兄弟ノードを追加する。</summary>
		/// <param name="self">対象ノード</param>
		/// <param name="sibling">追加する兄弟ノード</param>
		/// <returns>正常に追加できたかどうか示す値と現在のノードを返す。</returns>
		public static ResultWithValue<T> TryInsertPreviousSibling<T>(this ITreeNode<T> self,ITreeNode<T> sibling)
		where T : ITreeNode<T> {
			if (self.Parent != null) {
				var cash = self.Children.Count;
				var ncash = self.Parent.InsertChild(self.BranchIndex(), (T)sibling).Children.ToArray();
				if (cash + 1 == ncash.Length && ncash.Contains((T)sibling)) return new ResultWithValue<T>((T)self);
			}
			return new ResultWithValue<T>(false,(T)self);
		}
		/// <summary>現在のノードを削除する。</summary>
		/// <returns>正常に削除できた場合は親だったノードを、できなかった場合は現在のノードを付与した結果を返す。</returns>
		public static ResultWithValue<T> TryRemoveOwn<T>(this ITreeNode<T> self) where T : ITreeNode<T> {
			if (self.Parent != null) {
				var parent = self.Parent;
				self.Parent.RemoveChild((T)self);
				return new ResultWithValue<T>(parent);
			}
			return new ResultWithValue<T>(false, (T)self);
		}
		#endregion
	}
	
}
