using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {

	///// <summary>連動プロパティを提供する。</summary>
	//internal interface IAssociativePropertyNode : INodeFaculty<ILinkageProperty> {
	//	/// <summary>連動プロパティを設定する。</summary>
	//	void SetValue(AssociativeProperty key, object value);
	//	/// <summary>連動プロパティを取得する。</summary>
	//	object GetValue(AssociativeProperty key);
	//}
	///// <summary>連想プロパティのメタデータを表す。</summary>
	//internal interface IAssociativePropertyMetadata {
	//	/// <summary>デフォルト値を取得する。</summary>
	//	object DefaultValue { get; }
	//	/// <summary>プロパティ値変更時、値を強制する。</summary>
	//	object CoerceValueCallback(object value);
	//	/// <summary>連動プロパティ変更イベントを発行する。</summary>
	//	void OnAssociativePropertyChanged(IAssociativePropertyNode sender, object oldValue, object newValue);
	//	/// <summary>指定したノードの親ノードが変更されたとき、値を変更するかどうかを示す。</summary>
	//	/// <param name="currentOwnerNode">親ノードが変更されたノード</param>
	//	bool IsChangeByParentChanged(IAssociativePropertyNode currentOwnerNode);
	//}
	///// <summary>プロパティの識別子を定義する。</summary>
	//internal interface IPropertyIdentifier {
	//	/// <summary>プロパティ名</summary>
	//	string Name { get; }
	//	/// <summary>連動パターン</summary>
	//	LinkagePattern Pattern { get; }
	//	/// <summary>プロパティ値の検証を行う。</summary>
	//	/// <param name="value">プロパティ値</param>
	//	bool ValidateValueCallback(object value);
	//	/// <summary>指定された型に対するメタデータを取得する。</summary>
	//	IAssociativePropertyMetadata GetMetadata(Type forType);
	//}
	///// <summary>連動プロパティを定義する。</summary>
	//internal interface ILinkageProperty : IPropertyIdentifier {
	//	/// <summary>指定された型が有効な要素型として登録されているかどうかを示す値を返す。</summary>
	//	bool ContainsAsKey(Type forType);
	//}
	///// <summary>添付プロパティを定義する。</summary>
	//internal interface IAttachedProperty : IPropertyIdentifier {
	//	/// <summary>要素型を取得する。</summary>
	//	Type ElementType { get; }
	//}

	///// <summary>連動プロパティ変更イベントにデータを提供する。</summary>
	///// <typeparam name="TProp">連動プロパティの型</typeparam>
	//[Serializable]
	//public class AssociativePropertyChangedEventArgs<TProp> : EventArgs {
	//	internal AssociativePropertyChangedEventArgs(AssociativeProperty property, TProp oldValue, TProp newValue) {
	//		_property = property;
	//		_oldValue = oldValue;
	//		_newValue = newValue;
	//	}
	//	AssociativeProperty _property;
	//	TProp _oldValue;
	//	TProp _newValue;
	//	/// <summary>連動プロパティを取得する。</summary>
	//	public AssociativeProperty Property { get { return _property; } }
	//	/// <summary>変更前の値</summary>
	//	public TProp OldValue { get { return _oldValue; } }
	//	/// <summary>変更後の値</summary>
	//	public TProp NewValue { get { return _newValue; } }
	//}
	///// <summary>連動パターンを指定する。</summary>
	//public enum LinkagePattern {
	//	/// <summary>連動しない。</summary>
	//	None,
	//	/// <summary>全てのノードに連動する。</summary>
	//	Entirely,
	//	/// <summary>対象ノードを含む、子孫ノードに連動する。</summary>
	//	Dependency,
	//}

	//[Flags]
	//internal enum SetChecks {
	//	None = 0,
	//	DefaultSet = 1,
	//	ChangedHandler = 2,
	//	CoerceCallback = 4,
	//	ChangeByParentChangedCallback = 8,
	//}

}
