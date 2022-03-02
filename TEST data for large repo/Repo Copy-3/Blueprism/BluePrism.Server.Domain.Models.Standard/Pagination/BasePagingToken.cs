using System;
using System.Collections.Generic;

namespace BluePrism.Server.Domain.Models.Pagination
{
    public class BasePagingToken<T> : IComparable
    {
        public virtual T PreviousIdValue { get; set; }
        public string PreviousSortColumnValue { get; set; }
        public string DataType { get; set; }

        public object GetSqlPreviousSortColumnValue()
        {
            if (string.IsNullOrEmpty(PreviousSortColumnValue))
                return PreviousSortColumnValue;

            switch (DataType)
            {
                case "DateTimeOffset":
                case "TimeSpan":
                case "DateTime":
                    return PaginationValueTypeFormatter.GetObjectFromParsedString(DataType, PreviousSortColumnValue);
                case "Boolean":
                    return bool.Parse(PreviousSortColumnValue);
                case "Int16":
                case "Int32":
                case "Int64":
                    return int.Parse(PreviousSortColumnValue);
                default:
                    return PreviousSortColumnValue;
            }
        }

        public int CompareTo(BasePagingToken<T> other)
        {
            if (other == null)
                return 1;

            if (ReferenceEquals(this, other))
                return 0;

            var compare = string.Compare(PreviousSortColumnValue, other.PreviousSortColumnValue, StringComparison.Ordinal);
            if (compare != 0)
                return compare;

            compare = Comparer<T>.Default.Compare(PreviousIdValue, other.PreviousIdValue);
            if (compare != 0)
                return compare;

            compare = string.Compare(DataType, other.DataType, StringComparison.Ordinal);
            if (compare != 0)
                return compare;

            return 0;
        }

        public int CompareTo(object obj)
        {
            if (obj != null && obj.GetType() != GetType())
            {
                throw new ArgumentException($"Object must be of type {GetType()}");
            }
            return CompareTo((BasePagingToken<T>)obj);
        }

        public override bool Equals(object obj) =>
            obj is BasePagingToken<T> other
            && other.PreviousIdValue.Equals(PreviousIdValue)
            && other.DataType.Equals(DataType)
            && other.PreviousSortColumnValue.Equals(PreviousSortColumnValue);
    }
}
