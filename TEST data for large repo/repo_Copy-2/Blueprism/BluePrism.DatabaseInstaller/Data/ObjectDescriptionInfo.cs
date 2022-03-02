using System.Collections.Generic;

namespace BluePrism.DatabaseInstaller
{
    public class ObjectDescriptionInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public IList<ColumnDescriptionInfo> ColumnDescriptions { get; } = new List<ColumnDescriptionInfo>();
    }
}
