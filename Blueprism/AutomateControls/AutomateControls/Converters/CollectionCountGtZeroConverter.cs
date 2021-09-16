using System;
using System.Globalization;
using System.Windows.Data;

namespace AutomateControls.Converters
{
    /// <summary>
    /// Converter takes an ICollection type and returns true if the value is > 0.  If not a ICollection type, false is returned 
    /// </summary>
    public class CollectionCountGtZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as System.Collections.ICollection)?.Count > 0 == true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
