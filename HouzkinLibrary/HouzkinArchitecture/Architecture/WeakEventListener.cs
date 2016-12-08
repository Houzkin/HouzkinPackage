using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Architecture {

	/// <summary>
	/// ウィークイベントリスナーの基底クラスを表す。
	/// <para>拡張して使用する場合はジェネリック型を継承する。</para>
	/// </summary>
	/// <typeparam name="TEventArgs">イベント引数の型</typeparam>
	public class WeakEvent<TEventArgs> : IDisposable where TEventArgs : EventArgs {
		/// <summary>弱参照イベントとして登録する。</summary>
		/// <typeparam name="TSource">イベントソース</typeparam>
		/// <typeparam name="TListener">弱参照する対象</typeparam>
		/// <typeparam name="THandler">ソースにメンバーとして公開されてあるイベントハンドラーの型</typeparam>
		/// <param name="sender">イベントソース</param>
		/// <param name="listener">弱参照する対象</param>
		/// <param name="conv">イベントハンドラーの型へ変換する。</param>
		/// <param name="add">ソースにハンドラーを追加する。</param>
		/// <param name="remove">ソースからハンドラーを削除する。</param>
		/// <param name="handler">処理を行うハンドラー</param>
		public static IDisposable Register<TSource, TListener, THandler>(TSource sender, TListener listener, Func<EventHandler<TEventArgs>, THandler> conv,
		Action<TSource, THandler> add, Action<TSource, THandler> remove, Action<TListener, object, TEventArgs> handler) where TListener : class {
			return new WeakEventListener<TSource, TListener, THandler, TEventArgs>(sender, listener, conv, add, remove, handler);
		}
		/// <summary>弱参照イベントとして登録する。</summary>
		/// <typeparam name="TSource">イベントソース</typeparam>
		/// <typeparam name="TListener">弱参照する対象</typeparam>
		/// <param name="sender">イベントソース</param>
		/// <param name="listener">弱参照する対象</param>
		/// <param name="add">ソースにハンドラーを追加する。</param>
		/// <param name="remove">ソースからハンドラーを削除する。</param>
		/// <param name="handler">処理を行うハンドラー</param>
		public static IDisposable Register<TSource, TListener>(TSource sender, TListener listener, Action<TSource, EventHandler<TEventArgs>> add,
		Action<TSource, EventHandler<TEventArgs>> remove, Action<TListener, object, TEventArgs> handler) where TListener : class {
			return new WeakEventListener<TSource, TListener, EventHandler<TEventArgs>, TEventArgs>(sender, listener, x => x, add, remove, handler);
		}
		/// <summary>リスナーを生成する。</summary>
		/// <typeparam name="THandler">ソースにメンバーとして公開されてあるイベントハンドラーの型</typeparam>
		/// <param name="conv">イベントハンドラーの型へ変換する。</param>
		/// <param name="add">ソースへハンドラーを追加する。</param>
		/// <param name="remove">ソースからハンドラーを削除する。</param>
		/// <param name="handler">処理を行うハンドラー</param>
		public static IDisposable CreateListener<THandler>(Func<EventHandler<TEventArgs>, THandler> conv,
		Action<THandler> add, Action<THandler> remove, EventHandler<TEventArgs> handler) {
			return new WeakEventListener<THandler, TEventArgs>(conv, add, remove, handler);
		}
		/// <summary>リスナーを生成する。</summary>
		/// <param name="add">ソースへハンドラーを追加する。</param>
		/// <param name="remove">ソースからハンドラーを削除する。</param>
		/// <param name="handler">処理を行うハンドラー</param>
		public static IDisposable CreateListener(Action<EventHandler<TEventArgs>> add, Action<EventHandler<TEventArgs>> remove,
		EventHandler<TEventArgs> handler) {
			return new WeakEventListener<EventHandler<TEventArgs>, TEventArgs>(x => x, add, remove, handler);
		}
		internal WeakEvent() { }
		/// <summary>現在のインスタンスが既に破棄されているかどうかを示す値を取得する。</summary>
		protected bool IsDisposed { get; private set; }
		/// <summary>破棄されたインスタンスの操作を禁止する。</summary>
		protected void ThrowExceptionIfDisposed() {
			if (IsDisposed)
				throw new ObjectDisposedException("既に破棄されているインスタンスが操作されようとしました。");
		}
		/// <summary>リソースの解放を行う。</summary>
		protected virtual void Dispose(bool disposing) {
			if (IsDisposed) return;
			IsDisposed = true;
		}
		/// <summary>破棄する。</summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>ファイナライズ</summary>
		~WeakEvent() {
			Dispose(false);
		}
	}
	/// <summary>指定したインスタンスを弱参照するイベントリスナー。</summary>
	/// <typeparam name="TSource">イベントソース</typeparam>
	/// <typeparam name="TListener">弱参照する対象</typeparam>
	/// <typeparam name="THandler">イベントハンドラー</typeparam>
	/// <typeparam name="TEventArgs">イベント引数</typeparam>
	public class WeakEventListener<TSource, TListener, THandler, TEventArgs> : WeakEvent<TEventArgs>
		where TListener : class
		where TEventArgs : EventArgs {
		readonly WeakReference<TListener> listenerRef;
		Action _remove;
		static THandler CreateHandler(WeakEventListener<TSource, TListener, THandler, TEventArgs> self, TListener listener,
			Func<EventHandler<TEventArgs>, THandler> conv, Action<TListener, object, TEventArgs> handler) {
			EventHandler<TEventArgs> eh = (o, e) => {
				TListener tgt;
				if (self.listenerRef.TryGetTarget(out tgt)) {
					handler(tgt, o, e);
				} else {
					self.Dispose();
				}
			};
			THandler hnd = conv(eh);
			return hnd;
		}

		static void delegateCheck(params Tuple<Delegate, string>[] sig) {
			foreach (var t in sig) {
				if (t.Item1 == null)
					throw new ArgumentNullException(t.Item2);
				if (!t.Item1.Method.IsStatic)
					throw new ArgumentException("指定された式は式外の変数を参照しています。", t.Item2);
			}
		}
		/// <summary>新規インスタンスを初期化する。</summary>
		protected internal WeakEventListener(TSource sender, TListener listener, Func<EventHandler<TEventArgs>, THandler> conv,
			Action<TSource, THandler> add, Action<TSource, THandler> remove, Action<TListener, object, TEventArgs> handler) {
			if (sender == null) throw new ArgumentNullException("sender");
			if (listener == null) throw new ArgumentNullException("listener");
			delegateCheck(
				new Tuple<Delegate, string>(conv, "conv"),
				new Tuple<Delegate, string>(add, "add"),
				new Tuple<Delegate, string>(remove, "remove"),
				new Tuple<Delegate, string>(handler, "handler"));

			listenerRef = new WeakReference<TListener>(listener);
			THandler eh = CreateHandler(this, listener, conv, handler);
			_remove = () => remove(sender, eh);
			add(sender, eh);
		}
		/// <summary>リソースを解放する。</summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed) return;
			if (_remove != null) {
				_remove();
				_remove = null;
			}
			base.Dispose(disposing);
		}
	}

	/// <summary>リスナー自身を弱参照するイベントリスナー。</summary>
	/// <typeparam name="THandler">イベントハンドラー</typeparam>
	/// <typeparam name="TEventArgs">イベント引数</typeparam>
	public class WeakEventListener<THandler, TEventArgs> : WeakEvent<TEventArgs> where TEventArgs : EventArgs {
		EventHandler<TEventArgs> _excuteHandler;
		THandler _aduptHandler;
		Action<THandler> _remove;

		private static THandler CreateHandler(WeakReference<WeakEventListener<THandler, TEventArgs>> listenerWeakRef,
		Func<EventHandler<TEventArgs>, THandler> conv) {

			EventHandler<TEventArgs> eh = (s, e) => {
				WeakEventListener<THandler,TEventArgs> listener;
				if (listenerWeakRef.TryGetTarget(out listener)) {
					var handler = listener._excuteHandler;
					if (handler != null) handler(s, e);
				}
			};
			return conv(eh);
		}
		/// <summary>新規インスタンスを初期化する。</summary>
		protected internal WeakEventListener(Func<EventHandler<TEventArgs>, THandler> conv,
			Action<THandler> add, Action<THandler> remove, EventHandler<TEventArgs> handler) {

			_excuteHandler = handler;
			_remove = remove;
			_aduptHandler = CreateHandler(new WeakReference<WeakEventListener<THandler, TEventArgs>>(this), conv);
			add(_aduptHandler);
		}
		/// <summary>リソースを解放する。</summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed) return;
			if (_remove != null)
				_remove(_aduptHandler);
			if (disposing) {
				_excuteHandler = null;
				_aduptHandler = default(THandler);
				_remove = null;
			}
			base.Dispose(disposing);
		}
	}
}
