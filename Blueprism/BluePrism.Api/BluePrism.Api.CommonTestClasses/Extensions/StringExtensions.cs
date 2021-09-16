namespace BluePrism.Api.CommonTestClasses.Extensions
{
    public static class StringExtensions
    {
        public static string ToLowerCaseFirstCharacter(this string @this) =>
            string.IsNullOrWhiteSpace(@this)
                ? @this
                : char.ToLower(@this[0]) + @this.Substring(1);
    }
}
