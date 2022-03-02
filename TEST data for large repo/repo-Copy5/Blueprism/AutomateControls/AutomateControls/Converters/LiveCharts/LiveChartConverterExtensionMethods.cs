using LiveCharts;
using System;
using System.Linq;

namespace AutomateControls.Converters.LiveCharts
{
    /// <summary>
    /// Internal extenstion method to check if any data values exists within the LiveCharts SeriesCollection
    /// </summary>
    internal static class LiveChartConverterExtensionMethods
    {
        internal static bool ContainsData(this SeriesCollection seriesCollection)
        {
            if (seriesCollection == null)
            {
                throw new ArgumentNullException(nameof(seriesCollection));
            }

            foreach (var actualValues in seriesCollection.Select(t => t.ActualValues))
            {
                foreach (var values in actualValues)
                {
                    if (values is IConvertible convertible)
                    {                        
                        if (convertible.ToDouble(null) > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
