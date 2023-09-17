using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree {
//	/// <summary>連動プロパティ機構を実装したノードを表す。</summary>
	//	/// <typeparam name="TNode">ノードの型</typeparam>
	//	[Serializable]
	//	public abstract class AssociativePropertyNode<TNode> : ObservableTreeNode<TNode>, IAssociativePropertyNode
	//		where TNode : AssociativePropertyNode<TNode> {

	//		/// <summary>対象ノードに存在する連動プロパティを設定し、LinkagePattern に従って連動する。</summary>
	//		/// <param name="_self">対象</param>
	//		/// <param name="key">連動プロパティ</param>
	//		/// <param name="value">設定する値</param>
	//		private static void LinkageWithSetValue(AssociativePropertyNode<TNode> _self,
	//			AssociativeProperty key, object value){
	//			switch (key.Pattern) {
	//			case LinkagePattern.Entirely:
	//				(_self as IAssociativePropertyNode).SetValue(key, value);
	//				var rn = _self.Upstream()
	//					.Last()
	//					.Levelorder()
	//					.Where(x => x != _self)
	//					.OfType<IAssociativePropertyNode>();
	//				foreach (var n in rn) {
	//					n.SetValue(key, value);
	//				}
	//				break;
	//			case LinkagePattern.Dependency:
	//				_self.Levelorder();
	//				foreach (var v in _self.Levelorder()) {
	//					(v as IAssociativePropertyNode).SetValue(key, value);
	//				}
	//				break;
	//			default:
	//				(_self as IAssociativePropertyNode).SetValue(key, value);
	//				break;
	//			}
	//		}
	//		/// <summary>新規インスタンスを初期化する。</summary>
	//		public AssociativePropertyNode() : this(null) { }
	//		/// <summary>連想プロパティ機構における所有者型を指定して新規インスタンスを初期化する。</summary>
	//		/// <param name="elementType">要素型</param>
	//		protected AssociativePropertyNode(Type elementType) : base() {
	//			this.ElementType = elementType ?? this.GetType();
	//		}
	//		internal Type ElementType { get; private set; }

	//		Dictionary<AssociativeProperty, object> linkMap = new Dictionary<AssociativeProperty, object>();

	//		/// <summary>連動プロパティの設定を行う。</summary>
	//		/// <typeparam name="TProp">プロパティの型</typeparam>
	//		/// <param name="lp">連動プロパティ</param>
	//		/// <param name="value">プロパティ値</param>
	//		protected void SetValue<TProp>(LinkageProperty<TProp> lp, TProp value) {
	//			LinkageWithSetValue(this, lp, value);//連動させる
	//		}
	//		/// <summary>連動プロパティ値を取得する。</summary>
	//		/// <typeparam name="TProp">プロパティの型</typeparam>
	//		/// <param name="lp">連動プロパティ</param>
	//		/// <returns>プロパティ値</returns>
	//		protected TProp GetValue<TProp>(LinkageProperty<TProp> lp) { 
	//			return (TProp)(this as IAssociativePropertyNode).GetValue(lp);
	//		}
	//		/// <summary>添付プロパティの設定を行う。</summary>
	//		/// <typeparam name="TProp">プロパティの型</typeparam>
	//		/// <param name="ap">添付プロパティ</param>
	//		/// <param name="value">プロパティ値</param>
	//		public void SetValue<TProp>(AttachedProperty<TProp> ap, TProp value) {
	//			LinkageWithSetValue(this, ap, value);
	//		}
	//		/// <summary>添付プロパティ値を取得する</summary>
	//		/// <typeparam name="TProp">プロパティの型</typeparam>
	//		/// <param name="ap">添付プロパティ</param>
	//		/// <returns>プロパティ値</returns>
	//		public TProp GetValue<TProp>(AttachedProperty<TProp> ap) {
	//			return (TProp)(this as IAssociativePropertyNode).GetValue(ap);
	//		}
	//		void IAssociativePropertyNode.SetValue(AssociativeProperty key, object value) {
	//			var lk = key as ILinkageProperty;
	//			if (lk != null && !lk.ContainsAsKey(this.ElementType)) return;
	//			var md = key.GetMetadata(ElementType);

	//			Validate(key, value);
	//			value = md.CoerceValueCallback(value);
	//			Validate(key, value);

	//			object dfl = md.DefaultValue;
	//			object bf;
	//			if (!linkMap.TryGetValue(key, out bf)) {
	//				bf = dfl;
	//			}
	//			if (!object.Equals(value, bf)) {
	//				if (object.Equals(value, dfl)) {
	//					linkMap.Remove(key);
	//				} else {
	//					linkMap[key] = value;
	//				}
	//				md.OnAssociativePropertyChanged(this, bf, value);
	//			}
	//		}
	//		void Validate(AssociativeProperty lp, object value) {
	//			if (!lp.ValidateValueCallback(value))
	//				throw new ArgumentException("value");
	//		}
	//		object IAssociativePropertyNode.GetValue(AssociativeProperty key) {
	//			object vl;
	//			if (linkMap.TryGetValue(key, out vl)) return vl;
	//			try {
	//				vl = key.GetMetadata(this.ElementType).DefaultValue;
	//				return vl;
	//			} catch(Exception e) {
	//				throw new InvalidOperationException("指定された識別子を使用して取得可能な値が存在しません。", e);
	//			}
	//		}
	//		bool INodeFaculty<ILinkageProperty>.IsElementOf(ILinkageProperty key) {
	//			return key.ContainsAsKey(this.ElementType);
	//		}
	//		/// <summary>親ノード変更の決定を実行する。</summary>
	//		void SetLinkageValue(TNode newParent) {
	//			if (newParent != null) {
	//				var lps = AssociativeProperty.FromElementType(this.ElementType)
	//					.OfType<ILinkageProperty>()
	//					.Where(x => x.GetMetadata(this.ElementType).IsChangeByParentChanged(this));
	//				var ans = this.Upstream()
	//					.Skip(1)
	//					.OfType<IAssociativePropertyNode>();
	//				foreach (var lp in lps) {
	//					var p = ans.FirstOrDefault(x => x.IsElementOf(lp));
	//					if (p == null) continue;
	//					var val = p.GetValue((AssociativeProperty)lp);
	//					LinkageWithSetValue(this, (AssociativeProperty)lp, val);
	//				}
	//			}
	//		}
	//		/// <summary>指定したインデックスの位置に子ノードを指定する。</summary>
	//		/// <param name="index">インデックス</param>
	//		/// <param name="child">追加するノード</param>
	//		protected override void InsertChildNode(int index, TNode child) {
	//			base.InsertChildNode(index, child);
	//			child.SetLinkageValue((TNode)this);
	//		}
	//		/// <summary>指定したインデックスの位置にある子ノードを置き換える。</summary>
	//		/// <param name="index">インデックス</param>
	//		/// <param name="child">設定する子ノード</param>
	//		protected override void SetChildNode(int index, TNode child) {
	//			base.SetChildNode(index, child);
	//			child.SetLinkageValue((TNode)this);
	//		}
	//	}
}
