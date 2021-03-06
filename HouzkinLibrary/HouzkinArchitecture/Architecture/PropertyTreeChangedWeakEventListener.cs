﻿using System;
using System.ComponentModel;
using Livet;
using System.Reflection;
using System.Linq.Expressions;
using Livet.EventListeners.WeakEvents;
using System.Collections.Generic;

namespace Houzkin.Architecture {
	/// <summary>階層化したプロパティに追従するWeakEventListener</summary>
	/// <typeparam name="TSrc">ソース</typeparam>
	public class PropertyTreeChangedWeakEventListener<TSrc> : IDisposable 
		where TSrc : INotifyPropertyChanged {
		TSrc _eventSrc;
		List<IDisposable> _dsp = new List<IDisposable>();
		/// <summary>観測対象を指定して初期化する。</summary>
		/// <param name="obj">観測対象</param>
		public PropertyTreeChangedWeakEventListener(TSrc obj) {
			_eventSrc = obj;
		}
		/// <summary>ハンドラーを追加</summary>
		/// <typeparam name="TProp">型</typeparam>
		/// <param name="propExp">プロパティを示す式</param>
		/// <param name="handler">ハンドラー</param>
		public PropertyTreeChangedWeakEventListener<TSrc> RegisterHandler<TProp>(Expression<Func<TSrc,TProp>> propExp,PropertyChangedEventHandler handler) {
			_dsp.Add(createChildWeakEventListener(this, CreatePropertyTree(propExp.Body), handler));
			return this;
		}

		private IDisposable createChildWeakEventListener<TChild>(PropertyTreeChangedWeakEventListener<TChild> listener, _propertyTree tree, PropertyChangedEventHandler handler)
			where TChild : INotifyPropertyChanged {

		Action createCldLstnr = null;
		Action dispCldLstnr = null;
			if (tree != null) {
				var methodName = MethodBase.GetCurrentMethod().Name;
				createCldLstnr = () => {
					var pi = tree.PropertyInfo;
					var getterMi = pi.GetGetMethod();
					var getterType = typeof(Func<,>).MakeGenericType(pi.ReflectedType, pi.PropertyType);
					var childValue = Delegate
						.CreateDelegate(getterType, getterMi)
						.DynamicInvoke(listener._eventSrc);

					// プロパティに対してリスナーを作成。
					var childType = childValue.GetType();
					object childListener = null;
					if (typeof(INotifyPropertyChanged).IsAssignableFrom(childType)) {
						childListener = typeof(PropertyTreeChangedWeakEventListener<>)
							.MakeGenericType(childType)
							.GetConstructor(new Type[] { childType })
							.Invoke(new object[] { childValue });
						dispCldLstnr = () => {
							(childListener as IDisposable)?.Dispose();
						};
						listener
							.GetType()
							.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
							.MakeGenericMethod(childType)
							.Invoke(listener, new object[] { childListener, tree.Child, handler });
					}
				};
			}
			createCldLstnr?.Invoke();
			return new LivetWeakEventListener<PropertyChangedEventHandler, PropertyChangedEventArgs>(
				h => new PropertyChangedEventHandler(h),
				h => {
					_eventSrc.PropertyChanged += h;
				},
				h => {
					_eventSrc.PropertyChanged -= h;
					dispCldLstnr?.Invoke(); 
				},
				(s, e) => {
					if(e.PropertyName == tree.PropertyInfo.Name) {
						handler(s, e);
						dispCldLstnr?.Invoke();
						createCldLstnr?.Invoke();
					}
				});
		}
		
		private _propertyTree CreatePropertyTree(Expression exp, _propertyTree child = null) {
			var mExp = exp as MemberExpression;
			if (mExp == null) return null;
			
			var pi = mExp.Member as PropertyInfo;
			if (pi == null) throw new ArgumentException("式木からプロパティを取得できません。");
			
			var tree = new _propertyTree(pi, child);
			var parent = CreatePropertyTree(mExp.Expression, tree);
			if (parent != null) return parent;

			return tree;
		}
		/// <summary>インスタンスを破棄する。</summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>インスタンスを破棄する。</summary>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				foreach (var d in _dsp) d.Dispose();
				_dsp.Clear();
			}
		}
		private class _propertyTree {
			public _propertyTree(PropertyInfo pi, _propertyTree child) {
				PropertyInfo = pi;
				Child = child;
			}
			public PropertyInfo PropertyInfo { get; private set; }
			public _propertyTree Child { get; set; }
		}
	}
}
