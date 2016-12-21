using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Livet;
using System.Reflection;
using System.Linq.Expressions;

namespace Houzkin.Architecture {
	public class PropertyChainChangedEventListener<TObj> : IDisposable
	where TObj : INotifyPropertyChanged {
		public PropertyChainChangedEventListener(TObj obj) {
			_eventSource = obj;
		}

		private TObj _eventSource;
		private Action _disoposeAction;
		private PropertyChangedEventHandler _handler;

		public void RegisterHandler<TProp>(Expression<Func<TObj, TProp>> propExp, PropertyChangedEventHandler handler) {
			_handler += handler;
			var pt = CreatePropertyTree(propExp.Body);
			RegisterChildHandler(this, pt, handler);
		}

		// 式木とは逆順のプロパティ木構造を作成する。
		private PropertyTree CreatePropertyTree(Expression exp, PropertyTree child = null) {
			var mExp = exp as MemberExpression;
			if (mExp == null) {
				return null;
			}
			var pi = mExp.Member as PropertyInfo;
			if (pi == null) {
				throw new ArgumentException("式木からプロパティを取得できません。");
			}
			var tree = new PropertyTree(pi, child);
			var parent = CreatePropertyTree(mExp.Expression, tree);
			if (parent != null) {
				return parent;
			}
			return tree;
		}

		// 再帰的に PropertyListener を登録していく。
		private void RegisterChildHandler<TChild>(PropertyChainChangedEventListener<TChild> listener, PropertyTree tree, PropertyChangedEventHandler handler)
			where TChild : INotifyPropertyChanged {
			var propName = tree.PropertyInfo.Name;

			Action createChildListener=null;     // リスナー作成用のデリゲート
			Action disposeChildListener=null;    // リスナー解放用のデリゲート
			if (tree != null) {
				var methodName = MethodBase.GetCurrentMethod().Name;
				createChildListener += () =>
				{
					// プロパティを取得するデリゲートを作成＆実行
					var pi = tree.PropertyInfo;
					var getterMi = pi.GetGetMethod();
					var getterType = typeof(Func<,>).MakeGenericType(pi.ReflectedType, pi.PropertyType);
					var childValue = Delegate
						.CreateDelegate(getterType, getterMi)
						.DynamicInvoke(listener._eventSource);

					// プロパティ（子供）に対して PropertyListener を作成。
					var childType = childValue.GetType();
					object childListener = null;
					if (typeof(INotifyPropertyChanged).IsAssignableFrom(childType)) {
						childListener = typeof(PropertyChainChangedEventListener<>)
							.MakeGenericType(childType)
							.GetConstructor(new Type[] { childType })
							.Invoke(new object[] { childValue });

						// 解放用の処理を作成
						disposeChildListener += () => (childListener as IDisposable)?.Dispose();

						// 型パラメータを変えて再帰呼び出し。
						listener
							.GetType()
							.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
							.MakeGenericMethod(childType)
							.Invoke(listener, new object[] { childListener, tree.Child, handler });
					}
				};
			}
			// 上で作成したデリゲートを実行
			createChildListener();

			//PropertyChangedEventHandler pcHandler = (s, e) => {
			//	if (e.PropertyName == propName) {
			//		handler(s, e);              // 引数で渡された処理を実行
			//		disposeChildListener?.Invoke(); // 古いリスナーを解放
			//		createChildListener?.Invoke();  // 新しいプロパティに対してリスナーを作成
			//	}
			//};
			PropertyChangedEventHandler pcHandler 
				= createHandler(new WeakReference<PropertyChainChangedEventListener<TObj>>(this),createChildListener,disposeChildListener);

			// イベントハンドラを登録＆解放用の処理を追加
			listener._eventSource.PropertyChanged += pcHandler;
			_disoposeAction += () => {
				listener._eventSource.PropertyChanged -= pcHandler;
				disposeChildListener();
			};
		}
		private static PropertyChangedEventHandler createHandler(WeakReference<PropertyChainChangedEventListener<TObj>> ls,Action create,Action disp) {
			PropertyChangedEventHandler h = (s, e) => {
				PropertyChainChangedEventListener<TObj> listener;
				if (ls.TryGetTarget(out listener)) {
					listener._handler?.Invoke(s, e);
					disp?.Invoke();
					create?.Invoke();
				}
			};
			return h;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				_disoposeAction?.Invoke();
				_eventSource = default(TObj);
			}
		}

