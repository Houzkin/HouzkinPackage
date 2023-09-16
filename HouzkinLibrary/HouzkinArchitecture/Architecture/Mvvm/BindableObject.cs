using Livet.EventListeners.WeakEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Houzkin.Architecture {
	/// <summary>ビューによってバインドされる、参照モデルを内包するオブジェクトを提供する。</summary>
	/// <typeparam name="TModel">モデル</typeparam>
	public class BindableObject<TModel> : BindableObject {
		/// <summary>新規インスタンスを初期化する。</summary>
		/// <param name="model">モデル</param>
		public BindableObject(TModel model) : base(model) { }

		/// <summary>参照しているモデルを取得する。</summary>
		protected TModel Model {
			get { return (TModel)(base.InnerModel); }
		}
	}
	/// <summary>
	/// ビューによってバインドされる、参照モデルを内包するオブジェクトを提供する。
	/// </summary>
	public class BindableObject : INotifyPropertyChanged, IDisposable {

		/// <summary>新しいインスタンスを初期化する。</summary>
		/// <param name="model">参照するソース</param>
		public BindableObject(object model) {
			_model = model;
		}
		/// <summary>MvpvmViewModelを初期化するときに呼び出す。</summary>
		public BindableObject() { }

		object _model;
		/// <summary>参照するモデル。</summary>
		internal virtual object InnerModel {
			get { return _model; }
		}

		#region		INotifyPropertyChanged の実装

		/// <summary>プロパティ値が変更されたときに発生する。</summary>
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>プロパティ変更通知を発行する。</summary>
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		bool setProperty<TProp>(ref TProp storage, TProp value, string propertyName) {
			ThrowExceptionIfDisposed();
			if (object.Equals(storage, value)) {
				return false;
			}
			storage = value;
			return true;
		}
		/// <summary>値の設定と変更通知を行う</summary>
		protected virtual bool SetProperty<TProp>(ref TProp strage, TProp value, [CallerMemberName] string name = "") {
			return setProperty(ref strage, value, name);
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
		#endregion
		
		bool _isDisposed;
		/// <summary>既に破棄されているかどうかを示す値を取得する。</summary>
		protected bool IsDisposed { get { return _isDisposed; } }
		/// <summary>ビューモデルを破棄する。</summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>ビューモデルを破棄する。</summary>
		protected virtual void Dispose(bool disposing) {
			if (IsDisposed) return;
			if (disposing) {
				this.PropertyChanged = null;
			}
			_isDisposed = true;
		}
		/// <summary>既に破棄されているインスタンスの操作を禁止する。</summary>
		protected void ThrowExceptionIfDisposed() {
			if (IsDisposed)
				throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
		}
	}
}
