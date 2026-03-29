using Microsoft.UI.Xaml.Data;
using System;

namespace Tests_and_Interviews.Helpers
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                return dt.ToLocalTime().ToString("MMM dd, yyyy h:mm tt");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
