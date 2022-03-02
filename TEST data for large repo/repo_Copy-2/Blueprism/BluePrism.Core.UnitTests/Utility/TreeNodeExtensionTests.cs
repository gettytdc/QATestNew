#if UNITTESTS

using NUnit.Framework;
using System.Windows.Forms;
using BluePrism.Core.Extensions;

namespace BluePrism.Core.UnitTests.Utility
{
    public class TreeNodeExtensionTests
    {
        [Test]
        public void ValidateGetAllNodes_EmptyTree()
        {
            var tree = new TreeNode();

            Assert.IsTrue(tree.GetAllNodes().Count == 1);
        }

        [Test]
        public void ValidateGetAllNodes_1Tier()
        {
            var tree = new TreeNode();

            tree.Nodes.Add(new TreeNode());
            tree.Nodes.Add(new TreeNode());
            tree.Nodes.Add(new TreeNode());

            Assert.IsTrue(tree.GetAllNodes().Count == 4);
        }

        [Test]
        public void ValidateGetAllNodes_3Tiers()
        {
            var tree = new TreeNode();

            //builds a tree with 3 tiers:
            //       t
            //   /   |  \
            //   a   b   c
            // / |   |   |
            //a  aa  b   c

            //branch 1
            var t3_a = new TreeNode();
            var t3_aa = new TreeNode();
            var t2_a = new TreeNode();
            var t1_a = new TreeNode();
            //branch 2
            var t3_b = new TreeNode();
            var t2_b = new TreeNode();
            var t1_b = new TreeNode();
            //branch 3               
            var t3_c = new TreeNode();
            var t2_c = new TreeNode();
            var t1_c = new TreeNode();

            // build the tree
            t2_a.Nodes.Add(t3_a);
            t2_a.Nodes.Add(t3_aa);
            t1_a.Nodes.Add(t2_a);

            t2_b.Nodes.Add(t3_b);
            t1_b.Nodes.Add(t2_b);

            t2_c.Nodes.Add(t3_c);
            t1_c.Nodes.Add(t2_c);

            tree.Nodes.Add(t1_a);
            tree.Nodes.Add(t1_b);
            tree.Nodes.Add(t1_c);
    
        Assert.IsTrue(tree.GetAllNodes().Count == 11);
        }
    }
}
#endif
