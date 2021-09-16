namespace BluePrism.Core.Utility
{
    public static class BluePrismVersionExtensions
    {
        public static string GetBluePrismVersion(this object referenceObject, int fieldCount = 4)
        {
            return referenceObject.GetType().Assembly.GetName().Version.ToString(fieldCount);
        }
    }
}