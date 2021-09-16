using System;
using BluePrism.Core.Utility;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Utility
{
    [TestFixture]
    public class ObjectEncoderTests
    {
        [Serializable]
        public class TestObject
        {
            public int X { get; set; }
            public string Str { get; set; }
        }

        [Test]
        public void ObjectToBase64String_ConvertObjectToString()
        {
            var output = ObjectEncoder.ObjectToBase64String(new TestObject()
            {
                X = 100,
                Str = "Test"
            });
            Assert.IsNotNull(output);
        }

        [Test]
        public void ObjectToBase64String_NullParameterShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                TestObject testObject = null;
                ObjectEncoder.ObjectToBase64String(testObject);
            });
        }

        [Test]
        public void Base64StringToObject_NullParameterShouldThrow()
        {
            Assert.Throws<ArgumentException>(() =>
            {                
                ObjectEncoder.Base64StringToObject< TestObject>(null);
            });
        }

        [Test]
        public void Base64StringToObject_EmptyStringShouldThrow()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ObjectEncoder.Base64StringToObject<TestObject>(string.Empty);
            });
        }

        [Test]
        public void Base64StringToObject_ConvertKnownString()
        {
            var objString = "AAEAAAD/////AQAAAAAAAAAMAgAAAE9CbHVlUHJpc20uQ29yZS5Vbml0VGVzdHMsIFZlcnNpb249Ni4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsBQEAAAA+Qmx1ZVByaXNtLkNvcmUuVW5pdFRlc3RzLlV0aWxpdHkuT2JqZWN0RW5jb2RlclRlc3RzK1Rlc3RPYmplY3QCAAAAEjxYPmtfX0JhY2tpbmdGaWVsZBQ8U3RyPmtfX0JhY2tpbmdGaWVsZAABCAIAAABkAAAABgMAAAAEVGVzdAs=";
            var obj = ObjectEncoder.Base64StringToObject<TestObject>(objString);
            Assert.AreEqual(obj.X, 100);
            Assert.AreEqual(obj.Str, "Test");
        }


        [Test]
        public void ObjectToBase64String_Base64StringToObject()
        {
            var testObject = new TestObject()
            {
                X = 100,
                Str = "Test"
            };

            var input = ObjectEncoder.ObjectToBase64String(testObject);
            var output = ObjectEncoder.Base64StringToObject<TestObject>(input);
            Assert.AreEqual(testObject.X , output.X);
            Assert.AreEqual(testObject.Str, output.Str);
        }
    }
}
