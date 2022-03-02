using System;
using BluePrism.Core.Extensions;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Extensions
{
    [TestFixture]
    public class TreeViewExtensionTests
    {
        [Test]
        public void EmptyTreeViewDoesNotError()
        {
            try
            {
                var treeView = new TreeView();
                var exceptions = new List<TreeNode>() {new TreeNode() {Name = "SomeThing"}};
                treeView.ExpandAll(exceptions);

                Assert.True(true);
            }
            catch
            {
                Assert.True(false);
            }
        }

        [Test]
        public void EmptyExceptionsListDoesNotError()
        {
            try
            {
                var treeView = new TreeView(){Name = "ATreeView"};
                treeView.Nodes.Add(new TreeNode() { Name = "SomeThing" } );
                var exceptions = new List<TreeNode>();

                treeView.ExpandAll(exceptions);

                Assert.True(true);
            }
            catch
            {
                Assert.True(false);
            }
        }

        [Test]
        public void TreeViewIsExpandedRecursively()
        {
            var treeView = new TreeView() { Name = "ATreeView" };
            var rootTreeNode = new TreeNode() {Name = "Root"};
            var firstLevelNode = new TreeNode() {Name = "FirstLevel"};
            var secondLevelNode = new TreeNode() {Name = "SecondLevel"};

            firstLevelNode.Nodes.Add(secondLevelNode);
            rootTreeNode.Nodes.Add(firstLevelNode);
            treeView.Nodes.Add(rootTreeNode);

            var exceptions = new List<TreeNode>();

            treeView.ExpandAll(exceptions);

            Assert.True(treeView.Nodes[0].IsExpanded);//rootNode
            Assert.True(treeView.Nodes[0].Nodes[0].IsExpanded);//first
            Assert.True(treeView.Nodes[0].Nodes[0].Nodes[0].IsExpanded);//second
        }
        [Test]
        public void TreeViewIsExpandedButSkipsException()
        {
            var treeView = new TreeView() { Name = "ATreeView" };
            var treeNode1 = new TreeNode() { Name = "treeNode1" };
            var treeNode2 = new TreeNode() { Name = "treeNode2" };
            var treeNode3 = new TreeNode() { Name = "treeNode3" };
          
            treeView.Nodes.Add(treeNode1);
            treeView.Nodes.Add(treeNode2);
            treeView.Nodes.Add(treeNode3);

            var exceptions = new List<TreeNode>(){treeNode2};

            treeView.ExpandAll(exceptions);

            Assert.True(treeView.Nodes[0].IsExpanded);//treeNode1
            Assert.False(treeView.Nodes[1].IsExpanded);//treeNode1
            Assert.True(treeView.Nodes[2].IsExpanded);//treeNode3
        }
    }
}
