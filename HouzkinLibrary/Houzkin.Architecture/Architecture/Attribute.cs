using System;
using System.Reflection;

namespace Houzkin.Architecture {
	/// <summary>参照オブジェクトから対象メンバーと同名の変更通知を受け取ったとき、参照オブジェクトの値を対象オブジェクトに設定する。
	/// <para>対象オブジェクトに、変更時のロジック、またはフィールドに値を持つ場合に対応する。</para>
	/// <para>MVVM・MVPVM パターンにおけるプレゼンターまたはビューモデルの、書込み可能なプロパティで有効。</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ReflectReferenceValueAttribute : Attribute {
		/// <summary>新規インスタンスを初期化する。</summary>
		public ReflectReferenceValueAttribute() { }
		/// <summary>デフォルト値を取得・設定する。</summary>
		public object DefaultValue { get; set; }
	}
	internal class PropAtrbPair {
		public PropAtrbPair(PropertyInfo property, ReflectReferenceValueAttribute atrb) {
			Property = property;
			Attribute = atrb;
		}
		public PropertyInfo Property { get; private set; }
		public ReflectReferenceValueAttribute Attribute { get; private set; }
		bool CheckValue(object value, bool throwError) {
			if (value != null && !this.Property.PropertyType.IsAssignableFrom(value.GetType())) {
				if (throwError) throw new InvalidOperationException("対象プロパティに設定できない値が指定されました。");
				return false;
			}
			if (value == null && this.Property.PropertyType.IsValueType) {
				if (throwError) throw new InvalidOperationException("値型を扱うプロパティに null は使用できません。");
				return false;
			}
			return true;
		}
		internal bool CheckValue(object value) {
			return this.CheckValue(value, false);
		}
		internal object GetDefaultValue() {
			var v = this.Attribute.DefaultValue;
			CheckValue(v, true);
			return v;
		}
	}
}
