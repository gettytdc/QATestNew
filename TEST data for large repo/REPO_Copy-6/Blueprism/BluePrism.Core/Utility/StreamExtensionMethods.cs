namespace BluePrism.Core.Utility
{
    using System.IO;
    using Utilities.Functional;

    public static class StreamExtensionMethods
    {
        public static string ReadEntireStream(this Stream @this) =>
            new StreamReader(@this)
                .Use(x => x.ReadToEnd());
    }
}