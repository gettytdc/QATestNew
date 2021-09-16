namespace BluePrism.Data
{
    using System.Data;

    public static class DataParameterCollectionExtensionMethods
    {
        public static IDbDataParameter AddWithValue(this IDataParameterCollection @this, IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            @this.Add(parameter);

            return parameter;
        }
    }
}
