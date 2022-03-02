using LiveCharts;
using System;
using System.Globalization;
using System.Windows.Data;
using AutomateControls.Properties;

namespace AutomateControls.Converters.LiveCharts
{
    /// <summary>
    /// Convert takes a SeriesCollection input value and convert to true or false depending in the 
    /// collection is empty or not
    /// </summary>
    public class CollectionContainsDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SeriesCollection collection))
            {
                throw new InvalidOperationException(Resources.ExceptionMessageObjectTypeIsInvalid);
            }

            return collection.ContainsData();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(Resources.ExceptionNoConversionBackToSeriesCollection);
        }
    }
}
