using Microsoft.UI.Xaml.Data;
using System;

namespace Tests_and_Interviews.Helpers
{
    public class DurationToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int duration = (int)value;

            if (duration == 90) return 150;
            if (duration == 60) return 100;

            return 50;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}