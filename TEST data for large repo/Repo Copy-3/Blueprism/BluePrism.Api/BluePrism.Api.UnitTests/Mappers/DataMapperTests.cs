namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using BluePrism.Api.Mappers;
    using Common.Security;
    using CommonTestClasses.Extensions;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class DataMapperTests
    {
        [Test]
        [TestCaseSource(nameof(DataValueTypeMaps))]
        public void DataValueType_ToModel_ProducesExpectedResult(Domain.DataValueType mapFrom, Models.DataValueType expectedOutput)
        {
            mapFrom.ToModel().Should().Be(expectedOutput);
        }

        [Test]
        public void DataValueType_ToModel_OnInvalidValue_ShouldThrowArgumentException()
        {
            Action test = () => ((Domain.DataValueType)(-1)).ToModel();

            test.ShouldThrow<ArgumentException>();
        }

        [Test]
        [TestCaseSource(nameof(DataValueMaps))]
        public void DataValue_ToModel_ProducesExpectedResult(Domain.DataValue mapFrom, Models.DataValueModel expectedOutput)
        {
            mapFrom.ToModel().ShouldBeEquivalentTo(expectedOutput);
        }

        private static IEnumerable<TestCaseData> DataValueTypeMaps => new[]
        {
            (Domain.DataValueType.Binary, Models.DataValueType.Binary),
            (Domain.DataValueType.Collection, Models.DataValueType.Collection),
            (Domain.DataValueType.Date, Models.DataValueType.Date),
            (Domain.DataValueType.DateTime, Models.DataValueType.DateTime),
            (Domain.DataValueType.Flag, Models.DataValueType.Flag),
            (Domain.DataValueType.Image, Models.DataValueType.Image),
            (Domain.DataValueType.Number, Models.DataValueType.Number),
            (Domain.DataValueType.Password, Models.DataValueType.Password),
            (Domain.DataValueType.Text, Models.DataValueType.Text),
            (Domain.DataValueType.Time, Models.DataValueType.Time),
            (Domain.DataValueType.TimeSpan, Models.DataValueType.TimeSpan),
        }
        .ToTestCaseData();

        private static readonly DateTimeOffset TestDate =
            new DateTimeOffset(2020, 11, 17, 14, 25, 42, TimeSpan.FromHours(4));

        private static IEnumerable<TestCaseData> DataValueMaps => new []
            {
                (
                    new Domain.DataValue { ValueType = Domain.DataValueType.Binary, Value = new byte[] {0x01, 0x02, 0x03}},
                    new Models.DataValueModel { ValueType = Models.DataValueType.Binary, Value = new byte[] {0x01, 0x02, 0x03}}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.Date, Value = new DateTimeOffset(TestDate.Year, TestDate.Month, TestDate.Day, 0, 0, 0, TestDate.Offset)},
                    new Models.DataValueModel {ValueType = Models.DataValueType.Date, Value = new DateTimeOffset(TestDate.Year, TestDate.Month, TestDate.Day, 0, 0, 0, TestDate.Offset)}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.DateTime, Value = TestDate},
                    new Models.DataValueModel {ValueType = Models.DataValueType.DateTime, Value = TestDate}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.Flag, Value = true},
                    new Models.DataValueModel {ValueType = Models.DataValueType.Flag, Value = true}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.Image, Value = new byte[]{ 0x03, 0x02, 0x01}},
                    new Models.DataValueModel {ValueType = Models.DataValueType.Image, Value = Convert.ToBase64String( new byte[]{ 0x03, 0x02, 0x01})}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.Number, Value = 123.45},
                    new Models.DataValueModel {ValueType = Models.DataValueType.Number, Value = 123.45}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.Password, Value = "TestPassword".AsSecureString()},
                    new Models.DataValueModel {ValueType = Models.DataValueType.Password, Value = "TestPassword"}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.Text, Value = "This is a test"},
                    new Models.DataValueModel {ValueType = Models.DataValueType.Text, Value = "This is a test"}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.Time, Value = DateTimeOffset.MinValue.Add(TestDate.ToUniversalTime().TimeOfDay).ToOffset(TestDate.Offset)},
                    new Models.DataValueModel {ValueType = Models.DataValueType.Time, Value = DateTimeOffset.MinValue.Add(TestDate.ToUniversalTime().TimeOfDay).ToOffset(TestDate.Offset)}
                ),
                (
                    new Domain.DataValue {ValueType = Domain.DataValueType.TimeSpan, Value = TimeSpan.FromSeconds(12345)},
                    new Models.DataValueModel {ValueType = Models.DataValueType.TimeSpan, Value = TimeSpan.FromSeconds(12345)}
                ),
            }
            .ToTestCaseData();
    }
}
