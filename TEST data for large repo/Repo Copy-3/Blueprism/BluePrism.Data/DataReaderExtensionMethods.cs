namespace BluePrism.Data
{
    using System;
    using System.Data;
    using Utilities.Functional;

    public static class DataReaderExtensionMethods
    {
        public static T Get<T>(this IDataReader @this, string name) =>
            @this[name].Map(GetDefaultIfDbNull<T>);

        public static T Get<T>(this IDataReader @this, int ordinal) =>
            @this[ordinal].Map(GetDefaultIfDbNull<T>);

        private static T GetDefaultIfDbNull<T>(object value) =>
            value == DBNull.Value ? default(T) : (T) value;
    }
}