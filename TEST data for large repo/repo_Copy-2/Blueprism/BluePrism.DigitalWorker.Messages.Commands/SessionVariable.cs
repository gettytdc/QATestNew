namespace BluePrism.DigitalWorker.Messages.Commands
{
    public class SessionVariable
    {
        public string Name { get; }
        public string Description{ get; }
        public ProcessValue Value { get; }

        public SessionVariable(string name, string description, ProcessValue value)
        {
            Name = name;
            Value = value;
            Description = description;
            Value = value;
        }
    }
}
