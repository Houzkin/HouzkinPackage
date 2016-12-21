using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Houzkin.Tree;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Houzkin.Collections;
using Houzkin;
using Houzkin.Tree.Serialization;
using Houzkin.Xml.Serialization;

namespace TestSpace {
	//public class SerializeTest {
	//	public static void Run() {
	//		//var dic = new SerializableDictionary<int, string>();
	//		//dic.Add(1, "一");
	//		//dic.Add(7, "七");
	//		//var sria = new XmlSerializer(typeof(SerializableDictionary<int, string>));
	//		//using (FileStream fs = new FileStream("dic.xml", FileMode.Create)) {
	//		//	sria.Serialize(fs, dic);
	//		//}
	//		//SerializableDictionary<int, string> newDic;
	//		//using (FileStream fs = new FileStream("dic.xml", FileMode.Open)) {
	//		//	newDic = sria.Deserialize(fs) as SerializableDictionary<int, string>;
	//		//}
	//		//foreach (var p in newDic) Console.WriteLine("Key:{0}, Value:{1}", p.Key, p.Value);
	//		//Console.ReadKey();

	//		var root = new SampleNode() { Name = "Root" };
	//		root.AddChild(new SampleNode() { Name = "Gmgm" });
	//		root.AddChild(new SampleNode() { Name = "Brbr" }.AddChild(new SampleNode() { Name = "Htht" }));
	//		var r = new SampleRootNode();
			
	//		Console.WriteLine(root.Children.First().NodeIndex().ToString());

	//		var seri1 = new XmlSerializer(typeof(SerializableNodeMap<SampleNode>));
	//		using (FileStream fs = new FileStream("sample.xml", FileMode.Create)) {
	//			seri1.Serialize(fs, root.ToSerializableNodeMap());
	//		}
	//		using (FileStream fs = new FileStream("sample.xml", FileMode.Open)) {
	//			var rr = seri1.Deserialize(fs) as SerializableNodeMap<SampleNode>;
	//			var rt = rr.AssembleTree();
	//			foreach (var r in rt.Levelorder()) Console.WriteLine(r.NodePath(x => x.Name));
	//		}
	//		Console.ReadKey();
	//		//// コンストラクタにターゲットの型を渡す  
	//		//var ds = new DataContractSerializer(typeof(SampleNode));

	//		//// 出力先を作成  
	//		//var sw = new StringWriter(); 
	//		//var xw = new XmlTextWriter("sample.xml", new System.Text.UTF8Encoding(false));
			
	//		//xw.Formatting = Formatting.Indented;

	//		//ds.WriteObject(xw, root);
	//		var selializer = new XmlSerializer(typeof(SampleNode));
	//		using (FileStream fs = new FileStream("sample.xml", FileMode.Create)) {
	//			selializer.Serialize(fs, root);
	//		}
	//		XmlSerializer serializer = new XmlSerializer(typeof(SampleNode));
	//		SampleNode record2;// = new SampleNode();
	//		using (FileStream fs = new FileStream("sample.xml", FileMode.Open)) {
	//			record2 = serializer.Deserialize(fs) as SampleNode;
	//			// XML からオブジェクトが復元されている
	//		}
	//		foreach (var c in record2.Levelorder()) Console.WriteLine(c.NodePath(x => x.Name));

	//		var map = root.Postorder().ToDictionary(x => x.NodeIndex().ToArray());
	//		var seri = new SerializableDictionary<int[], SampleNode>(map);
	//	}
	//}
	//public class SampleNode : TreeNode<SampleNode> //, IXmlSerializable 
	//{
	//	#region static 
	//	//public SerializableDictionary<int[], SampleNode> GetSerializing() {
	//	//	return new SerializableDictionary<int[], SampleNode>(this.Postorder().ToDictionary(x => x.NodePathCode().ToArray()));
	//	//}
	//	//public static SampleNode Build(SerializableDictionary<int[], SampleNode> obj) {
	//	//	//var seq = obj.ToDictionary(x => new NodePathCode(x.Key), x => x.Value).OrderBy(x => x.Key, NodePathCode.GetPostorderComparer);
	//	//	var seq = obj.Select(x => new { Path = new NodePathCode(x.Key), Value = x.Value })
	//	//		.OrderBy(x => x.Path, NodePathCode.GetPostorderComparer());
	//	//}
		
	//	#endregion

	//	#region instance
	//	NodeCore core = new NodeCoreEx();

	//	public string Name {
	//		get { return core.Name; }
	//		set { core.Name = value; }
	//	}

		
	//	#endregion
	//}
	//public class SampleRootNode : SampleNode { }
	////public class SampleNodeEx : SampleNode {

	////}
	//public class NodeCore {
	//	public string Name { get; set; }
	//}
	//public class NodeCoreEx : NodeCore {
	//	public int Age { get; set; }
	//}
}
