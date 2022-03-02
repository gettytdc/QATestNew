namespace BluePrism.Core.Logging
{
    using System;
    using System.Text.RegularExpressions;

    public class SanitizedValue
    {
        private static readonly Regex SanitizingRegex = new Regex(@"[\n\r]+", RegexOptions.Compiled);
        private readonly int _sanitizeLimit;
        private readonly Lazy<string> _value;

        public string UnsanitizedValue { get; }

        public SanitizedValue(string value, int lengthLimit)
        {
            UnsanitizedValue = value;
            _sanitizeLimit = lengthLimit;
            _value = new Lazy<string>(GetValue);
        }

        public override string ToString() => _value.Value;

        private string GetValue()
        {
            var result = string.Empty;

            if (string.IsNullOrEmpty(UnsanitizedValue))
                return result;

            result = UnsanitizedValue.Substring(0, Math.Min(_sanitizeLimit, UnsanitizedValue.Length));
            result = SanitizingRegex.Replace(result, string.Empty);

            return result;
        }
    }
}
