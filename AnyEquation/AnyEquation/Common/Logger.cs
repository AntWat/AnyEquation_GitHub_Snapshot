using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    class Logger
    {
        public static void WriteLine(string message)
        {
            ILogger debugger = DependencyService.Get<ILogger>(DependencyFetchTarget.GlobalInstance);
            debugger.WriteLine(message);
        }
    }
}
