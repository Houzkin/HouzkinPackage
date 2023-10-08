using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Houzkin;
using Houzkin.Tree;

namespace TestProject {
	public static class Program {
		public static void Main(string[] args) {

			var dic = new Dictionary<int, string>();
			dic.Add(1, "AAA");
			dic.Add(2, "BBB");
			var rst = ResultWithValue.Of<int, string>(dic.TryGetValue);
			//var r = new RootTree();
			//var chl1 = new BaseTree();
			//r.AddChild(chl1);
			//Console.WriteLine(r.Children.Count);
			//r.DismantleDescendants();
			//Console.WriteLine(r.Children.Count);

			Console.ReadKey();
		}
	}
	internal class ClearTest {
	}
	public class BaseTree : ObservableTreeNode<BaseTree> {

	}
	public class RootTree : BaseTree {

	}
}
