namespace BluePrism.Server.Core.UnitTests
{
    using Autofac;
    using BluePrism.Data;
    using BluePrism.Utilities.Functional;
    using BluePrism.Utilities.Testing;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Data;
    using System.Linq;

    [TestFixture]
    public class SqlHelperTests : UnitTestBase<SqlHelper>
    {
        public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.Register<Func<string, IDbCommand>>(ctx => _ => GetMock<IDbCommand>().Object);
            });

            var commandMock = GetMock<IDbCommand>();
            commandMock.SetupGet(m => m.Parameters).Returns(() => GetMock<IDataParameterCollection>().Object);
            commandMock.Setup(m => m.CreateParameter()).Returns(() => GetMock<IDbDataParameter>().Object);
        }

        [Test]
        public void SelectMultipleIds_WithNoPlaceholder_ThrowsArgumentException()
        {
            var connectionMock = GetMock<IDatabaseConnection>();

            Action test = () => ClassUnderTest.SelectMultipleIds(connectionMock.Object, new[] { 1, 2 }, _ => { }, "This query doesn't have the placeholder in it");
            test.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void SelectMultipleIds_WithValidInput_CallsSelectorForEachReturnedItem()
        {
            var connectionMock = GetMock<IDatabaseConnection>();
            connectionMock
                .Setup(m => m.ExecuteReturnDataReader(It.IsAny<IDbCommand>()))
                .Returns(() => GetMock<IDataReader>().Object);

            GetMock<IDataReader>().SetupSequence(m => m.Read()).Returns(true).Returns(true).Returns(false);

            var commandMock = GetMock<IDbCommand>();
            commandMock.SetupGet(m => m.CommandText).Returns("Initial text");

            var parametersMock = GetMock<IDataParameterCollection>();
            parametersMock.SetupGet(m => m.Count).Returns(0);

            var callCount = 0;
            ClassUnderTest.SelectMultipleIds(connectionMock.Object, new[] { 1, 2 }, _ => { ++callCount; }, "select * from notarealtable where test in ({multiple-ids})");

            callCount.Should().Be(2);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(10)]
        [TestCase(2010)]
        public void SelectMultipleIds_WithValidInput_CreatesExpectedCommandString(int parameterCount)
        {
            var connectionMock = GetMock<IDatabaseConnection>();
            connectionMock
                .Setup(m => m.ExecuteReturnDataReader(It.IsAny<IDbCommand>()))
                .Returns(() => GetMock<IDataReader>().Object);

            GetMock<IDataReader>().SetupSequence(m => m.Read()).Returns(true).Returns(true).Returns(false);

            var commandMock = GetMock<IDbCommand>();
            commandMock.SetupGet(m => m.CommandText).Returns("Initial text");

            var parametersMock = GetMock<IDataParameterCollection>();
            parametersMock.SetupGet(m => m.Count).Returns(0);

            ClassUnderTest.SelectMultipleIds(connectionMock.Object, Enumerable.Range(1, parameterCount).ToArray(), _ => { }, "select * from notarealtable where test in ({multiple-ids})");

            var expectedResult = $"select * from notarealtable where test in ({Enumerable.Range(0, Math.Min(parameterCount, 2000)).Select(x => $"@id{x}").Map(string.Join, ",")})";

            commandMock.VerifySet(m => m.CommandText = expectedResult, Times.Once);
        }

        [Test]
        public void SelectMultipleIds_WithValidInput_SetsParameterValues()
        {
            var connectionMock = GetMock<IDatabaseConnection>();
            connectionMock
                .Setup(m => m.ExecuteReturnDataReader(It.IsAny<IDbCommand>()))
                .Returns(() => GetMock<IDataReader>().Object);

            GetMock<IDataReader>().SetupSequence(m => m.Read()).Returns(true).Returns(true).Returns(false);

            var commandMock = GetMock<IDbCommand>();
            commandMock.SetupGet(m => m.CommandText).Returns("Initial text");

            var parametersMock = GetMock<IDataParameterCollection>();
            parametersMock.SetupGet(m => m.Count).Returns(0);

            ClassUnderTest.SelectMultipleIds(connectionMock.Object, new[] { 1, 2 }, _ => { }, "select * from notarealtable where test in ({multiple-ids})");

            var parameterMock = GetMock<IDbDataParameter>();
            parameterMock.VerifySet(x => x.Value = 1, Times.Once);
            parameterMock.VerifySet(x => x.Value = 2, Times.Once);
            parameterMock.VerifySet(x => x.Value = 3, Times.Never);
        }
    }
}
