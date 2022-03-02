using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrism.Core.Network
{

    public interface IMessageCounter
    {
        event EventHandler RequestReset;

        int MessageCount { get; }
        void Increment();
        void Reset();

        void Connected(DateTime datetime);
        void Disconnected(DateTime datetime);
        (State, DateTime)[] ConnectionHistory();
        bool ThresholdExceeded();
    }
    
    public enum State
    {
        Connected,
        Disconnected
    }

    public class MessageCounter : IMessageCounter
    {
        public event EventHandler RequestReset;

        public string  Name {get;}
        private readonly int _maxCount;
        public MessageCounter(string name,int maxCount)
        {
            Name = name;
            _maxCount = maxCount;
        }

        public int MessageCount { get; private set; }

        public void Increment()
        {
            if(MessageCount++ > _maxCount)
            {
                RequestReset?.Invoke(this, new EventArgs());
            }
        }
        public void Reset() => MessageCount = 0;

        private readonly IList<(State, DateTime)> _connectionHistory = new List<(State, DateTime)>();
        
        public void Connected(DateTime datetime)
        {
            _connectionHistory.Add((State.Connected, datetime));
            Reset();
        }
        
        public void Disconnected(DateTime datetime)
        {
            _connectionHistory.Add((State.Disconnected, datetime));
        }

        public (State,DateTime)[] ConnectionHistory() => _connectionHistory.ToArray();


        public bool ThresholdExceeded() => MessageCount >= _maxCount;
    }
}
