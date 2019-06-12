using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    /// <summary>
    /// This is used to represent a UOM manupulation that has gone wrong.  
    /// For example, if you multiple an SI Units value with a British units value you cannot say what units the result is in, so we mark it with this.
    /// </summary>
    public class BadUOM : UOM
    {
        public BadUOM() : base(null)
        {
        }

        //// ---------------------------

        //public override double Convert(double value, UOM toUOM)
        //{
        //    return Double.NaN;
        //}
    }
}
