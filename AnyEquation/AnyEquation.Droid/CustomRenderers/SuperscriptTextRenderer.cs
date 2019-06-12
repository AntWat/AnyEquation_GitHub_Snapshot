using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AnyEquation.Common;

using Android.Util;
using Android.Widget;
using Android.Views;

[assembly: ExportRenderer(typeof(SuperscriptText),
                          typeof(AnyEquation.Droid.CustomRenderers.SuperscriptTextRenderer))]

namespace AnyEquation.Droid.CustomRenderers
{
    public class SuperscriptTextRenderer : ViewRenderer<SuperscriptText, TextView>
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

            fSize = (float)0.7 * fSize;

            if (Control == null)
            {
                SetNativeControl(new TextView(Context)
                {
                    Text = sText,
                });

                TextView textView = (TextView)Control;

                textView.SetTextColor(ColorExtensions.ToAndroid(textColor));
                textView.Gravity = GravityFlags.Top | GravityFlags.Left;

                Control.SetTextSize(ComplexUnitType.Sp, fSize);
            }


        }
    }
}
