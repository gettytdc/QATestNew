using BluePrism.AutomateAppCore.Resources;
using BluePrism.UIAutomation.Classes.SearchBar;
using NUnit.Framework;
using System.Collections.Generic;


namespace AutomateUI.UnitTests.Classes
{
    [TestFixture]
    public class FilterQueryTests
    {
        private FilterQuery _fq;
        [SetUp]
        public void CreateSearchTree()
        {
            _fq = new FilterQuery(My.Resources.Resources.Name.ToLower(), My.Resources.Resources.SearchKeyWordIn.ToLower());


            _fq.AddColumnNode(new SearchTreeNode(My.Resources.Resources.Name.ToLower(), new List<SearchTreeNode>()));
            _fq.AddColumnNode(new SearchTreeNode(My.Resources.Resources.ctlResourceView_State.ToLower(), new List<SearchTreeNode>()));
            _fq.AddColumnNode(new SearchTreeNode(My.Resources.Resources.ctlResourceView_SessionInfo.ToLower(), new List<SearchTreeNode>()));
            _fq.AddColumnNode(new SearchTreeNode(My.Resources.Resources.ctlResourceView_Connection.ToLower(), new List<SearchTreeNode>()));
            _fq.AddColumnNode(new SearchTreeNode(My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), new List<SearchTreeNode>()));

