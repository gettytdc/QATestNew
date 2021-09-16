
using System.Resources;


namespace Internationalisation
{
    /// <summary>
    /// Provides access to strings from the appropriate resource file for the current locale.
    /// </summary>
    public class ResMan
    {
        private static readonly ResourceManager _resourceManager = new ResourceManager("Internationalisation.Resources",
            System.Reflection.Assembly.GetExecutingAssembly());

        /// <summary>
        /// Returns a string from the resource file with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetString(string name)
        {
            return _resourceManager.GetString(name);
        }
        public static string GetString(string name, System.Globalization.CultureInfo culture)
        {
            return _resourceManager.GetString(name, culture);
        }
    }
}
