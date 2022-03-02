using System.Collections.Generic;

namespace BluePrism.Core.Plugins
{
    /// <summary>
    /// Interface for the configuration of plugins.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Validates the configuration.
        /// </summary>
        void Validate();

        /// <summary>
        /// Provides access to the configuration elements.
        /// </summary>
        IEnumerable<IConfigElement> Elements { get; }
    }
}