		private class PropertyTree {
			public PropertyTree(PropertyInfo pi, PropertyTree child) {
				PropertyInfo = pi;
				Child = child;
			}
			public PropertyInfo PropertyInfo { get; private set; }
			public PropertyTree Child { get; set; }
		}
	}
	public class PropertyListener<TObj> : IDisposable
	where TObj : INotifyPropertyChanged {
		public PropertyListener(TObj obj) {
			_eventSource = obj;
		}

		private TObj _eventSource;
		private Action _disoposeAction = () => { };

		public void RegisterHandler<TProp>(Expression<Func<TObj, TProp>> propExp, Action handler) {
			var pt = CreatePropertyTree(propExp.Body);
			RegisterChildHandler<TObj>(this, pt, handler);
		}

		// 式木とは逆順のプロパティ木構造を作成する。
		private PropertyTree CreatePropertyTree(Expression exp, PropertyTree child = null) {
			var mExp = exp as MemberExpression;
			if (mExp == null) {
				return null;
			}
			var pi = mExp.Member as PropertyInfo;
			if (pi == null) {
				throw new ArgumentException("式木からプロパティを取得できません。");
			}
			var tree = new PropertyTree(pi, child);
			var parent = CreatePropertyTree(mExp.Expression, tree);
			if (parent != null) {
				return parent;
			}
			return tree;
		}

		// 再帰的に PropertyListener を登録していく。
		private void RegisterChildHandler<TChild>(PropertyListener<TChild> listener, PropertyTree tree, Action handler)
			where TChild : INotifyPropertyChanged {
			var propName = tree.PropertyInfo.Name;

			Action createChildListener = () => { };     // リスナー作成用のデリゲート
			Action disposeChildListener = () => { };    // リスナー解放用のデリゲート
			if (tree != null) {
				var methodName = MethodBase.GetCurrentMethod().Name;
				createChildListener = () =>
				{
					// プロパティを取得するデリゲートを作成＆実行
					var pi = tree.PropertyInfo;
					var getterMi = pi.GetGetMethod();
					var getterType = typeof(Func<,>).MakeGenericType(pi.ReflectedType, pi.PropertyType);
					var cv = Delegate
						.CreateDelegate(getterType, getterMi);
					var childValue = cv
						.DynamicInvoke(listener._eventSource);

					// プロパティ（子供）に対して PropertyListener を作成。
					var childType = childValue.GetType();
					object childListener = null;
					if (typeof(INotifyPropertyChanged).IsAssignableFrom(childType)) {
						childListener = typeof(PropertyListener<>)
							.MakeGenericType(childType)
							.GetConstructor(new Type[] { childType })
							.Invoke(new object[] { childValue });

						// 解放用の処理を作成
						disposeChildListener = () =>
						{
							var d = childListener as IDisposable;
							if (d != null) {
								d.Dispose();
							};
						};

						// 型パラメータを変えて再帰呼び出し。
						listener
							.GetType()
							.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
							.MakeGenericMethod(childType)
							.Invoke(listener, new object[] { childListener, tree.Child, handler });
					}
				};
			}
			// 上で作成したデリゲートを実行
			createChildListener();

			PropertyChangedEventHandler pcHandler = (_, e) =>
			{
				if (e.PropertyName == propName) {
					handler();              // 引数で渡された処理を実行
					disposeChildListener(); // 古いリスナーを解放
					createChildListener();  // 新しいプロパティに対してリスナーを作成
				}
			};

			// イベントハンドラを登録＆解放用の処理を追加
			listener._eventSource.PropertyChanged += pcHandler;
			_disoposeAction += () =>
			{
				listener._eventSource.PropertyChanged -= pcHandler;
				disposeChildListener();
			};
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				_disoposeAction();
				_eventSource = default(TObj);
			}
		}

		private class PropertyTree {
			public PropertyTree(PropertyInfo pi, PropertyTree child) {
				PropertyInfo = pi;
				Child = child;
			}
			public PropertyInfo PropertyInfo { get; private set; }
			public PropertyTree Child { get; set; }
		}
	}
}
