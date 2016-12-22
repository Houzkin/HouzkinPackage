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
			#region
			//SerializeTest.Run();
			//Console.ReadKey();

			//EventHandler<StructureChangedEventArgs<TestTreeNode>> ev = (o, e) => {
			//	Console.WriteLine("sender:{0},target:{1},previous:{2},action:{3}", o, e.Target, e.PreviousParentOfTarget,e.TreeAction);
			//	if (e.DescendantsChanged) {
			//		Console.WriteLine("\tdesAction:{0}", e.DescendantInfo.NodeAction);
			//	}
			//	if (e.AncestorChanged) {
			//		Console.WriteLine("\tRootChanged:{0}", e.AncestorInfo.RootChanged);
			//	}
			//};
			//var A = new TestTreeNode("A");
			//A.StructureChanged += ev;

			//var B = new TestTreeNode("B");
			//B.StructureChanged += ev;

			//var C = new TestTreeNode("C");
			//C.StructureChanged += ev;

			//var D = new TestTreeNode("D");
			//D.StructureChanged += ev;

			//A.AddChild(B);
			//C.Parent = A;

			//B.AddChild(C);
			//Console.WriteLine("- - - - - - - - - - - - - - - -");

			//A.ChildNodes[0] = D;
			//A.AddChild(B).ChildNodes.First()
			//	.AddChild(C).ChildNodes.First()
			//	.AddChild(D);

			//A.ChildNodes[0] = D;

			//A.AddChild(B).AddChild(C);

			//Console.WriteLine("- - - - - - - - - - - - - - - -");
			//A.Move();
			//B.AddChild(D);
			//var tt = D.NodePath(x => x.ToString());
			//var pth = NodePath<string>.Create(D, x => x.ToString());
			//Console.WriteLine(pth);
			//Console.ReadKey();
			#endregion
			var model = new TestModel();
			var wlistener = new Livet.EventListeners.WeakEvents.PropertyChangedWeakEventListener(model);
			wlistener.RegisterHandler((s, e) => { Console.WriteLine("prop changed by livet"); });
			var listener = new Houzkin.Architecture.PropertyTreeChangedWeakEventListener<TestModel>(model);
			listener.RegisterHandler(t => t.Obj.Name,(s,e)=> { Console.WriteLine("prop changed "+e.PropertyName); });

			GC.Collect();

			model.Obj = new TestModel2();
			model.Obj.Name = "Hoge";
			wlistener = null;
			listener = null;

			GC.Collect();
			Console.WriteLine("- - - - GC");

			model.Obj = new TestModel2();
			model.Obj.Name = "Fuga";
			Console.ReadKey();
		}
	}
	class TestModel : Livet.NotificationObject {
		public TestModel() {
			obj = new TestModel2();
		}
		TestModel2 obj;
		public TestModel2 Obj {
			get { return obj; }
			set {
				obj = value; this.RaisePropertyChanged();
			}
		}
	}
	class TestModel2 : Livet.NotificationObject {
		string name = "";
		public string Name {
			get { return name; }
			set {
				name = value;
				RaisePropertyChanged();
			}
		}
	}
}
