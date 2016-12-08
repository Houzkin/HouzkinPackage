using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Houzkin.Tree;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TestSpace {
	public static class LpTestSpace {
		public static void Run() {
			var n1 = new LpAgeNode("n1");
			var n2 = new LpNameNode("n2");
			var n3 = new LpSubAgeNode("n3");
			var n4 = new LpNameNode("n4");
			EventHandler<ChainEventArgs> ce = (s, e) => { Console.WriteLine(s.ToString()); };
			n1.AddChild(n2);
			n2.AddChild(n3);
			n3.AddChild(n4);
			n3.LpSubSectionChanged += (s, e) => { Console.WriteLine("DirectChanged "+ e.SourceNode.ToString()); };
			n3.Age = 4;
			//n1.SectionChanged += ce;
			//n2.SectionChanged += ce;
			n3.SectionChanged += ce;
			//n4.SectionChanged += ce;
			
			n2.Section = "xx";
			Console.WriteLine("n1:{0} n2:{1} n3:{2} n4:{3}",n1.Section,n2.Section,n3.Section,n4.Section);
			Console.ReadKey();


			// オブジェクトをファイルalice.binとしてシリアライズ
			using (Stream stream = File.OpenWrite("alice.bin")) {
				BinaryFormatter formatter = new BinaryFormatter();

				formatter.Serialize(stream, n1);
			}

			// ファイルの内容を元にデシリアライズする
			LpAgeNode deserialized = null;

			using (Stream stream = File.OpenRead("alice.bin")) {
				BinaryFormatter formatter = new BinaryFormatter();

				deserialized = (LpAgeNode)formatter.Deserialize(stream);
			}

			Console.WriteLine(deserialized);
			deserialized.Section = "yy";
			Console.ReadKey();
		}
		[Serializable]
		class LpNameNode : SympathizeableNode<LpNameNode> {
			public static readonly LinkageProperty<string> SectionProperty
				= AssociativeProperty.RegisterLinkage<string>("Section", typeof(LpNameNode), LinkagePattern.Dependency,
				new LinkagePropertyMetadata<string>(RaiseSectionChanged));
			public static readonly ChainEvent<ChainEventArgs> SectionChangedEvent
				= ChainEvent.Register<ChainEventArgs>("NameChanged", typeof(LpNameNode), ChainStrategy.Bubble);
			public string Section {
				get { return base.GetValue(SectionProperty); }
				set { this.SetValue(SectionProperty, value); }
			}
			static void RaiseSectionChanged(object sender,AssociativePropertyChangedEventArgs<string> e) {
				var s = sender as LpNameNode;
				if (s != null)
					s.RaiseEvent(SectionChangedEvent, new ChainEventArgs());
			}
			public event EventHandler<ChainEventArgs> SectionChanged {
				add { this.AddHandler(SectionChangedEvent, value); }
				remove { this.RemoveHandler(SectionChangedEvent, value); }
			}
			public LpNameNode(string name) {
				this.Name = name;
			}
			public string Name { get; private set; }
			public override string ToString() {
				return this.Name;
			}
		}
		[Serializable]
		class LpAgeNode : LpNameNode {
			public static readonly LinkageProperty<int> AgeProperty
				= AssociativeProperty.RegisterLinkage<int>("Age", typeof(LpAgeNode), LinkagePattern.Entirely);
			public int Age {
				get { return this.GetValue(AgeProperty); }
				set { this.SetValue(AgeProperty, value); }
			}
			public LpAgeNode(string name) : base(name) { }
		}
		[Serializable]
		class LpSubAgeNode : LpNameNode {
			public static readonly SourceHandler<ChainEventArgs> SectionChangedSpecificEvent
				= ChainEvent.EstablishSourceHandler(LpNameNode.SectionChangedEvent);//, new Predicate<LpSubAgeNode>(x => true));
			public static readonly LinkageProperty<int> AgeProperty
				= LpAgeNode.AgeProperty.AddElementType(typeof(LpSubAgeNode));
			static LpSubAgeNode() {
				LpNameNode.SectionProperty.OverrideMetadata(typeof(LpSubAgeNode), 
					new LinkagePropertyMetadata<string>((s, e) => {
						var ss = s as LpSubAgeNode;
						if (ss != null) ss.sectionChanged();
					}));
			}
			public int Age {
				get { return this.GetValue(AgeProperty); }
				set { this.SetValue(AgeProperty, value); }
			}
			public event EventHandler<ChainEventArgs> LpSubSectionChanged {
				add { this.AddHandler(SectionChangedSpecificEvent, value); }
				remove { this.RemoveHandler(SectionChangedSpecificEvent, value); }
			}
			void sectionChanged() {
				Console.WriteLine("Name:{0} is changed", Name);
			}
			public LpSubAgeNode(string name) : base(name) { }
		}
	}
}
