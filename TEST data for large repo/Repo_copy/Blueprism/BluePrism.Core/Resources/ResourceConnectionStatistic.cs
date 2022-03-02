using System;
using System.Runtime.Serialization;

namespace BluePrism.Core.Resources
{
    [Serializable]
    [DataContract(Namespace = "bp", Name = "rcs")]
    public class ResourceConnectionStatistic
    {
        
        [DataMember(Name="cs")]
        public bool ConnectionSuccess { get; private set; }
        [DataMember(Name="lp")]
        public long LastPing { get; private set; }
        [DataMember(Name="lc")]
        public DateTime LastConnected { get; private set; }


        public ResourceConnectionStatistic(bool madeConnection,
            long ping,
            DateTime time)
        {
            ConnectionSuccess = madeConnection;
            LastPing = ping;
            LastConnected = time;
        }

        public ResourceConnectionStatistic()
        {
            ConnectionSuccess = false;
            LastPing = 0;
            LastConnected = DateTime.UtcNow;
        }
    }
}
