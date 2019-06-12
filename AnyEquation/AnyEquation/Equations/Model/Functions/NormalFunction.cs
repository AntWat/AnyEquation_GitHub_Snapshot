using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    /// <summary>
    /// This level is currently just used to distinguish a 'NormalFunction' (that calculates a result from args)
    /// from an equation that calculates one arg from the other.
    /// </summary>
    public abstract class NormalFunction : Function
    {
        public NormalFunction(bool isOperator, string name, FuncLayout funcLayout, string symbol, string asciiSymbol, FuncLayout asciiLayout = FuncLayout.Unspecified) :
            base(isOperator, name, funcLayout, symbol, asciiSymbol, (asciiLayout==FuncLayout.Unspecified ? funcLayout : asciiLayout) )
        {
        }

        public override bool IsNormalFunction { get { return true; } }      // If true, calculation will only be called if all the args are known, and we don't expect the calculation to change any of the args
    }
}
