using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model.Functions
{
    class FnLog : FnOneArg
    {
        public const string StrFunctionName = "Log";

        public FnLog() :
            base(StrFunctionName, _dimensionlessDim, _dimensionlessDim)
        { }

        public static Dimensions _dimensionlessDim { get; } = new Dimensions();

        protected override double CalcValue(double number) { return Math.Log(number); }
        protected override string InverseName() { return "Exp"; }
    }

    class FnExp : FnOneArg
    {
        public const string StrFunctionName = "Exp";

        public FnExp() :
            base(StrFunctionName, _dimensionlessDim, _dimensionlessDim)
        { }

        public static Dimensions _dimensionlessDim { get; } = new Dimensions();

        protected override double CalcValue(double number) { return Math.Exp(number); }
        protected override string InverseName() { return "Log"; }
    }
}
