using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Livet.Commands;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using Livet.Commands;

namespace Houzkin.Architecture {

	/// <summary>
	/// 引数を取り、バックグラウンドで処理されるコマンドを表す。
	/// <para>このコマンドは多重実行を阻止する。</para>
	/// </summary>
	public class AsyncListenerCommand<T> : Command, ICommand, INotifyPropertyChanged
	{

		BackgroundWorker _worker = new BackgroundWorker();
		Action<object> _completed;
		Func<bool> _canExecute;
		Action<Exception> _error;

		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="action">非同期で実行されるコマンド処理</param>
		/// <param name="canExecute">コマンド処理が実行可能かどうかを示す値を返す関数。<para>この処理はUIスレッドで実行される。</para></param>
		/// <param name="completed">コマンド処理が正常に終了したときに実行される処理。<para>この処理はUIスレッドで実行される。</para></param>
		/// <param name="error">コマンド処理で例外が投げられたときに実行される処理。<para>この処理はUIスレッドで実行される。</para></param>
		public AsyncListenerCommand(Action<T> action, Func<bool> canExecute = null, Action<object> completed = null,
		Action<Exception> error = null)
		{
			_canExecute = canExecute ?? new Func<bool>(() => { return true; });
			_completed = completed;
			_error = error;

			_worker.DoWork += (s, e) =>
			{
				this.UIAction(new Action(() => this.RaiseCanExecuteChanged()));
				if (e.Argument == null)
					action(default(T));
				else
					action((T)e.Argument);
			};
			_worker.RunWorkerCompleted += (s, e) =>
			{

				if (_completed != null && e.Error == null)
					this.UIAction(new Action(() => _completed(e.Result)));

				if (_error != null && e.Error != null)
					this.UIAction(new Action(() => _error(e.Error)));
				this.UIAction(new Action(() => this.RaiseCanExecuteChanged()));
			};
		}
		object UIAction(Delegate dlgt)
		{
			Dispatcher appDispatcher = Livet.DispatcherHelper.UIDispatcher;
			if (Thread.CurrentThread != appDispatcher.Thread)
			{
				return appDispatcher.Invoke(dlgt);
			}
			else
			{
				return Dispatcher.CurrentDispatcher.Invoke(dlgt);
			}
		}
		/// <summary>バックグラウンドで処理を実行する。</summary>
		public void Execute(object param)
		{
			_worker.RunWorkerAsync(param);
		}
		/// <summary>コマンド処理を実行可能かどうかを示す値を取得する。</summary>
		public bool CanExecute
		{
			get { return !(_worker.IsBusy) ? _canExecute() : false; }
			//return (CanExecuteAction == null) ? !(_worker.IsBusy) : !(_worker.IsBusy) && CanExecuteAction();
		}

		/// <summary>実行中の操作をキャンセルする。</summary>
		public void Cancel()
		{
			if (_worker.IsBusy)
				_worker.CancelAsync();
		}
		/// <summary>コマンド処理を実行中かどうかを示す値を取得する。</summary>
		public bool IsExecuting
		{
			get { return _worker.IsBusy; }
		}

		/// <summary>
		/// コマンドが実行可能かどうかが変化した時に発生します。
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanExecute)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExecuting)));
		}

		/// <summary>
		/// コマンドの実行が可能かどうか示す値が変化したかもしれない。
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			OnPropertyChanged();
			OnCanExecuteChanged();
		}
		bool ICommand.CanExecute(object parameter)
		{
			return CanExecute;
		}
		void ICommand.Execute(object parameter)
		{
			this.Execute(parameter);
		}
	}
}
