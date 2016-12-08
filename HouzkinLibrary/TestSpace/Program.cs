using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Houzkin.Tree;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using System.Diagnostics;
using System.Dynamic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace TestSpace {
	
	class Program {
		static void Main(string[] args) {
			SerializeTest.Run();
			Console.ReadKey();

			EventHandler<StructureChangedEventArgs<TestTreeNode>> ev = (o, e) => {
				Console.WriteLine("sender:{0},target:{1},previous:{2},action:{3}", o, e.Target, e.PreviousParentOfTarget,e.TreeAction);
				if (e.DescendantsChanged) {
					Console.WriteLine("\tdesAction:{0}", e.DescendantInfo.NodeAction);
				}
				if (e.AncestorChanged) {
					Console.WriteLine("\tRootChanged:{0}", e.AncestorInfo.RootChanged);
				}
			};
			var A = new TestTreeNode("A");
			A.StructureChanged += ev;

			var B = new TestTreeNode("B");
			B.StructureChanged += ev;

			var C = new TestTreeNode("C");
			C.StructureChanged += ev;

			var D = new TestTreeNode("D");
			D.StructureChanged += ev;

			A.AddChild(B);
			C.Parent = A;

			B.AddChild(C);
			Console.WriteLine("- - - - - - - - - - - - - - - -");

			//A.ChildNodes[0] = D;
			//A.AddChild(B).ChildNodes.First()
			//	.AddChild(C).ChildNodes.First()
			//	.AddChild(D);

			//A.ChildNodes[0] = D;

			//A.AddChild(B).AddChild(C);

			//Console.WriteLine("- - - - - - - - - - - - - - - -");
			//A.Move();
			B.AddChild(D);
			var tt = D.NodePath(x => x.ToString());
			var pth = NodePath<string>.Create(D, x => x.ToString());
			Console.WriteLine(pth);
			Console.ReadKey();
		}
	}
	class TestTreeNode : ObservableTreeNode<TestTreeNode> {
		string _name;
		public TestTreeNode(string name) {
			_name = name;
		}
		public override string ToString() {
			return _name;
		}
		public void Move() {
			this.ChildNodes.Move(0, 1);
		}
	}
}
