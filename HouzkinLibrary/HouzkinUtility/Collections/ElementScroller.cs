using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Collections {

	/// <summary>シーケンスの巡回をサポートする。</summary>
	/// <typeparam name="T">要素の型</typeparam>
	public interface IElementScroller<T> {
		/// <summary>現在の位置にある要素を取得する。</summary>
		T Current { get; }
		/// <summary>現在の位置を取得する。</summary>
		int CurrentIndex { get; }
		/// <summary>移動シーケンスを取得する。</summary>
		IEnumerable<T> GetSequence();

		/// <summary>指定した要素の位置へ移動する。</summary>
		/// <param name="element">移動先の位置にある要素</param>
		IElementScroller<T> MoveTo(T element);

		/// <summary>現在の位置を０とし、前方をマイナス、後方をプラスの整数で示した値だけ移動する。</summary>
		/// <param name="moveCount">移動方向と距離を示す値</param>
		IElementScroller<T> Move(int moveCount);

		/// <summary>現在の位置から指定された方向と距離だけ移動可能かどうかを示す値を取得する。</summary>
		/// <param name="moveCount">移動方向と距離を示す値</param>
		bool CanMove(int moveCount);
	}

	/// <summary>指定されたシーケンスを巡回する列挙子を表す。</summary>
	/// <typeparam name="T">要素の型</typeparam>
	public class ElementScroller<T> : IElementScroller<T>{
		/// <summary>指定されたシーケンスをコピーして、新規インスタンスを初期化する。</summary>
		/// <param name="sequence">巡回するシーケンス</param>
		public ElementScroller(IEnumerable<T> sequence) {
			if (sequence == null) throw new ArgumentNullException();
			if (!sequence.Any()) throw new ArgumentOutOfRangeException("sequence","シーケンスが空です。");
			seq = sequence.ToArray();
		}

		IList<T> seq;
		int curIdx = 0;

		/// <summary>現在の位置にある要素を取得する。</summary>
		public T Current {
			get { return seq[curIdx]; }
		}
		/// <summary>現在の位置を取得する。</summary>
		public int CurrentIndex {
			get { return curIdx; }
		}
		/// <summary>移動シーケンスを取得する。</summary>
		public IEnumerable<T> GetSequence() {
			return seq;
		}
		
		/// <summary>現在の位置を０とし、前方をマイナス、後方をプラスの整数で示した値だけ移動する。</summary>
		/// <param name="moveCount">移動方向と距離を示す値</param>
		public IElementScroller<T> Move(int moveCount) {
			var cnt = CurrentIndex + moveCount;
			if (cnt < 0 || seq.Count <= cnt) throw new ArgumentOutOfRangeException("moveCount");
			curIdx = cnt;
			return this;
		}
		/// <summary>現在の位置から指定された方向と距離だけ移動可能かどうかを示す値を取得する。</summary>
		/// <param name="moveCount">移動方向と距離を示す値</param>
		public bool CanMove(int moveCount) {
			var cnt = CurrentIndex + moveCount;
			if (cnt < 0 || seq.Count <= cnt) return false;
			else return true;
		}
		/// <summary>指定した要素の位置へ移動する。</summary>
		/// <param name="element">移動先の位置にある要素</param>
		public IElementScroller<T> MoveTo(T element) {
			var idx = seq.IndexOf(element);
			if (idx < 0) throw new ArgumentException("指定された要素はシーケンスに存在しません。", "element");
			curIdx = idx;
			return this;
		}
	}
}
