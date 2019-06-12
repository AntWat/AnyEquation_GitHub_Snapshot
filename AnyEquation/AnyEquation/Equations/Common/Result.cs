using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Common
{
    public class Result<T>
    {
        Result(StatusAndMessage statusAndMessage, T theValue)
        {
            StatusAndMessage = statusAndMessage;
            Value = theValue;
        }
        Result(StatusAndMessage statusAndMessage)
        {
            StatusAndMessage = statusAndMessage;
        }

        public StatusAndMessage StatusAndMessage { get; set; }
        public T Value { get; set; }


        // --------------------------------------------
        public CalcStatus CalcStatus { get { return StatusAndMessage.CalcStatus; } }
        public string Message { get { return StatusAndMessage.Message; } }

        public bool IsGood()
        { return StatusAndMessage.CalcStatus == CalcStatus.Good; }

        public bool IsBad()
        { return StatusAndMessage.CalcStatus == CalcStatus.Bad; }

        public bool IsUknown()
        { return StatusAndMessage.CalcStatus == CalcStatus.Uknown; }

        public bool IsNotGood()
        { return !IsGood(); }


        // --------------------------------------------
        public static Result<T> Bad(string msg = "Undefined error")
        {
            return new Result<T>(StatusAndMessage.Bad(msg));
        }
        public static Result<T> Bad(Exception ex)
        {
            return new Result<T>(StatusAndMessage.Bad(ex));
        }

        public static Result<T> Good(T theValue)
        {
            return new Result<T>(StatusAndMessage.Good(), theValue);
        }

        public static Result<T> Uknown()
        {
            return new Result<T>(StatusAndMessage.Uknown());
        }

    }

    // -------------------------------------------------------------------------

    public enum CalcStatus
    {
        Uknown, Good, Bad
    }

    public struct StatusAndMessage
    {
        public StatusAndMessage(CalcStatus calcStatus, string message)
        {
            CalcStatus = calcStatus;
            Message = message;
        }

        public CalcStatus CalcStatus { get; set; }
        public string Message { get; set; }


        public static StatusAndMessage Bad(string msg = "Undefined error")
        {
            return new StatusAndMessage(CalcStatus.Bad, msg);
        }
        public static StatusAndMessage Bad(Exception ex)
        {
            return Bad(ex?.Message);
        }

        public static StatusAndMessage Good()
        {
            return new StatusAndMessage(CalcStatus.Good, "");
        }

        public static StatusAndMessage Uknown()
        {
            return new StatusAndMessage(CalcStatus.Uknown, "");
        }

    }

    public class ErrorUtils
    {

        // --------------------------------------------
        public static string UnspecifiedErrorMsg(string methodName, string msg = "", [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return string.Format("Unexpected error in {0}: {1}", methodName, msg);
        }
        public static string UnspecifiedErrorMsg(string methodName, Exception ex)
        {
            return UnspecifiedErrorMsg(methodName, ex?.Message);
        }
        public static string UnspecifiedErrorMsg2(string msg = "", [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return string.Format("Unexpected error in {0}: {1}", memberName, msg);
        }

    }

}