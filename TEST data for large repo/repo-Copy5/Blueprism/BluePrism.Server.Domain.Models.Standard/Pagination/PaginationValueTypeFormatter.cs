namespace BluePrism.Server.Domain.Models.Pagination
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Func;

    public static class PaginationValueTypeFormatter
    {
        private const string DateTimeFormatValue = "O";
        private const string TimeSpanFormatValue = "c";

        private static readonly IDictionary<string, Func<string, object>> ValidDataTypes =
            new Dictionary<string, Func<string, object>>
            {
                {"DateTimeOffset", v => DateTimeOffset.ParseExact(v, DateTimeFormatValue, CultureInfo.InvariantCulture)},
                {"DateTime", v => DateTime.ParseExact(v, DateTimeFormatValue, CultureInfo.InvariantCulture)},
                {"TimeSpan", v => TimeSpan.ParseExact(v, TimeSpanFormatValue, CultureInfo.InvariantCulture)},
            };

        public static object GetObjectFromParsedString(string dataType, string value) =>
            ValidDataTypes.TryGetValue(dataType, out var func)
                ? func(value)
                : throw new ArgumentException($"Invalid DataType for pagination token value {nameof(dataType)}");

        public static string GetStringValueFromObject(object obj)
        {
            switch (obj)
            {
                case Some<DateTimeOffset> x:
                    return x.Value.ToString(DateTimeFormatValue);
                case Some<DateTime> x:
                    return x.Value.ToString(DateTimeFormatValue);
                case DateTimeOffset y:
                    return y.ToString(DateTimeFormatValue);
                case DateTime y:
                    return y.ToString(DateTimeFormatValue);
                case Some<TimeSpan> t:
                    return t.Value.ToString(TimeSpanFormatValue);
                case TimeSpan ts:
                    return ts.ToString(TimeSpanFormatValue);
                case Enum enm:
                    return ((int)Enum.Parse(enm.GetType(), enm.ToString())).ToString();
                case None _:
                    return string.Empty;
                default:
                    return obj.ToString();
            }
        }
    }
}
