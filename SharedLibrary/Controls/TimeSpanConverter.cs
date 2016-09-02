using System;
using Windows.UI.Xaml.Data;

namespace JacobC.Xiami.Controls
{
    internal class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Covert((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static string Covert(double value)
        {
            var secs = (int)value;
            return $"{Fill(secs / 60)}:{Fill(secs % 60)}";
        }
        //避免使用Format引用System.Globalization空间，增加内存
        private static string Fill(int a)
        {
            if (a < 10)
                return "0" + a.ToString();
            else
                return a.ToString();
        }
    }
}
