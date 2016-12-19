using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace System.ComponentModel {

	/// <summary>
	/// データの検証を担うオブジェクトを提供する。
	/// </summary>
	public sealed class DataErrorNotificationManager : INotifyPropertyChanged, INotifyDataErrorInfo {

		ValidationContext _context;
		readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="validationInstance">検証するオブジェクトのインスタンス</param>
		public DataErrorNotificationManager(object validationInstance) {
			_context = new ValidationContext(validationInstance);
		}
		/// <summary>検証するオブジェクトに対し、アノテーションによるプロパティ値の検証を行う。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="value">プロパティ値</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		public void ValidateProperty<TProp>(TProp value, [CallerMemberName]string propertyName = "") {
			_context.MemberName = propertyName;
			ClearError(propertyName);
			var ve = new List<ValidationResult>();
			if (!Validator.TryValidateProperty(value, _context, ve)) {
				var ers = ve.Select(x => x.ErrorMessage);
				foreach (var er in ers) {
					SetError(er, propertyName);
				}
			}
		}
		/// <summary>検証するオブジェクトに対し、アノテーションとデリゲートによるプロパティ値の検証を行う。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="value">プロパティ値</param>
		/// <param name="errorMessage">エラーメッセージ。デリゲートを使用しない場合は不要。</param>
		/// <param name="validation">エラーを<c>false</c>とする、検証を行うデリゲート。</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		public void ValidateProperty<TProp>(TProp value,
			string errorMessage, Predicate<TProp> validation, [CallerMemberName]string propertyName = "") {
			this.ValidateProperty(value, propertyName);
			if (validation != null && !string.IsNullOrEmpty(errorMessage)) {
				if (!validation(value))
					SetError(errorMessage, propertyName);
			}
		}
		/// <summary>検証するオブジェクトに対し、アノテーションとデリゲートによるプロパティ値の検証を行う。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="value">プロパティ値</param>
		/// <param name="validation">検証を行うマルチキャストデリゲート。<c>null</c>または空の文字列でない場合をエラーとする。</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		public void ValidateProperty<TProp>(TProp value,
			Func<TProp, string> validation, [CallerMemberName]string propertyName = "") {
			this.ValidateProperty(value, propertyName);
			if (validation != null) {
				foreach (Func<TProp, string> vali in validation.GetInvocationList()) {
					var msg = vali(value);
					if (!string.IsNullOrEmpty(msg)) SetError(msg, propertyName);
				}
			}
		}
		NotifyChangedEventManager _ncem;
		NotifyChangedEventManager ncem {
			get {
				if (_ncem == null) _ncem = new NotifyChangedEventManager(this,Application.Current.Dispatcher);
				return _ncem;
			}
		}
		/// <summary>プロパティの値が変更されたときに発生する。</summary>
		public event PropertyChangedEventHandler PropertyChanged {
			add { ncem.PropertyChanged += value; }
			remove { ncem.PropertyChanged -= value; }
		}
		/// <summary>検証エラーが変更されたときに発生する。</summary>
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		/// <summary>指定したプロパティの検証エラーを取得する。</summary>
		/// <param name="propertyName">プロパティ名</param>
		/// <returns>検証エラー</returns>
		public System.Collections.IEnumerable GetErrors(string propertyName) {
			if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
				return null;
			return _errors[propertyName];
		}
		/// <summary>検証エラーがあるかどうかを示す値を取得する。</summary>
		public bool HasErrors {
			get { return _errors.Any(); }
		}
		void SetError(string errorMessage, string propertyName) {
			if (!_errors.ContainsKey(propertyName))
				_errors[propertyName] = new List<string>();

			if (!_errors[propertyName].Contains(errorMessage)) {
				_errors[propertyName].Add(errorMessage);
				OnErrorsChanged(propertyName);
			}
			ncem.OnPropertyChanged("HasError");
			//OnPropertyChanged(() => this.HasErrors);
		}
		void ClearError(string propertyName) {
			if (_errors.ContainsKey(propertyName))
				_errors.Remove(propertyName);

			OnErrorsChanged(propertyName);
			ncem.OnPropertyChanged("HasError");
			//OnPropertyChanged(() => this.HasErrors);
		}
		/// <summary>エラーの変更通知を発行する。</summary>
		/// <param name="propertyName"></param>
		public void OnErrorsChanged(string propertyName) {
			Action action = new Action(() => {
				ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
			});
			if (Dispatcher.CurrentDispatcher != Application.Current.Dispatcher) {
				Application.Current.Dispatcher.Invoke(action);
			} else {
				action();
			}
		}

	}

}
