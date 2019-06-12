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
using AnyEquation.Common;

[assembly: Dependency(typeof(AnyEquation.Droid.Logger))]

namespace AnyEquation.Droid
{
    class Logger : ILogger
    {
        public void WriteLine(string message)
        {
#if DEBUG
            Android.Util.Log.Debug("AnyEquation", "Dbg:" + message);
            //System.Console.WriteLine("AnyEquation: Dbg2:" + message);
#else
            Android.Util.Log.Debug("AnyEquation", "Rls:" + message);
#endif
        }
    }
}