using System;

namespace BluePrism.CharMatching
{
    public static class ElementParser
    {
        public delegate bool TryParseHandler<T>(string value, out T result);

        public static T ParseElement<T>(string elementValue, string errorMessage, TryParseHandler<T> handler)
        {
            if (!handler(elementValue, out var parsedValue))
            {
                throw new InvalidOperationException(errorMessage);
            }

            return parsedValue;

        }
    }
}
