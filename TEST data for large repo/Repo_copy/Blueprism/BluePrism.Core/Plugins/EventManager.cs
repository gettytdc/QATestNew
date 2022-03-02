using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Core.Plugins
{
    /// <summary>
    /// Manages events using the blueprism plugin architecture.
    /// </summary>
    public class EventManager
    {
        public const string PluginDirName = "plugins";

        private static EventManager _instance;

        public static EventManager GetInstance()
        {
            if (_instance == null)
                _instance = new EventManager();
            return _instance;
        }

        [ImportMany(typeof(IEventHandlerFactory))]
        private IEnumerable<IEventHandlerFactory> _factories = null;

        private ICollection<IEventHandler> _handlers;

        private CompositionContainer _container;

        /// <summary>
        /// Constructs a new event manager.
        /// </summary>
        public EventManager()
        {
            _handlers = new List<IEventHandler>();

            var cat = new AggregateCatalog();
            // Main location is the ./plugins directory beneath the executing dir
            cat.Catalogs.Add(
                new DirectoryCatalog(Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    PluginDirName
                ))
            );
            _container = new CompositionContainer(cat);

            try
            {
                _container.ComposeParts(this);
            }
            catch (Exception ex)
            {
                Debug.Print("Error while composing parts: {0}", ex);
            }
        }

        public void AddHandler(
            string factoryName,
            string instanceName,
            IDictionary<string, object> config)
        {
            // If we have one with this name already, ignore the add. It's already
            // there.
            if (_handlers.Any(i => i.Name == instanceName))
                return;

            var fact = _factories.FirstOrDefault(f => f.Name == factoryName);
            if (fact == null) throw new NoSuchElementException(
                "No factory with name '{0}' found", factoryName);

            var inst = fact.Create(instanceName);
            foreach (var pair in config)
            {
                inst.Configuration.Configure(pair.Key, pair.Value);
            }
            _handlers.Add(inst);
        }

        public void SendEvent(IDictionary<string,object> data)
        {
            var handlers = _handlers;
            if (handlers == null)
            {
                Debug.Print("No handlers found to handle the event");
                return;
            }
            foreach (var h in handlers)
            {
                try
                {
                    h.HandleEvent(data);
                }
                catch (Exception ex)
                {
                    Debug.Print("Error in handler: {0}: {1}", h, ex);
                }
            }
        }

    }
}
