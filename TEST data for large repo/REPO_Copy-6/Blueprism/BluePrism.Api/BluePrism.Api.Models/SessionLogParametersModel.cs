using System.Collections.Generic;

namespace BluePrism.Api.Models
{
    public class SessionLogParametersModel
    {
        public IReadOnlyDictionary<string, DataValueModel> Inputs { get; set; }
        public IReadOnlyDictionary<string, DataValueModel> Outputs { get; set; }
    }
}
