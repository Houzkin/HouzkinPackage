using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace Houzkin.Architecture.Mvpvm {
	/// <summary>MVPVM パターンにおいて、プレゼンターとしての基本機能を定義する。</summary>
	internal interface IPresenter : INotifyPropertyChanged, INotifyDataErrorInfo, IDisposable{//, INotifyCollectionChanged

		/// <summary>ビューを取得する。</summary>
		FrameworkElement View { get; }

		/// <summary>ビューモデルを取得、設定する。</summary>
		MvpvmViewModel ViewModel { get; set; }

		/// <summary>参照するモデルを取得、設定する。</summary>
		object Model { get; set; }

		/// <summary>Premodelが付与されているプロパティ属性を取得する。</summary>
		IEnumerable<PropertyInfo> PremodelProperties { get; }

		/// <summary>Premodelが付与されているメソッド属性を取得する。</summary>
		IEnumerable<MethodInfo> PremodelMethods { get; }

	}
}
