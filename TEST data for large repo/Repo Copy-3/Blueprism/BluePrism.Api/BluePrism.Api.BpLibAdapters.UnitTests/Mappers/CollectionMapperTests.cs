namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;

    using AutomateProcessCore;
    using BluePrism.Api.BpLibAdapters.Mappers;
    using Common.Security;
    using CommonTestClasses.Extensions;
    using Domain;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class CollectionMapperTests
    {
        [Test]
        [TestCaseSource(nameof(DataTypeMaps))]
        public void ProcessValue_ToDomainObject_ProducesExpectedResult(DataType dataType, string value,
            DataValue expectedOutput)
        {
            clsProcessValue.Decode(dataType, value).ToDomainObject().ShouldBeEquivalentTo(expectedOutput);
        }

        [Test]
        public void ProcessValue_ToDomainObject_WithSafeString_ProducesExpectedResult()
        {
            var expectedOutput =
                new DataValue {ValueType = DataValueType.Password, Value = "TestPassword".AsSecureString()};

            new clsProcessValue(new SafeString("TestPassword")).ToDomainObject()
                .ShouldBeEquivalentTo(expectedOutput);
        }

        [Test]
        public void ProcessValue_ToDomainObject_OnInvalidValue_ShouldThrowArgumentException()
        {
            Action test = () => new clsProcessValue {DataType = (DataType)(-1)}.ToDomainObject();

            test.ShouldThrow<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> DataTypeMaps => new[]
            {
                (DataType.binary, Convert.ToBase64String(new byte[] {0x01, 0x02, 0x03}),
                    new DataValue {ValueType = DataValueType.Binary, Value = new byte[] {0x01, 0x02, 0x03}}),
                (DataType.date, "2020/08/20",
                    new DataValue
                    {
                        ValueType = DataValueType.Date,
                        Value = new DateTimeOffset(2020, 8, 20, 0, 0, 0, TimeSpan.Zero)
                    }),
                (DataType.datetime, "2020-08-20 12:34:56Z",
                    new DataValue
                    {
                        ValueType = DataValueType.DateTime,
                        Value = new DateTimeOffset(2020, 08, 20, 12, 34, 56, TimeSpan.Zero)
                    }),
                (DataType.flag, "True", new DataValue {ValueType = DataValueType.Flag, Value = true}),
                (DataType.number, "123.45", new DataValue {ValueType = DataValueType.Number, Value = 123.45}),
                (DataType.text, "This is a test",
                    new DataValue {ValueType = DataValueType.Text, Value = "This is a test"}),
                (DataType.time, "12:34:56",
                    new DataValue
                    {
                        ValueType = DataValueType.Time,
                        Value = new DateTimeOffset(1, 1, 1, 12, 34, 56, TimeSpan.Zero)
                    }),
                (DataType.timespan, "1.02:03:04",
                    new DataValue {ValueType = DataValueType.TimeSpan, Value = new TimeSpan(1, 2, 3, 4)})
            }
            .ToTestCaseData();

    }
}
