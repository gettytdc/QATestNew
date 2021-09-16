namespace BluePrism.DatabaseInstaller
{
    public class ColumnDescriptionInfo
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Description { get; set; }
        public int PrimaryKey { get; set; }
        public int ForeignKey { get; set; }
        public bool Nullable { get; set; }
        public string Default { get; set; }
    }
}
