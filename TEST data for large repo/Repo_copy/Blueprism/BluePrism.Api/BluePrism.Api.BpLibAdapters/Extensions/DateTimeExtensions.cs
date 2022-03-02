namespace BluePrism.Api.BpLibAdapters.Extensions
{
    using System;
    using Func;

    public static class DateTimeExtensions
    {
        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime) =>
            dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime
                ? DateTimeOffset.MinValue
                : new DateTimeOffset(dateTime);

        public static DateTimeOffset? ToUtc(this Option<DateTimeOffset> dateTimeOffsetOption) =>
            dateTimeOffsetOption is Some<DateTimeOffset> dateTimeOffset ?
                DateTime.SpecifyKind(dateTimeOffset.Value.DateTime, DateTimeKind.Utc) :
                (DateTimeOffset?)null;
    }
}
