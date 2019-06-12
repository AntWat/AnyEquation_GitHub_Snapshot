using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AnyEquation.Equations.Views
{
    public class NumericConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string formattedString;

                // Return NaN as blank
                double dbl = (double)value;

                if (double.IsNaN(dbl))
                {
                    formattedString = "";
                }
                else if(dbl==0.0)
                {
                    formattedString = "0.000";
                }
                else if (Math.Abs(dbl)<0.1 || Math.Abs(dbl) > 99999)
                {
                    // Scientific notation....
                    formattedString = string.Format("{0:E03}", value);
                }
                else
                {
                    formattedString = $"{dbl:F03}";
                }

                return formattedString;
            }
            catch (Exception ex)
            {
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return double.NaN;
        }
    }
}
