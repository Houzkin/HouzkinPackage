using Houzkin.Tree;
using Houzkin.Architecture;


namespace AppLogger
{
	public class TestNode : ObservableTreeNode<TestNode>
    {
		public TestNode(string name) { Name = name; }
		public string Name { get; private set; }
	}
	public class ShadowNode : ReadOnlyBindableTreeNode<TestNode, ShadowNode>
	{
		public ShadowNode(TestNode model) : base(model)
		{
		}

		protected override ShadowNode GenerateChild(TestNode modelChildNode)
		{
			return new ShadowNode(modelChildNode);
		}
		protected override IEnumerable<TestNode> DesignateChildCollection(IEnumerable<TestNode> modelChildNodes)
		{
			return base.DesignateChildCollection(modelChildNodes);
		}

	}
}