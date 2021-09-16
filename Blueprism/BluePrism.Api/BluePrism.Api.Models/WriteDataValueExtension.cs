namespace BluePrism.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Security;
    using Newtonsoft.Json;

    public static class WriteDataValueExtension
    {
        public static DataValueModel MapToDate(this WriteDataValueModel model)
        {
            var hasBindError = !DateTime.TryParse(model.Value, out var result);

            if (!hasBindError)
            {
                result = new DateTime(result.Year, result.Month, result.Day);
            }

            return new DataValueModel
            {
                HasBindError = hasBindError,
                ValueType = model.ValueType,
                Value = result
            };
        }

        public static DataValueModel MapToDateTime(this WriteDataValueModel model)
        {
            var hasBindError = !DateTimeOffset.TryParse(model.Value, out var result);

            return new DataValueModel()
            {
                HasBindError = hasBindError,
                ValueType = model.ValueType,
                Value = result
            };
        }

        public static DataValueModel MapToTime(this WriteDataValueModel model)
        {
            var hasBindError = !DateTimeOffset.TryParse(model.Value, out var result);

            if (!hasBindError)
            {
                result = new DateTimeOffset(2, 1, 1, result.Hour, result.Minute, result.Second, result.Offset);
            }

            return new DataValueModel()
            {
                HasBindError = hasBindError,
                ValueType = model.ValueType,
                Value = result
            };
        }

        public static DataValueModel MapToByteArray(this WriteDataValueModel model)
        {
            var hasBindError = false;
            byte[] result = null;
            try
            {
                result = Convert.FromBase64String(model.Value);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                hasBindError = true;
            }

            return new DataValueModel()
            {
                HasBindError = hasBindError,
                ValueType = model.ValueType,
                Value = result
            };
        }

        public static DataValueModel MapToBoolean(this WriteDataValueModel model) =>
            new DataValueModel
            {
                HasBindError = !bool.TryParse(model.Value, out var result),
                ValueType = model.ValueType,
                Value = result
            };

        public static DataValueModel MapToDecimal(this WriteDataValueModel model) =>
            new DataValueModel
            {
                HasBindError = !decimal.TryParse(model.Value, out var result),
                ValueType = model.ValueType,
                Value = result
            };

        public static DataValueModel MapToText(this WriteDataValueModel model) =>
            new DataValueModel
            {
                ValueType = model.ValueType,
                Value = string.IsNullOrEmpty(model.Value) ? string.Empty : model.Value
            };

        public static DataValueModel MapToTimeSpan(this WriteDataValueModel model)
        {
            var hasBindError = !TimeSpan.TryParse(model.Value, out var result);

            return new DataValueModel()
            {
                HasBindError = hasBindError,
                ValueType = model.ValueType,
                Value = result
            };
        }

        public static DataValueModel MapToPassword(this WriteDataValueModel model)
        {
            var hasBindError = string.IsNullOrEmpty(model.Value);
            var result = new SecureString();
            if (!hasBindError)
            {
                result = ToSecureString(model.Value);
            }

            return new DataValueModel
            {
                HasBindError = hasBindError,
                ValueType = model.ValueType,
                Value = result
            };
        }

        private static SecureString ToSecureString(string plainString)
        {
            if (string.IsNullOrEmpty(plainString))
            {
                return null;
            }

            var secureString = new SecureString();
            foreach (var c in plainString)
            {
                secureString.AppendChar(c);
            }

            return secureString;
        }

        public static DataValueModel MapToBitMap(this WriteDataValueModel model)
        {
            var hasBindError = false;
            Bitmap result = null;
            try
            {
                var bytes = Convert.FromBase64String(model.Value);
                using (var stream = new MemoryStream(bytes))
                {
                    result = (Bitmap)Image.FromStream(stream);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                hasBindError = true;
            }
            return new DataValueModel
            {
                HasBindError = hasBindError,
                ValueType = model.ValueType,
                Value = result
            };
        }

        public static DataValueModel MapToCollection(this WriteDataValueModel row)
        {
            var hasBindError = false;
            WriteDataCollectionModel deserializeResult = null;
            try
            {
                deserializeResult = JsonConvert.DeserializeObject<WriteDataCollectionModel>(row.Value);
            }
            catch
            {
                hasBindError = true;
            }

            if (!hasBindError && deserializeResult.Rows != null)
            {
                return MapToCollectionRecursive(deserializeResult);
            }

            
            return new DataValueModel
            {
                HasBindError = true,
                ValueType = row.ValueType,
            };
        }

        private static DataValueModel MapToCollectionRecursive(WriteDataCollectionModel dataCollection)
        {
            var dataValueModels = new Dictionary<string, DataValueModel>();
            foreach (var rowData in dataCollection.Rows)
            {
                foreach (var value in rowData)
                {
                    if (value.Value.ValueType == DataValueType.Collection)
                    {   
                        var writeDataCollectionModel  = JsonConvert.DeserializeObject<WriteDataCollectionModel>(value.Value.Value);
                        var mappedCollectionDataValueModel = MapToCollectionRecursive(writeDataCollectionModel);
                        dataValueModels.Add(value.Key, mappedCollectionDataValueModel);
                    }
                    else
                    {
                        
                        var mappedDataRow = MapFromDictionary(value);
                        dataValueModels.Add(value.Key, mappedDataRow);
                    }
                }
            }
            
            var dataCollectionRows = new List<IReadOnlyDictionary<string, DataValueModel>>()
            {
                dataValueModels
            };
            var dataCollectionModel = new DataCollectionModel
            {
                Rows = dataCollectionRows
            };
            var result = new DataValueModel
            {
                Value = dataCollectionModel,
                ValueType = DataValueType.Collection,
                HasBindError = false
            };

            return result;
        }

        private static DataValueModel MapFromDictionary(KeyValuePair<string, WriteDataValueModel> writeDataValueModel)
        {
            switch (writeDataValueModel.Value.ValueType)
            {
                case DataValueType.Date:
                    return writeDataValueModel.Value.MapToDate();
                case DataValueType.DateTime:
                    return writeDataValueModel.Value.MapToDateTime();
                case DataValueType.Time:
                    return writeDataValueModel.Value.MapToTime();
                case DataValueType.Flag:
                    return  writeDataValueModel.Value.MapToBoolean();
                case DataValueType.Binary:
                    return writeDataValueModel.Value.MapToByteArray();
                case DataValueType.Number:
                    return writeDataValueModel.Value.MapToDecimal();
                case DataValueType.Text:
                    return writeDataValueModel.Value.MapToText();
                case DataValueType.Password:
                    return writeDataValueModel.Value.MapToPassword();
                case DataValueType.TimeSpan:
                    return writeDataValueModel.Value.MapToTimeSpan();
                case DataValueType.Image:
                    return writeDataValueModel.Value.MapToBitMap();
                default:
                    throw new ArgumentException("Value was not of a recognized type", nameof(writeDataValueModel));
            }
        }

    }
}
