using System.Linq;
using System.Threading.Tasks;

namespace BluePrism.Core.Plugins
{
    public static class Extensions
    {
        /// <summary>
        /// Sets the value of a configuration with a given name.
        /// </summary>
        /// <param name="cfg">The configuration to alter</param>
        /// <param name="configName">The name of the configuration element</param>
        /// <param name="value">The value to set on the configuration element</param>
        public static void Configure(this IConfiguration cfg, string configName, object value)
        {
            var elem = cfg.Elements.FirstOrDefault(e => e.Name == configName);
            if (elem != null)
                elem.Value = value;
        }

        /// <summary>
        /// Gets the configuration element with the given name.
        /// </summary>
        /// <typeparam name="T">The type of the configuration element</typeparam>
        /// <param name="configuration">The configuration to alter</param>
        /// <param name="configName">The name of the configuration element</param>
        /// <returns>The value of the configuration element</returns>
        public static T GetConfig<T>(this IConfiguration configuration, string configName) =>
            GetConfig(configuration, configName, default(T));

        //// <summary>
        /// Gets the configuration element with the given name.
        /// </summary>
        /// <typeparam name="T">The type of the configuration element</typeparam>
        /// <param name="configuration">The configuration to alter</param>
        /// <param name="configName">The name of the configuration element</param>
        /// <param name="defaultValue">The default value to return if the element is not found</param>
        /// <returns>The value of the configuration element</returns>
        public static T GetConfig<T>(this IConfiguration configuration, string configName, T defaultValue) =>
            (T) (
                configuration.Elements
                    .FirstOrDefault(e => e.Name == configName)
                    ?.Value
                ?? defaultValue);

        /// <summary>
        /// Allows a task to continue without any further action.
        /// we use ContinueWith((n) => {}) since we want to "Fire and forget"
        /// </summary>
        /// <param name="t">The task to continue</param>
        public static void Continue(this Task t) =>
            t.ContinueWith(_ => {});
    }
}
