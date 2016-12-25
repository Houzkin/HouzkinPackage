using Houzkin.Tree;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Reflection;
using System.Collections.Specialized;
using Livet.EventListeners.WeakEvents;

namespace Houzkin.Architecture.Mvpvm {

	/// <summary>MVPVMパターンにおけるプレゼンターとして機能を提供する。</summary>
	/// <typeparam name="TView">ビュー</typeparam>
	public class Presenter<TView> : Presenter where TView:FrameworkElement {
		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="view">ビュー</param>
		/// <param name="model">モデル</param>
		public Presenter(TView view, object model = null) : base(view, model) { }
		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="view">ビュー</param>
		/// <param name="viewModel">ビューモデル</param>
		/// <param name="model">モデル</param>
		public Presenter(TView view, MvpvmViewModel viewModel,object model = null) :base(view,viewModel,model) { }
		/// <summary>このプレゼンターが担うビュー。</summary>
		public new TView View { get { return base.View as TView; } }
	}
	
	/// <summary>
	/// MVPVM パターンにおけるプレゼンターとして機能を提供する。
	/// </summary>
	public class Presenter : ObservableTreeNode<Presenter>, IPresenter{

		/// <summary>既定のビューモデルを使用して新しいインスタンスを初期化する。</summary>
		/// <param name="view">ビュー</param>
		/// <param name="model">モデル</param>
		public Presenter(FrameworkElement view, object model = null)
			: this(view, null, model) { }
		/// <summary>新しいインスタンスを初期化する。</summary>
		/// <param name="view">ビュー</param>
		/// <param name="viewModel">ビューモデル。null指定により、MvpViewModelが使用される。</param>
		/// <param name="model">モデル</param>
		public Presenter(FrameworkElement view, MvpvmViewModel viewModel, object model = null) : base() {

			this._view = view;
			this.Model = model;
			this.ViewModel = viewModel ?? new MvpvmViewModel();

			this.StructureChanged += (o, e) =>
				this.OnPremodelPropertyChanged(x => x.GetCustomAttribute<PremodelAttribute>(false).DependOnStructure);
		}
		#region Presenter
		void RefreshWrappingMember() {
			foreach (var p in this.modelWrappers) {
				this.onModelPropertyChanged(this, new PropertyChangedEventArgs(p.Property.Name));
			}
			//this.OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
		IEnumerable<PropertyInfo> IPresenter.PremodelProperties {
			get {
				return this.GetType().GetProperties()
							.Where(x => x.GetCustomAttributes<PremodelAttribute>(false).Any());
			}
		}
		IEnumerable<MethodInfo> IPresenter.PremodelMethods {
			get {
				return this.GetType().GetMethods()
							.Where(x => x.GetCustomAttributes<PremodelAttribute>(false).Any());
			}
		}
		/// <summary>条件を満たすプレモデルプロパティの変更通知を発行する。</summary>
		/// <param name="predicate">通知をするプレモデルプロパティの条件。null 指定でプレモデルプロパティ全ての変更通知を発行。</param>
		protected void OnPremodelPropertyChanged(Predicate<PropertyInfo> predicate = null) {
			predicate = predicate ?? new Predicate<PropertyInfo>(x => true);
			foreach (var p in (this as IPresenter).PremodelProperties) {
				if (predicate(p)) this.OnPropertyChanged(p.Name);
			}
		}
		#endregion

		#region View
		readonly FrameworkElement _view;
		/// <summary>このプレゼンターが担うビュー。</summary>
		public FrameworkElement View { get { return _view; } }

		FrameworkElement IPresenter.View {
			get { return _view; }
		}
		/// <summary>所有するビューを取得する。</summary>
		/// <typeparam name="TView">扱うビューの型</typeparam>
		/// <returns>指定した型として扱うことができる場合は true。</returns>
		public ResultWithValue<TView> MaybeViewAs<TView>() where TView : class {
			var view = _view as TView;
			if (view != null) return new ResultWithValue<TView>(view);
			else return new ResultWithValue<TView>();
		}
		#endregion View

		#region ViewModel
		MvpvmViewModel _viewModel = null;
		/// <summary>ビューにバインディングされるビューモデルを取得、設定する。</summary>
		internal protected MvpvmViewModel ViewModel {
			get { return _viewModel; }
			set {
				if (object.Equals(_viewModel, value)) return;
				this.ChangeViewModel(value, _viewModel);
			}
		}
		MvpvmViewModel IPresenter.ViewModel {
			get { return this.ViewModel; }
			set { this.ViewModel = value; }
		}
		/// <summary>ビューモデルの変更を実行する。</summary>
		protected virtual void ChangeViewModel(MvpvmViewModel newViewModel, MvpvmViewModel oldViewModel) {
			if (this.View != null) this.View.DataContext = DependencyProperty.UnsetValue;
			_viewModel = newViewModel;
			if (oldViewModel != null) {
				if (oldViewModel.Presenter == this) 
					oldViewModel.Presenter = null;
			}
			if (this.ViewModel != null) {
				this.ViewModel.Presenter = this;
				this.ViewModel.RefreshWrappingMember();
				if (this.View != null) this.View.DataContext = this.ViewModel;
			}
		}
		/// <summary>所有するビューモデルを取得する。</summary>
		/// <typeparam name="TViewModel">ビューモデルとして扱う型</typeparam>
		/// <returns>指定した型として扱うことができる場合は true。</returns>
		public ResultWithValue<TViewModel> MaybeViewModelAs<TViewModel>() where TViewModel : class {
			var vm = _viewModel as TViewModel;
			if (vm != null) return new ResultWithValue<TViewModel>(vm);
			else return new ResultWithValue<TViewModel>();
		}
		#endregion ViewModel

		#region Model
		object _model = null;
		object IPresenter.Model {
			get { return this.Model; }
			set { this.Model = value; }
		}
		/// <summary>モデルを設定する。</summary>
		/// <param name="newModel">設定するモデル</param>
		protected void SetModel(object newModel) { this.Model = newModel; }
		/// <summary>MVPVMパターンにおけるモデルを取得、設定する。</summary>
		internal object Model {
			get { return _model; }
			set {
				if (object.Equals(_model, value)) return;
				ChangeModel(value, _model);
				if (this.View != null) this.View.DataContext = DependencyProperty.UnsetValue;
				this.RefreshWrappingMember();
				if (this.ViewModel != null) {
					if (this.View != null) this.View.DataContext = this.ViewModel;
					this.ViewModel.RefreshWrappingMember();
				}
			}
		}
		IDisposable listener;
		/// <summary>モデルの変更を実行する。</summary>
		protected virtual void ChangeModel(object newModel, object oldModel) {
			_model = newModel;
			if (oldModel != null) {
				if (listener != null) listener.Dispose();
			}
			if (this.Model != null) {
				var nm = newModel as INotifyPropertyChanged;
				//listener = WeakEvent<PropertyChangedEventArgs>.CreateListener(
				listener = new LivetWeakEventListener<PropertyChangedEventHandler,PropertyChangedEventArgs>(
					h => new PropertyChangedEventHandler(h),
					h => nm.PropertyChanged += h,
					h => nm.PropertyChanged -= h,
					this.onModelPropertyChanged); 
			}
		}

		IEnumerable<PropAtrbPair> 〆pra = null;
		private IEnumerable<PropAtrbPair> modelWrappers {
			get {
				if (〆pra == null) {
					〆pra = from x in this.GetType().GetProperties()
						   where x.GetCustomAttribute<ReflectReferenceValueAttribute>() != null
						   let pna = new PropAtrbPair(x, x.GetCustomAttribute<ReflectReferenceValueAttribute>())
						   where pna.Attribute != null
						   select pna;
				}
				return 〆pra;
			}
		}
		/// <summary>モデルから変更通知を受け取った場合の処理</summary>
		void onModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (this.IsBlocking(e.PropertyName)) return;

			var u = modelWrappers
				.FirstOrDefault(x => x.Property.Name == e.PropertyName);

			using (OnPropertyChanged(sender,e.PropertyName, true)) {
				if (u != null) {
					if (!u.Property.CanWrite)
						throw new InvalidOperationException("プレゼンターにプロパティ名 " + e.PropertyName + " の Setter が存在しません。");
					
					object vl;
					PropertyInfo pi;
					if (this.Model == null) { pi = null; } 
					else { pi = this.Model.GetType().GetProperty(u.Property.Name); }
					if (pi != null && pi.CanRead) {
						vl = pi.GetValue(this.Model, null);
						if (!u.CheckValue(vl)) vl = u.GetDefaultValue();
					} else {
						vl = u.GetDefaultValue();
					}
					u.Property.SetValue(this, vl);
				}
			}
		}
		/// <summary>所有するモデルを取得する。</summary>
		/// <typeparam name="TModel">モデルとして扱う型</typeparam>
		/// <returns>指定した型として扱うことができる場合は true。</returns>
		protected ResultWithValue<TModel> MaybeModelAs<TModel>() {
			if (this.Model != null && this.Model is TModel)
				return new ResultWithValue<TModel>((TModel)Model);
			else
				return new ResultWithValue<TModel>(false, default(TModel));
		}
		#endregion Model

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
		protected void OnPropertyChanged( [CallerMemberName] string name = null) { ChangedEventManager.OnPropertyChanged(name); }
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
		/// <summary>プレゼンターを破棄する。</summary>
		protected override void Dispose(bool disposing) {
			if (this.IsDisposed) return;
			if (disposing) {
				this.ViewModel = null;
				this.Model = null;
			}
			base.Dispose(disposing);
		}
	}
}
