using System;

namespace BluePrism.Datapipeline.Logstash.Wrappers
{
    public class StandardOutputEventArgs : EventArgs
    {
        public StandardOutputEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
