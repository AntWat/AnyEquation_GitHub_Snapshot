using System;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    [ContentProperty("Source")]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
                return null;

            ImageSource img = ImageSource.FromResource(Source);
            return img;
        }
    }
}

