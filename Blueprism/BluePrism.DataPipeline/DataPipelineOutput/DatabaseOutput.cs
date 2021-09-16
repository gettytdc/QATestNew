using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BluePrism.DataPipeline.DataPipelineOutput
{
    [DataContract(Namespace = "bp"), Serializable]
    public class DatabaseOutput : OutputType
    {
        public const string SecurityType = "security";
        public const string IntegratedSecurity = "integratedSecurity";
        public const string CredentialSecurity = "credentialSecurity";

        public const string Credential = "credential";
        public const string Server = "server";
        public const string TableName = "tableName";
        public const string DatabaseName = "databaseName";
               
        public DatabaseOutput(string name, string id) : base(name, id)
        {
        }

        public override string GetConfig(List<OutputOption> outputOptions)
        {
            Dictionary<string, string> outputOptionLookup = outputOptions.ToDictionary<OutputOption, string, string>(y => y.Id, y=>y.Value);

            var sb = new StringBuilder();

            sb.AppendLine($"{OutputIdToLogstashOutputIdentifier(Id)} {{ ");

            string security = "integratedSecurity=true";
            if (outputOptionLookup[SecurityType] == CredentialSecurity)
            {
                var credentialName = outputOptionLookup[Credential];
                security = $"user=<%{credentialName}.username%>;password=<%{credentialName}.password%>";
            }

            sb.AppendLine($"connection_string => \"jdbc:sqlserver://{outputOptionLookup[Server]};databaseName={outputOptionLookup[DatabaseName]};{security};\"");
            sb.AppendLine(@"driver_jar_path => ""..\sqljdbc_4.2\enu\jre8\sqljdbc42.jar""");
            sb.AppendLine(@"driver_class => ""com.microsoft.sqlserver.jdbc.SQLServerDriver""");
            sb.AppendLine($"statement => [\"insert into {outputOptionLookup[TableName]}(EventType, EventData) values(?, ?)\", \"[event][EventType]\", \"[event][EventData]\"]");

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
