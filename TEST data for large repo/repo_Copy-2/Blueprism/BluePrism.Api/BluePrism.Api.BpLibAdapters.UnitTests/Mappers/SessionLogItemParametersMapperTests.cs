namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using BluePrism.Api.CommonTestClasses.Extensions;
    using Domain;
    using BpLibAdapters.Mappers;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class SessionLogItemParametersMapperTests
    {
        [Test]
        public void ToSessionLogItemParameters_ShouldReturnEmptyDataCollection_WhenStringIsEmpty()
        {
            var result = string.Empty.ToSessionLogItemParameters();
            result.Inputs.Should().BeEmpty();
            result.Outputs.Should().BeEmpty();
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldThrowArgumentException_WhenStringInvalidXml()
        {
            Action action = () => "xml".ToSessionLogItemParameters();
            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldReturnDataCollection_WhenXmlIsValid()
        {
            var result = "<parameters><inputs /><outputs /></parameters>".ToSessionLogItemParameters();
            result.Should().NotBeNull();
        }


        [Test]
        public void ToSessionLogItemParameters_ShouldReturnEmptyDataCollection_WhenInputsAndOutputsAreEmpty()
        {
            var result = "<parameters><inputs /><outputs /></parameters>".ToSessionLogItemParameters();
            result.Inputs.Should().BeEmpty();
            result.Outputs.Should().BeEmpty();
        }

        [TestCaseSource(nameof(SimpleDataTypeMaps))]
        public void ToSessionLogItemParameters_ShouldReturnExpectedInputs_WhenInputsContainsBasicItems(string type, string value, DataValue dataValue)
        {
            var xml =
                $@"<parameters><inputs><input name=""{type}"" type=""{type}"" value=""{value}"" /></inputs><outputs /></parameters>";

            var result = xml.ToSessionLogItemParameters();
            result.Inputs.Should().ContainSingle()
                .And.Subject.Single()
                .ShouldBeEquivalentTo(new KeyValuePair<string, DataValue>(type, dataValue));
        }

        [TestCaseSource(nameof(SimpleDataTypeMaps))]
        public void ToSessionLogItemParameters_ShouldReturnExpectedOutputs_WhenOutputsContainsBasicItems(string type, string value, DataValue dataValue)
        {
            var xml =
                $@"<parameters><outputs><output name=""{type}"" type=""{type}"" value=""{value}"" /></outputs><inputs /></parameters>";

            var result = xml.ToSessionLogItemParameters();
            result.Outputs.Should().ContainSingle()
                .And.Subject.Single()
                .ShouldBeEquivalentTo(new KeyValuePair<string, DataValue>(type, dataValue));
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldReturnExpectedInputs_WhenInputsContainsBasicCollection()
        {
            var result = BasicInputCollectionXml.ToSessionLogItemParameters();

            var expectedDataValue = new DataValue
            {
                ValueType = DataValueType.Collection,
                Value = new DataCollection
                {
                    Rows = new[]
                    {
                        new Dictionary<string, DataValue>
                        {
                            ["Field1"] = new DataValue { ValueType = DataValueType.Text, Value = "\"1\"" },
                            ["Field2"] = new DataValue { ValueType = DataValueType.Number, Value = 2M },
                        }
                    }
                }
            };

            result.Inputs.Should().ContainSingle()
                .And.Subject.Single()
                .ShouldBeEquivalentTo(new KeyValuePair<string, DataValue>("collectionName", expectedDataValue));
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldReturnExpectedInputs_WhenInputsContainsNestedCollection()
        {
            var result = NestedInputCollectionXml.ToSessionLogItemParameters();

            var expectedDataValue = new DataValue
            {
                ValueType = DataValueType.Collection,
                Value = GetExpectedDataCollection(3)
            };

            result.Inputs.Should().ContainSingle()
                .And.Subject.Single()
                .ShouldBeEquivalentTo(
                    new KeyValuePair<string, DataValue>("Data", expectedDataValue),
                    options => options.AllowingInfiniteRecursion());
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldReturnExpectedOutputs_WhenOutputsContainsBasicCollection()
        {
            var result = BasicOutputCollectionXml.ToSessionLogItemParameters();

            var expectedDataValue = new DataValue
            {
                ValueType = DataValueType.Collection,
                Value = new DataCollection
                {
                    Rows = new[]
                    {
                        new Dictionary<string, DataValue>
                        {
                            ["Field1"] = new DataValue { ValueType = DataValueType.Text, Value = "\"1\"" },
                            ["Field2"] = new DataValue { ValueType = DataValueType.Number, Value = 2M },
                        }
                    }
                }
            };

            result.Outputs.Should().ContainSingle()
                .And.Subject.Single()
                .ShouldBeEquivalentTo(new KeyValuePair<string, DataValue>("collectionName", expectedDataValue));
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldReturnExpectedOutputs_WhenOutputsContainsNestedCollection()
        {
            var result = NestedOutputCollectionXml.ToSessionLogItemParameters();

            var expectedDataValue = new DataValue
            {
                ValueType = DataValueType.Collection,
                Value = GetExpectedDataCollection(3)
            };

            result.Outputs.Should().ContainSingle()
                .And.Subject.Single()
                .ShouldBeEquivalentTo(
                    new KeyValuePair<string, DataValue>("Data", expectedDataValue),
                    options => options.AllowingInfiniteRecursion());
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldReturnExpectedInputs_WhenInputsContainAllBasicItems()
        {
            var dataTypeMaps = SimpleDataTypeMaps.Select(x =>
                (Type: x.Arguments[0].ToString(), Value: x.Arguments[1].ToString(), DataValue: x.Arguments[2] as DataValue))
                .ToArray();

            var xml = new XElement("parameters",
                    new XElement("inputs",
                        dataTypeMaps.Select(x => new XElement("input",
                            new XAttribute("name", x.Type + x.Value),
                            new XAttribute("type", x.Type),
                            new XAttribute("value", x.Value)))),
                    new XElement("outputs"))
                .ToString();

            var result = xml.ToSessionLogItemParameters();

            foreach (var (value, expected) in result.Inputs.Zip(dataTypeMaps, (v, e) => (v.Value, e.DataValue)))
            {
                value.ShouldBeEquivalentTo(expected);
            }
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldReturnEmptyInputs_WhenNoInputsSupplied()
        {
            var emptyInputsXml =
            "<parameters><outputs /></parameters>";

            var result = emptyInputsXml.ToSessionLogItemParameters();

            result.Inputs.Should().BeEmpty();
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldReturnEmptyOutputs_WhenNoOutputsSupplied()
        {
            var emptyOutputsXml =
            "<parameters><inputs /></parameters>";

            var result = emptyOutputsXml.ToSessionLogItemParameters();

            result.Outputs.Should().BeEmpty();
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldThrowArgumentException_WhenInvalidTypeParameterSupplied()
        {
            var xmlString = $@"<parameters><inputs><input name=""type"" type=""abc"" value=""value"" /></inputs><outputs /></parameters>";

            Action action = () => xmlString.ToSessionLogItemParameters();
            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldThrowArgumentNullException_WhenNoTypeParameterSupplied()
        {
            var xmlString = $@"<parameters><inputs><input name=""type"" value=""value"" /></inputs><outputs /></parameters>";

            Action action = () => xmlString.ToSessionLogItemParameters();
            action.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void ToSessionLogItemParameters_ShouldThrowArgumentNullException_WhenIncorrectTypeParameterSupplied()
        {
            var xmlString = $@"<parameters><inputs><input type=""text"" value=""value"" /></inputs><outputs /></parameters>";

            Action action = () => xmlString.ToSessionLogItemParameters();
            action.ShouldThrow<ArgumentNullException>();
        }

        private DataCollection GetExpectedDataCollection(int maxLevel, int currentLevel = 1)
        {
            var row = new Dictionary<string, DataValue>
            {
                [$"Level{currentLevel}"] = new DataValue { ValueType = DataValueType.Text, Value = $"Level {currentLevel}" }
            };

            if (currentLevel < maxLevel)
                row["Child"] = new DataValue { ValueType = DataValueType.Collection, Value = GetExpectedDataCollection(maxLevel, currentLevel + 1) };

            return new DataCollection
            {
                Rows = new[] { row }
            };
        }

        private const string BasicInputCollectionXml =
            "<parameters><inputs><input name=\"collectionName\" type=\"collection\"><row><field name=\"Field1\" type=\"text\" value=\"&quot;1&quot;\" /><field name=\"Field2\" type=\"number\" value=\"2\" /></row></input></inputs><outputs />\r\n</parameters>";

        private const string BasicOutputCollectionXml =
            "<parameters><outputs><output name=\"collectionName\" type=\"collection\"><row><field name=\"Field1\" type=\"text\" value=\"&quot;1&quot;\" /><field name=\"Field2\" type=\"number\" value=\"2\" /></row></output></outputs><inputs />\r\n</parameters>";

        private const string NestedInputCollectionXml =
        @"<parameters>
                    <inputs>
                        <input name=""Data"" type=""collection"">
                            <row>
                          <field name=""Level1"" type=""text"" value=""Level 1"" />
                          <field name=""Child"" type=""collection"" value=""&lt;collection&gt;&lt;row&gt;&lt;field name=&quot;Level2&quot; type=&quot;text&quot; value=&quot;Level 2&quot; /&gt;&lt;field name=&quot;Child&quot; type=&quot;collection&quot; value=&quot;&amp;lt;collection&amp;gt;&amp;lt;row&amp;gt;&amp;lt;field name=&amp;quot;Level3&amp;quot; type=&amp;quot;text&amp;quot; value=&amp;quot;Level 3&amp;quot; /&amp;gt;&amp;lt;/row&amp;gt;&amp;lt;/collection&amp;gt;&quot; /&gt;&lt;/row&gt;&lt;/collection&gt;"" />
                         </row>
                        </input>
                    </inputs>
                    <outputs><output name=""test"" type=""flag"" value=""True"" /></outputs>
                </parameters>";

        private const string NestedOutputCollectionXml =
            @"<parameters>
                    <outputs>
                        <output name=""Data"" type=""collection"">
                            <row>
                          <field name=""Level1"" type=""text"" value=""Level 1"" />
                          <field name=""Child"" type=""collection"" value=""&lt;collection&gt;&lt;row&gt;&lt;field name=&quot;Level2&quot; type=&quot;text&quot; value=&quot;Level 2&quot; /&gt;&lt;field name=&quot;Child&quot; type=&quot;collection&quot; value=&quot;&amp;lt;collection&amp;gt;&amp;lt;row&amp;gt;&amp;lt;field name=&amp;quot;Level3&amp;quot; type=&amp;quot;text&amp;quot; value=&amp;quot;Level 3&amp;quot; /&amp;gt;&amp;lt;/row&amp;gt;&amp;lt;/collection&amp;gt;&quot; /&gt;&lt;/row&gt;&lt;/collection&gt;"" />
                         </row>
                        </output>
                    </outputs>
                    <inputs />
                </parameters>";

        private static IEnumerable<TestCaseData> SimpleDataTypeMaps => new[]
        {
            ("text", "abc", new DataValue { ValueType = DataValueType.Text, Value = "abc" }),
            ("flag", "True", new DataValue { ValueType = DataValueType.Flag, Value = true }),
            ("flag", "", new DataValue { ValueType = DataValueType.Flag, Value = "" }),
            ("number", "5", new DataValue { ValueType = DataValueType.Number, Value = 5.0M }),
            ("password", "???????", new DataValue { ValueType = DataValueType.Password, Value = "???????" }),
            ("time", "00:45:20", new DataValue { ValueType = DataValueType.Time, Value = new DateTimeOffset(1, 1, 1, 0, 45, 20, TimeSpan.Zero)}),
            ("binary", Convert.ToBase64String(new byte[] { 0x01, 0x02, 0x03 }), new DataValue { ValueType = DataValueType.Binary, Value = new byte[] { 0x01, 0x02, 0x03 }}),
            ("date", "2020/08/20", new DataValue { ValueType = DataValueType.Date, Value = new DateTimeOffset(2020, 8, 20, 0, 0, 0, TimeSpan.Zero) }),
            ("datetime", "2020-08-20 12:34:56Z", new DataValue { ValueType = DataValueType.DateTime, Value = new DateTimeOffset(2020, 08, 20, 12, 34, 56, TimeSpan.Zero)}),
            ("timespan", "1.02:03:04", new DataValue { ValueType = DataValueType.TimeSpan, Value = new TimeSpan(1, 2, 3, 4)}),
            ("image", "320x240", new DataValue { ValueType = DataValueType.Image, Value = "320x240" })
        }.ToTestCaseData();
    }
}
