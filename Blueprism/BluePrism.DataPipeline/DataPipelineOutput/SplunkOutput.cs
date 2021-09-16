using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BluePrism.DataPipeline.DataPipelineOutput
{

    [DataContract(Namespace = "bp"), Serializable]
    public class SplunkOutput : OutputType
    {
        public const string URL = "url";
        public const string Token = "token";
        
        public SplunkOutput(string name, string id) : base(name, id)
        {
        }

        public override string GetConfig(List<OutputOption> outputOptions)
        {
            string url = outputOptions.FirstOrDefault(x => x.Id == URL).Value;
            string token = outputOptions.FirstOrDefault(x => x.Id == Token).Value;

            var sb = new StringBuilder();
            sb.AppendLine($"{OutputIdToLogstashOutputIdentifier(Id)} {{ ");
            sb.AppendLine(@"http_method => ""post""");
            sb.AppendLine($"url => \"{url}\"");
            sb.AppendLine($"headers => [\"Authorization\", \"Splunk {token}\"]");
            sb.AppendLine(@"mapping => {""event"" => ""%{event}""}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
