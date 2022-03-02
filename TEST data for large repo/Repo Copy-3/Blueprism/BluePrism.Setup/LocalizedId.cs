namespace BluePrism.Setup
{
    class LocalizedId
    {
        public string Value { get; set; }

        public LocalizedId(string value)
        {
            Value = value;
        }

        public static implicit operator string(LocalizedId rhs)
        {
            return rhs.Value;
        }
    }
}
