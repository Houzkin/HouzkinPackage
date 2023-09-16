using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Houzkin.Architecture {
	/// <summary>MVVMパターンにおけるビューモデルとしての機能を提供する。</summary>
	/// <typeparam name="TModel">モデルの型</typeparam>
	public class ViewModelBase<TModel> : ViewModelBase {

		///// <summary>新しいインスタンスを初期化する。</summary>
		//public MarshalViewModel() : base() { }

		/// <summary>新しいインスタンスを初期化する。</summary>
		/// <param name="model">参照するモデル</param>
		public ViewModelBase(TModel model) : base(model) { }

		/// <summary>モデルを取得、設定する。</summary>
		protected TModel Model {
			get {
				return  (TModel)(base.InnerModel);
			}
		}
	}
	/// <summary>MVVMパターンにおけるビューモデルとしての機能を提供する。</summary>
	public class ViewModelBase : BindableObject, INotifyDataErrorInfo {
		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="model">モデル</param>
		public ViewModelBase(object model) : base(model) {
			_context = new ValidationContext(this);
		}

		/// <summary>新規インスタンスを初期化する。</summary>
		public ViewModelBase() : base() {
			_context = new ValidationContext(this);
		}
		#region		INotifyDataErrorInfo の実装

		ValidationContext _context;
		readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

		/// <summary>アノテーションとデリゲートによるプロパティ値の検証を行う。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="value">プロパティ値</param>
		/// <param name="errorMessage">エラーメッセージ。デリゲートを使用しない場合は不要。</param>
		/// <param name="validation">エラーを<c>false</c>とする、検証を行うデリゲート。</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		protected void ValidateProperty<TProp>(TProp value, string errorMessage,
			Predicate<TProp> validation, [CallerMemberName] string propertyName = null) {
			this.ValidateProperty(value, propertyName);
			if (validation != null && !string.IsNullOrEmpty(errorMessage))
			{
				if (!validation(value))
					SetError(errorMessage, propertyName);
			}
		}
		/// <summary>アノテーションとデリゲートによるプロパティ値の検証を行う。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="value">プロパティ値</param>
		/// <param name="validation">検証を行うマルチキャストデリゲート。<c>null</c>または空の文字列でない場合をエラーとする。</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		protected void ValidateProperty<TProp>(TProp value, Func<TProp, string> validation, [CallerMemberName] string propertyName = null) {
			this.ValidateProperty(value, propertyName);
			if (validation != null)
			{
				foreach (Func<TProp, string> vali in validation.GetInvocationList())
				{
					var msg = vali(value);
					if (!string.IsNullOrEmpty(msg)) SetError(msg, propertyName);
				}
			}
		}
		/// <summary>アノテーションによるプロパティ値の検証を行う。</summary>
		/// <param name="value">プロパティ値</param>
		/// <param name="propertyName">プロパティ名</param>
		protected void ValidateProperty<TProp>(TProp value, [CallerMemberName]string propertyName = null) {
			ThrowExceptionIfDisposed();
			_context.MemberName = propertyName;
			this.ClearError(propertyName);
			var ve = new List<ValidationResult>();
			if (!Validator.TryValidateProperty(value, _context, ve))
			{
				var ers = ve.Select(x => x.ErrorMessage);
				foreach (var er in ers)
				{
					SetError(er, propertyName);
				}
			}
		}
		internal virtual void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e) {
			ThrowExceptionIfDisposed();
			this.ErrorsChanged?.Invoke(this, e);
		}
		protected virtual void OnErrorsChanged(string propName)
		{
			this.OnErrorsChanged(this, new DataErrorsChangedEventArgs(propName));
		}
		internal virtual System.Collections.IEnumerable GetErrors(string propertyName) {
			if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName)) return null;
			return _errors[propertyName];
		}
		/// <summary>検証エラーがあるかどうかを示す値を取得する。</summary>
		public virtual bool HasErrors {
			get { return _errors.Any(); }
		}
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
		
		void SetError(string errorMessage,string propName)
		{
			if (!_errors.ContainsKey(propName)) _errors[propName] = new List<string>();
			if (!_errors[propName].Contains(errorMessage))
			{
				_errors[propName].Add(errorMessage);
				OnErrorsChanged(propName);
			}
			this.OnPropertyChanged(nameof(HasErrors));
		}
		void ClearError(string propName)
		{
			if(_errors.ContainsKey(propName)) _errors.Remove(propName);
			OnErrorsChanged(propName);
			OnPropertyChanged(nameof(HasErrors));
		}


		#region interface
		bool INotifyDataErrorInfo.HasErrors {
			get { return this.HasErrors; }
		}
		System.Collections.IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName) {
			return this.GetErrors(propertyName);
		}
		#endregion

		#endregion

		#region override
		/// <summary>プロパティが変化する場合のみ値を更新し、アノテーションまたはデリゲートによるプロパティ値の検証と変更イベントを発行させる。
		/// <para>この処理過程において、プロパティ変更通知の重複は無効とする。</para></summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="storage">値を持つインスタンス</param>
		/// <param name="value">変更後のプロパティ値</param>
		/// <param name="errorMessage">エラーメッセージ。デリゲートを使用しない場合は不要。</param>
		/// <param name="validation">エラーを false とする、検証を行うデリゲート。デリゲートを使用しない場合は省略可。</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		/// <returns>値が変更された場合は<c>true</c>、それ以外は<c>false</c></returns>
		protected bool SetProperty<TProp>(ref TProp storage, TProp value, string errorMessage,
			Predicate<TProp> validation, [CallerMemberName] string propertyName = null) {
			var isChanged = base.SetProperty(ref storage, value, propertyName);
			if (!isChanged) return false;
			this.ValidateProperty(value, errorMessage, validation, propertyName);
			return true;
		}
		/// <summary>プロパティが変化する場合のみ値を更新し、アノテーションまたはデリゲートによるプロパティ値の検証と変更イベントを発行させる。
		/// <para>この処理過程において、プロパティ変更通知の重複は無効とする。</para></summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="storage">値を持つインスタンス</param>
		/// <param name="value">変更後のプロパティ値</param>
		/// <param name="validation">検証を行うマルチキャストデリゲート。<c>null</c>または空の文字列でない場合をエラーとする。</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		/// <returns>値が変更された場合は<c>true</c>、それ以外は<c>false</c></returns>
		protected bool SetProperty<TProp>(ref TProp storage, TProp value, Func<TProp, string> validation, [CallerMemberName] string propertyName = null) {
			var isChanged = base.SetProperty(ref storage, value, propertyName);
			if (!isChanged) return false;
			this.ValidateProperty(value, validation, propertyName);
			return true;
		}
		/// <summary>プロパティが変化する場合のみ値を更新し、アノテーションによるプロパティ値の検証と変更イベントを発行させる。
		/// <para>この処理過程において、プロパティ変更通知の重複は無効とする。</para></summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="storage">値を持つインスタンス</param>
		/// <param name="value">変更後のプロパティ値</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		/// <returns>値が変更された場合は<c>true</c>、それ以外は<c>false</c></returns>
		protected override bool SetProperty<TProp>(ref TProp storage, TProp value, [CallerMemberName] string propertyName = null) {
			return this.SetProperty(ref storage, value, null, propertyName);
		}
		#endregion
	}

}
