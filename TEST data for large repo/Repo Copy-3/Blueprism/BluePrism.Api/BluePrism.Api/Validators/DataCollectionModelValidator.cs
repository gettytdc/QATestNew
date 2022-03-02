namespace BluePrism.Api.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Security;
    using FluentValidation;
    using Models;

    public class DataCollectionModelValidator : AbstractValidator<IReadOnlyDictionary<string, DataValueModel>>
    {
        private static readonly Dictionary<Type, Func<DataValueModel, bool>> DataTypeValidatorDictionary = new Dictionary<Type, Func<DataValueModel, bool>>()
        {
            {typeof(string), x => HasMatchingDataTypes(x.ValueType,DataValueType.Text)},
            {typeof(decimal), x => HasMatchingDataTypes(x.ValueType,DataValueType.Number)},
            {typeof(bool), x => HasMatchingDataTypes(x.ValueType,DataValueType.Flag)},
            {typeof(DateTime), x => HasMatchingDataTypes(x.ValueType,DataValueType.Date)},
            {typeof(DateTimeOffset), x => HasMatchingDataTypes(x.ValueType,DataValueType.Date, DataValueType.DateTime, DataValueType.Time)},
            {typeof(SecureString), x => HasMatchingDataTypes(x.ValueType,DataValueType.Password)},
            {typeof(byte[]), x => HasMatchingDataTypes(x.ValueType,DataValueType.Binary)},
            {typeof(TimeSpan), x => HasMatchingDataTypes(x.ValueType,DataValueType.TimeSpan)},
            {typeof(Bitmap), x => HasMatchingDataTypes(x.ValueType,DataValueType.Image)},
            {typeof(DataCollectionModel), x => HasMatchingDataTypes(x.ValueType,DataValueType.Collection)},
        };

        public DataCollectionModelValidator()
        {
            RuleForEach(x => x.Values)
                .Must(x => !x.HasBindError)
                .WithMessage(x => "Data Type bind error encountered");

            RuleForEach(x => x.Values)
                .Must(IsValidDataType)
                .WithMessage(x => "Data Type mismatch error encountered");

            When(x => x.Values.Any(), () =>
            {
                RuleForEach(x => x.Values)
                    .Must(IsNotEmptyCollection)
                    .WithMessage(x => "The request contains empty Data Collection items – Data Collection items for create work queue requests cannot be empty.");
            });

            RuleForEach(x => x.Keys)
                .Must(x => !string.IsNullOrEmpty(x))
                .WithMessage(x => "A key is required for the data value.");
        }

        private static bool IsValidDataType(DataValueModel dataValueModel)
        {
            if (dataValueModel.Value == null)
            {
                return false;
            }

            var dataType = dataValueModel.Value.GetType();
            if (!DataTypeValidatorDictionary.ContainsKey(dataType))
            {
                return false;
            }

            return DataTypeValidatorDictionary[dataType](dataValueModel);
        }

        private static bool IsNotEmptyCollection(DataValueModel dataValueModel)
        {
            if (!(dataValueModel.Value is DataCollectionModel collection))
            {
                return true;
            }

            return collection.Rows.All(row => row.Count != 0);
        }

        private static bool HasMatchingDataTypes(DataValueType modelDataValueType,params DataValueType[] expectedTypes) => expectedTypes.Any(x => x == modelDataValueType);
    }
}
