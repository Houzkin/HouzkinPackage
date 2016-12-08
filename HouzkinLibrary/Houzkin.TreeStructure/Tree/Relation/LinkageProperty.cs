using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Houzkin.Tree {

	///// <summary>プロパティ名と、そのプロパティを管理または所有するオブジェクトの型をキーとして関連付けられる識別子を表す。</summary>
	//[Serializable]
	//public class AssociativeProperty : IPropertyIdentifier {
	//	#region static
	//	/// <summary>プロパティ名と要素型をキーとして、連想プロパティデータを保持する。</summary>
	//	internal static Dictionary<Tuple<string, Type>, AssociativeProperty> PropertyDic = new Dictionary<Tuple<string, Type>, AssociativeProperty>();
		
	//	/// <summary>添付プロパティを登録する。</summary>
	//	/// <typeparam name="TProp">プロパティの型</typeparam>
	//	/// <param name="propertyName">プロパティ名</param>
	//	/// <param name="elementType">要素型</param>
	//	/// <param name="metaData">メタデータ</param>
	//	/// <param name="validateValueCallback">プロパティ値の検証コールバック</param>
	//	/// <returns>添付プロパティ識別子
	//	/// <para>後でプログラムで設定の変更などをする際、この識別子を使用して添付プロパティを参照可能。</para></returns>
	//	public static AttachedProperty<TProp> RegisterAttached<TProp>(string propertyName, Type elementType,
	//		AttachedPropertyMetadata<TProp> metaData = null,
	//		Predicate<TProp> validateValueCallback = null) {
	//		return new AttachedProperty<TProp>(propertyName, elementType, metaData, validateValueCallback);
	//	}
	//	/// <summary>連動プロパティを登録する。
	//	/// <para>キーとなるクラス内の public static readonly フィールドで使用し、取得した値を公開する必要がある。</para></summary>
	//	/// <typeparam name="TProp">プロパティの型</typeparam>
	//	/// <param name="propertyName">プロパティ名</param>
	//	/// <param name="elementType">要素型</param>
	//	/// <param name="pattern">連動パターン</param>
	//	/// <param name="metaData">メタデータ</param>
	//	/// <param name="validateValueCallback">プロパティ値の検証コールバック</param>
	//	/// <returns>連動プロパティ識別子
	//	/// <para>後でプログラムで値を設定したり、メタデータを取得したりする際に、この識別子を使用して連動プロパティを参照可能。</para></returns>
	//	public static LinkageProperty<TProp> RegisterLinkage<TProp>(string propertyName, Type elementType,
	//		LinkagePattern pattern,
	//		LinkagePropertyMetadata<TProp> metaData = null,
	//		Predicate<TProp> validateValueCallback = null) {

	//		return new LinkageProperty<TProp>(propertyName, elementType, pattern, metaData, validateValueCallback);
	//	}
	//	/// <summary>指定された型を管理者または所有者として有効な、登録されている連想プロパティを返す。</summary>
	//	/// <param name="forType">キーの型またはその派生型</param>
	//	public static IEnumerable<AssociativeProperty> FromElementType(Type forType) {
	//		return PropertyDic
	//			.Select(x => x.Value)
	//			.Distinct()
	//			.Where(x => {
	//				var xl = x as ILinkageProperty;
	//				if (xl != null && xl.ContainsAsKey(forType))
	//					return true;
	//				var xa = x as IAttachedProperty;
	//				if (xa != null && xa.ElementType == forType)
	//					return true;
	//				return false;
	//			});
	//	}
	//	#endregion static
	//	#region instance
	//	internal AssociativeProperty(string name,LinkagePattern pattern) {
	//		if (string.IsNullOrEmpty(name)) throw new ArgumentException("連動プロパティ名を指定してください。");
	//		this.Name = name;
	//	}
	//	/// <summary>連想プロパティ名を取得する。</summary>
	//	public string Name { get; private set; }

	//	/// <summary>連動パターンを取得する。</summary>
	//	public LinkagePattern Pattern { get; private set; }

	//	/// <summary>プロパティ値の検証を行う。</summary>
	//	/// <param name="value">プロパティ値</param>
	//	public virtual bool ValidateValueCallback(object value) { throw new NotSupportedException(); }
		
	//	internal virtual IAssociativePropertyMetadata GetMetadata(Type forType) {
	//		throw new NotSupportedException();
	//	}
	//	IAssociativePropertyMetadata IPropertyIdentifier.GetMetadata(Type forType) {
	//		return this.GetMetadata(forType);
	//	}
	//	#endregion instance

	//}
	///// <summary>再帰構造をなすノードに対して、添付プロパティを提供する。</summary>
	///// <typeparam name="TProp">プロパティ型</typeparam>
	//[Serializable]
	//public sealed class AttachedProperty<TProp> :  AssociativeProperty, IAttachedProperty {
	//	AttachedPropertyMetadata<TProp> _metaData;
	//	Predicate<TProp> _validateValueCallback;

	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	internal AttachedProperty(string propertyName, Type elementType, AttachedPropertyMetadata<TProp> metaData,
	//	Predicate<TProp> validateValueCallback) : base(propertyName, LinkagePattern.None) {
	//		metaData = metaData ?? new AttachedPropertyMetadata<TProp>();
	//		metaData.SetIdentifier(this);
	//		this.addElementType(propertyName, metaData);
	//		this.ElementType = elementType;
	//		_validateValueCallback = validateValueCallback;
	//	}
	//	void addElementType(string propertyName, AttachedPropertyMetadata<TProp> metaData) {
	//		var key = Tuple.Create(propertyName, this.ElementType);
	//		if (PropertyDic.ContainsKey(key))
	//			throw new InvalidOperationException("指定された要素型で、同名のプロパティが既に登録されています。");
	//		PropertyDic.Add(key, this);
	//		this._metaData = metaData;
	//	}
	//	/// <summary>要素型として関連付けられているキーの型を取得する。</summary>
	//	public Type ElementType { get; private set; }

	//	/// <summary>プロパティ値の検証を行う。</summary>
	//	/// <param name="value">プロパティ値</param>
	//	public override bool ValidateValueCallback(object value) {
	//		if (_validateValueCallback == null) return true;
	//		var vl = (TProp)value;
	//		return _validateValueCallback(vl);
	//	}
	//	internal override IAssociativePropertyMetadata GetMetadata(Type forType) {
	//		return _metaData;
	//	}

	//}
	///// <summary>再帰構造をなすノードにおいて、連動可能なプロパティを提供する。</summary>
	//[Serializable]
	//public sealed class LinkageProperty<TProp> : AssociativeProperty, ILinkageProperty {
	//	List<InheritNode<TProp>> keyTypeTrees = new List<InheritNode<TProp>>();
	//	Predicate<TProp> _validateValueCallback;

	//	/// <summary>新規インスタンスを初期化する。</summary>
	//	internal LinkageProperty(string propertyName, Type elementType,
	//		LinkagePattern pattern,
	//		LinkagePropertyMetadata<TProp> metaData,
	//		Predicate<TProp> validateValueCallback)
	//		: base(propertyName,pattern) {

	//		_validateValueCallback = validateValueCallback;
	//		this.AddElementType(elementType, metaData);
	//	}
	//	/// <summary>プロパティ値の検証を行う。</summary>
	//	/// <param name="value">プロパティ値</param>
	//	public override bool ValidateValueCallback(object value) {
	//		if (_validateValueCallback == null) return true;
	//		var vl = (TProp)value;
	//		return _validateValueCallback(vl);
	//	}
	//	/// <summary>指定された型、またはその基本型が要素型として含まれているかどうかを判定する。</summary>
	//	/// <param name="forType">所有者型またはその派生型</param>
	//	public bool ContainsAsKey(Type forType) {
	//		if (this.keyTypeTrees.Any(x => x.ElementType.IsAssignableFrom(forType))) {
	//			return true;
	//		}
	//		return false;
	//	}
	//	/// <summary>指定された型で取得可能なメタデータを返す。</summary>
	//	/// <param name="forType">要素型またはその派生型</param>
	//	internal override IAssociativePropertyMetadata GetMetadata(Type forType){
	//		var nn = keyTypeTrees
	//			.First(x => x.ElementType.IsAssignableFrom(forType))
	//			.Evolve(
	//				n => n.Children.Where(x => x.ElementType.IsAssignableFrom(forType)),
	//				(a, b) => a.Concat(b))
	//			.LastOrDefault();
	//		if (nn == null) 
	//			throw new ArgumentException("指定された型は連動プロパティとして登録されていないため、対応するメタデータは存在しません。", "forType");
			
	//		var md = new LinkagePropertyMetadata<TProp>(nn.Upstream().ToDictionary(x => x.ElementType, y => y.MetaData));
	//		md.SetProperty(this, nn.ElementType);
	//		return md;
	//	}
	//	void editTree(InheritNode<TProp> root, InheritNode<TProp> newNode) {
	//		var tnn = root
	//			.Evolve(
	//				n => n.Children.Where(x => x.ElementType.IsAssignableFrom(newNode.ElementType)),
	//				(a, b) => a.Concat(b))
	//			.LastOrDefault();
	//		if (tnn.ElementType == newNode.ElementType)
	//			throw new ArgumentException("連動プロパティの定義が重複しています。", "elementType");
	//		var cnn = tnn
	//			.Levelorder()
	//			.Where(x => x.ElementType.IsSubclassOf(newNode.ElementType));
	//		if (cnn.Any()) {
	//			int cnl = cnn.Min(x => x.NodeIndex().CurrentDepth);
	//			var cnc = cnn.Where(x => x.NodeIndex().CurrentDepth == cnl);
	//			foreach (var n in cnc) { newNode.AddChild(n); }
	//		}
	//		tnn.AddChild(newNode);
	//	}
	//	/// <summary>指定した型のインスタンスに存在する連動プロパティに、基本型に対応するメタデータをオーバーライドして提供する。</summary>
	//	/// <param name="elementType">要素型</param>
	//	/// <param name="metaData">メタデータ</param>
	//	public void OverrideMetadata(Type elementType, LinkagePropertyMetadata<TProp> metaData) {
	//		if (keyTypeTrees.Any(x => x.ElementType == elementType))
	//			throw new ArgumentException("登録済みの型で上書きできません。","elementType");
	//		var rt = keyTypeTrees.FirstOrDefault(x => elementType.IsSubclassOf(x.ElementType));
	//		if (rt == null) 
	//			throw new ArgumentException("上書きする要素型が存在しません","elementType"); 
	//		metaData = metaData ?? new LinkagePropertyMetadata<TProp>();
	//		metaData.SetProperty(this, elementType);
	//		var nnd = new InheritNode<TProp>(elementType, metaData);
	//		editTree(rt, nnd);
	//	}
	//	/// <summary>登録済みの連動プロパティの要素型へ、別の型を追加する。
	//	/// <para>追加する所有者型クラス内の public static readonly フィールドで使用し、取得した値を公開する必要がある。</para></summary>
	//	/// <param name="elementType">要素型</param>
	//	/// <param name="metaData">メタデータ</param>
	//	/// <returns>連動プロパティを識別する元の LinkageProperty への参照。</returns>
	//	public LinkageProperty<TProp> AddElementType(Type elementType, LinkagePropertyMetadata<TProp> metaData = null) {
	//		if (elementType == null)
	//			throw new ArgumentNullException("ownerType");
	//		if (elementType.IsInterface)
	//			throw new ArgumentException("要素型にインターフェイスを指定することはできません。", "elementType");
	//		var key = Tuple.Create(this.Name, elementType);
	//		if (PropertyDic.ContainsKey(key)) 
	//			throw new ArgumentException("指定された型で同一名のプロパティが既に登録されています。","elementType");
	//		if (keyTypeTrees.Any(x => elementType.IsSubclassOf(x.ElementType))) {
	//			var tgt = keyTypeTrees.First(x => elementType.IsSubclassOf(x.ElementType));
	//			throw new ArgumentException("基底クラス "+tgt.ElementType.ToString() + " が要素型として既に登録済みです。","elementType");
	//		}
	//		if (keyTypeTrees.Any(x => x.ElementType.IsSubclassOf(elementType))) {
	//			var tgt = keyTypeTrees.First(x => x.ElementType.IsSubclassOf(elementType));
	//			throw new ArgumentException("派生クラス "+ tgt.ElementType.ToString() +" が要素型として既に登録されています。","elementType");
	//		}
	//		metaData = metaData ?? new LinkagePropertyMetadata<TProp>();
	//		metaData.SetProperty(this, elementType);
	//		metaData.Seal();
	//		keyTypeTrees.Add(new InheritNode<TProp>(elementType, metaData));
	//		PropertyDic[key] = this;
	//		return this;
	//	}
	//	#region component class
	//	[Serializable]
	//	private class InheritNode<TMeta> : TreeNode<InheritNode<TMeta>>{
	//		public Type ElementType { get; private set; }
	//		LinkagePropertyMetadata<TMeta> _metaData;
	//		public LinkagePropertyMetadata<TMeta> MetaData {
	//			get { return _metaData; }
	//		}
	//		public InheritNode(Type elementType, LinkagePropertyMetadata<TMeta> metaData) {
	//			ElementType = elementType;
	//			_metaData = metaData;
	//		}
	//	}
	//	#endregion
	//}
}
