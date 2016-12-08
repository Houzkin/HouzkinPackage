using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Houzkin.Architecture {

	/// <summary>全ての処理がUIスレッドで実行することが保障されたコマンドを表す。</summary>
	public class RelayUICommand : RelayCommand {

		/// <summary>UIスレッドへのディスパッチャ</summary>
		private Dispatcher appDispatcher;

		/// <summary>新しいインスタンスを初期化する。</summary>
		/// <param name="execute">UIスレッドで実行されるコマンド処理</param>
		/// <param name="canExecute"></param>
		public RelayUICommand(Action execute, Func<bool> canExecute)
			: base(execute, canExecute) {
			appDispatcher = Application.Current.Dispatcher;
		}
		/// <summary>UIスレッドでコマンド処理を実行する。</summary>
		public override void Execute() {
			this.UIAction(new Action(() => base.Execute()));
		}
		/// <summary>コマンド処理が実行可能かどうかを示す値を取得する。
		/// <para>この処理はUIスレッドで実行される。</para></summary>
		public override bool CanExecute() {
			return (bool)this.UIAction(new Func<bool>(() => base.CanExecute()));
		}
		/// <summary>コマンドの実行可否状態が変更された事を知らせるイベントを発行する。
		/// <para>この処理はUIスレッドで実行される。</para></summary>
		/// <param name="sender">イベントソース</param>
		/// <param name="e">イベント引数</param>
		protected override void OnCanExecuteChanged(object sender, EventArgs e) {
			this.UIAction(new Action(() => base.OnCanExecuteChanged(sender, e)));
		}
		/// <summary>UIスレッドで指定した処理を行う。</summary>
		protected object UIAction(Delegate dlgt) {
			if (Thread.CurrentThread != appDispatcher.Thread) {
				return appDispatcher.Invoke(dlgt);
			} else {
				return Dispatcher.CurrentDispatcher.Invoke(dlgt);
			}
		}
		/// <summary>コマンドを破棄する。</summary>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				appDispatcher = null;
			}
			base.Dispose();
		}
	}
}
