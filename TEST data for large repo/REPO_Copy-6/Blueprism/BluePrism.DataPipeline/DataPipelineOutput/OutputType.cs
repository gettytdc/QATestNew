using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BluePrism.DataPipeline.DataPipelineOutput
{
    [DataContract(Namespace = "bp"), Serializable]
    [KnownType(typeof(DatabaseOutput))]
    [KnownType(typeof(SplunkOutput))]
    public class OutputType
    {
        public OutputType(string name, string id)
        {
            Name = name;
            Id = id;
        }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Id { get; set; }

        public virtual string GetConfig(List<OutputOption> outputOptions)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{OutputIdToLogstashOutputIdentifier(Id)} {{ ");

            foreach (var option in outputOptions)
            {
                sb.AppendLine(option.GetConfig());
            }

            sb.AppendLine("}");
            return sb.ToString();
        }


        protected string OutputIdToLogstashOutputIdentifier(string id)
        {
            switch(id)
            {
                case "http":
                case "splunk":
                    return "bphttp";

                case "database":
                    return "bpjdbc";

                case "file":
                    return "file";

                default:
                    throw new ArgumentException($"Unexpected output id: {id}");
            }
        }
    }
}