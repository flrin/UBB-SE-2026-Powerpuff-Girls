using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Tests_and_Interviews.Models.Enums;
using Windows.UI;

namespace Tests_and_Interviews.Helpers
{
    public class IdToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SlotStatus status)
            {
                return status == SlotStatus.Occupied
                   ? new SolidColorBrush(Color.FromArgb(255, 99, 102, 255))
                   : new SolidColorBrush(Color.FromArgb(255, 206, 213, 255));
            }

            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}