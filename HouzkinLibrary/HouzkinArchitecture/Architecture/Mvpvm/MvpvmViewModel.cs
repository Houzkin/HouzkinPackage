using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System;
using System.ComponentModel;
using System.Collections;
using Livet.EventListeners.WeakEvents;
using Houzkin.Architecture.Dynamic;

namespace Houzkin.Architecture.Mvpvm {
	
	/// <summary>MVPVM パターンにおけるビューモデルを表す。</summary>
	public class MvpvmViewModel : DynamicViewModel {

		/// <summary>新しいインスタンスを初期化する。</summary>
		public MvpvmViewModel(): base() { }

		/// <summary>このビューモデルが参照するモデル。</summary>
		internal override object InnerModel {
			get {
				if (this.Presenter == null) { return null; } 
				else { return this.Presenter.Model; }
			}
		}
		#region		Presenter 関連の実装
		IPresenter _presenter = null;
		/// <summary>プレゼンターを取得、設定する。</summary>
		internal IPresenter Presenter {
			get { return _presenter; }
			set {
				if (_presenter != value) this.ChangePresenter(value , _presenter);
			}
		}
		IDisposable propListener = null;
		IDisposable errListener = null;
		/// <summary>プレゼンターの変更を実行する。</summary>
		internal virtual void ChangePresenter(IPresenter newPresenter, IPresenter oldPresenter) {
			this._presenter = newPresenter;
			//プレゼンターの参照プロパティをリセット
			//IncludedProperties = null;
			//includedMethods = null;
			if (oldPresenter != null) {
				if (oldPresenter.ViewModel == this) oldPresenter.ViewModel = null;
				//oldPresenter.PropertyChanged -= ModelPropertyChangedAction;
				//oldPresenter.ErrorsChanged -= OnErrorsChanged;
				if (propListener != null) propListener.Dispose();
				if (errListener != null) errListener.Dispose();
			}
			if (this.Presenter != null) {
				ThrowExceptionIfDisposed();
				this.Presenter.ViewModel = this;
				//newPresenter.PropertyChanged += ModelPropertyChangedAction;
				//newPresenter.ErrorsChanged += OnErrorsChanged;
				propListener = new LivetWeakEventListener<PropertyChangedEventHandler, PropertyChangedEventArgs>(//= WeakEvent<PropertyChangedEventArgs>.CreateListener(
					h => new PropertyChangedEventHandler(h),
					h => newPresenter.PropertyChanged += h,
					h => newPresenter.PropertyChanged -= h,
					ModelPropertyChangedAction);
				errListener = new LivetWeakEventListener<EventHandler<DataErrorsChangedEventArgs>,DataErrorsChangedEventArgs>(//WeakEvent<DataErrorsChangedEventArgs>.CreateListener(
					h => h,
					h => newPresenter.ErrorsChanged += h,
					h => newPresenter.ErrorsChanged -= h,
					OnErrorsChanged);
			}
		}
		/// <summary>プレゼンターから発生した、PreModel属性のないメンバーの通知をはじく</summary>
		internal override sealed void ModelPropertyChangedAction(object sender, PropertyChangedEventArgs e) {
			ThrowExceptionIfDisposed();
			if (object.ReferenceEquals(sender, this.Presenter)) {
				if (!includedProperties.Any(x => x.Name == e.PropertyName)) return;
			}
			base.ModelPropertyChangedAction(sender, e);
		}
		//IEnumerable<PropertyInfo> 〆includedProperties = null;
		private IEnumerable<PropertyInfo> includedProperties {
			get {
				if (this.Presenter == null) return new PropertyInfo[0];
				else return this.Presenter.PremodelProperties;
			}
		}
		private IEnumerable<MethodInfo> includedMethods {
			get {
				if (this.Presenter == null) return new MethodInfo[0];
				else return this.Presenter.PremodelMethods;
			}
		}
		#endregion

