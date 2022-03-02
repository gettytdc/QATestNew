using LiveCharts;
using System;
using System.Globalization;
using System.Windows.Data;
using AutomateControls.Properties;

namespace AutomateControls.Converters.LiveCharts
{
    public class NoDataAvailableVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SeriesCollection collection))
            {
                throw new InvalidOperationException( Resources.ExceptionMessageObjectTypeIsInvalid);
            }
            return !collection.ContainsData() ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(Resources.ExceptionNoConversionFromVisibilityBackToSeriesCollection);
        }
    }
}
