using System;
using Windows.UI.Xaml.Data;

namespace JacobC.Xiami.Controls
{
    internal class MinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return -(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
