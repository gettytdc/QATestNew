using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BluePrism.NativeMessaging.EventArgs
{
    public class MessageReceivedEventArgs : System.EventArgs
    {
        public JObject Data
        {
            get;
            set;
        }

        public List<Guid> Pages { get; set; }
    }
}
