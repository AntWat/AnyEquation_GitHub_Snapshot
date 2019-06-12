using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using AnyEquation.Common;

//using Android.Util;
//using Android.Widget;
//using Android.Views;

[assembly: ExportRenderer(typeof(SuperscriptText),
                          typeof(AnyEquation.UWP.CustomRenderers.SuperscriptTextRenderer))]

namespace AnyEquation.UWP.CustomRenderers
{
    public class SuperscriptTextRenderer : ViewRenderer<SuperscriptText, TextBlock>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SuperscriptText> args)
        {
            base.OnElementChanged(args);

            string sText = string.Empty;
            float fSize = 8;
            Color textColor = Color.Black;

            if (args.NewElement != null)
            {
                sText = args.NewElement.Text;
                fSize = args.NewElement.FontSize;
                textColor = args.NewElement.TextColor;
            }

            if (sText.Length == 0) return;

            fSize = (float)0.7 * fSize;

            if (Control == null)
            {
                SetNativeControl(new TextBlock()
                {
                    Text = sText,
                });

                TextBlock textView = (TextBlock)Control;

                textView.FontSize = fSize;

                var converter = new ColorConverter();

                textView.Foreground =
                    (SolidColorBrush)
                        converter.Convert(textColor, null, null, null);
            }


        }
    }
}
