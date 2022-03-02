namespace BluePrism.Scheduling
{
    /// <summary>
    /// Enumeration of the 'nth of month' values supported by the
    /// scheduling engine.
    /// </summary>
    public enum NthOfMonth : int
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Fifth = 5,
        Last = -1
    }
    public static class NthOfMonthExtensions
    {
        public static string GetLocalizedFriendlyName(this NthOfMonth me)
        {
            string result = Properties.Resources.ResourceManager.GetString($"NthOfMonth_{me}");
            return result == null ? me.ToString() : result;
        }
    }
}
