using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{
    class BadCalcQuantity : CalcQuantity
    {
        public BadCalcQuantity(string message) :
            base(Double.NaN, CalcStatus.Bad, message, null)
        {
        }
    }
}
