using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    public class MultiplyDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object multiplier, CultureInfo culture)
        {
            double __multiplier = Utils.parseToDouble(multiplier, 1.0);
            double dVal = Utils.parseToDouble(value, double.NaN);

            return (__multiplier * dVal);
        }

        public object ConvertBack(object value, Type targetType,
                                  object multiplier, CultureInfo culture)
        {
            double divider = Utils.parseToDouble(multiplier, 1.0);
            double dVal = Utils.parseToDouble(value, double.NaN);

            return (double)(dVal / divider);
        }
    }
}
