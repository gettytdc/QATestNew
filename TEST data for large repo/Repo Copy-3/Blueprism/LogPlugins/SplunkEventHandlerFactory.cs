namespace BluePrism.Core.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reflection;

    using BPCoreLib;

    /// <summary>
    /// Factory for the Splunk event handler.
    /// </summary>
    [Export(typeof(IEventHandlerFactory))]
    public class SplunkEventHandlerFactory : IEventHandlerFactory
    {
        private static readonly IReadOnlyCollection<RequestFactoryDetails> _requestFactoryDetails;

        private static readonly Dictionary<string, SplunkEventHandler> _eventHandlers =
            new Dictionary<string, SplunkEventHandler>();

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => "Splunk Sender";

        static SplunkEventHandlerFactory()
        {
            // Ideally this would be done by a DI framework but for now it'll have
            // to be done here.
            _requestFactoryDetails =
                Assembly.GetExecutingAssembly()
                    .GetConcreteImplementations<IRequestFactory>()
                    .Select(GetRequestFactoryDetails)
                    .ToList();
        }

        public IEventHandler Create(string instanceName) =>
            _eventHandlers.ContainsKey(instanceName)
            ? _eventHandlers[instanceName]
            : _eventHandlers[instanceName] =
                new SplunkEventHandler(GetRequestFactory)
                {
                    Name = instanceName
                };

        private static IRequestFactory GetRequestFactory(IConfiguration configuration) =>
            _requestFactoryDetails
            .SingleOrDefault(x => x.ConfigurationIsSuitable(configuration))
            ?.Create(configuration);

        private static RequestFactoryDetails GetRequestFactoryDetails(Type type) =>
            new RequestFactoryDetails(type);

        private class RequestFactoryDetails
        {
            public readonly Func<IConfiguration, bool> ConfigurationIsSuitable;
            public readonly Func<IConfiguration, IRequestFactory> Create;

            public RequestFactoryDetails(Type type)
            {
                Create = 
                    c => 
                    (IRequestFactory)
                    type.GetConstructor(new[] { typeof(IConfiguration) })
                    ?.Invoke(new object[] { c });

                ConfigurationIsSuitable =
                    c =>
                    (bool?)
                    type.GetMethod("ConfigurationIsSuitable", BindingFlags.Static | BindingFlags.Public)
                    ?.Invoke(null, new object[] { c })
                    ?? false;
            }
        }
    }
}
