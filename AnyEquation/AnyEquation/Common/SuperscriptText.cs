using System;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    public class SuperscriptText : View
    {
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                "Text",
                typeof(String),
                typeof(SuperscriptText),
                String.Empty);

        public String Text
        {
            set { SetValue(TextProperty, value); }
            get { return (String)GetValue(TextProperty); }
        }

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(
                "FontSize",
                typeof(float),
                typeof(SuperscriptText),
                (float)12);

        public float FontSize
        {
            set { SetValue(FontSizeProperty, value); }
            get { return (float)GetValue(FontSizeProperty); }
        }


        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(
                "TextColor",
                typeof(Color),
                typeof(SuperscriptText),
                Color.Default);

        public Color TextColor
        {
            set { SetValue(TextColorProperty, value); }
            get { return (Color)GetValue(TextColorProperty); }
        }


    }
}
