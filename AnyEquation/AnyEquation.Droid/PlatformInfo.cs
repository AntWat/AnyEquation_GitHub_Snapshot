using System;
using Android.OS;
using Xamarin.Forms;
using AnyEquation.Common;
using Android.Content;

[assembly: Dependency(typeof(AnyEquation.Droid.PlatformInfo))]

namespace AnyEquation.Droid
{
    public class PlatformInfo : IPlatformInfo
    {
        public string GetModel()
        {
            return String.Format("{0} {1}", Build.Manufacturer,
                                            Build.Model);
        }

        public string GetVersion()
        {
            return Build.VERSION.Release.ToString();
        }

        public string GetAppVersion()
        {
            Context context = Forms.Context;
            return context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
        }
    }
}
