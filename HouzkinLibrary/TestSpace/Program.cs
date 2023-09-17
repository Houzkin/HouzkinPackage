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
using Houzkin.Architecture;
namespace TestSpace {

	//以下 テストコード
	class Program {
		static void Main(string[] args) {
			var bo = new BindableObject("sss");
			var model = new TestModel();
			var listener = new PropertyTreeChangedWeakEventListener<TestModel>(model);
			listener.RegisterHandler(t => t.Hoge.Fuga,(s,e)=> { Console.WriteLine("prop changed "+e.PropertyName); });


			var pickedHoge = model.Hoge;
			model.Hoge = new TestModel2("s - 02");
			Console.WriteLine("---- start ----");
			listener.Dispose();
			pickedHoge.Fuga = "xxx";

			listener = null;

			GC.Collect();
			Console.WriteLine("- - - - GC");

			pickedHoge.Fuga = "xxx";
			model.Hoge.Fuga = "xxx";

			Console.ReadKey();
		}
	}
	class TestModel : Livet.NotificationObject {
		public TestModel(string id = "01") {
			_hoge = new TestModel2();
			this.ID = id;
		}
		TestModel2 _hoge;
		public TestModel2 Hoge {
			get { return _hoge; }
			set {
				_hoge = value;
				this.RaisePropertyChanged();
			}
		}
		public string ID { get; set; }
	}
	class TestModel2 : Livet.NotificationObject {
		public TestModel2(string subId = "s - 01") {
			SubID = subId;
		}
		string _fuga = "";
		public string Fuga {
			get { return _fuga; }
			set {
				_fuga = value;
				RaisePropertyChanged();
			}
		}
		public string SubID { get; set; }
	}
}
