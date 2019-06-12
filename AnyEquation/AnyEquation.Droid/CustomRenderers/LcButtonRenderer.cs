using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AnyEquation.Common;

using Android.Util;

[assembly: ExportRenderer(typeof(LcButton),
                          typeof(AnyEquation.Droid.CustomRenderers.LcButtonRenderer))]

namespace AnyEquation.Droid.CustomRenderers
{
    /// <summary>
    /// Allows a button to show lower case text, using a custom renderer, since Android apparently capitalizes button text by default
    /// </summary>
    class LcButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);
            var button = this.Control;
            button.SetAllCaps(false);
        }
    }
}