            _fq.AddLeafToColumnNode(My.Resources.Resources.Name.ToLower(), new SearchTreeNode("BPEU000:Res0", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_State.ToLower(), new SearchTreeNode("Working", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_SessionInfo.ToLower(), new SearchTreeNode("Running", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_Connection.ToLower(), new SearchTreeNode(ResourceConnectionState.Connected.ToString(), null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), new SearchTreeNode("Connected", null));

            _fq.AddLeafToColumnNode(My.Resources.Resources.Name.ToLower(), new SearchTreeNode("BPEU000:Res1", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_State.ToLower(), new SearchTreeNode("Working", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_SessionInfo.ToLower(), new SearchTreeNode("Running", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_Connection.ToLower(), new SearchTreeNode(ResourceConnectionState.Connected.ToString(), null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), new SearchTreeNode("Connected", null));

            _fq.AddLeafToColumnNode(My.Resources.Resources.Name.ToLower(), new SearchTreeNode("BPEU000:Res2", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_State.ToLower(), new SearchTreeNode("Idle", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_SessionInfo.ToLower(), new SearchTreeNode("Running", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_Connection.ToLower(), new SearchTreeNode(ResourceConnectionState.Connected.ToString(), null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), new SearchTreeNode("Connecting", null));

            _fq.AddLeafToColumnNode(My.Resources.Resources.Name.ToLower(), new SearchTreeNode("BPEU000:Res3", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_State.ToLower(), new SearchTreeNode("Offline", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_SessionInfo.ToLower(), new SearchTreeNode("Stopped", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_Connection.ToLower(), new SearchTreeNode(ResourceConnectionState.Connected.ToString(), null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), new SearchTreeNode("Connecting", null));

            _fq.AddLeafToColumnNode(My.Resources.Resources.Name.ToLower(), new SearchTreeNode("BPEU000:Res4", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_State.ToLower(), new SearchTreeNode("Missing", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_SessionInfo.ToLower(), new SearchTreeNode("Gone", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_Connection.ToLower(), new SearchTreeNode(ResourceConnectionState.Disconnected.ToString(), null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), new SearchTreeNode("Missing", null));

            _fq.AddLeafToColumnNode(My.Resources.Resources.Name.ToLower(), new SearchTreeNode("BPEU000:Res5", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_State.ToLower(), new SearchTreeNode("Missing", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_SessionInfo.ToLower(), new SearchTreeNode("Gone", null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_Connection.ToLower(), new SearchTreeNode(ResourceConnectionState.Disconnected.ToString(), null));
            _fq.AddLeafToColumnNode(My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower(), new SearchTreeNode("Missing", null));
        }

        [Test]
        public void Evaluate_InColumnName_ColumnNameSetToSameName()
        {
            _fq.Evaluate("in: " + My.Resources.Resources.Name.ToLower());
            Assert.AreEqual(_fq.ColumnName, My.Resources.Resources.Name.ToLower());

            _fq.Evaluate("in: " + My.Resources.Resources.ctlResourceView_State.ToLower());
            Assert.AreEqual(_fq.ColumnName, My.Resources.Resources.ctlResourceView_State.ToLower());

            _fq.Evaluate("in: " + My.Resources.Resources.ctlResourceView_SessionInfo.ToLower());
            Assert.AreEqual(_fq.ColumnName, My.Resources.Resources.ctlResourceView_SessionInfo.ToLower());

            _fq.Evaluate("in: " + My.Resources.Resources.ctlResourceView_Connection.ToLower());
            Assert.AreEqual(_fq.ColumnName, My.Resources.Resources.ctlResourceView_Connection.ToLower());

            _fq.Evaluate("in: " + My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower());
            Assert.AreEqual(_fq.ColumnName, My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower());
        }

        [Test]
        public void GetAutoFills_EvaluateIn_ReturnsColumnNameAutofills()
        {
            //Arrange
            _fq.MaxAutoFills = 5;
            var expect = new List<string>() { "in: " + My.Resources.Resources.Name.ToLower(),
                                              "in: " + My.Resources.Resources.ctlResourceView_State.ToLower(),
                                              "in: " + My.Resources.Resources.ctlResourceView_SessionInfo.ToLower(),
                                              "in: " + My.Resources.Resources.ctlResourceView_Connection.ToLower(),
                                              "in: " + My.Resources.Resources.ctlResourceView_LatestConnectionMessage.ToLower() };
            //Act
            _fq.Evaluate("in:");
            var result = _fq.GetAutoFills();
            //Assert
            Assert.AreEqual(expect, result);
        }

        [Test]
        public void GetAutoFills_EvaluateInName_ReturnsResoruceNameAutofills()
        {
            //Arrange
            _fq.MaxAutoFills = 6;
            var expect = new List<string>() { "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res0",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res1",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res2",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res3",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res4",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res5"};
            //Act
            _fq.Evaluate("in: " + My.Resources.Resources.Name.ToLower());
            var result = _fq.GetAutoFills();
            //Assert
            Assert.AreEqual(expect, result);
        }

        [Test]
        public void GetAutoFills_EvaluateSimpleNameQuery_ReturnsResoruceNameAutofills() 
        {
            //Arrange
            _fq.MaxAutoFills = 6;
            var expect = new List<string>() { "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res0",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res1",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res2",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res3",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res4",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res5"};
            //Act
            _fq.Evaluate("in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res");
            var result = _fq.GetAutoFills();
            //Assert
            Assert.AreEqual(expect, result);
        }

        [Test]
        public void GetAutoFills_EvaluateCAPSSimpleNameQuery_ReturnsResoruceNameAutofills()
        {
            //Arrange
            _fq.MaxAutoFills = 6;
            var expect = new List<string>() { "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res0",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res1",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res2",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res3",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res4",
                                              "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res5"};
            //Act
            _fq.Evaluate("in: " + My.Resources.Resources.Name.ToLower() + "BPEU000:RES");
            var result = _fq.GetAutoFills();
            //Assert
            Assert.AreEqual(expect, result);
        }

        [Test]
        public void GetAutoFills_EvaluateSimpleNameQuery_ReturnsSingleAutoFill()
        {
            //Arrange
            _fq.MaxAutoFills = 6;
            var expect = new List<string>() { "in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res0"};
            //Act
            _fq.Evaluate("in: " + My.Resources.Resources.Name.ToLower() + " BPEU000:Res0");
            var result = _fq.GetAutoFills();
            //Assert
            Assert.AreEqual(expect, result);
        }

        [Test]
        public void GetAutoFills_EvaluateSimpleMissingQuery_ReturnsNothing()
        {
            //Act
            _fq.Evaluate("in: " + My.Resources.Resources.Name.ToLower() + "BPEU000:Res6");
            var result = _fq.GetAutoFills();
            //Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}
