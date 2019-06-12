using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{
    public abstract class Quantity
    {
        public Quantity()
        {
            Value = Double.NaN;
        }

        public Quantity(double value, CalcStatus calcStatus, string message)
        {
            Value = value;
            CalcStatus = calcStatus;
            Message = message;
        }

        public double Value { get; set; }
        public CalcStatus CalcStatus { get; set; }
        public string Message { get; set; }     // Used to return simple error messages from calculation etc.

        public abstract UOM GetUOM();

        public virtual void CopyMe(Quantity tgtQuantity)
        {
            tgtQuantity.Value = Value;
            tgtQuantity.CalcStatus = CalcStatus;
            tgtQuantity.Message = Message;
        }

        public void Clear()
        {
            Value = Double.NaN;
            CalcStatus = CalcStatus.Uknown;
            Message = "";
        }
    }
}
