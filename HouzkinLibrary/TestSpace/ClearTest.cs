using Houzkin.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSpace {
    public static class Program {
        public static void Main(string[] args) {
            var r = new RootTree();
            var chl1 = new BaseTree();
            r.AddChild(chl1);
            Console.WriteLine(r.Children.Count);
            r.DismantleDescendants();
            Console.WriteLine(r.Children.Count);

            Console.ReadKey();
        }
    }
	internal class ClearTest {
	}
    public class  BaseTree : ObservableTreeNode<BaseTree> {

    }
    public class RootTree : BaseTree {

    }
}
