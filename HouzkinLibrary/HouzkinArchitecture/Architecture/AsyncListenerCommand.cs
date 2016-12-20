using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Livet.Commands;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace Houzkin.Architecture {
	public class AsyncListenerCommand<T>  : Command, ICommand, INotifyPropertyChanged {

		BackgroundWorker _worker = new BackgroundWorker();
		Action<object> _completed;
		Func<bool> _canExecute;
		Action<Exception> _error;

		public AsyncListenerCommand(Action<T> action, Func<bool> canExecute = null, Action<object> completed = null,
		Action<Exception> error = null ){
			_canExecute = canExecute ?? new Func<bool>(() => { return true; });
			_completed = completed;
			_error = error;

			_worker.DoWork += (s, e) => {
				this.UIAction(new Action(() => this.RaiseCanExecuteChanged()));
				if (e.Argument == null)
					action(default(T));
				else
					action((T)e.Argument);
			};
			_worker.RunWorkerCompleted += (s, e) => {

				if (_completed != null && e.Error == null)
					this.UIAction(new Action(()=>_completed(e.Result)));

				if (_error != null && e.Error != null)
					this.UIAction(new Action(() => _error(e.Error)));
				this.UIAction(new Action(() => this.RaiseCanExecuteChanged()));
			};
		}
		object UIAction(Delegate dlgt) {
			Dispatcher appDispatcher = Livet.DispatcherHelper.UIDispatcher;
			if (Thread.CurrentThread != appDispatcher.Thread) {
				return appDispatcher.Invoke(dlgt);
			} else {
				return Dispatcher.CurrentDispatcher.Invoke(dlgt);
			}
		}
		/// <summary>バックグラウンドで処理を実行する。</summary>
		public void Execute(object param) {
			_worker.RunWorkerAsync(param);
		}
		/// <summary>コマンド処理を実行可能かどうかを示す値を取得する。</summary>
		public bool CanExecute {
			get { return !(_worker.IsBusy) ? _canExecute() : false; }
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

		/// <summary>
		/// コマンドが実行可能かどうかが変化した時に発生します。
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged() {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanExecute)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExecuting)));
		}

		public void RaiseCanExecuteChanged() {
			OnPropertyChanged();
			OnCanExecuteChanged();
		}
		bool ICommand.CanExecute(object parameter) {
			return CanExecute;
		}
		void ICommand.Execute(object parameter) {
			this.Execute(parameter);
		}
	}
}
