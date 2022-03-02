namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using AutomateProcessCore;
    using Domain;

    public static class SessionLogItemParametersMapper
    {
        public static SessionLogItemParameters ToSessionLogItemParameters(this string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return new SessionLogItemParameters()
                {
                    Inputs = new Dictionary<string, DataValue>(),
                    Outputs = new Dictionary<string, DataValue>()
                };
            }
            return ParseParametersXml(xml);
        }

        private static SessionLogItemParameters ParseParametersXml(string xml)
        {
            var xmldoc = ParseXDocumentOrThrow(xml);

            return new SessionLogItemParameters
            {
                Inputs = ParseValues(xmldoc.Root, "in"),
                Outputs = ParseValues(xmldoc.Root, "out")
            };
        }

        private static XDocument ParseXDocumentOrThrow(string xml)
        {
            try
            {
                return XDocument.Parse(xml);
            }
            catch (XmlException)
            {
                throw new ArgumentException("Invalid XML provided");
            }
        }

        private static DataValue ParseDataValue(XElement element)
        {
            var dataType = ParseDataTypeOrThrow(element);
            var stringValue = element.Attribute("value")?.Value;

            switch (dataType)
            {
                case DataType.collection:
                    return string.IsNullOrEmpty(stringValue) ? CreateCollection(element) : CreateNestedCollection(stringValue);
                case DataType.image:
                    return CreateImage(stringValue);
                case DataType.password:
                    return CreatePassword(stringValue);
                case DataType.flag:
                    return string.IsNullOrEmpty(stringValue) ? CreateEmptyFlag() : clsProcessValue.Decode(dataType, stringValue).ToDomainObject();
                default:
                    return clsProcessValue.Decode(dataType, stringValue).ToDomainObject();
            }
        }

        private static DataValue CreateImage(string stringValue) =>
            new DataValue
            {
                Value = stringValue,
                ValueType = DataValueType.Image
            };

        private static DataValue CreatePassword(string stringValue) =>
            new DataValue
            {
                Value = stringValue,
                ValueType = DataValueType.Password
            };

        private static DataValue CreateCollection(XElement element) =>
            new DataValue
            {
                Value = new DataCollection { Rows = ParseCollectionRows(element) },
                ValueType = DataValueType.Collection
            };

        private static DataValue CreateNestedCollection(string stringValue) =>
            new DataValue
            {
                Value = new DataCollection { Rows = ParseCollectionRows(ParseXDocumentOrThrow(stringValue).Root) },
                ValueType = DataValueType.Collection
            };

        private static DataValue CreateEmptyFlag() =>
            new DataValue
            {
                Value = "",
                ValueType = DataValueType.Flag
            };

        private static DataType ParseDataTypeOrThrow(XElement element)
        {
            var typeValue = element.Attribute("type")?.Value ??
                            throw new ArgumentNullException(nameof(element), "Invalid type supplied");
            try
            {
                return (DataType)Enum.Parse(typeof(DataType), typeValue, true);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid type supplied");
            }
        }

        private static string ParseNameParameterOrThrow(XElement element) =>
            element.Attribute("name")?.Value ??
            throw new ArgumentNullException(nameof(element), "Invalid name supplied");

        private static IReadOnlyDictionary<string, DataValue> ParseValues(XElement rootElement, string direction) =>
            rootElement
                ?.Element($"{direction}puts")
                ?.Elements($"{direction}put")
                .ToDictionary(ParseNameParameterOrThrow, ParseDataValue) ?? new Dictionary<string, DataValue>();

        private static IReadOnlyCollection<IReadOnlyDictionary<string, DataValue>> ParseCollectionRows(XElement rootElement) =>
            rootElement
                ?.Elements("row")
                .Select(ParseFields)
                .ToList() ?? new List<IReadOnlyDictionary<string, DataValue>>();

        private static IReadOnlyDictionary<string, DataValue> ParseFields(XElement rootElement) =>
            rootElement
                .Elements("field")
                .ToDictionary(ParseNameParameterOrThrow, ParseDataValue);
    }
}
