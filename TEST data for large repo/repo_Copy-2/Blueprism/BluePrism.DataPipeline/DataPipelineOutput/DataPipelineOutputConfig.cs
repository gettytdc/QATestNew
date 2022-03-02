using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BluePrism.DataPipeline.DataPipelineOutput
{
    [DataContract(Namespace = "bp"), Serializable]
    public class DataPipelineOutputConfig
    {
        public DataPipelineOutputConfig()
        {
            OutputOptions = new List<OutputOption>();
            SessionLogFields = new List<string>()
            {
                "SessionNumber",
                "StageId",
                "StageName",
                "StageType",
                "ProcessName",
                "PageName",
                "ObjectName",
                "ActionName",
                "StartDate",
                "EndDate",
                "ResourceId",
                "ResourceName",
                "Result",
                "ResultType",
                "Attributes"
            };
        }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public Guid UniqueReference { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsSessions { get; set; }
        [DataMember]
        public bool IsDashboards { get; set; }
        [DataMember]
        public bool IsWqaSnapshotData { get; set; }
        [DataMember]
        public bool IsCustomObjectData { get; set; }
        [DataMember]
        public OutputType OutputType { get; set; }
        [DataMember]
        public List<OutputOption> OutputOptions { get; set; }
        [DataMember]
        public string SessionCols { get; set; }
        [DataMember]
        public string DashboardCols { get; set; }
        [DataMember]
        public DateTime? DateCreated { get; set; }
        [DataMember]
        public string AdvancedConfiguration { get; set; }
        [DataMember]
        public bool IsAdvanced { get; set; }
        [DataMember]
        public List<string> SelectedDashboards { get; set; }
        [DataMember]
        public List<string> SelectedSessionLogFields { get; set; }
        [DataMember]
        public List<string> SessionLogFields { get; set; }

        public string GetLogstashConfig()
        {
            var sb = new StringBuilder();

            List<EventType> eventTypes = new List<EventType>();

            if (IsSessions)
                eventTypes.Add(EventType.SessionLog);

            if (IsDashboards)
                eventTypes.Add(EventType.Dashboard);

            if (IsWqaSnapshotData)
                eventTypes.Add(EventType.WqaSnapshotData);

            if (IsCustomObjectData)
                eventTypes.Add(EventType.CustomData);

            if (!eventTypes.Any())
                throw new InvalidOperationException("No output type selected");

            if (IsSessionFilteringConfigRequired())
            {
                sb.Append("if (");
            }
            else
            {
                sb.Append("if ");
            }

            for (int i = 0; i < eventTypes.Count; i++)
            {

                if (eventTypes[i] == EventType.Dashboard)
                {
                    sb.Append($"([event][EventType] == {(int)eventTypes[i]} ");

                    if (IsDashboards && SelectedDashboards?.Count > 0)
                    {
                       sb = HandleDashboardSection(sb);
                    }
                    sb.Append(") ");

                    if (i + 1 != eventTypes.Count)
                    {
                        sb.Append("or ");
                    }
                }
                else
                {
                    sb.Append($"[event][EventType] == {(int)eventTypes[i]} ");
                    if (i + 1 != eventTypes.Count)
                    {
                        sb.Append("or ");
                    }
                }
            }

            if (IsSessionFilteringConfigRequired())
            {
                sb.Append($" ) and [type] == \"{Name}\" ");
            }

            sb.AppendLine(" {");

            sb.Append(OutputType.GetConfig(OutputOptions));

            sb.Append("}");
            return sb.ToString();
        }

        private StringBuilder HandleDashboardSection(StringBuilder sb)
        {
            if (sb != null)
            {
                sb.Append("and (");
                for (var dashBoardCount = 0; dashBoardCount < SelectedDashboards.Count; dashBoardCount++)
                {
                    sb.Append($"[event][EventData][Source] == \"{SelectedDashboards[dashBoardCount]}\"");

                    sb.Append(dashBoardCount + 1 != SelectedDashboards.Count ? " or " : ")");
                }
            }

            return sb;
        }

        public string GetSessionFilter()
        {
            var sb = new StringBuilder();
            var fieldList = new StringBuilder();

            List<EventType> eventTypes = new List<EventType>();

            if (IsSessionFilteringConfigRequired())
            {
                sb.AppendLine($"if [event][EventType] == 1 and [type] == \"{Name}\" {{");
                sb.AppendLine("ruby {");
                sb.AppendLine("code => \"");
                sb.AppendLine($"event.get('[event][EventData]').to_hash.keys.each {{|k,v|");

                SelectedSessionLogFields.ForEach(x =>
                {
                    if (fieldList.Length > 0)
                    {
                        fieldList.Append(",");
                    }
                    fieldList.Append($"'{x}'");
                });

                sb.AppendLine($"if (![{fieldList.ToString()}].include?(k))");
                sb.AppendLine($"event.remove('[event][EventData]' + '[' + k + ']')");
                sb.AppendLine("end");
                sb.AppendLine("}\"");
                sb.AppendLine("}");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        public void AddOrReplaceOption(OutputOption option)
        {
            var existing = this.OutputOptions.Find(o => o.Id == option.Id);

            if (existing == null)
            {
                this.OutputOptions.Add(option);
            }
            else
            {
                OutputOptions[OutputOptions.FindIndex(o => o.Id == option.Id)] = option;
            }
        }

        public bool IsSessionFilteringConfigRequired()
        {
            return (IsSessions && SelectedSessionLogFields != null && SelectedSessionLogFields.Count > 0 &&
                !SessionLogFields.OrderBy(x => x).SequenceEqual(SelectedSessionLogFields.OrderBy(x => x)));
        }

        public OutputOption GetOrCreateOutputOptionById(string key, string defaultValue = "")
        {
            var existing = this.OutputOptions.Find(o => o.Id == key);

            if (existing != null)
            {
                return existing;
            }

            var uo = OutputOptionFactory.GetOutputOption(key, defaultValue);
            OutputOptions.Add(uo);
            return uo;
        }
    }
}
