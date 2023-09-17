using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

//namespace Houzkin.Architecture {
//    /// <summary>
//    /// 変更通知オブジェクトの基底クラス。
//    /// </summary>
//    [Serializable]
//    public class NotificationObject : INotifyPropertyChanged{

//		/// <summary>新規インスタンスを初期化する。</summary>
//		public NotificationObject() { }
//		/// <summary>プロパティ変更通知イベントを発行する。</summary>
//		/// <param name="propertyExpression">() => プロパティ形式のラムダ式</param>
//		/// <exception cref="ArgumentException">() => プロパティ 以外の形式のラムダ式が指定されました。</exception>
//		protected  void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression) {
//			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");

//			if (!(propertyExpression.Body is MemberExpression)) throw new ArgumentException("()=>プロパティ の形式で指定してください。");

//			var memberExpression = (MemberExpression)propertyExpression.Body;
//			OnPropertyChanged(memberExpression.Member.Name);
//		}

//		/// <summary>プロパティ変更通知イベントを発行する。</summary>
//		/// <param name="propertyName">プロパティ名</param>
//		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
//			ncem.OnPropertyChanged(propertyName);
//			//var threadSafeHandler = Interlocked.CompareExchange(ref PropertyChanged, null, null);
//			//if (threadSafeHandler != null) {
//			//	var e = new PropertyChangedEventArgs(propertyName);
//			//	threadSafeHandler(this, e);
//			//}
//		}
//		NotifyChangedEventManager _ncem;
//		NotifyChangedEventManager ncem {
//			get {
//				if (_ncem == null) _ncem = new NotifyChangedEventManager(this,Application.Current.Dispatcher);
//				return _ncem;
//			}
//		}
//		/// <summary>プロパティの値が変更されたときに発生する。</summary>
//		public event PropertyChangedEventHandler PropertyChanged {
//			add { ncem.PropertyChanged += value; }
//			remove { ncem.PropertyChanged -= value; }
//		}

//    }

//}
