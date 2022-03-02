namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using Api.Mappers;
    using Common.Security;
    using CommonTestClasses.Extensions;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class DataValueModelMapperTest
    {
        [Test]
        [TestCaseSource(nameof(DataValueModelTypeMaps))]
        public void DataValueModelType_ToDomain_ProducesExpectedResult(Models.DataValueType mapFrom,
            Domain.DataValueType expectedOutput) =>
            mapFrom.ToDomainModel().Should().Be(expectedOutput);

        [Test]
        public void DataValueType_ToModel_OnInvalidValue_ShouldThrowArgumentException()
        {
            Action test = () => ((Models.DataValueType)(-1)).ToDomainModel();

            test.ShouldThrow<ArgumentException>();
        }

        [Test]
        [TestCaseSource(nameof(DataValueModelMaps))]
        public void DataValue_ToModel_ProducesExpectedResult(Models.DataValueModel mapFrom, Domain.DataValue expectedOutput) =>
            mapFrom.ToDomainModel().ShouldBeEquivalentTo(expectedOutput);

        private static IEnumerable<TestCaseData> DataValueModelTypeMaps => new[]
            {
                (Models.DataValueType.Binary, Domain.DataValueType.Binary),
                (Models.DataValueType.Collection, Domain.DataValueType.Collection),
                (Models.DataValueType.Date, Domain.DataValueType.Date),
                (Models.DataValueType.DateTime, Domain.DataValueType.DateTime),
                (Models.DataValueType.Flag, Domain.DataValueType.Flag),
                (Models.DataValueType.Image, Domain.DataValueType.Image),
                (Models.DataValueType.Number, Domain.DataValueType.Number),
                (Models.DataValueType.Password, Domain.DataValueType.Password),
                (Models.DataValueType.Text, Domain.DataValueType.Text),
                (Models.DataValueType.Time, Domain.DataValueType.Time),
                (Models.DataValueType.TimeSpan, Domain.DataValueType.TimeSpan),
            }
            .ToTestCaseData();

        private static readonly DateTimeOffset TestDate =
            new DateTimeOffset(2020, 11, 17, 14, 25, 42, TimeSpan.FromHours(4));

        private static IEnumerable<TestCaseData> DataValueModelMaps => new[]
            {
                (
                    new Models.DataValueModel
                    {
                        ValueType = Models.DataValueType.Binary, Value = new byte[] {0x01, 0x02, 0x03}
                    },
                    new Domain.DataValue
                    {
                        ValueType = Domain.DataValueType.Binary, Value = new byte[] {0x01, 0x02, 0x03}
                    }
                ),
                (
                    new Models.DataValueModel
                    {
                        ValueType = Models.DataValueType.Date,
                        Value = new DateTimeOffset(TestDate.Year, TestDate.Month, TestDate.Day, 0, 0, 0,
                            TestDate.Offset)
                    },
                    new Domain.DataValue
                    {
                        ValueType = Domain.DataValueType.Date,
                        Value = new DateTimeOffset(TestDate.Year, TestDate.Month, TestDate.Day, 0, 0, 0,
                            TestDate.Offset)
                    }
                ),
                (
                    new Models.DataValueModel {ValueType = Models.DataValueType.DateTime, Value = TestDate},
                    new Domain.DataValue {ValueType = Domain.DataValueType.DateTime, Value = TestDate}
                ),
                (
                    new Models.DataValueModel {ValueType = Models.DataValueType.Flag, Value = true},
                    new Domain.DataValue {ValueType = Domain.DataValueType.Flag, Value = true}
                ),
                (
                    new Models.DataValueModel
                    {
                        ValueType = Models.DataValueType.Image,
                        Value = Convert.ToBase64String(new byte[] {0x03, 0x02, 0x01})
                    },
                    new Domain.DataValue
                    {
                        ValueType = Domain.DataValueType.Image, Value = Convert.ToBase64String(new byte[] {0x03, 0x02, 0x01})
                    }
                ),
                (
                    new Models.DataValueModel {ValueType = Models.DataValueType.Number, Value = 123.45},
                    new Domain.DataValue {ValueType = Domain.DataValueType.Number, Value = 123.45}
                ),
                (
                    new Models.DataValueModel
                    {
                        ValueType = Models.DataValueType.Password, Value = "TestPassword".AsSecureString()
                    },
                    new Domain.DataValue
                    {
                        ValueType = Domain.DataValueType.Password, Value = "TestPassword".AsSecureString()
                    }
                ),
                (
                    new Models.DataValueModel {ValueType = Models.DataValueType.Text, Value = "This is a test"},
                    new Domain.DataValue {ValueType = Domain.DataValueType.Text, Value = "This is a test"}
                ),
                (
                    new Models.DataValueModel
                    {
                        ValueType = Models.DataValueType.Time,
                        Value = DateTimeOffset.MinValue.Add(TestDate.ToUniversalTime().TimeOfDay)
                            .ToOffset(TestDate.Offset)
                    },
                    new Domain.DataValue
                    {
                        ValueType = Domain.DataValueType.Time,
                        Value = DateTimeOffset.MinValue.Add(TestDate.ToUniversalTime().TimeOfDay)
                            .ToOffset(TestDate.Offset)
                    }
                ),
                (
                    new Models.DataValueModel
                    {
                        ValueType = Models.DataValueType.TimeSpan, Value = TimeSpan.FromSeconds(12345)
                    },
                    new Domain.DataValue
                    {
                        ValueType = Domain.DataValueType.TimeSpan, Value = TimeSpan.FromSeconds(12345)
                    }
                ),
            }.ToTestCaseData();
    }
}
