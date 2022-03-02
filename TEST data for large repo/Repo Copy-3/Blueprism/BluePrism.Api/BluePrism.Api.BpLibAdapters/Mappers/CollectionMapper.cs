namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using AutomateProcessCore;
    using Common.Security;
    using Extensions;
    
    public static class CollectionMapper
    {
        public static Domain.DataCollection ToDomainObject(this clsCollection collection) =>
            new Domain.DataCollection {Rows = collection.Rows.Select(x => x.ToDomainObject()).ToArray()};

        public static IReadOnlyDictionary<string, Domain.DataValue> ToDomainObject(this clsCollectionRow row) =>
            row.ToDictionary(k => k.Key, v => v.Value.ToDomainObject());

        public static Domain.DataValue ToDomainObject(this clsProcessValue value)
        {
            switch (value.DataType)
            {
                case DataType.binary:
                    return CreateDataValue(Domain.DataValueType.Binary, (byte[])value);
                case DataType.collection:
                    return CreateDataValue(Domain.DataValueType.Collection, value.Collection.ToDomainObject());
                case DataType.date:
                    return CreateDataValue(Domain.DataValueType.Date, ((DateTime)value).ToDateTimeOffset());
                case DataType.datetime:
                    return CreateDataValue(Domain.DataValueType.DateTime, ((DateTime)value).ToDateTimeOffset());
                case DataType.flag:
                    return CreateDataValue(Domain.DataValueType.Flag, (bool)value);
                case DataType.image:
                    return CreateDataValue(Domain.DataValueType.Image, GetBitmapData((Bitmap)value));
                case DataType.number:
                    return CreateDataValue(Domain.DataValueType.Number, (decimal)value);
                case DataType.password:
                    return CreateDataValue(Domain.DataValueType.Password, ((SafeString)value).SecureString);
                case DataType.text:
                    return CreateDataValue(Domain.DataValueType.Text, (string)value);
                case DataType.time:
                    return CreateDataValue(Domain.DataValueType.Time, ((DateTime)value).ToTimeOffset());
                case DataType.timespan:
                    return CreateDataValue(Domain.DataValueType.TimeSpan, (TimeSpan)value);
                default:
                    throw new ArgumentException("Value was not of a recognized type", nameof(value));
            }
        }

        private static DateTimeOffset ToTimeOffset(this DateTime time) =>
            time.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime
                ? DateTimeOffset.MinValue
                : new DateTimeOffset(1, 1, 1, time.Hour, time.Minute, time.Second, time.Millisecond,
                    ((DateTimeOffset)time).Offset);

        private static byte[] GetBitmapData(Bitmap bitmap)
        {
            try
            {
                if (bitmap == null)
                    return null;

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return stream.GetBuffer();
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Unable to get bitmap data");
            }
        }

        private static Domain.DataValue CreateDataValue(Domain.DataValueType valueType, object value) =>
            new Domain.DataValue {ValueType = valueType, Value = value,};
    }
}
