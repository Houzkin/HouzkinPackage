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
	public class BindableObject : DynamicObject, INotifyPropertyChanged, IDisposable {

		/// <summary>新しいインスタンスを初期化する。</summary>
		/// <param name="model">参照するソース</param>
		public BindableObject(object model) {
			if (model == null) throw new ArgumentNullException(); //_model = null;
			else setModel(model);
		}
		/// <summary>MvpvmViewModelを初期化するときに呼び出す。</summary>
		internal BindableObject() { }

		object _model;
		/// <summary>参照するモデル。</summary>
		internal virtual object InnerModel {
			get { return _model; }
		}
		IDisposable modelListener = null;
		void setModel(object newModel) {
			object oldModel = _model;
			_model = newModel;
			if (oldModel != null) {
				if (modelListener != null) modelListener.Dispose();
			}
			if (newModel != null) {
				ThrowExceptionIfDisposed();
				var nnpc = newModel as INotifyPropertyChanged;
				if (nnpc != null) {// nnpc.PropertyChanged += ModelPropertyChangedAction;
					modelListener = new LivetWeakEventListener<PropertyChangedEventHandler, PropertyChangedEventArgs>(
						h => new PropertyChangedEventHandler(h),
						h => nnpc.PropertyChanged += h,
						h => nnpc.PropertyChanged -= h,
						this.ModelPropertyChangedAction);
				}
			}
		}
		/// <summary>現在のオブジェクトが参照しているモデルのインスタンスを取得する。</summary>
		/// <typeparam name="TModel">モデルとして扱う型</typeparam>
		/// <returns>指定した型として扱うことができる場合は true。</returns>
		protected ResultWithValue<TModel> MaybeModelAs<TModel>() {
			if (InnerModel != null && InnerModel is TModel) {
				return new ResultWithValue<TModel>(true, (TModel)InnerModel);
			} else {
				return new ResultWithValue<TModel>(false, default(TModel));
			}
		}
		/// <summary>モデルとして参照するオブジェクトが設定または変更されたときに、ラッピングしているメンバーの値を設定し直す。</summary>
		internal void RefreshWrappingMember() {
			ThrowExceptionIfDisposed();
			var wp = this.ModelWrappers;
			foreach (var p in wp) {
				this.ModelPropertyChangedAction(this.InnerModel, new PropertyChangedEventArgs(p.Property.Name));
			}
		}

		IEnumerable<PropAtrbPair> 〆pra = null;
		internal IEnumerable<PropAtrbPair> ModelWrappers {
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

		/// <summary>モデルのプロパティ変更通知を受け取った時、ビューモデル内に存在するモデルをラップするメンバーの挙動を処理する。</summary>
		internal virtual void ModelPropertyChangedAction(object sender, PropertyChangedEventArgs e) {
			ThrowExceptionIfDisposed();
			if (IsBlocking(e.PropertyName)) return;

			var u = this.ModelWrappers
				.FirstOrDefault(x => x.Property.Name == e.PropertyName);

			using (OnPropertyChanged(e.PropertyName, true)) {
				if (u != null) {
					if (!u.Property.CanWrite)
						throw new InvalidOperationException("ビューモデルにプロパティ名 " + e.PropertyName + " の Setter が存在しません。");
					this.ModelPropertyReset(u, (x) => u.Property.SetValue(this, x));
				}
			}
		}
		/// <summary>モデルから取得した値を設定する。</summary>
		void ModelPropertyReset(PropAtrbPair pap, Action<object> setAction) {
			object vl;
			if (this.TryGetMember<object>(out vl, pap.Property.Name)) {
				if (!pap.CheckValue(vl)) vl = pap.GetDefaultValue();
			} else {
				vl = pap.GetDefaultValue();
			}
			setAction(vl);
		}

		#region	DynamicObject の実装
		#region method
		/// <summary>動的なメソッドの呼び出しを指定する。</summary>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
			ThrowExceptionIfDisposed();
			if (InnerModel == null) {
				result = null;
				return false;
			}
			var parTyp = args.Select(x => x.GetType()).ToArray();
			var mif = InnerModel.GetType().GetMethod(binder.Name, parTyp);
			if (mif != null) {
				result = mif.Invoke(InnerModel, args);
				return true;
			}
			result = null;
			return false;
		}
		/// <summary>参照オブジェクトのメソッドの呼び出しを行う。</summary>
		/// <typeparam name="TResult">メソッドの戻り値の型</typeparam>
		/// <param name="args">メソッドの引数</param>
		/// <param name="result">メソッドの戻り値</param>
		/// <param name="name">メソッド名</param>
		/// <returns>正常にメソッドの呼び出しが完了した場合 true。</returns>
		bool TryInvokeMember<TResult>(object[] args, out TResult result, [CallerMemberName] string name = null) {
			try {
				object rst;
				if (TryInvokeMember(new invokeMb(name, args), args, out rst)) {
					try {
						result = rst == null ? default(TResult) : (TResult)rst;
					} catch (InvalidCastException) {
						result = default(TResult);
						Debug.Assert(false, "メソッド名:" + name + " は正常に呼び出されましたが、戻り値の型が一致したいため、型:" + typeof(TResult).Name + " の初期値を返します。");
					}
					return true;
				}
			} catch { }
			result = default(TResult);
			return false;

		}
		///// <summary>戻り値を無視して、参照オブジェクトのメソッドの呼び出しを行う。</summary>
		///// <param name="args">メソッドの引数</param>
		///// <param name="name">メソッド名</param>
		///// <returns>指定したメソッドが存在し、正常に呼び出しが完了した場合は true。</returns>
		//protected bool TryInvokeMember(object[] args, [CallerMemberName] string name = null) {
		//	try {
		//		object rst;
		//		if (TryInvokeMember(new invokeMb(name, args), args, out rst)) {
		//			return true;
		//		}
		//	} catch { }
		//	return false;
		//}
		/// <summary>戻り値の取得を前提に、参照オブジェクトのメソッドの呼び出しを行う。</summary>
		/// <typeparam name="TResult">戻り値の型</typeparam>
		/// <param name="args">メソッドの引数</param>
		/// <param name="name">メソッド名</param>
		/// <returns>メソッドまたはメソッドの戻り値が存在しなかった場合はデフォルト値を付与した結果を返す。</returns>
		protected ResultWithValue<TResult> MaybeInvokeMember<TResult>(object[] args, [CallerMemberName] string name = null) {
			return getInvokeResult<TResult>(args, name);
			//TResult rst;
			//if (this.TryInvokeMember(args, out rst, name)) {
			//	return rst;
			//}
			//return default(TResult);
		}
		///// <summary>戻り値の取得を前提に、参照オブジェクトのメソッドの呼び出しを行う。</summary>
		///// <typeparam name="TResult">戻り値の型</typeparam>
		///// <param name="args">メソッドの引数</param>
		///// <param name="result">結果に対する処理</param>
		///// <param name="name">メソッド名</param>
		///// <returns>メソッドまたはメソッドの戻り値が存在しなかった場合、デフォルト値を返す。</returns>
		//protected ResultWithValue<TResult> MaybeInvoke<TResult>(object[] args, Result<TResult> result = null, [CallerMemberName] string name = null) {
		//	ResultWithValue<TResult> rwv = getInvokeResult<TResult>(args, name);
		//	if(result != null) result(rwv);
		//	return rwv;
		//}
		ResultWithValue<TResult> getInvokeResult<TResult>(object[] args,string name) {
			TResult rstV;
			if (TryInvokeMember<TResult>(args, out rstV, name))
				return new ResultWithValue<TResult>(true, rstV);
			else
				return new ResultWithValue<TResult>(false, rstV);
		}
		///// <summary>参照オブジェクトのメソッドの呼び出しを行う。</summary>
		///// <typeparam name="TResult">戻り値の型</typeparam>
		///// <param name="args">メソッドの引数</param>
		///// <param name="result">結果とその値から現在のメソッドの戻り値を決定する。</param>
		///// <param name="name">メソッド名</param>
		//protected TResult MaybeInvoke<TResult>(object[] args, Func<ResultWithValue<TResult>, TResult> result, [CallerMemberName] string name = null) {
		//	return result(getInvokeResult<TResult>(args, name));
		//}
		#endregion method

		#region index
		/// <summary>インデックスを使用して値を取得する。</summary>
		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
			ThrowExceptionIfDisposed();
			if (InnerModel == null) {
				result = null;
				return false;
			}
			var prm = indexes.Select(x => x.GetType());
			var pi = InnerModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanRead)
				.FirstOrDefault(pp => pp.GetIndexParameters().Select(pr => pr.ParameterType).SequenceEqual(prm));
			if (pi == null) {
				result = null;
				return false;
			} else {
				result = pi.GetValue(InnerModel, indexes);
				return true;
			}
		}
		/// <summary>インデックスを使用して値を設定する。</summary>
		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
			if (InnerModel == null) return false;
			var prm = indexes.Select(x => x.GetType());
			var pi = InnerModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite)
				.FirstOrDefault(pp => pp.GetIndexParameters().Select(pr => pr.ParameterType).SequenceEqual(prm));
			if (pi == null) return false;
			string pNm = pi.Name + "[]";

			using (this.OnPropertyChanged(pNm, true)) {
				pi.SetValue(InnerModel, value, indexes);
			}
			return true;
		}
		/// <summary>インデックスを使用して参照オブジェクトから値を取得する。</summary>
		/// <typeparam name="TProp">値の型</typeparam>
		/// <param name="indexes">インデックス</param>
		/// <param name="result">取得した値を出力</param>
		bool TryGetIndex<TProp>(object[] indexes, out TProp result) {
			try {
				object rst;
				if (TryGetIndex(null, indexes, out rst)) {
					try {
						result = (rst == null) ? default(TProp) : (TProp)rst;
					} catch (InvalidCastException) {
						result = default(TProp);
						Debug.Assert(false, "インデックスによって正常に呼び出されましたが、戻り値の型が一致したいため、型:" + typeof(TProp).Name + " の初期値を返します。");
					}
					return true;
				}
			} catch { }
			result = default(TProp);
			return false;
		}
		///// <summary>インデックスを使用して参照オブジェクトから値を取得する。</summary>
		///// <typeparam name="TProp">値の型</typeparam>
		///// <param name="indexes">インデックス</param>
		///// <returns>正常に取得できなかった場合はデフォルト値を返す。</returns>
		//protected internal TProp MaybeGetIndex<TProp>(params object[] indexes) {
		//	TProp rst;
		//	if (TryGetIndex(indexes, out rst)) return rst;
		//	return default(TProp);
		//}
		/// <summary>インデックスを使用して参照オブジェクトから値を取得する。</summary>
		/// <typeparam name="TProp">値の型</typeparam>
		/// <param name="indexes">インデックス</param>
		/// <returns>正常に取得できなかった場合はデフォルト値を付与した結果を返す。</returns>
		protected ResultWithValue<TProp> MaybeGetIndex<TProp>(object[] indexes) {
			//ResultWithValue<TProp> rwv = getIndexResult<TProp>(indexes);
			//if (result != null) result(rwv);
			//return rwv.Value;
			return getIndexResult<TProp>(indexes);
		}
		ResultWithValue<TProp> getIndexResult<TProp>(object[] indexes) {
			TProp rstV;
			if (TryGetIndex(indexes, out rstV)) 
				return new ResultWithValue<TProp>(rstV);
			else 
				return new ResultWithValue<TProp>(false, rstV);
		}
		///// <summary>インデックスを使用して参照オブジェクトから値を取得する。</summary>
		///// <typeparam name="TProp"></typeparam>
		///// <param name="indexes"></param>
		///// <param name="result"></param>
		///// <returns></returns>
		//protected TProp MaybeGetIndex<TProp>(object[] indexes, Func<ResultWithValue<TProp>, TProp> result) {
		//	return result(getIndexResult<TProp>(indexes));
		//}
		/// <summary>インデックスを使用して参照オブジェクトに値を設定する。</summary>
		/// <typeparam name="TProp">値の型</typeparam>
		/// <param name="indexes">インデックス</param>
		/// <param name="value">設定する値</param>
		/// <returns>正常に設定できた場合は true を示す結果を返す。</returns>
		protected ResultWithValue<TProp> MaybeSetIndex<TProp>(TProp value, params object[] indexes) {
			try {
				var result = TrySetIndex(null, indexes, value);
				return new ResultWithValue<TProp>(result, value);
			} catch { return new ResultWithValue<TProp>(false, value); }
		}
		#endregion

		#region property
		/// <summary>モデルに存在するプロパティ値を取得する。</summary>
		public override bool TryGetMember(GetMemberBinder binder, out object result) {
			ThrowExceptionIfDisposed();
			var propertyName = binder.Name;
			if (InnerModel == null) {
				result = null;
				return false;
			}
			var mdl = InnerModel.GetType().GetProperties()
				.Where(x => x.Name == propertyName)
				.Where(x => x.CanRead);
			if (mdl.Any()) {
				result = mdl.First().GetValue(InnerModel, null);
				return true;
			} else {
				result = null;
				return false;
			}
		}
		/// <summary>モデルに存在するプロパティの設定を実行する。</summary>
		public override bool TrySetMember(SetMemberBinder binder, object value) {
			var propertyName = binder.Name;
			if (InnerModel == null) return false;
			var mdl = InnerModel.GetType().GetProperties()
				.Where(x => x.Name == propertyName)
				.Where(x => x.CanWrite);
			if (mdl.Any()) {
				using (this.OnPropertyChanged(propertyName, true)) {
					mdl.First().SetValue(InnerModel, value, null);
				}
				return true;
			} else {
				return false;
			}
		}
		/// <summary>参照オブジェクトからプロパティ値を取得する。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="result">出力用変数</param>
		/// <param name="name">プロパティ名。省略で呼び出し元のメンバー名。</param>
		/// <returns>値を取得できた場合は<c>true</c></returns>
		bool TryGetMember<TProp>(out TProp result, [CallerMemberName]string name = null) {
			try {
				object rst;
				if (this.TryGetMember(new InstantGetMemberBinder(name), out rst)) {
					try {
						result = (TProp)rst;
					} catch (InvalidCastException) {
						result = default(TProp);
						Debug.Assert(false, "プロパティ名:" + name + " は正常に呼び出されましたが、戻り値の型が一致したいため、型:" + typeof(TProp).Name + " の初期値を返します。");
					}
					return true;
				}
			} catch { }
			result = default(TProp);
			return false;
		}
		///// <summary>参照オブジェクトからプロパティ値を取得する。</summary>
		///// <typeparam name="TProp">プロパティの型</typeparam>
		///// <param name="name">プロパティ名。省略で呼び出し元のメンバー名。</param>
		///// <returns>値を取得できた場合はその値を、できなかった場合はデフォルト値を返す。</returns>
		//protected TProp MaybeGetMember<TProp>([CallerMemberName] string name = null) {
		//	TProp rst;
		//	if (TryGetMember<TProp>(out rst, name)) {
		//		return rst;
		//	}
		//	return default(TProp);
		//}
		/// <summary>参照オブジェクトからプロパティ値を取得する。</summary>
		/// <typeparam name="TProp">プロパティの型</typeparam>
		/// <param name="name">プロパティ名。省略で呼び出し元のメンバー名。</param>
		/// <returns>値を取得できた場合はその値を、できなかった場合はデフォルト値を付与した結果を返す。</returns>
		protected ResultWithValue<TProp> MaybeGetMember<TProp>([CallerMemberName] string name = "") {
			//ResultWithValue<TProp> rwv = getMemberResult<TProp>(name);
			//if(result != null) result(rwv);
			//return rwv.Value;
			return getMemberResult<TProp>(name);
		}
		ResultWithValue<TProp> getMemberResult<TProp>(string name) {
			TProp prpV;
			if (TryGetMember<TProp>(out prpV, name))
				return new ResultWithValue<TProp>(true, prpV);
			else
				return new ResultWithValue<TProp>(false, prpV);
		}
		//protected TProp MaybeGetMember<TProp>(Func<ResultWithValue<TProp>, TProp> result, [CallerMemberName] string name = "") {
		//	return result(getMemberResult<TProp>(name));
		//}
		/// <summary>参照オブジェクトに値を設定する。</summary>
		/// <param name="value">設定する値</param>
		/// <param name="name">プロパティ名。省略で呼び出し元のメンバー名。</param>
		/// <returns>正常に設定できた場合は true を示す結果を返す。</returns>
		protected ResultWithValue<TProp> MaybeSetMember<TProp>(TProp value, [CallerMemberName]string name = null) {
			try {
				var result = this.TrySetMember(new InstantSetMemberBinder(name), value);
				return new ResultWithValue<TProp>(result, value);
			} catch { return new ResultWithValue<TProp>(false,value); }
		}
		#endregion property

		#region binder
		// * * * * * * * * * * method * * * * * * * * * *
		private class invokeMb : InvokeMemberBinder {
			public invokeMb(string name, object[] args) : base(name, false, new CallInfo(args.Length)) { }
			public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
				throw new NotImplementedException();
			}
			public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) {
				throw new NotImplementedException();
			}
		}
		// * * * * * * * * * * property * * * * * * * * * *
		private class InstantGetMemberBinder : GetMemberBinder {
			public InstantGetMemberBinder(string name = null) : base(name, false) { }

			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
				throw new NotImplementedException();
			}
		}
		private class InstantSetMemberBinder : SetMemberBinder {
			public InstantSetMemberBinder(string name = null) : base(name, false) { }

			public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
				throw new NotImplementedException();
			}
		}

		#endregion
		#endregion DynamicObject

		#region		INotifyPropertyChanged の実装
		NotifyChangedEventManager _cpm;
		/// <summary>変更通知の管理オブジェクトを取得する。</summary>
		internal NotifyChangedEventManager ChangedEventManager {
			get {
				if (_cpm == null) _cpm = new NotifyChangedEventManager(this);
				return _cpm;
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
		/// <summary>現在ブロックされているかどうかを示す値を取得する。</summary>
		protected bool IsBlocking(string name) {
			return ChangedEventManager.IsBlocking(name);
		}
		bool setProperty<TProp>(object sender, ref TProp storage, TProp value, string propertyName) {
			ThrowExceptionIfDisposed();
			if (object.Equals(storage, value)) {
				return false;
			}
			using (OnPropertyChanged(propertyName, true)) {
				storage = value;
			}
			return true;
		}
		/// <summary>値の設定と変更通知を行う</summary>
		protected virtual bool SetProperty<TProp>(ref TProp strage, TProp value, [CallerMemberName] string name = "") {
			return setProperty(this, ref strage, value, name);
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
				setModel(null);
			}
			_isDisposed = true;
		}
		/// <summary>既に破棄されているインスタンスの操作を禁止する。</summary>
		protected void ThrowExceptionIfDisposed() {
			if (IsDisposed)
				throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
		}
		/// <summary>現在のオブジェクトを表す文字列を返す。</summary>
		public override string ToString() {
			if (this.InnerModel != null) return this.InnerModel.ToString();
			return base.ToString();
		}
	}
}
