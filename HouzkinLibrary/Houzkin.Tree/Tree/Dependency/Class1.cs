using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Houzkin.Tree{

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

