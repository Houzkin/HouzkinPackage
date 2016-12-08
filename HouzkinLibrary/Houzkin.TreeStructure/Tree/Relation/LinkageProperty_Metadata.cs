using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {
	///// <summary>添付プロパティの動作を定義する。</summary>
	///// <typeparam name="TProp">添付プロパティの型</typeparam>
	//[Serializable]
	//public class AttachedPropertyMetadata<TProp> : IAssociativePropertyMetadata {
	//	AssociativeProperty _ap;
	//	Func<TProp> _defaultValueGenerater;
	//	EventHandler<AssociativePropertyChangedEventArgs<TProp>> _apcea;
	//	Func<TProp, TProp> _cvcb;
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	/// <param name="defaultValue">デフォルト値<para>参照型の場合、デフォルト値は共有される。</para></param>
	//	/// <param name="propertyChanged">プロパティ値が変更されたときに処理されるハンドラーへの参照</param>
	//	/// <param name="coerceValue">プロパティ値を設定したとき、値の強制を行う。</param>
	//	public AttachedPropertyMetadata(TProp defaultValue,
	//		EventHandler<AssociativePropertyChangedEventArgs<TProp>> propertyChanged = null,
	//		Func<TProp,TProp> coerceValue = null) {
	//		_defaultValueGenerater = new Func<TProp>(() => defaultValue);
	//		_apcea = propertyChanged;
	//		_cvcb = coerceValue;
	//	}
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	/// <param name="getDefaultValue">デフォルト値を生成する関数</param>
	//	/// <param name="propertyChanged">プロパティ値が変更されたときに処理されるハンドラーへの参照</param>
	//	/// <param name="coerceValue">プロパティ値を設定したとき、値の強制を行う。</param>
	//	public AttachedPropertyMetadata(Func<TProp> getDefaultValue = null,
	//		EventHandler<AssociativePropertyChangedEventArgs<TProp>> propertyChanged = null,
	//		Func<TProp, TProp> coerceValue = null) {
	//		_defaultValueGenerater = getDefaultValue;
	//		_apcea = propertyChanged;
	//		_cvcb = coerceValue;
	//	}
	//	internal AttachedPropertyMetadata() { }
	//	internal void SetIdentifier(AttachedProperty<TProp> ap) { this._ap = ap; }
	//	object IAssociativePropertyMetadata.DefaultValue {
	//		get {
	//			if (_defaultValueGenerater != null) return _defaultValueGenerater();
	//			return default(TProp);
	//		}
	//	}
	//	object IAssociativePropertyMetadata.CoerceValueCallback(object value) {
	//		if (_cvcb != null) return _cvcb((TProp)value);
	//		return value;
	//	}
	//	void IAssociativePropertyMetadata.OnAssociativePropertyChanged(IAssociativePropertyNode sender, object oldValue, object newValue) {
	//		if (_apcea != null)
	//			_apcea(sender, new AssociativePropertyChangedEventArgs<TProp>(_ap,(TProp)oldValue,(TProp)newValue));
	//	}
	//	bool IAssociativePropertyMetadata.IsChangeByParentChanged(IAssociativePropertyNode currentOwnerNode) {
	//		return false;
	//	}
	//}
	///// <summary>連動プロパティの動作を定義する。</summary>
	///// <typeparam name="TProp">連動プロパティの型</typeparam>
	//[Serializable]
	//public class LinkagePropertyMetadata<TProp> : IAssociativePropertyMetadata{
	//	LinkageProperty<TProp> _lp;
	//	Type _elementType;
	//	Func<TProp> _defaultValueCreate;
	//	EventHandler<AssociativePropertyChangedEventArgs<TProp>> _pccb;
	//	Func<TProp, TProp> _cvcb;
	//	Predicate<TProp> _cpccb;

	//	#region ctor
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	/// <param name="defaultValueCreate">デフォルト値を生成する関数</param>
	//	/// <param name="propertyChanged">プロパティが変更されたときに処理されるハンドラーへの参照。
	//	/// <para>メタデータのオーバーライドに使用する場合、この関数はマルチキャストされる。</para></param>
	//	/// <param name="coerceValue">プロパティ値を設定したとき、値の強制を行う。
	//	/// <para>メタデータのオーバーライドに使用する場合、null 指定でオーバーライドしない。</para></param>
	//	/// <param name="changeByParentChanged">親ノードが変更されたとき、連動プロパティ値を変更するかどうかを示す。
	//	/// <para>null 指定で、LinkagePattern に応じた関数を使用する</para>
	//	/// <para>メタデータのオーバーライドに使用する場合、null 指定でオーバーライドしない。</para></param>
	//	public LinkagePropertyMetadata(Func<TProp> defaultValueCreate,
	//		EventHandler<AssociativePropertyChangedEventArgs<TProp>> propertyChanged = null,
	//		Func<TProp,TProp> coerceValue = null,
	//		Predicate<TProp> changeByParentChanged = null)
	//		: this(propertyChanged, coerceValue, changeByParentChanged) {

	//		DefaultValueCreate = defaultValueCreate;
	//	}
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	/// <param name="defaultValue">デフォルト値<para>参照型の場合、デフォルト値は共有される。</para></param>
	//	/// <param name="propertyChanged">プロパティが変更されたときに処理されるハンドラーへの参照。
	//	/// <para>メタデータのオーバーライドに使用する場合、この関数はマルチキャストされる。</para></param>
	//	/// <param name="coerceValue">プロパティ値を設定したとき、値の強制を行う。
	//	/// <para>メタデータのオーバーライドに使用する場合、null 指定でオーバーライドしない。</para></param>
	//	/// <param name="changeByParentChanged">親ノードが変更されたとき、連動プロパティ値を変更するかどうかを示す。
	//	/// <para>null 指定で、LinkagePattern に応じた関数を使用する</para>
	//	/// <para>メタデータのオーバーライドに使用する場合、null 指定でオーバーライドしない。</para></param>
	//	public LinkagePropertyMetadata(TProp defaultValue,
	//		EventHandler<AssociativePropertyChangedEventArgs<TProp>> propertyChanged = null,
	//		Func<TProp, TProp> coerceValue = null,
	//		Predicate<TProp> changeByParentChanged = null)
	//		: this(propertyChanged,coerceValue,changeByParentChanged) {

	//		DefaultValueCreate = () => defaultValue;
	//	}
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	/// <param name="propertyChanged">プロパティが変更されたときに処理されるハンドラーへの参照。
	//	/// <para>メタデータのオーバーライドに使用する場合、この関数はマルチキャストされる。</para></param>
	//	/// <param name="coerceValue">プロパティ値を設定したとき、値の強制を行う。
	//	/// <para>メタデータのオーバーライドに使用する場合、null 指定でオーバーライドしない。</para></param>
	//	/// <param name="changeByParentChanged">親ノードが変更されたとき、連動プロパティ値を変更するかどうかを示す。
	//	/// <para>メタデータのオーバーライドに使用する場合、null 指定でオーバーライドしない。</para></param>
	//	public LinkagePropertyMetadata(EventHandler<AssociativePropertyChangedEventArgs<TProp>> propertyChanged = null,
	//		Func<TProp,TProp> coerceValue = null,
	//		Predicate<TProp> changeByParentChanged = null)
	//		: this() {

	//		if (propertyChanged != null)
	//			PropertyChangedCallback = propertyChanged;
	//		if (coerceValue != null)
	//			CoerceValueCallback = coerceValue;
	//		if (changeByParentChanged != null)
	//			ChangeByParentChangedCallback = changeByParentChanged;
	//	}
	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	internal LinkagePropertyMetadata() { this.SetCheck = SetChecks.None; }
	//	/// <summary>継承過程を表す Dictionary を使用して、公開用メタデータを初期化する。</summary>
	//	internal LinkagePropertyMetadata(Dictionary<Type,LinkagePropertyMetadata<TProp>> dic) :this() {
	//		var ddc = dic.OrderBy(x => x.Key, 
	//			Comparer<Type>.Create((x, y) => {
	//				if (x.IsSubclassOf(y)) return -1;
	//				if (y.IsSubclassOf(x)) return 1;
	//				Debug.Assert(false, "LinkagePropertyの継承関係が不適切です。");
	//				return 0; 
	//			}));
	//		initialize(dic.Select(x => x.Value));
	//	}
	//	#endregion
	//	private void initialize(IEnumerable<LinkagePropertyMetadata<TProp>> lst) {
	//		this.DefaultValueCreate = 
	//			lst.First(x => x.SetCheck.HasFlag(SetChecks.DefaultSet)).DefaultValueCreate;
	//		this.CoerceValueCallback = 
	//			lst.First(x => x.SetCheck.HasFlag(SetChecks.CoerceCallback)).CoerceValueCallback;
	//		this.ChangeByParentChangedCallback = 
	//			lst.First(x => x.SetCheck.HasFlag(SetChecks.ChangeByParentChangedCallback)).ChangeByParentChangedCallback;
	//		foreach (var ls in lst.Reverse())
	//			if (ls.SetCheck.HasFlag(SetChecks.ChangedHandler) && ls.PropertyChangedCallback != null)
	//				this.PropertyChanged += ls.PropertyChangedCallback;
	//	}
	//	internal SetChecks SetCheck { get; private set; }
	//	/// <summary>対象メタデータを使用可能な状態にする。</summary>
	//	internal void SetProperty(LinkageProperty<TProp> property,Type forType) {
	//		this._lp = property;
	//		this._elementType = forType;
	//	}
	//	/// <summary>連動プロパティのデフォルト値を取得する。</summary>
	//	public Func<TProp> DefaultValueCreate { 
	//		get { return _defaultValueCreate; }
	//		internal set {
	//			if (SetCheck.HasFlag(SetChecks.DefaultSet))
	//				throw new InvalidOperationException();
	//			_defaultValueCreate = value;
	//			SetCheck = SetCheck | SetChecks.DefaultSet;
	//		}
	//	}
	//	/// <summary>継承過程を辿りメタデータを生成する場合に使用する、プロパティ変更イベントを蓄積するハンドラー。</summary>
	//	internal event EventHandler<AssociativePropertyChangedEventArgs<TProp>> PropertyChanged {
	//		add {
	//			if (!SetCheck.HasFlag(SetChecks.ChangedHandler)) { 
	//				PropertyChangedCallback = value;
	//			} else {
	//				_pccb += value;
	//			}
	//		}
	//		remove { throw new NotSupportedException(); }
	//	}
	//	/// <summary>連動プロパティの値が変更されたときに実行される処理。</summary>
	//	public EventHandler<AssociativePropertyChangedEventArgs<TProp>> PropertyChangedCallback { 
	//		get { return _pccb; }
	//		internal set {
	//			if (SetCheck.HasFlag(SetChecks.ChangedHandler))
	//				throw new InvalidOperationException();
	//			_pccb = value;
	//			SetCheck = SetCheck | SetChecks.ChangedHandler;
	//		}
	//	}
	//	/// <summary>プロパティ値を設定時、値の強制を行う。</summary>
	//	public Func<TProp, TProp> CoerceValueCallback { 
	//		get { return _cvcb; }
	//		internal set {
	//			if (SetCheck.HasFlag(SetChecks.CoerceCallback))
	//				throw new InvalidOperationException();
	//			_cvcb = value;
	//			SetCheck = SetCheck | SetChecks.CoerceCallback;
	//		}
	//	}
	//	/// <summary>親ノードが設定されたとき、親ノードのプロパティ値を設定するかどうかを表す。</summary>
	//	public Predicate<TProp> ChangeByParentChangedCallback {
	//		get {
	//			if (_cpccb != null) return _cpccb;
	//			if (this._lp != null && this._elementType != null) {
	//				if (_lp.Pattern == LinkagePattern.Dependency) {
	//					return x => object.Equals(this._lp.GetMetadata(_elementType).DefaultValue, x) ? true : false;
	//				} else {
	//					return x => true;
	//				}
	//			}
	//			throw new InvalidOperationException();
	//		}
	//		internal set {
	//			if (SetCheck.HasFlag(SetChecks.ChangeByParentChangedCallback))
	//				throw new InvalidOperationException();
	//			_cpccb = value;
	//			SetCheck = SetCheck | SetChecks.ChangeByParentChangedCallback;
	//		}
	//	}
	//	/// <summary>現在設定されている動作を書換え不可にする。</summary>
	//	internal void Seal() {
	//		SetCheck = SetChecks.DefaultSet | SetChecks.ChangedHandler | SetChecks.CoerceCallback | SetChecks.ChangeByParentChangedCallback;
	//	}
	//	#region interface
	//	object IAssociativePropertyMetadata.DefaultValue {
	//		get {
	//			if (_defaultValueCreate != null) return _defaultValueCreate();
	//			return default(TProp);
	//		}
	//	}
	//	object IAssociativePropertyMetadata.CoerceValueCallback(object value) {
	//		var vl = (TProp)value;
	//		if (this.CoerceValueCallback != null) return this.CoerceValueCallback(vl);
	//		return vl;
	//	}
	//	bool IAssociativePropertyMetadata.IsChangeByParentChanged(IAssociativePropertyNode ownerNode) {
	//		if (this._lp == null || this._elementType == null) 
	//			throw new InvalidOperationException("このメソッドを操作するために必要な変数が設定されていません。");
	//		if (!ownerNode.IsElementOf(_lp)) return false;
	//		var slfVal = (TProp)(ownerNode.GetValue(_lp));
	//		if (this.ChangeByParentChangedCallback != null) {
	//			return this.ChangeByParentChangedCallback(slfVal);
	//		}
	//		if (_lp.Pattern == LinkagePattern.Dependency) {
	//			return object.Equals(this._lp.GetMetadata(_elementType).DefaultValue, slfVal) ? true : false;
	//		} else {
	//			return true;
	//		}
	//	}
	//	void IAssociativePropertyMetadata.OnAssociativePropertyChanged(IAssociativePropertyNode sender, object oldValue, object newValue) {
	//		var eh = this.PropertyChangedCallback;
	//		if (eh == null) return;
	//		eh(sender, new AssociativePropertyChangedEventArgs<TProp>(this._lp, (TProp)oldValue, (TProp)newValue));
	//	}
	//	#endregion 
	//}
}
