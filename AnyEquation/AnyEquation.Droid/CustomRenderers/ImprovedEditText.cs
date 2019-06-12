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
using Xamarin.Forms.Platform.Android;
using System.ComponentModel;

namespace AnyEquation.Droid.CustomRenderers
{
    public class ImprovedEditText : EditText
    {
        public ImprovedEditText(Context context) : base(context)
        { }

        //public void DoOnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    base.OnElementPropertyChanged(sender, e);
        //}
    }
}