		#region ErrorInfo
		internal override sealed void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e) {
			ThrowExceptionIfDisposed();
			if (object.Equals(sender, this.Presenter)) {
				if (!includedProperties.Any(x => x.Name == e.PropertyName))
					return;
			} 
			base.OnErrorsChanged(this, e);
		}
		internal override IEnumerable GetErrors(string propertyName) {
			var ers = (base.GetErrors(propertyName) ?? new string[0]).OfType<object>();
			if (this.Presenter == null) return ers;
			var pers = (this.Presenter.GetErrors(propertyName) ?? new string[0]).OfType<object>();
			return ers.Union(pers);
		}
		/// <summary>検証エラーがあるかどうかを示す値を取得する。</summary>
		public override bool HasErrors {
			get {
				if (this.Presenter == null) return base.HasErrors;
				return Presenter.HasErrors || base.HasErrors;
			}
		}
		#endregion

		#region DynamicObject の実装
		/// <summary>動的なメソッドの呼び出しを指定する。</summary>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
			ThrowExceptionIfDisposed();
			if (this.Presenter == null) { result = null; return false; }
			var mtd = includedMethods
				.Where(x => x.Name == binder.Name)
				.Where(x => x.GetParameters()
					.Select(y => y.ParameterType)
					.SequenceEqual(args.Select(z => z.GetType())));
			if (mtd.Any()) {
				result = mtd.First().Invoke(this.Presenter, args);
				return true;
			}
			return base.TryInvokeMember(binder, args, out result);
		}
		/// <summary>インデックスを使用して値を取得する。</summary>
		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
			if (this.Presenter == null) { result = null; return false; }

			var prm = indexes.Select(x =>x.GetType());
			var prp = includedProperties
				.Where(x =>x.CanRead)
				.FirstOrDefault(pp => pp.GetIndexParameters().Select(pr => pr.ParameterType).SequenceEqual(prm));
			if (prp != null) {
				result = prp.GetValue(this.Presenter, indexes);
				return true;
			}
			return base.TryGetIndex(binder, indexes, out result);
		}
		/// <summary>インデックスを使用して値を設定する。</summary>
		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
			ThrowExceptionIfDisposed();
			if (this.Presenter == null) { return false; }

			var prm = indexes.Select(x => x.GetType());
			var prp = includedProperties
				.Where(x => x.CanWrite)
				.FirstOrDefault(pp => pp.GetIndexParameters().Select(pr => pr.ParameterType).SequenceEqual(prm));
			if (prp != null) {
				string pNm = prp.Name + "[]";
				using (this.OnPropertyChanged(pNm, true)) {
					prp.SetValue(this.Presenter, value, indexes);
				}
				return true;
			}
			return base.TrySetIndex(binder, indexes, value);
		}
		/// <summary>プロパティ値を設定する。</summary>
		public override bool TryGetMember(GetMemberBinder binder, out object result) {
			var propertyName = binder.Name;
			//プレゼンターが設定されてない場合は機能しない
			if (this.Presenter == null) { result = null; return false; }
			//プレゼンターを検索
			var prp = includedProperties
				.Where(x => x.Name == propertyName)
				.Where(x => x.CanRead);
			if (prp.Any()) {
				result = prp.First().GetValue(this.Presenter, null);
				return true;
			}
			//モデルから該当プロパティを検索
			return base.TryGetMember(binder, out result);
		}
		/// <summary>プロパティ値を設定する。</summary>
		public override bool TrySetMember(SetMemberBinder binder, object value) {
			ThrowExceptionIfDisposed();
			var propertyName = binder.Name;
			//プレゼンターが設定されてない場合は機能しない
			if (this.Presenter == null) { return false; }
			//プレゼンターを検索
			var prp = includedProperties
				.Where(x => x.Name == propertyName)
				.Where(x => x.CanWrite);
			if (prp.Any()) {
				using (this.OnPropertyChanged(propertyName, true)) {
					prp.First().SetValue(Presenter, value, null);
				}
				return true;
			}
			//モデルから該当プロパティを検索
			return base.TrySetMember(binder, value);
		}
		#endregion DynamicObject
		/// <summary>ビューモデルを破棄する。</summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed) return;
			if (disposing) {
				this.Presenter = null;
			}
			base.Dispose(disposing);
		}
	}
}
