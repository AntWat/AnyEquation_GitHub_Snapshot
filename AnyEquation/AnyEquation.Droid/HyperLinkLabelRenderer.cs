
// TODO: When I try to use this it tries to turn any recognisable link text into a link.
// This seems to conflict with the implementation of the main HyperLinkLabel code, in which the link is processed explicity,
// rather than through an Android class.
// For now, I am setting the build action to None...



// ***********************************************************************
// The code in this file came from: https://github.com/XLabs/Xamarin-Forms-Labs/wiki/HyperLinkLabel
//       and was licensed under the Apache 2.0 license
//       https://github.com/XLabs/Xamarin-Forms-Labs/blob/master/LICENSE
//       XLabs is a open source project that aims to provide a powerfull and cross 
//       platform set of controls tailored to work with Xamarin Forms.
// ***********************************************************************
// 

using Android.Text.Util;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AnyEquation.Common;
using AnyEquation.Droid;

[assembly: ExportRenderer(typeof(HyperLinkLabel), typeof(HyperLinkLabelRenderer))]

namespace AnyEquation.Droid
{
    /// <summary>
    /// Class HyperLinkLabelRenderer.
    /// </summary>
    public class HyperLinkLabelRenderer : LabelRenderer
    {
        /// <summary>
        /// Called when [element changed].
        /// </summary>
        /// <param name="e">The e.</param>
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {

                var nativeEditText = (global::Android.Widget.TextView)Control;

                Linkify.AddLinks(nativeEditText, MatchOptions.All);
            }
        }
    }
}