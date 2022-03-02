#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.Processes;
using BluePrism.AutomateProcessCore.Stages;
using BluePrism.Common.Security;
using BluePrism.Core.Expressions;
using Moq;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.StageTests
{
    [TestFixture]
    public class CollectionStageTests
    {
        private static clsCollectionRow CreateNew80sBackgroundMemberRow(int id, string name, SafeString pword)
        {
            var row = new clsCollectionRow();
            row["ID"] = id;
            row["Name "] = name;
            row["Password"] = pword;
            return row;
        }

        private static void Check80sBackgroundMemberRow(clsCollectionRow row, int expectedId, string expectedName, SafeString expectedBand)
        {
            Assert.That(row["ID"], Is.EqualTo(new clsProcessValue(expectedId)), "ID incorrect for Id:{0}, Name:{1}", (object)expectedId, expectedName);
            Assert.That(row["Name "], Is.EqualTo(new clsProcessValue(expectedName)), "Name incorrect for Id:{0}, Name:{1}", (object)expectedId, expectedName);
            Assert.That(row["Password"], Is.EqualTo(new clsProcessValue(expectedBand)), "Password incorrect for Id:{0}, Name:{1}", (object)expectedId, expectedName);
        }

        [Test(Description = "Tests that a collection stage handles a to/from XML round trip correctly")]
        public void TestXmlRoundTrip()
        {
            var stg = new clsCollectionStage(null)
            {
                Name = "new"
            };
            stg.AddField("ID", DataType.number);
            stg.AddField("Name ", DataType.text);
            stg.AddField("Password", DataType.password);
            var coll = new clsCollection();
            coll.CopyDefinition(stg.Definition);
            coll.Add(CreateNew80sBackgroundMemberRow(1, "Andrew Ridgeley", new SafeString("Wham!")));
            coll.Add(CreateNew80sBackgroundMemberRow(2, "Curt Smith", new SafeString("Tears For Fears")));
            coll.Add(CreateNew80sBackgroundMemberRow(3, "Vince Clarke", new SafeString("Erasure")));
            coll.Add(CreateNew80sBackgroundMemberRow(4, "Chris Lowe", new SafeString("Pet Shop Boys")));
            stg.SetInitialValue(new clsProcessValue(coll));
            var doc = new XmlDocument();
            var stgElem = doc.CreateElement("stage");
            stg.ToXml(doc, stgElem, false);
            var xml = stgElem.OuterXml;
            var newStg = new clsCollectionStage(null);
            newStg.FromXML(stgElem);
            var newColl = newStg.InitialCollection;
            Assert.That(newColl, Is.Not.Null);
            Assert.That(newColl.Rows, Has.Count.EqualTo((object)4));
            Assert.That(newColl.Definition, Is.Not.Null);
            Assert.That(newColl.Definition.GetField("ID"), Is.Not.Null);
            Assert.That(newColl.Definition.GetField("Name "), Is.Not.Null);
            Assert.That(newColl.Definition.GetField("Password"), Is.Not.Null);
            Check80sBackgroundMemberRow(newColl.Rows.ElementAt(0), 1, "Andrew Ridgeley", new SafeString("Wham!"));
            Check80sBackgroundMemberRow(newColl.Rows.ElementAt(1), 2, "Curt Smith", new SafeString("Tears For Fears"));
            Check80sBackgroundMemberRow(newColl.Rows.ElementAt(2), 3, "Vince Clarke", new SafeString("Erasure"));
            Check80sBackgroundMemberRow(newColl.Rows.ElementAt(3), 4, "Chris Lowe", new SafeString("Pet Shop Boys"));
        }

        [Test]
        public void XmlGeneration_SingleRow_GeneratedCorrectly()
        {
            var stg = new clsCollectionStage(null)
            {
                Name = "new"
            };
            stg.AddField("ID", DataType.number);
            stg.AddField("Name", DataType.text);
            stg.SingleRow = true;
            var coll = new clsCollection();
            coll.CopyDefinition(stg.Definition);
            coll.Add();
            coll.SetField("ID", 1);
            coll.SetField("Name", "Bob");
            stg.SetInitialValue(new clsProcessValue(coll));
            var doc = new XmlDocument();
            var stgElem = doc.CreateElement("stage");
            stg.ToXml(doc, stgElem, false);
            var xml = stgElem.OuterXml;
            var collectionInfo = stgElem.SelectSingleNode("collectioninfo").InnerXml;
            var initialValue = stgElem.SelectSingleNode("initialvalue").InnerXml;
            const string expectedCollInfo = "<singlerow /><field name=\"ID\" type=\"number\" /><field name=\"Name\" type=\"text\" />";
            const string expectedInitValue = "<singlerow /><row><field name=\"ID\" type=\"number\" value=\"1\" /><field name=\"Name\" type=\"text\" value=\"Bob\" /></row>";

            Assert.That(collectionInfo, Is.EqualTo(expectedCollInfo));
            Assert.That(initialValue, Is.EqualTo(expectedInitValue));
        }

        [Test]
        public void TestLoop_IteratesCorrectly()
        {
            var mock = new Mock<IGroupObjectDetails>().Object;
            var process = new clsProcess(mock, DiagramType.Process, false);
            var collectionStage = (clsCollectionStage)process.AddDataStage("Collection Stage", DataType.collection);
            collectionStage.Value.Collection = new clsCollection(XElement.Parse("<collection><row><field name=\"Id\" type=\"number\" value=\"1\"/></row><row><field name=\"Id\" type=\"number\" value=\"2\"/></row><row><field name=\"Id\" type=\"number\" value=\"3\"/></row></collection>").ToString());
            var resultStage = process.AddDataStage("Result", DataType.text);
            var stages = new Dictionary<Guid, clsProcessStage>();
            var endStage = AddEndStage(process);
            var endStageId = endStage.GetStageID();
            var groupId = Guid.NewGuid();
            var loopEnd = AddLoopEndStage(process, groupId);
            stages.Add(loopEnd.GetStageID(), loopEnd);
            var calcStage = AddCalculationStage(process, "[Result]&[Collection Stage.Id]", "Result");
            stages.Add(calcStage.GetStageID(), calcStage);
            var loopStart = AddLoopStartStage(process, "Collection Stage", groupId);
            stages.Add(loopStart.GetStageID(), loopStart);
            loopStart.OnSuccess = calcStage.GetStageID();
            calcStage.OnSuccess = loopEnd.GetStageID();
            loopEnd.OnSuccess = endStageId;
            var nextStage = loopStart.GetStageID();
            var logger = new CompoundLoggingEngine();
            while (!(nextStage == endStageId))
            {
                stages.First(a => a.Key == nextStage).Value.Execute(ref nextStage, logger);
            }

            Assert.AreEqual("123", resultStage.Value.FormattedValue);
        }

        [Test]
        public void TestLoop_NestedCollection_IteratesCorrectly()
        {
            var mock = new Mock<IGroupObjectDetails>().Object;
            var process = new clsProcess(mock, DiagramType.Process, false);
            var collectionStage = (clsCollectionStage)process.AddDataStage("Collection Stage", DataType.collection);
            collectionStage.Value.Collection = new clsCollection(XElement.Parse("<collection><row>\r\n                           <field name=\"Id\" type=\"number\" value=\"1\"/>\r\n                           <field name=\"Names\" type=\"collection\">\r\n                               <row><field name=\"Name\" type=\"text\" value=\"Amy\"/></row>\r\n                               <row><field name=\"Name\" type=\"text\" value=\"Belle\"/></row>\r\n                           </field>\r\n                       </row><row>\r\n                           <field name=\"Id\" type=\"number\" value=\"2\"/>\r\n                           <field name=\"Names\" type=\"collection\">\r\n                               <row><field name=\"Name\" type=\"text\" value=\"Andy\"/></row>\r\n                               <row><field name=\"Name\" type=\"text\" value=\"Bill\"/></row>\r\n                           </field>\r\n                       </row></collection>").ToString());
            var resultStage = process.AddDataStage("Result", DataType.text);
            var stages = new Dictionary<Guid, clsProcessStage>();
            var endStage = AddEndStage(process);
            var endStageId = endStage.GetStageID();
            var outerGroupId = Guid.NewGuid();
            var innerGroupId = Guid.NewGuid();
            var outerLoopEnd = AddLoopEndStage(process, outerGroupId);
            stages.Add(outerLoopEnd.GetStageID(), outerLoopEnd);
            var innerLoopEnd = AddLoopEndStage(process, innerGroupId);
            stages.Add(innerLoopEnd.GetStageID(), innerLoopEnd);
            var calcStage = AddCalculationStage(process, "[Result]&[Collection Stage.Names.Name]", "Result");
            stages.Add(calcStage.GetStageID(), calcStage);
            var innerLoopStart = AddLoopStartStage(process, "Collection Stage.Names", innerGroupId);
            stages.Add(innerLoopStart.GetStageID(), innerLoopStart);
            var outerLoopStart = AddLoopStartStage(process, "Collection Stage", outerGroupId);
            stages.Add(outerLoopStart.GetStageID(), outerLoopStart);
            outerLoopStart.OnSuccess = innerLoopStart.GetStageID();
            innerLoopStart.OnSuccess = calcStage.GetStageID();
            calcStage.OnSuccess = innerLoopEnd.GetStageID();
            innerLoopEnd.OnSuccess = outerLoopEnd.GetStageID();
            outerLoopEnd.OnSuccess = endStageId;
            var nextStage = outerLoopStart.GetStageID();
            var logger = new CompoundLoggingEngine();
            while (!(nextStage == endStageId))
            {
                stages.First(a => a.Key == nextStage).Value.Execute(ref nextStage, logger);
            }

            Assert.AreEqual("AmyBelleAndyBill", resultStage.Value.FormattedValue);
        }

        [Test]
        public void TestCalcStage_NestedCollection_StoresCorrectly()
        {
            var mock = new Mock<IGroupObjectDetails>().Object;
            var process = new clsProcess(mock, DiagramType.Process, false);
            var collectionStage = (clsCollectionStage)process.AddDataStage("Collection Stage", DataType.collection);
            collectionStage.Value.Collection = new clsCollection(XElement.Parse("<collection><row><field name=\"Id\" type=\"number\" value=\"1\"/></row><row><field name=\"Id\" type=\"number\" value=\"2\"/></row><row><field name=\"Id\" type=\"number\" value=\"3\"/></row></collection>").ToString());
            var resultStage = (clsCollectionStage)process.AddDataStage("Result", DataType.collection);
            resultStage.Value.Collection = new clsCollection(XElement.Parse("<collection><row>\r\n                               <field name=\"Items\" type=\"collection\">\r\n                                   <row>\r\n                                       <field name=\"Output\" type=\"text\"></field>\r\n                                   </row>\r\n                               </field>\r\n                           </row></collection>").ToString());
            var stages = new Dictionary<Guid, clsProcessStage>();
            var endStage = AddEndStage(process);
            var endStageId = endStage.GetStageID();
            var groupId = Guid.NewGuid();
            var loopEnd = AddLoopEndStage(process, groupId);
            stages.Add(loopEnd.GetStageID(), loopEnd);
            var calcStage = AddCalculationStage(process, "[Result.Items.Output]&[Collection Stage.Id]", "Result.Items.Output");
            stages.Add(calcStage.GetStageID(), calcStage);
            var loopStart = AddLoopStartStage(process, "Collection Stage", groupId);
            stages.Add(loopStart.GetStageID(), loopStart);
            loopStart.OnSuccess = calcStage.GetStageID();
            calcStage.OnSuccess = loopEnd.GetStageID();
            loopEnd.OnSuccess = endStageId;
            var nextStage = loopStart.GetStageID();
            var logger = new CompoundLoggingEngine();
            while (!(nextStage == endStageId))
            {
                stages.First(a => a.Key == nextStage).Value.Execute(ref nextStage, logger);
            }

            Assert.AreEqual(new clsProcessValue("123"), resultStage.CurrentCollection.GetField("Items").Collection.GetField("Output"));
        }

        private static clsCalculationStage AddCalculationStage(clsProcess process, string expression, string storeIn)
        {
            var calcStage = (clsCalculationStage)process.AddStage(StageTypes.Calculation, "Calc Stage");
            calcStage.Expression = BPExpression.FromNormalised(expression);
            calcStage.StoreIn = storeIn;
            calcStage.SetStageID(Guid.NewGuid());
            return calcStage;
        }

        private static clsLoopStartStage AddLoopStartStage(clsProcess process, string collectionName, Guid groupId)
        {
            var loopStart = (clsLoopStartStage)process.AddStage(StageTypes.LoopStart, "Loop Start");
            loopStart.LoopData = collectionName;
            loopStart.SetGroupID(groupId);
            loopStart.SetStageID(Guid.NewGuid());
            return loopStart;
        }

        private static clsLoopEndStage AddLoopEndStage(clsProcess process, Guid groupId)
        {
            var loopEnd = (clsLoopEndStage)process.AddStage(StageTypes.LoopEnd, "Loop End");
            loopEnd.SetGroupID(groupId);
            loopEnd.SetStageID(Guid.NewGuid());
            return loopEnd;
        }

        private static clsEndStage AddEndStage(clsProcess process)
        {
            var endStage = (clsEndStage)process.AddStage(StageTypes.End, "End");
            endStage.SetStageID(Guid.NewGuid());
            return endStage;
        }
    }
}
#endif
