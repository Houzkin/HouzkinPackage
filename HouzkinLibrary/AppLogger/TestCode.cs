using Houzkin;
using Houzkin.Tree;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
			var nodeA = new TestNode() { Name = "A", };
			var nodeB = new TestNode() { Name = "B", };
			var nodeC = new TestNode() { Name = "C", };
			var nodeD = new TestNode() { Name = "D", };
			var nodeE = new TestNode() { Name = "E" };
			var nodeF = new TestNode() { Name = "F", };
			var nodeG = new TestNode() { Name = "G", };
			var nodeH = new TestNode() { Name = "H", };
			var nodeI = new TestNode() { Name = "I", };

			nodeA.AddChild(nodeB);
			nodeB.AddChild(nodeC);
			nodeC.AddChild(nodeD);
			nodeD.AddChild(nodeE);
			nodeE.AddChild(nodeF);
			Console.WriteLine("init set");
            //Console.WriteLine(string.Join("-", nodeA.Preorder().Select(x => x.NodeIndex().ToString())));
            Console.WriteLine(nodeA.ToTreeDiagram(a => a.Name));
            Console.WriteLine(nodeA.ToTreeDiagram(a => $"Name : {a.Name}, Height : {a.Height()} NodeIndex : {a.NodeIndex()}, Depth : {a.Depth()}" ));
            nodeA.AddChild(nodeE);
			nodeD.AddChild(nodeG);
			nodeC.AddChild(nodeH);
			nodeE.AddChild(nodeI);
            //Console.WriteLine(string.Join("-", nodeA.Preorder().Select(x => x.Name)));
            Console.WriteLine(nodeA.ToTreeDiagram(a => a.Name));
            Console.WriteLine(nodeA.ToTreeDiagram(a => $"Name : {a.Name}, Height : {a.Height()}, NodeIndex : {a.NodeIndex()}, Depth : {a.Depth()}"));
			//Console.WriteLine(node6.Path);
			//         Console.WriteLine("dispose start");
			//nodeC.Dispose();
			//Console.WriteLine(string.Join("-",nodeA.Levelorder().Select(x=>x.Name)));
			//TestExp(() => nodeA.Name);
			//TestExp(() => nodeB.Name.Length);
			var root = new RootClass();
			
			root.NotifyObject = new NotifyObj() { Name = "test1" };
			root.NotifyObject.Name = "test2";

            root.NotifyObject = new NotifyObj() { Name = "test3" };
            root.NotifyObject.Name = "test4";
            Console.ReadKey();
		}
	}
	public class TestNode : ObservableTreeNode<TestNode> {
		public TestNode() {
            //this.Disposed += TestNode_Disposed;
            //this.StructureChanged += TestNode_StructureChanged;
		}

        private void TestNode_StructureChanged(object? sender, StructureChangedEventArgs<TestNode> e) {
			Console.WriteLine($"Notice from {this.Name} structure changed {e.TreeAction} : {e.Target.Name}");
        }

        private void TestNode_Disposed(object? sender, EventArgs e) {
			Console.WriteLine($"Dispose {this.Name}");
        }
		string name = "";

        public string Name {
			get { return name; }
			set {
				if (name != value) {
					name = value;
					//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				}
			}
		}
	}
	public class RootClass : BindableBase {
		public RootClass() {
			Command = new DelegateCommand(() => { })/*.ObservesProperty(() => NotifyObject)*/.ObservesProperty(() => NotifyObject.Name);
			Command.CanExecuteChanged += (s, e) => { Console.WriteLine($"changed! {NotifyObject.Name}"); };
			
		}
		NotifyObj notify = new NotifyObj();
		public NotifyObj NotifyObject {
			get { return notify; }
			set {
				SetProperty(ref notify, value);
			}
		}
		DelegateCommand Command { get; set; }
	}
    public class NotifyObj : BindableBase {
		public NotifyObj() { }
		string name = "default";
        public string Name {
            get { return name; }
            set {
                this.SetProperty(ref name, value);
            }
        }
    }
    

}
