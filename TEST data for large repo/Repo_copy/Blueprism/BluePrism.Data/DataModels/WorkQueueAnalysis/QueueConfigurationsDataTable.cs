using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class QueueConfigurationsDataTable 
    {
        [DataMember]
        public DataTable Table { get; set; }  

        public QueueConfigurationsDataTable(DataTable table, Dictionary<int, int > configIDsAndConfiguredSnapshotRowCount) 
        {
            if (table.Columns.Count == 3)
            {
                table.Columns[0].ColumnName = "QueueId";
                table.Columns[1].ColumnName = "ConfigurationId";
                table.Columns[2].ColumnName = "ConfigurationEnabled";
                if (table.Columns["QueueId"].DataType == typeof(int) &&
                    table.Columns["ConfigurationId"].DataType == typeof(int) &&
                    table.Columns["ConfigurationEnabled"].DataType == typeof(bool))
                {
                    Table = table;
                    Table.Columns.Add(new DataColumn("ConfiguredSnapshotRowsPerQueue", typeof(int)));
                    foreach (DataRow row in table.Rows)
                    {
                        int configId = (int) row["ConfigurationId"];
                        if(configIDsAndConfiguredSnapshotRowCount.ContainsKey(configId))
                        {
                            row["ConfiguredSnapshotRowsPerQueue"] = configIDsAndConfiguredSnapshotRowCount[configId];
                        }
                        else
                        {
                            row["ConfiguredSnapshotRowsPerQueue"] = 0;
                        }
                        
                        
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }
  
        public bool QueueHasExistingConfiguration(int queueId)
        {
            return Table.Select($"QueueId = {queueId}").Count() != 0;
        }

        public bool ExistingConfigIsEnabled(int queueId)
        {
            DataRow[] existingConfigs = Table.Select($"QueueId = {queueId}");
            if (existingConfigs.Length == 0)
            {
                return false;
            }
            else
            {
                return (bool)existingConfigs[0]["ConfigurationEnabled"];
            }
         
        }

        public int GetConfiguredSnapshotRowsForQueue(int queueId)
        {
            DataRow[] existingConfigs = Table.Select($"QueueId = {queueId}");
            if (existingConfigs.Length == 0)
            {
                return 0;
            }
            else
            {
                return (int)existingConfigs[0]["ConfiguredSnapshotRowsPerQueue"];
            }
        }
    }
}
