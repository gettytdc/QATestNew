using System;

namespace BluePrism.DigitalWorker.Messages.Commands
{
    public class ProcessValue
    {
        public string EncodedValue { get; }
        public ProcessValueType Type { get; }

        public ProcessValue(string encodedValue, ProcessValueType type)
        {
            EncodedValue = encodedValue;
            Type = type;
        }
    }
}
