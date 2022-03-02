using System;
using System.Globalization;
using System.Windows.Data;

namespace AutomateControls.Converters
{
    /// <summary>
    /// Converter takes an ICollection type and returns Visible if the collection is empty; otherwise, it will return collapsed
    /// </summary>
    public class CollectionHasNoDataVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as System.Collections.ICollection)?.Count == 0 == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
