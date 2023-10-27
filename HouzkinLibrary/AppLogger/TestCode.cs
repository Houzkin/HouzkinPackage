using Houzkin;
using Houzkin.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLogger {
	public static class TestCode {

		public static void Main(string[] args) {
			//var dic = new Dictionary<int, string>();
			//dic.Add(1, "AAA");
			//dic.Add(2, "BBB");
			//var rst = ResultWithValue.Of<int, string>(dic.TryGetValue,5);
			//Console.WriteLine(rst.Result);

			//string num = "1000";
			////int.TryParse(num,result)
			//var result = ResultWithValue.Of<int>(int.TryParse, num);
			//Console.WriteLine(result.Value);
			var node1 = new TestNode() { Name = "A", };
			var node2 = new TestNode() { Name = "B", };
			var node3 = new TestNode() { Name = "C", };
			var node4 = new TestNode() { Name = "D", };
			var node5 = new TestNode() { Name = "E" };
			var node6 = new TestNode() { Name = "F", };

			node1.AddChild(node2);
			node2.AddChild(node3);
			node3.AddChild(node4);
			node4.AddChild(node5);
			node5.AddChild(node6);
			Console.WriteLine("init set");
            Console.WriteLine(string.Join("-", node1.Levelorder().Select(x => x.Name)));
			//Console.WriteLine(node6.Path);
			Console.WriteLine("dispose start");
			node3.Dispose();
			Console.WriteLine(string.Join("-",node1.Levelorder().Select(x=>x.Name)));

			Console.ReadKey();
		}
	}
	public class TestNode : ObservableTreeNode<TestNode> {
		public TestNode() {
            this.Disposed += TestNode_Disposed;
            this.StructureChanged += TestNode_StructureChanged;
		}

        private void TestNode_StructureChanged(object? sender, StructureChangedEventArgs<TestNode> e) {
			Console.WriteLine($"Notice from {this.Name} structure changed {e.TreeAction} : {e.Target.Name}");
        }

        private void TestNode_Disposed(object? sender, EventArgs e) {
			Console.WriteLine($"Dispose {this.Name}");
        }

        public string Name { get; set; }
		
	}
}
