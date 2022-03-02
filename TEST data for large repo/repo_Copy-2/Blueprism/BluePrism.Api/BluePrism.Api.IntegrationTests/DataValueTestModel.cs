namespace BluePrism.Api.IntegrationTests
{
    using Models;

    public class DataValueTestModel
    {
        public DataValueTestModel(string key, object value, DataValueType valueType, bool expectedBindErrorResult, object expectedResultObject)
        {
            Key = key;
            Value = value;
            ValueType = valueType;
            ExpectedBindErrorResult = expectedBindErrorResult;
            ExpectedResultObject = expectedResultObject;
        }

        public string Key { get; set; }
        public object Value { get; set; }
        public DataValueType ValueType { get; set; }
        public bool ExpectedBindErrorResult { get; set; }
        public object ExpectedResultObject { get; set; }
    }
}
