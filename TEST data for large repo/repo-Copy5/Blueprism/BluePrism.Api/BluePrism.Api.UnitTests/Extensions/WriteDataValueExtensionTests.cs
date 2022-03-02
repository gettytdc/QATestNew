namespace BluePrism.Api.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using CommonTestClasses.Extensions;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;
    using Utilities.Testing;
    
    [TestFixture]
    public class WriteDataValueExtensionTests : UnitTestBase<WriteDataValueModel>
    {
        [Test]
        public void MapToType_ShouldHaveExpectedHasBindErrorValue_WhenNullArgumentPassedThrough(
            [Values] DataValueType valueType)
        {
            ClassUnderTest.Value = null;
            var mapToMethod = MapToTypeDictionary[valueType];

            var result = mapToMethod();

            if (valueType == DataValueType.Text)
            {
                result.HasBindError.Should().BeFalse();
            }
            else
            {
                result.HasBindError.Should().BeTrue();
            }
        }

        [TestCaseSource(nameof(InvalidFormatCases))]
        public void MapToType_ShouldHaveBindError_WhenFormatException(DataValueType valueType, string invalidFormatValue)
        {
            ClassUnderTest.Value = invalidFormatValue;
            var mapToMethod = MapToTypeDictionary[valueType];

            var result = mapToMethod();
            result.HasBindError.Should().BeTrue();
        }

        private Dictionary<DataValueType, Func<DataValueModel>> MapToTypeDictionary =>
            new Dictionary<DataValueType, Func<DataValueModel>>()
            {
                {DataValueType.Date, ClassUnderTest.MapToDate},
                {DataValueType.DateTime, ClassUnderTest.MapToDateTime},
                {DataValueType.Time, ClassUnderTest.MapToTime},
                {DataValueType.TimeSpan, ClassUnderTest.MapToTimeSpan},
                {DataValueType.Number, ClassUnderTest.MapToDecimal},
                {DataValueType.Flag, ClassUnderTest.MapToBoolean},
                {DataValueType.Binary, ClassUnderTest.MapToByteArray},
                {DataValueType.Text, ClassUnderTest.MapToText},
                {DataValueType.Password, ClassUnderTest.MapToPassword},
                {DataValueType.Collection, ClassUnderTest.MapToCollection},
                {DataValueType.Image, ClassUnderTest.MapToBitMap}
            };

        private static IEnumerable<TestCaseData> InvalidFormatCases() =>
            new[]
            {
                (DataValueType.Date, "testDate"),
                (DataValueType.DateTime, "testDateTime"),
                (DataValueType.Time, "testTime"),
                (DataValueType.TimeSpan, "testTimeSpan"),
                (DataValueType.Number, "testNumber"),
                (DataValueType.Flag, "testFlag"),
                (DataValueType.Binary, "testBinary"),
                (DataValueType.Password, string.Empty),
                (DataValueType.Image, "testImage"),
                (DataValueType.Collection, "testCollection"),
            }.ToTestCaseData();
    }
}
