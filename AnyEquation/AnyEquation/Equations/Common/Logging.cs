using AnyEquation.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Common
{
    public class Logging
    {
        public static void LogException(Exception ex)
        {
            Debugger.Break();

            // TODO: Debug.WriteLine no longer produces any output in VS 2017 Community edition.  
            // See: https://stackoverflow.com/questions/42907667/console-writeline-does-not-output-to-output-window-in-vs-2017
            // Find another debug trace method
            Logger.WriteLine(ex.Message);
        }

        public static void LogMessage(string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            string fullMsg = $"{message}  // [{memberName}]";
            Debugger.Break();

            // TODO: Debug.WriteLine no longer produces any output in VS 2017 Community edition.  
            // See: https://stackoverflow.com/questions/42907667/console-writeline-does-not-output-to-output-window-in-vs-2017
            // Find another debug trace method
            Logger.WriteLine(fullMsg);
        }
    }
}
