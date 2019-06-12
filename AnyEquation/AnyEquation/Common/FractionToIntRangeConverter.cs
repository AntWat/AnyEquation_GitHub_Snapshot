using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    class FractionToIntRangeConverter : IValueConverter
    {
        //double parseToDouble(object oVal, double defaultVal)
        //{
        //    double dVal = double.NaN;

        //    if (oVal is string)
        //    {
        //        if (!Double.TryParse(oVal as string, out dVal))
        //            dVal = defaultVal;
        //    }
        //    else if (oVal is int)
        //    {
        //        dVal = (double)(int)oVal;
        //    }
        //    else if (oVal is double)
        //    {
        //        dVal = (double)oVal;
        //    }

        //    return dVal;
        //}

        public object Convert(object value, Type targetType,
                              object intRange, CultureInfo culture)
        {
            double multiplier = Utils.parseToDouble(intRange, 1);
            double dVal = Utils.parseToDouble(value, double.NaN);

            return (int)Math.Round(multiplier * dVal);
        }

        public object ConvertBack(object value, Type targetType,
                                  object intRange, CultureInfo culture)
        {
            double divider = Utils.parseToDouble(intRange, 1);
            double dVal = Utils.parseToDouble(value, double.NaN);

            return (double)(dVal / divider);
        }
    }
}
