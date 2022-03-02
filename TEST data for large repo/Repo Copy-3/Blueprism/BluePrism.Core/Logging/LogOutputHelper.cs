namespace BluePrism.Core.Logging
{
    /// <summary>
    /// Methods to assist in Logging functionality
    /// </summary>
    public static class LogOutputHelper
    {
        private const int SanitizeDefaultLimit = 1000;

        public static SanitizedValue Sanitize(string value, int lengthLimit = SanitizeDefaultLimit)
        {
            var sanitizedValueType = new SanitizedValue(value, lengthLimit);

            return sanitizedValueType;
        }
    }
}
