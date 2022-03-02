namespace BluePrism.Core.Conversion
{
    using System;
    using Utilities.Functional;

    public static class UrlBase64Convertor
    {
        public static string ToBase64String(byte[] data) =>
            data.Map(Convert.ToBase64String).Replace('+', '-').Replace('/', '_').Replace("=", "");

        public static byte[] FromBase64String(string data) =>
            data
                .Replace('_', '/')
                .Replace('-', '+')
                .PadRight((int) Math.Ceiling(data.Length / 4.0) * 4, '=')
                .Map(Convert.FromBase64String);
    }
}