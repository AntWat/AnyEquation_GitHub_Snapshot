using System;
using System.Globalization;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    public class AddDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object amountToAdd, CultureInfo culture)
        {
            double toAdd = 0;

            if (!Double.TryParse(amountToAdd as string, out toAdd))
                toAdd = 0;

            return (double)value + toAdd;
        }

        public object ConvertBack(object value, Type targetType,
                                  object amountToAdd, CultureInfo culture)
        {
            double toAdd = 0;

            if (!Double.TryParse(amountToAdd as string, out toAdd))
                toAdd = 0;

            return (double)value - toAdd;
        }
    }
}
