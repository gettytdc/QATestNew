using System.Collections.Generic;

namespace BluePrism.Core.Plugins
{
    public abstract class BaseEventHandler : IEventHandler
    {
        private BaseConfiguration _cfg;

        public virtual string Name { get; set; }

        protected BaseEventHandler()
        {
            _cfg = new BaseConfiguration();
        }

        protected BaseEventHandler(BaseConfiguration cfg)
        {
            _cfg = cfg;
        }

        IConfiguration IEventHandler.Configuration { get { return Config; } }

        string IEventHandler.Name { get { return this.Name; } }

        protected BaseConfiguration Config { get { return _cfg; } }

        public abstract void HandleEvent(IDictionary<string, object> data);

    }
}
