#if UNITTESTS
using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AutomateAppCore.UnitTests.Sessions
{
    /// <summary>
    /// Test class for session variable parsing
    /// </summary>
    [TestFixture]
    public class SessionVariableTester
    {
        private IDictionary<string, clsSessionVariable> BuildTestMatrix()
        {
            var map = new Dictionary<string, clsSessionVariable>();
            var sv = new clsSessionVariable
            {
                // "[Data1] number "0" "hello""
                Name = "Data1",
                Value = new clsProcessValue(DataType.number, "0")
            };
            sv.Value.Description = "hello";
            map["[Data1] number \"0\" \"hello\""] = sv;

            // "[Time Running] timespan ""00:00:20"" ""The amount of \""time\"" it's been running"""
            sv = new clsSessionVariable
            {
                Name = "Time Running",
                Value = new clsProcessValue(DataType.timespan, "00:00:20")
            };
            sv.Value.Description = "The amount of \"time\" it's been running";
            map[@"[Time Running] timespan ""00:00:20"" ""The amount of \""time\"" it's been running"""] = sv;

            // "[Counter] number "2004" "The number of times we've (new line...)
            // been round the loop"
            sv = new clsSessionVariable
            {
                Name = "Counter",
                Value = new clsProcessValue(DataType.number, "2004")
            };
            sv.Value.Description = @"The number of times we've (new line...)\r\nbeen round the loop";
            map[@"[Counter] number ""2004"" ""The number of times we've (new line...)\r\nbeen round the loop"""] = sv;

            // "[Stop Me] flag ""False"" ""Set this\r\nto True\r\nto \""make\""\r\nthe process\r\n\""stop\"""
            sv = new clsSessionVariable
            {
                Name = "Stop Me",
                Value = new clsProcessValue(false)
                {
                    Description = string.Format("Set this{0}to True{0}to \"make\"{0}the process{0}\"stop\"", Environment.NewLine)
                }
            };
            map[@"[Stop Me] flag ""False"" ""Set this\r\nto True\r\nto \""make\""\r\nthe process\r\n\""stop\"""] = sv;
            return map;
        }

        [Test]
        public void TestParsing()
        {
            var testMatrix = BuildTestMatrix();
            foreach (var input in testMatrix.Keys)
            {
                var sv = clsSessionVariable.Parse(input);
                var expected = testMatrix[input];
                Assert.That(sv, Is.Not.Null);
                Assert.That(sv.Name, Is.EqualTo(expected.Name));
                Assert.That(sv.Value, Is.EqualTo(expected.Value));
            }
        }
    }
}
#endif