namespace BluePrism.Api.BpLibAdapters
{
    using AutomateAppCore;

    public struct SessionItem<TAdapter> where TAdapter : IServerAdapter
    {
        public IServer Server { get; }
        public TAdapter Adapter { get; }
        public long ExpiresTick { get; }

        public SessionItem(IServer server, TAdapter adapter, long expiresTick)
        {
            Server = server;
            Adapter = adapter;
            ExpiresTick = expiresTick;
        }
    }
}
