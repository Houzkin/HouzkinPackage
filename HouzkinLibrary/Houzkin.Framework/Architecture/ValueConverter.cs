using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Houzkin.Architecture {
	/// <summary>
	/// データをバインディングに適用する関数の生成をサポートする。
	/// </summary>
	public static class ValueConverter {
		/// <summary>型を指定して、データバインディングに適用する関数を生成する。</summary>
		/// <typeparam name="TSource">データソース</typeparam>
		/// <typeparam name="TView">バインディングソース</typeparam>
		/// <param name="convert">変換関数</param>
		/// <param name="convertBack">逆変換関数</param>
		public static IValueConverter Create<TSource, TView>(Converter<TSource, object> convert, Converter<TView, TSource> convertBack) {
			var vc = new _Converter<TSource, TView>(convert, convertBack);
			return vc;
		}
		/// <summary>データバインディングに適用する関数を生成する。</summary>
		/// <param name="convert">変換関数</param>
		/// <param name="convertBack">逆変換関数</param>
		public static IValueConverter Create(Converter<object, object> convert, Converter<object, object> convertBack) {
			return new _Converter<object, object>(convert, convertBack);
		}
		/// <summary>データソースの型を指定して、データバインディングに適用する関数を生成する。</summary>
		/// <typeparam name="TSource">データソース</typeparam>
		/// <param name="convert">変換関数</param>
		/// <param name="convertBack">逆変換関数</param>
		public static IValueConverter Create<TSource>(Converter<TSource, object> convert, Converter<object, TSource> convertBack) {
			return new _Converter<TSource, object>(convert, convertBack);
		}
		private class _Converter<T, U> : IValueConverter {
			public _Converter(Converter<T, object> convert, Converter<U, T> convertBack) {
				this.eveConvert = convert;
				this.eveConvertBack = convertBack;
			}
			Converter<T, object> eveConvert;
			Converter<U, T> eveConvertBack;

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
				if (eveConvert == null) throw new InvalidOperationException("データソースから表示用の型への変換関数が設定されていません。");
				object vl;
				try {
					//if(value is T){
					if (typeof(T).IsAssignableFrom(value.GetType())) {
						vl = value;
					} else if (typeof(T) == typeof(string)) {
						vl = value.ToString();
					} else {
						return eveConvert(default(T));
					}
					return eveConvert((T)vl);
				} catch (Exception e) {
					Debug.Assert(false, "表示用データへの変換過程でエラーを捕捉しました。", e.ToString());
					return null;
				}
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
				if (eveConvertBack == null) throw new InvalidOperationException("表示用データをデータソースで扱う型への変換関数が設定されていません。");
				try {
					object vl;
					if (typeof(U).IsAssignableFrom(value.GetType())) {
						vl = value;
					} else if (typeof(U) == typeof(string)) {
						vl = value.ToString();
					} else { 
						return eveConvertBack(default(U));
					}
					return eveConvertBack((U)vl);
				} catch (Exception e) {
					Debug.Assert(false, "表示用データからの変換過程でエラーを捕捉しました。", e.ToString());
					return null;
				}
			}
		}
	}
}
