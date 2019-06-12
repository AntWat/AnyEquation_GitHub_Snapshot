using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace AnyEquation.Droid
{
    [Activity(Label = "AnyEquation", Icon = "@drawable/icon", Theme = "@style/MainTheme", 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
        //MainLauncher = true, 
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            // --------------------- 
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            // --------------------- 
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            // --------------------- From PanGesture sample
            var width = Resources.DisplayMetrics.WidthPixels;
            var height = Resources.DisplayMetrics.HeightPixels;
            var density = Resources.DisplayMetrics.Density;

            AnyEquation.App.ScreenWidth = (width - 0.5f) / density;
            AnyEquation.App.ScreenHeight = (height - 0.5f) / density;

            // --------------------- 
            LoadApplication(new App());
        }
    }
}

