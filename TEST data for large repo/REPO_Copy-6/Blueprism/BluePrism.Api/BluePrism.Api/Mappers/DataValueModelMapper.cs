namespace BluePrism.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using Common.Security;

    public static class DataValueModelMapper
    {
        public static Domain.DataCollection ToDomainModel(this Models.DataCollectionModel dataCollectionModel) =>
            new Domain.DataCollection
            {
                Rows = dataCollectionModel.Rows
                    .Select(ToDomainModel)
                    .ToArray()
            };

        public static IReadOnlyDictionary<string, Domain.DataValue> ToDomainModel(this IReadOnlyDictionary<string, Models.DataValueModel> row) =>
            row.ToDictionary(k => k.Key, v => v.Value.ToDomainModel());

        public static Domain.DataValue ToDomainModel(this Models.DataValueModel dataValue) =>
            new Domain.DataValue
            {
                ValueType = dataValue.ValueType.ToDomainModel(),
                Value = ValueToDomainModel(dataValue),
            };

        private static object ValueToDomainModel(Models.DataValueModel dataValue)
        {
            switch (dataValue.ValueType)
            {
                case Models.DataValueType.Password:
                    return ((SecureString)dataValue.Value);
                case Models.DataValueType.Collection:
                    return ((Models.DataCollectionModel)dataValue.Value).ToDomainModel();
                default:
                    return dataValue.Value;
            }
        }

        public static Domain.DataValueType ToDomainModel(this Models.DataValueType dataValueType)
        {
            switch (dataValueType)
            {
                case Models.DataValueType.Binary:
                    return Domain.DataValueType.Binary;
                case Models.DataValueType.Collection:
                    return Domain.DataValueType.Collection;
                case Models.DataValueType.Date:
                    return Domain.DataValueType.Date;
                case Models.DataValueType.DateTime:
                    return Domain.DataValueType.DateTime;
                case Models.DataValueType.Flag:
                    return Domain.DataValueType.Flag;
                case Models.DataValueType.Image:
                    return Domain.DataValueType.Image;
                case Models.DataValueType.Number:
                    return Domain.DataValueType.Number;
                case Models.DataValueType.Password:
                    return Domain.DataValueType.Password;
                case Models.DataValueType.Text:
                    return Domain.DataValueType.Text;
                case Models.DataValueType.Time:
                    return Domain.DataValueType.Time;
                case Models.DataValueType.TimeSpan:
                    return Domain.DataValueType.TimeSpan;
                default:
                    throw new ArgumentException("Unexpected data value type", nameof(dataValueType));
            }
        }
    }
}
