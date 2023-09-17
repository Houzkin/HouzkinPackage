using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Collections {
	/// <summary>
	/// 要素の巡回操作の拡張メソッドを提供する。
	/// </summary>
	public static class ElementScroller {
		/// <summary>現在の位置にある要素に対して処理を行う。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="action">現在の位置にある要素に対する処理。</param>
		public static IElementScroller<T> Current<T>(this IElementScroller<T> scroller, Action<T> action) {
			if (action == null) throw new ArgumentNullException("action");
			action(scroller.Current);
			return scroller;
		}
		/// <summary>現在の位置にある要素に対して処理を行う。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="action">現在の位置と要素に対する処理。</param>
		public static IElementScroller<T> Current<T>(this IElementScroller<T> scroller, Action<T, int> action) {
			if (action == null) throw new ArgumentNullException("action");
			action(scroller.Current, scroller.CurrentIndex);
			return scroller;
		}
		/// <summary>現在の位置の後ろに続くシーケンスを取得する。</summary>
		static IEnumerable<T> nextSequence<T>(this IElementScroller<T> scroller) {
			return scroller.GetSequence()
				.SkipWhile(x => !Equals(x, scroller.Current))
				.Skip(1);
		}
		/// <summary>現在の位置より前にあるシーケンスから現在のノードに近い順のシーケンスを取得する。</summary>
		static IEnumerable<T> previousSequence<T>(this IElementScroller<T> scroller) {
			return scroller.GetSequence()
				.TakeWhile(x => !Equals(x, scroller.Current))
				.Reverse();
		}

		#region 移動
		#region Next
		/// <summary>次の位置へ移動する。</summary>
		/// <param name="scroller">対象インスタンス</param>
		/// <typeparam name="T">要素の型</typeparam>
		public static IElementScroller<T> Next<T>(this IElementScroller<T> scroller) {
			return scroller.Next(1);
		}
		/// <summary>現在位置から指定した数だけ後方に移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="count">移動距離を表す、0以上の値</param>
		public static IElementScroller<T> Next<T>(this IElementScroller<T> scroller, int count) {
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			return scroller.Move(count);
		}
		/// <summary>現在位置より後方の、条件を満たす位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件</param>
		public static IElementScroller<T> Next<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			var cnt = scroller.nextSequence()
				.Select((v, i) => new { Value = v, Index = i })
				.First(x => predicate(x.Value))
				.Index + 1;
			return scroller.Next(cnt);
		}
		/// <summary>現在の位置より後方に要素が存在する場合は移動する。</summary>
		/// <param name="scroller">対象インスタンス</param>
		/// <typeparam name="T">要素の型</typeparam>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> MaybeNext<T>(this IElementScroller<T> scroller) {
			return scroller.MaybeNext(1);
		}
		/// <summary>現在位置から後方へ指定した距離だけ移動可能な場合は移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="count">移動距離</param>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> MaybeNext<T>(this IElementScroller<T> scroller, int count) {
			if (scroller.HasNext(count))
				return new ResultWithValue<IElementScroller<T>>(true, scroller.Next(count));
			else
				return new ResultWithValue<IElementScroller<T>>(false, scroller);
		}
		/// <summary>現在位置より後方に条件を満たす要素が存在する場合はその位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件</param>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> MaybeNext<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			if (scroller.HasNext(predicate))
				return new ResultWithValue<IElementScroller<T>>(true, scroller.Next(predicate));
			else
				return new ResultWithValue<IElementScroller<T>>(false, scroller);
		}
		#endregion next

		#region Previous
		/// <summary>前の位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		public static IElementScroller<T> Previous<T>(this IElementScroller<T> scroller) {
			return scroller.Previous(1);
		}
		/// <summary>現在位置から指定した数だけ前方へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="count">移動距離を表す、0以上の値</param>
		public static IElementScroller<T> Previous<T>(this IElementScroller<T> scroller,int count) {
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			return scroller.Move(count);
		}
		/// <summary>現在位置より前方の、条件を満たす位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件</param>
		public static IElementScroller<T> Previous<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			var cnt = scroller.previousSequence()
				.Select((v, i) => new { Value = v, Index = i })
				.First(x => predicate(x.Value))
				.Index + 1;
			return scroller.Previous(cnt);
		}
		/// <summary>現在の位置より前方に要素が存在する場合は移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> MaybePrevious<T>(this IElementScroller<T> scroller) {
			return scroller.MaybePrevious(1);
		}
		/// <summary>現在位置より前方に条件を満たす要素が存在した場合はその位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件</param>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> MaybePrevious<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			if (scroller.HasPrevious(predicate))
				return new ResultWithValue<IElementScroller<T>>(scroller.Previous(predicate));
			else
				return new ResultWithValue<IElementScroller<T>>(false, scroller);
		}
		/// <summary>現在位置から前方へ指定した距離だけ移動可能な場合は移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="count">距離</param>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> MaybePrevious<T>(this IElementScroller<T> scroller, int count) {
			if (scroller.HasPrevious(count))
				return new ResultWithValue<IElementScroller<T>>(scroller.Previous(count));
			else
				return new ResultWithValue<IElementScroller<T>>(false, scroller);
		}
		#endregion previous

		#region First Last
		/// <summary>シーケンス内の最初の位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		public static IElementScroller<T> First<T>(this IElementScroller<T> scroller) {
			//return scroller.Move(1 - scroller.CurrentIndex);
			return scroller.MoveTo(0);
		}
		/// <summary>シーケンス内の条件を満たす最初の位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件</param>
		public static IElementScroller<T> First<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			var s = scroller.GetSequence().First(x => predicate(x));
			return scroller.MoveTo(s);
		}
		/// <summary>シーケンス内に条件を満たす要素が存在する場合、最初に見つかった要素の位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件</param>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> MaybeFirst<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			if (scroller.GetSequence().Any(x => predicate(x)))
				return new ResultWithValue<IElementScroller<T>>(scroller.First(predicate));
			else
				return new ResultWithValue<IElementScroller<T>>(false, scroller);
		}
		/// <summary>シーケンス内の最後の位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		public static IElementScroller<T> Last<T>(this IElementScroller<T> scroller) {
			//return scroller.Move(scroller.GetSequence().Count() - scroller.CurrentIndex - 1);
			return scroller.MoveTo(scroller.GetSequence().Count() - 1);
		}
		/// <summary>シーケンス内の条件を満たす最後の位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件</param>
		public static IElementScroller<T> Last<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			var s = scroller.GetSequence().Last(x => predicate(x));
			return scroller.MoveTo(s);
		}
		/// <summary>シーケンス内に条件を満たす要素が存在する場合、最後に見つかった要素の位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件</param>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> MaybeLast<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			if (scroller.GetSequence().Any(x => predicate(x)))
				return new ResultWithValue<IElementScroller<T>>(scroller.Last(predicate));
			else
				return new ResultWithValue<IElementScroller<T>>(false, scroller);
		}
		#endregion first last
		/// <summary>シーケンス内に指定した要素が存在する場合、その位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="element">要素</param>
		/// <returns>結果</returns>
		public static ResultWithValue<IElementScroller<T>> TryMoveTo<T>(this IElementScroller<T> scroller, T element) {
			if (scroller.GetSequence().Contains(element))
				return new ResultWithValue<IElementScroller<T>>(scroller.MoveTo(element));
			else
				return new ResultWithValue<IElementScroller<T>>(false, scroller);
		}
		/// <summary>シーケンス内の指定されたインデックスの位置へ移動する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="index">インデックス</param>
		public static IElementScroller<T> MoveTo<T>(this IElementScroller<T> scroller, int index) {
			var cnt = index - scroller.CurrentIndex;
			return scroller.Move(cnt);
		}
		/// <summary>シーケンス内の指定したインデックスにアクセス可能な場合、その位置へ移動する。</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="scroller"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static ResultWithValue<IElementScroller<T>> TryMoveTo<T>(this IElementScroller<T> scroller, int index) {
			var cnt = index - scroller.CurrentIndex;
			if (scroller.CanMove(cnt))
				return new ResultWithValue<IElementScroller<T>>(scroller.MoveTo(cnt));
			else
				return new ResultWithValue<IElementScroller<T>>(false, scroller);
		}
		///// <summary>スタックが存在する場合は取り出し、その位置へ移動する。</summary>
		///// <typeparam name="T">要素の型</typeparam>
		///// <param name="scroller">対象インスタンス</param>
		///// <returns>結果</returns>
		//public static ResultWithValue<IElementScroller<T>> TryPop<T>(this IElementScroller<T> scroller) {
		//	if (0 < scroller.StackCount)
		//		return new ResultWithValue<IElementScroller<T>>(scroller.Pop());
		//	else
		//		return new ResultWithValue<IElementScroller<T>>(false, scroller);
		//}
		#endregion 移動

		#region 判定
		/// <summary>現在の位置の次に要素が存在するかどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		public static bool HasNext<T>(this IElementScroller<T> scroller) {
			return scroller.HasNext(1);
		}
		/// <summary>現在の位置の次に要素が存在するかどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="count">距離</param>
		public static bool HasNext<T>(this IElementScroller<T> scroller, int count) {
			if (count <= 0) throw new ArgumentOutOfRangeException("count");
			count = count - 1;
			return scroller.nextSequence().Skip(count).Any();
		}
		/// <summary>現在の位置の後方に、指定した条件を満たす要素が存在するかどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件を表す関数</param>
		public static bool HasNext<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			return scroller.nextSequence().Any(x => predicate(x));
		}
		/// <summary>現在の位置の名に要素が存在するかどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		public static bool HasPrevious<T>(this IElementScroller<T> scroller) {
			return scroller.HasPrevious(1);
		}
		/// <summary>現在の位置の前に要素が存在するかどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="count">距離</param>
		public static bool HasPrevious<T>(this IElementScroller<T> scroller, int count) {
			if (count <= 0) throw new ArgumentOutOfRangeException("count");
			count = count - 1;
			return scroller.previousSequence().Skip(count).Any();
		}
		/// <summary>現在の位置の前方に、指定した条件を満たす要素が存在するかどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件を表す関数</param>
		public static bool HasPrevious<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			return scroller.previousSequence().Any(x => predicate(x));
		}
		/// <summary>現在の位置が巡回シーケンスの先頭だ銅貨を示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		public static bool IsFirst<T>(this IElementScroller<T> scroller) {
			return scroller.IsFirst(x => true);
		}
		/// <summary>現在の位置が巡回シーケンスの先頭かどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件を表す関数</param>
		public static bool IsFirst<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			return Equals(scroller.Current, scroller.GetSequence().FirstOrDefault(x => predicate(x)));
		}
		/// <summary>現在の位置が巡回シーケンスの最後かどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		public static bool IsLast<T>(this IElementScroller<T> scroller) {
			return scroller.IsLast(x => true);
		}
		/// <summary>現在の位置が巡回シーケンスの最後かどうかを示す値を取得する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="predicate">条件を表す関数</param>
		public static bool IsLast<T>(this IElementScroller<T> scroller, Predicate<T> predicate) {
			return Equals(scroller.Current, scroller.GetSequence().LastOrDefault(x => predicate(x)));
		}
		#endregion 判定

		#region 取得
		/// <summary>述語の処理により現在の位置が移動した場合、処理前の位置へ戻す。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="action">処理</param>
		public static IElementScroller<T> Restore<T>(this IElementScroller<T> scroller, Action<IElementScroller<T>> action) {
			var b = scroller.Current;
			action(scroller);
			scroller.MoveTo(b);
			return scroller;
		}
		/// <summary>指定した回数だけ、インスタンスに対する処理を行う。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="count">回数</param>
		/// <param name="process">処理</param>
		public static IElementScroller<T> Repeat<T>(this IElementScroller<T> scroller, int count, Action<IElementScroller<T>> process) {
			if (scroller == null) throw new ArgumentNullException("scroller");
			for (int i = 0; i < count; i++) { process(scroller); }
			return scroller;
		}
		/// <summary>初回は現在のインスタンスで処理を行い、そのインスタンスが条件を満たす限り繰り返し処理を行う。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="isContinue">現在のインスタンスに対して継続の判定を行う。</param>
		/// <param name="process">処理</param>
		public static IElementScroller<T> DoWhile<T>(this IElementScroller<T> scroller, Predicate<IElementScroller<T>> isContinue, Action<IElementScroller<T>> process) {
			if (scroller == null) throw new ArgumentNullException("scroller");
			do {
				process(scroller);
			} while (isContinue(scroller));
			return scroller;
		}
		/// <summary>指定された継続条件を満たす限り、現在のインスタンスを引数にとる関数を実行する。</summary>
		/// <typeparam name="T">要素の型</typeparam>
		/// <param name="scroller">対象インスタンス</param>
		/// <param name="isContinue">初回から適用される継続条件</param>
		/// <param name="process">処理</param>
		public static IElementScroller<T> While<T>(this IElementScroller<T> scroller,Predicate<IElementScroller<T>> isContinue,Action<IElementScroller<T>> process) {
			if (scroller == null) throw new ArgumentNullException("scroller");
			while (isContinue(scroller)) {
				process(scroller);
			}
			return scroller;
		}
		#endregion
	}
}
