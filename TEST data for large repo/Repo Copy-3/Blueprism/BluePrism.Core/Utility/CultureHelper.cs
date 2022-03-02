using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Helper functionality for working with Cultures
    /// </summary>

    public static class CultureHelper
    {
        private static readonly List<string> latinSpanishCultures = new List<string> {
                                                //Latin (South America)
                                                "es-ar",
                                                "es-bo",
                                                "es-cl",
                                                "es-co",
                                                "es-cr",
                                                "es-do",
                                                "es-ec",
                                                "es-gt",
                                                "es-hn",
                                                "es-mx",
                                                "es-ni",
                                                "es-pa",
                                                "es-pe",
                                                "es-pr",
                                                "es-py",
                                                "es-sv",
                                                "es-uy",
                                                "es-ve",
                                                "es-019",
                                                "es-419",
                                                //Spanish (United States)
                                                "es-us"
                                             };
        public const string LatinAmericanSpanish = "es-419";
        public const string LatinAmericanSpanishHelpCode = "es-la";

        public static bool IsLatinAmericanSpanish(string UIcultureOpt = null)
        {
            string UIculture = UIcultureOpt;

            if(UIculture == null)
                UIculture = Thread.CurrentThread.CurrentUICulture.Name;

            if (string.IsNullOrWhiteSpace(UIculture))
                return false;

            var isSpanish = latinSpanishCultures.Any( item => item == UIculture.ToLower() );

            return isSpanish;
        }  
    }
}
