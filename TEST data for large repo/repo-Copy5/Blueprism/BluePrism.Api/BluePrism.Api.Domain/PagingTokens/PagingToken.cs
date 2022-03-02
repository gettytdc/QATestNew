namespace BluePrism.Api.Domain.PagingTokens
{
    using System;
    using System.Text;
    using Func;
    using Newtonsoft.Json;

    public class PagingToken<T>
    {
        public T PreviousIdValue { get; set; }
        public string PreviousSortColumnValue { get; set; }
        public string DataType { get; set; }
        public string ParametersHashCode { get; set; }

        public override string ToString() =>
           this
               .Map(JsonConvert.SerializeObject)
               .Map(Encoding.UTF8.GetBytes)
               .Map(Convert.ToBase64String);
    };
}
