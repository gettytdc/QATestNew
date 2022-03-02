namespace AutomateAppCore.UnitTests
{
    using BluePrism.AutomateAppCore;
    using BluePrism.Utilities.Testing;
    using BluePrism.UnitTesting;
    using NUnit.Framework;
    using System;

    [TestFixture]
    class TalkerTests : UnitTestBase<clsTalker>
    {
        protected override clsTalker TestClassConstructor() => new clsTalker(30);

        [SetUp]
        public void SetUp()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            base.Setup();
        }

        [Test]
        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("PING\r","PING")]
        [TestCase("PING\rPONG", "PING")]
        [TestCase("PING\n", "PING")]
        [TestCase("PING\nPONG", "PING")]
        [TestCase("PING\r\n", "PING")]
        [TestCase("PING\r\nPONG", "PING")]
        [TestCase("PING\rPONG\n", "PING")]
        [TestCase("PING\nPONG\r", "PING")]
        public void TestGetNextIncomingLine(string buffer, string expected)
        {
            ClassUnderTest.mInputBuffer = buffer;
            var result = ClassUnderTest.GetNextIncomingLine(TimeSpan.FromSeconds(1));

            Assert.That(result, Is.EqualTo(expected));
        }


        [Test]
        [TestCase("PING\r\n", "PONG\r", "PING", "PONG")]
        [TestCase("PING\r", "\nPONG\r", "PING", "PONG")]
        [TestCase("PING", "\r\nPONG\r", "", "PING")] //Because of the timeout we expect blank first reply
        public void TestGetNextIncomingLineParts(string part1, string part2,
                                                 string expected1, string expected2)
        {
            ClassUnderTest.mInputBuffer = part1;
            var result = ClassUnderTest.GetNextIncomingLine(TimeSpan.FromSeconds(1));

            Assert.That(result, Is.EqualTo(expected1));

            ClassUnderTest.mInputBuffer += part2;
            result = ClassUnderTest.GetNextIncomingLine(TimeSpan.FromSeconds(1));

            Assert.That(result, Is.EqualTo(expected2));
        }


        [Test]
        [TestCase("", null)]
        [TestCase(null, null)]
        [TestCase("PING\r", "PING")]
        [TestCase("PING\rPONG", "PING")]
        [TestCase("PING\n", "PING")]
        [TestCase("PING\nPONG", "PING")]
        [TestCase("PING\r\n", "PING")]
        [TestCase("PING\r\nPONG", "PING")]
        [TestCase("PING\rPONG\n", "PING")]
        [TestCase("PING\nPONG\r", "PING")]
        public void TestLineReader(string buffer, string expected)
        {
            var result = LineReader.ReadLine(ref buffer);

            Assert.That(result, Is.EqualTo(expected));
        }


        [Test]
        [TestCase("PING\r\n", "PONG\r", "PING", "PONG")]
        [TestCase("PING\r", "\nPONG\r", "PING", "PONG")]
        [TestCase("PING", "\r\nPONG\r", null, "PING")]
        [TestCase("\r\n", "\r\n", "", "")]
        [TestCase("HTTP\n", "\r\n", "HTTP", "")]
        public void TestLineReaderParts(string part1, string part2,
                                        string expected1, string expected2)
        {
            var buffer = part1;
            var result = LineReader.ReadLine(ref buffer);

            Assert.That(result, Is.EqualTo(expected1));

            buffer += part2;
            result = LineReader.ReadLine(ref buffer);

            Assert.That(result, Is.EqualTo(expected2));
        }

    }
}
