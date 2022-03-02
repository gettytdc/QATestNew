using System;

namespace BluePrism.BrowserAutomation.Exceptions
{
    public class BrowserAutomationException : Exception
    {
        public BrowserAutomationException()
        {
        }

        public BrowserAutomationException(string message)
            : base(message)
        {
        }

        public BrowserAutomationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
