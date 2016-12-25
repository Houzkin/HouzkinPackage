using System;
using System.Windows.Input;

//namespace Houzkin.Architecture {

//	/// <summary>処理の中継を目的とするコマンドを提供する。</summary>
//	public class RelayCommand : ICommand, IDisposable {

//		/// <summary>実行される関数</summary>
//		private Action _executeAction;

//		/// <summary>実行可能かどうかを示す値を返す関数</summary>
//		private Func<bool> _canExecuteAction;

//		/// <summary>新しいインスタンスを初期化する。</summary>
//		/// <param name="execute">Executeメソッドで実行される処理</param>
//		/// <param name="canExecute">CanExecuteメソッドで実行される処理</param>
//		public RelayCommand(Action execute, Func<bool> canExecute=null) {
//			_executeAction = execute ?? new Action(() => { });
//			_canExecuteAction = canExecute;
//		}
//		/// <summary>コマンド処理を実行する。</summary>
//		public virtual void Execute() {
//			_executeAction();
//		}
//		/// <summary>実行可能かどうか示す値を取得する。</summary>
//		public virtual bool CanExecute() {
//			return _canExecuteAction != null ? _canExecuteAction() : true;
//		}
//		event EventHandler canExecuteChanged;

//		event EventHandler ICommand.CanExecuteChanged {
//			add {
//				this.canExecuteChanged += value;
//				CommandManager.RequerySuggested += value;
//			}
//			remove { 
//				this.canExecuteChanged -= value;
//				CommandManager.RequerySuggested -= value;
//			}
//		}
//		/// <summary>コマンドの実行可否状態が変更された事を知らせるイベントを発行する。</summary>
//		public void OnCanExecuteChanged() { this.OnCanExecuteChanged(this, EventArgs.Empty); }

//		/// <summary>コマンドの実行可否状態が変更された事を知らせるイベントを発行する。</summary>
//		/// <param name="sender">イベントソース</param>
//		/// <param name="e">イベント引数</param>
//		protected virtual void OnCanExecuteChanged(object sender,EventArgs e) {
//			this.canExecuteChanged?.Invoke(sender, e);
//		}
//		void ICommand.Execute(object parameter) {
//			this.Execute();
//		}
//		bool ICommand.CanExecute(object parameter) {
//			return CanExecute();
//		}
//		/// <summary>このコマンドが破棄されているかどうか示す値を取得する。</summary>
//		public bool IsDisposed { get; private set; }

//		/// <summary>コマンドを破棄する。</summary>
//		protected virtual void Dispose(bool disposing) {
//			if (disposing) {
//				_executeAction = () => { };
//				_canExecuteAction = () => false;
//				this.OnCanExecuteChanged();
//				this.IsDisposed = true;
//			}
//		}
//		/// <summary>コマンドを破棄する。</summary>
//		public void Dispose() {
//			if(!IsDisposed)
//				this.Dispose(true);
//		}
//	}
//}
