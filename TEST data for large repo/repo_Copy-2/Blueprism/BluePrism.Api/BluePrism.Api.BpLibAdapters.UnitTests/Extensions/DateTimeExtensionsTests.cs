namespace BluePrism.Api.BpLibAdapters.UnitTests.Extensions
{
    using System;
    using BpLibAdapters.Extensions;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;

    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [Test]
        public void ToUtc_ShouldReturnNullDateTimeOffset_WhenDateTimeOffsetOptionOfNoneMapped()
        {
            var noneDateTimeOffset = OptionHelper.None<DateTimeOffset>();
            var result = noneDateTimeOffset.ToUtc();
            result.Should().BeNull();
        }

        [Test]
        public void ToUtc_ShouldReturnDateTimeOffsetInUTC_WhenValidDateTimeOffsetMapped()
        {
            var expectedDate = new DateTime(2021, 1, 1, 0, 0, 0);
            var expectedTimeOffset = TimeSpan.Zero;
            var noneDateTimeOffset = OptionHelper.Some(new DateTimeOffset(expectedDate, new TimeSpan(0, 1, 0)));
            var result = noneDateTimeOffset.ToUtc();
            result.Should().NotBeNull();
            result.Value.DateTime.Should().Be(expectedDate);
            result.Value.Offset.Should().Be(expectedTimeOffset);
        }
    }
}
