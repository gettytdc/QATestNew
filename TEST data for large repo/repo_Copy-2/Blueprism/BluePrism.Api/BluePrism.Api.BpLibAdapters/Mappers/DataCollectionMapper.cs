namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using AutomateProcessCore;
    using Common.Security;
    using Domain;

    public static class DataCollectionMapper
    {
        private static readonly Dictionary<DataValueType, Func<DataValue, clsProcessValue>> ProcessValueCreationDictionary = new Dictionary<DataValueType, Func<DataValue, clsProcessValue>>()
        {
            {DataValueType.Binary, x => CreateProcessValue(x.CastToByteArray())},
            {DataValueType.Collection, x => CreateProcessValue(x.CastToDataCollection().ToBluePrismObject())},
            {DataValueType.Date, x => CreateProcessValue(DataType.date, x.CastToDate())},
            {DataValueType.DateTime, x => CreateProcessValue(DataType.datetime, x.CastToDateTime())},
            {DataValueType.Time, x => CreateProcessValue(DataType.time, x.CastToTime())},
            {DataValueType.TimeSpan, x => CreateProcessValue(x.CastToTimespan())},
            {DataValueType.Flag, x => CreateProcessValue(x.CastToBool())},
            {DataValueType.Image, x => CreateProcessValue(x.CastToBitmap())},
            {DataValueType.Number, x => CreateProcessValue(x.CastToDecimal())},
            {DataValueType.Password, x => CreateProcessValue(x.CastToSafeString())},
            {DataValueType.Text, x => CreateProcessValue(x.CastToString())}
        };

        public static clsCollection ToBluePrismObject(this DataCollection @this)
        {
            var result = new clsCollection();
            if (@this?.Rows != null)
            {
                foreach (var row in @this.Rows)
                {
                    var collectionRow = new clsCollectionRow();
                    foreach (var keyValuePair in row)
                    {
                        collectionRow.Add(keyValuePair.Key, keyValuePair.Value.ToBluePrismObject());
                    }

                    result.Add(collectionRow);
                }
            }

            return result;
        }

        public static clsProcessValue ToBluePrismObject(this DataValue @this)
        {
            if (!ProcessValueCreationDictionary.ContainsKey(@this.ValueType))
            {
                throw new ArgumentException("Value was not of a recognized type", nameof(@this));
            }

            return ProcessValueCreationDictionary[@this.ValueType](@this);
        }

        private static byte[] CastToByteArray(this DataValue @this) => (byte[])@this.Value;

        private static string CastToString(this DataValue @this) => (string) @this.Value;

        private static DateTime CastToDate(this DataValue @this) => (DateTime)@this.Value;

        private static DateTime CastToDateTime(this DataValue @this) => ((DateTimeOffset) @this.Value).UtcDateTime;

        private static bool CastToBool(this DataValue @this) => (bool) @this.Value;

        private static decimal CastToDecimal(this DataValue @this) => Convert.ToDecimal(@this.Value);

        private static DateTime CastToTime(this DataValue @this) => ((DateTimeOffset)@this.Value).UtcDateTime;

        private static SafeString CastToSafeString(this DataValue @this) => new SafeString(@this.Value.ToString());

        private static Bitmap CastToBitmap(this DataValue @this) => (Bitmap)@this.Value;

        private static TimeSpan CastToTimespan(this DataValue @this) => (TimeSpan)@this.Value;

        private static DataCollection CastToDataCollection(this DataValue @this) => (DataCollection) @this.Value;
        
        private static clsProcessValue CreateProcessValue(byte[] value) =>
            new clsProcessValue(value);

        private static clsProcessValue CreateProcessValue(decimal value) =>
            new clsProcessValue(value);

        private static clsProcessValue CreateProcessValue(string value) =>
            new clsProcessValue(value);

        private static clsProcessValue CreateProcessValue(bool value) =>
            new clsProcessValue(value);

        private static clsProcessValue CreateProcessValue(SafeString value) =>
            new clsProcessValue(value);

        private static clsProcessValue CreateProcessValue(Bitmap value) =>
            new clsProcessValue(value);

        private static clsProcessValue CreateProcessValue(TimeSpan value) =>
            new clsProcessValue(value);
        
        private static clsProcessValue CreateProcessValue(clsCollection value) =>
            new clsProcessValue(value);

        private static clsProcessValue CreateProcessValue(DataType dataType, DateTime value) =>
            new clsProcessValue(dataType, value);
    }
}
