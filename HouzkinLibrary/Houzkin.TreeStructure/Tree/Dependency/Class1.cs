using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree{
	public class AbsNode : TreeNode<AbsNode>, IDependencyNode{
		public static readonly DependencyProperty NameProperty
			= DependencyProperty.Register("Name", typeof(string), typeof(AbsNode));
		public int Age {
			get { return (int)NameProperty.GetValue(this); }
			set { NameProperty.SetValue(this, value); }
		}
		IDependencyNode IReadOnlyTreeNode<IDependencyNode>.ParentNode {
			get { return this.ParentNode; }
		}
		IReadOnlyList<IDependencyNode> IReadOnlyTreeNode<IDependencyNode>.ChildNodes {
			get {
				return this.ChildNodes.OfType<IDependencyNode>().ToList();
				//return this.ChildNodes as IReadOnlyList<IDependencyNode>;
			}
		}
	}
	public class DependencyProperty {
		public static DependencyProperty Register(string name, Type propertyType, Type ownerType) { 
			throw new NotImplementedException();
		}
		Dictionary<IDependencyNode, object> dependencyValues = new Dictionary<IDependencyNode, object>();

		public object GetValue(IDependencyNode owner) {
			object v;
			var key = owner.Upstream().FirstOrDefault(x => dependencyValues.ContainsKey(x));
			if (key != null) return dependencyValues[key];
			else return new Object();
		}
		public void SetValue<T>(T owner, object value) { }
		public void SetUnsetValue<T>(T owner) { }
	}
	public interface IDependencyNode : IReadOnlyTreeNode<IDependencyNode> {
		
	}
}

