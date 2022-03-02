namespace BluePrism.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using Common.Security;

    public static class DataMapper
    {
        public static Models.DataCollectionModel ToModel(this Domain.DataCollection dataCollection) =>
            new Models.DataCollectionModel
            {
                Rows = dataCollection.Rows
                    .Select(ToModel)
                    .ToArray()
            };

        public static IReadOnlyDictionary<string, Models.DataValueModel> ToModel(this IReadOnlyDictionary<string, Domain.DataValue> row) =>
            row.ToDictionary(k => k.Key, v => v.Value.ToModel());

        public static Models.DataValueModel ToModel(this Domain.DataValue dataValue) =>
            new Models.DataValueModel
            {
                ValueType = dataValue.ValueType.ToModel(),
                Value = ValueToModel(dataValue),
            };

        private static object ValueToModel(Domain.DataValue dataValue)
        {
            if (dataValue.Value is string)
                return dataValue.Value;

            switch (dataValue.ValueType)
            {
                case Domain.DataValueType.Password:
                    return ((SecureString)dataValue.Value).AsString();
                case Domain.DataValueType.Collection:
                    return ((Domain.DataCollection)dataValue.Value).ToModel();
                case Domain.DataValueType.Image:
                    return GetBitMapData((byte[])dataValue.Value);
                default:
                    return dataValue.Value;
            }
        }

        public static string GetBitMapData(byte[] bitmapBytes) => bitmapBytes != null ? Convert.ToBase64String(bitmapBytes) : string.Empty;

        public static Models.DataValueType ToModel(this Domain.DataValueType dataValueType)
        {
            switch (dataValueType)
            {
                case Domain.DataValueType.Binary:
                    return Models.DataValueType.Binary;
                case Domain.DataValueType.Collection:
                    return Models.DataValueType.Collection;
                case Domain.DataValueType.Date:
                    return Models.DataValueType.Date;
                case Domain.DataValueType.DateTime:
                    return Models.DataValueType.DateTime;
                case Domain.DataValueType.Flag:
                    return Models.DataValueType.Flag;
                case Domain.DataValueType.Image:
                    return Models.DataValueType.Image;
                case Domain.DataValueType.Number:
                    return Models.DataValueType.Number;
                case Domain.DataValueType.Password:
                    return Models.DataValueType.Password;
                case Domain.DataValueType.Text:
                    return Models.DataValueType.Text;
                case Domain.DataValueType.Time:
                    return Models.DataValueType.Time;
                case Domain.DataValueType.TimeSpan:
                    return Models.DataValueType.TimeSpan;
                default:
                    throw new ArgumentException("Unexpected data value type", nameof(dataValueType));
            }
        }
    }
}
