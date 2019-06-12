using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{
    public class CalcQuantity : Quantity
    {
        public CalcQuantity() { }

        public CalcQuantity(double value, CalcStatus calcStatus, string message, AnonUOM anonUOM) :
            base(value, calcStatus, message)
        {
            AnonUOM = anonUOM;
        }

        public CalcQuantity(CalcQuantity source)
        {
            source.CopyMe(this);
        }

        public AnonUOM AnonUOM { get; set; }
        public override UOM GetUOM()
        {
            return AnonUOM;
        }

        public override void CopyMe(Quantity tgtQuantity)
        {
            base.CopyMe(tgtQuantity);

            if (tgtQuantity is CalcQuantity tgtCalcQuantity)
            {
                tgtCalcQuantity.AnonUOM = AnonUOM;
            }
        }

        // -----------------------------------------
        public static CalcQuantity NewMathResult(double value, AnonUOM anonUOM)
        {
            CalcStatus calcStatus = CalcStatus.Good;
            string message = "";

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                calcStatus = CalcStatus.Bad;
                message = "Math error";
            }

            CalcQuantity result = new CalcQuantity(
                value: value,
                calcStatus: calcStatus, message: message, anonUOM: anonUOM);
            return result;
        }
    }
}
