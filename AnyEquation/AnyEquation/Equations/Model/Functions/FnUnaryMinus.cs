using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model.Functions
{
    public class FnUnaryMinus : NormalFunction
    {
        public const string StrFunctionName = "UnaryMinus";

        public FnUnaryMinus() :
            base(isOperator: true, name: StrFunctionName, funcLayout: FuncLayout.Op_Term, symbol: "-", asciiSymbol: "-")
        {
        }

        // ---------------------------------------

        public override Result<Dimensions> CalcDimensions(IList<Dimensions> argDimensions, IList<double?> argConstants)
        {
            if (argDimensions.Count != 1) return Result<Dimensions>.Bad(Name + " must have 1 argument");

            return Result<Dimensions>.Good(argDimensions[0]);
        }

        public override CalcQuantity Calculate_Here(IList<CalcQuantity> args, UOMSet uomSet)
        {
            if (args.Count != 1) return new BadCalcQuantity(Name + " must have 1 argument");

            if (args[0].CalcStatus != CalcStatus.Good) return new BadCalcQuantity("Missing or bad arguments");

            try
            {
                CalcQuantity result = CalcQuantity.NewMathResult(
                    value: (-1*args[0].Value),
                    anonUOM: args[0].AnonUOM);
                return result;
            }
            catch (Exception)
            {
                return new BadCalcQuantity("Calculation error");
            }
        }

        // ---------------------------------------

        public override Result<string> GetRearrangement(IList<string> varNames, string resultName, int varRequired)
        {
            string rearrangment = null;

            if (varNames.Count != 1) return Result<string>.Bad(Name + " must have 1 argument");

            if (varRequired == 0)
            {
                rearrangment = $"+{resultName}";
            }
            else
            {
                return Result<string>.Bad($"Bad value for arg 'varRequired': {varRequired}");
            }

            if (string.IsNullOrEmpty(rearrangment))
            {
                return Result<string>.Bad($"Rearrangement failed");
            }
            return Result<string>.Good(rearrangment);
        }

    }
}
