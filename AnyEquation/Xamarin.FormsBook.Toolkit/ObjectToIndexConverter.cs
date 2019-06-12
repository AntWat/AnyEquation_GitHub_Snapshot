using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace Xamarin.FormsBook.Toolkit
{
    [ContentProperty("ListItems")]
    public class ObjectToIndexConverter<T> : IValueConverter
    {
        public IList<T> ListItems { set; get; }

        public ObjectToIndexConverter()
        {
            ListItems = new List<T>();
        }

        public object Convert(object value, Type targetType, 
                              object parameter, CultureInfo culture)
        {
            if (value == null || !(value is T) || ListItems == null)
                return -1;

            return ListItems.IndexOf((T)value);
        }

        public object ConvertBack(object value, Type targetType, 
                                  object parameter, CultureInfo culture)
        {
            int index = (int)value;

            if (index < 0 || ListItems == null || index >= ListItems.Count)
                return null;

            return ListItems[index];
        }
    }
}
