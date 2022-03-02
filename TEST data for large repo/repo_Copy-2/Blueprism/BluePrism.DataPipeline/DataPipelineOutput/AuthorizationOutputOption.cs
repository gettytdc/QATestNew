using System;
using System.Runtime.Serialization;

namespace BluePrism.DataPipeline.DataPipelineOutput
{
    [DataContract(Namespace = "bp"), Serializable]
    public class AuthorizationOutputOption : OutputOption
    {
        public override string GetConfig()
        {
            return string.Format(
                "headers => {{\"Authorization\" => \"Basic <base64><%{0}.username%>:<%{0}.password%></base64>\"}}", Value);
        }

        public AuthorizationOutputOption(string key, string value) : base(key, value)
        {
        }
    }
}