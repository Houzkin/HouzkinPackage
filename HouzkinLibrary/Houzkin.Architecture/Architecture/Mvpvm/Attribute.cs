using System;

namespace Houzkin.Architecture.Mvpvm {
	
	/// <summary>
	/// MVPVM パターンにおけるプレゼンターのメンバーに使用される。
	/// <para>ビューモデルへの動的アクセス時に該当するメンバーが存在しなかったとき、モデルのメンバーより優先的に検索される。</para>
	/// <para>プレゼンターのメンバーに一致するプロパティが見つかった場合、モデルの検索は行わない。</para>
	/// <para>またプレゼンターを発生源とするプロパティ値変更通知のうち、この属性を付与されたメンバー名はビューモデルでも発行される。</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class PremodelAttribute : Attribute {
		/// <summary>新規インスタンスを初期化する。</summary>
		public PremodelAttribute() { }
		/// <summary>
		/// プレゼンターの階層構造の変更によって値が変化し得るかどうかを示す。
		/// </summary>
		public bool DependOnStructure { get; set; }
	}
}
