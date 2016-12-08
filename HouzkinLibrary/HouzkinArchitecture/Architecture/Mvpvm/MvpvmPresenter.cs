using System;
using System.Windows;
using System.Dynamic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace Houzkin.Architecture.Mvpvm {
	public class Presenter<TView, TViewModel> : Presenter
	where TView : FrameworkElement {
		public Presenter(TView view, TViewModel viewModel) { this.View = view; this.ViewModel = viewModel; }
		//public TView View { get; private set; }
		public override FrameworkElement ViewContent {
			get { return this.View; }
		}
		protected TView View { get; private set; }
		TViewModel _viewModel;
		protected TViewModel ViewModel {
			get { return _viewModel; }
			set { changeViewModel(_viewModel, value); }
		}
		void changeViewModel(TViewModel oldVM, TViewModel newVM) {
			if (object.Equals(oldVM, newVM)) return;
			if (newVM == null) throw new ArgumentNullException("newVM","newViewModel is null.");
			_viewModel = newVM;
			if (oldVM != null) { }
			if (this.ViewModel != null) this.View.DataContext = this.ViewModel;
		}
		//protected void IfChanged<TProp>(Expression<Func<TViewModel, TProp>> expression, Action changed) {

		//}
		/// <summary>プレゼンターを破棄する。</summary>
		protected override void Dispose(bool disposing) {
			if (this.IsDisposed) return;
			if (disposing) {
				this.ViewModel = default(TViewModel);
				this.View.DataContext = null;
				this.View = null;
				//this.Model = null;
			}
			base.Dispose(disposing);
		}
	}
	public abstract class Presenter : Houzkin.Tree.SympathizeableNode<Presenter>, INotifyPropertyChanged, INotifyDataErrorInfo {
		internal Presenter() { }
		public abstract FrameworkElement ViewContent { get; }
		#region		INotifyPropertyChanged の実装
		NotifyChangedEventManager _cmp = new NotifyChangedEventManager(Application.Current.Dispatcher);
		/// <summary>変更通知の管理オブジェクトを取得する。</summary>
		internal NotifyChangedEventManager ChangedEventManager {
			get {
				if (_cmp == null) _cmp = new NotifyChangedEventManager(this, Application.Current.Dispatcher);
				return _cmp;
			}
		}
		/// <summary>プロパティ値が変更されたときに発生する。</summary>
		public event PropertyChangedEventHandler PropertyChanged {
			add { ChangedEventManager.PropertyChanged += value; }
			remove { ChangedEventManager.PropertyChanged -= value; }
		}
		/// <summary>プロパティ変更通知を発行する。</summary>
		protected void OnPropertyChanged([CallerMemberName] string name = null) { ChangedEventManager.OnPropertyChanged(name); }
		/// <summary>プロパティ変更通知の発行と、ブロックの解除キーを取得する。</summary>
		/// <param name="name">プロパティ名</param>
		/// <param name="delay">ブロックした直後に通知を発行する場合は false。解除直前に発行する場合は true を指定する。</param>
		protected IDisposable OnPropertyChanged(string name, bool delay) {
			return ChangedEventManager.OnPropertyChanged(name, delay);
		}
		private IDisposable OnPropertyChanged(object sender, string name, bool delay) {
			return ChangedEventManager.OnPropertyChanged(sender, name, delay);
		}
		/// <summary>現在ブロックされているかどうかを示す値を取得する。</summary>
		protected bool IsBlocking(string name) {
			return ChangedEventManager.IsBlocking(name);
		}
		/// <summary>プロパティ変更通知を発行する。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="property">変更したプロパティを表す式</param>
		protected void OnPropertyChanged<TProp>(Expression<Func<TProp>> property) {
			MemberExpression memExp;
			memExp = property.Body as MemberExpression;
			if (memExp == null) throw new ArgumentException("プロパティ名を特定できません");

			string memName = memExp.Member.Name;
			this.OnPropertyChanged(memName);
		}
		/// <summary>値の設定と変更通知を行う。<para>この処理過程において、プロパティ変更通知の重複は無効とする。</para></summary>
		bool setProperty<TProp>(ref TProp storage, TProp value, string propertyName = null) {
			if (object.Equals(storage, value)) {
				return false;
			}
			using (OnPropertyChanged(propertyName, true)) {
				storage = value;
			}
			return true;
		}
		#endregion

		#region		INotifyDataErrorInfo
		DataErrorNotificationManager _em;
		private DataErrorNotificationManager ErrorManager {
			get {
				if (_em == null) {
					_em = new DataErrorNotificationManager(this);
					//_em.ErrorsChanged += (s, e) => OnErrorsChanged(this, e);
					_em.PropertyChanged += (s, e) => OnPropertyChanged(() => this.HasErrors);
				}
				return _em;
			}
		}
		/// <summary>アノテーションとデリゲートによるプロパティ値の検証を行う。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="value">プロパティ値</param>
		/// <param name="errorMessage">エラーメッセージ。デリゲートを使用しない場合は不要。</param>
		/// <param name="validation">エラーを<c>false</c>とする、検証を行うデリゲート。</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		protected void ValidateProperty<TProp>(TProp value, string errorMessage,
			Predicate<TProp> validation, [CallerMemberName] string propertyName = null) {
			ErrorManager.ValidateProperty(value, errorMessage, validation, propertyName);
		}
		/// <summary>アノテーションとデリゲートによるプロパティ値の検証を行う。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="value">プロパティ値</param>
		/// <param name="validation">検証を行うマルチキャストデリゲート。<c>null</c>または空の文字列でない場合をエラーとする。</param>
		/// <param name="propertyName">プロパティ名。省略で呼び出し元のメンバー名。</param>
		protected void ValidateProperty<TProp>(TProp value, Func<TProp, string> validation, [CallerMemberName] string propertyName = null) {
			this.ErrorManager.ValidateProperty(value, validation, propertyName);
		}
		/// <summary>アノテーションによるプロパティ値の検証を行う。</summary>
		/// <param name="value">プロパティ値</param>
		/// <param name="propertyName">プロパティ名</param>
		protected void ValidateProperty<TProp>(TProp value, [CallerMemberName]string propertyName = null) {
			this.ValidateProperty(value, null, propertyName);
		}

		event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged {
			add { this.ErrorManager.ErrorsChanged += value; }
			remove { this.ErrorManager.ErrorsChanged -= value; }
		}

		System.Collections.IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName) {
			return this.ErrorManager.GetErrors(propertyName);
		}

		bool INotifyDataErrorInfo.HasErrors {
			get { return this.HasErrors; }
		}
		/// <summary>検証エラーがあるかどうかを示す値を取得する。</summary>
		[Premodel]
		public bool HasErrors {
			get { return this.ErrorManager.HasErrors; }
		}
		#endregion

		#region INotifyPropertyChanged INotifyDataErrorInfo 共通
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
			var isChanged = this.setProperty(ref storage, value, propertyName);
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
			var isChanged = this.setProperty(ref storage, value, propertyName);
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
		protected bool SetProperty<TProp>(ref TProp storage, TProp value, [CallerMemberName] string propertyName = null) {
			return this.SetProperty(ref storage, value, null, propertyName);

		}
		#endregion
	}

}
