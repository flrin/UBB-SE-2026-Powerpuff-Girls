using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Tests_and_Interviews.Models;
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
                    ? new SolidColorBrush(Color.FromArgb(255, 33, 150, 243))
                    : new SolidColorBrush(Colors.LightGray);
            }

            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}