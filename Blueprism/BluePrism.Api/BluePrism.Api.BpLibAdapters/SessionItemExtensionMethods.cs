namespace BluePrism.Api.BpLibAdapters
{
    public static class SessionItemExtensionMethods
    {
        public static SessionItem<TAdapter> SetExpiry<TAdapter>(this SessionItem<TAdapter> @this, long ticks)
            where TAdapter : IServerAdapter
            =>
            new SessionItem<TAdapter>(@this.Server, @this.Adapter, ticks);
    }
}
