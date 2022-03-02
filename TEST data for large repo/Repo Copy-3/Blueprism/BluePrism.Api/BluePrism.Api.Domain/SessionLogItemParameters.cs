namespace BluePrism.Api.Domain
{
    using System.Collections.Generic;

    public class SessionLogItemParameters
    {
        public IReadOnlyDictionary<string, DataValue> Inputs { get; set; }
        public IReadOnlyDictionary<string, DataValue> Outputs { get; set; }
    }
}
