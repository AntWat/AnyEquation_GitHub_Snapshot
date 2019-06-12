using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using AnyEquation.Common;

[assembly: Dependency(typeof(AnyEquation.UWP.Logger))]

namespace AnyEquation.UWP
{
    class Logger : ILogger
    {
        public void WriteLine(string message)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("AnyEquation", "Dbg:" + message);
            //System.Diagnostics.Trace.WriteLine("AnyEquation", "Dbg:" + message);
            //System.Console.WriteLine("AnyEquation", "Dbg:" + message);


            //System.Diagnostics.Debug.  .Console.WriteLine("AnyEquation", "Dbg:" + message);
#else
            System.Diagnostics.Debug.WriteLine("AnyEquation", "Rls:" + message);
#endif
        }
    }
}
