using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Houzkin.Architecture {

	/// <summary>
	/// バックグラウンドで処理されるコマンドを表す。
	/// <para>このコマンドは多重実行を阻止する。</para>
	/// </summary>
	public class AsyncRelayCommand : RelayUICommand {

		BackgroundWorker _worker = new BackgroundWorker();
		Action<object> _completed;
		Action<Exception> _error;

		/// <summary>新しいインスタンスを初期化する。</summary>
		/// <param name="action">非同期で実行されるコマンド処理</param>
		/// <param name="canExecute">コマンド処理が実行可能かどうかを示す値を返す関数。<para>この処理はUIスレッドで実行される。</para></param>
		/// <param name="completed">コマンド処理が正常に終了したときに実行される処理。<para>この処理はUIスレッドで実行される。</para></param>
		/// <param name="error">コマンド処理で例外が投げられたときに実行される処理。<para>この処理はUIスレッドで実行される。</para></param>
		public AsyncRelayCommand(Action action, Func<bool> canExecute = null, Action<object> completed = null,
		Action<Exception> error = null ):base(action, canExecute) {

			_completed = completed;
			_error = error;

			_worker.DoWork += (s, e) => {
				this.OnCanExecuteChanged();
				action();
			};
			_worker.RunWorkerCompleted += (s, e) => {

				if (_completed != null && e.Error == null)
					this.UIAction(new Action(()=>_completed(e.Result)));

				if (_error != null && e.Error != null)
					this.UIAction(new Action(() => _error(e.Error)));

				this.OnCanExecuteChanged();
			};
		}

		/// <summary>バックグラウンドで処理を実行する。</summary>
		public override void Execute() {
			_worker.RunWorkerAsync();
		}
		/// <summary>コマンド処理を実行可能かどうかを示す値を取得する。</summary>
		public override bool CanExecute() {
			return !(_worker.IsBusy) ? base.CanExecute() : false;
			//return (CanExecuteAction == null) ? !(_worker.IsBusy) : !(_worker.IsBusy) && CanExecuteAction();
		}

		/// <summary>実行中の操作をキャンセルする。</summary>
		public void Cancel() {
			if (_worker.IsBusy)
				_worker.CancelAsync();
		}
		/// <summary>コマンド処理を実行中かどうかを示す値を取得する。</summary>
		public bool IsExecuting {
			get { return _worker.IsBusy; }
		}

		/// <summary>コマンドを破棄する。</summary>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				_completed = null;
				_error = null;
				_worker.Dispose();
			}
			base.Dispose();
		}
	}
	
}
