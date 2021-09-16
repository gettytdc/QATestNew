namespace BluePrism.Core.Utility
{
    using System;
    using System.Windows.Forms;

    public static class DataGridViewCellExtensionMethods
    {
        /// <summary>
        /// Convert DataGridViewCell value to bool.
        /// </summary>
        /// <param name="this">The DataGridViewCell to convert.</param>
        /// <returns>The DataGridViewCell value converted to boolean.</returns>
        public static bool ValueToBool(this DataGridViewCell @this)
            => Convert.ToBoolean(@this.Value);
    }
}
