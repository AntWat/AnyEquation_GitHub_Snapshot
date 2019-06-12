using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model.Functions
{
    public class FnSubtract : NormalFunction
    {
        public const string StrFunctionName = "Subtract";

        public FnSubtract() :
            base(isOperator: true, name: StrFunctionName, funcLayout: FuncLayout.Term_Op_Term, symbol: "-", asciiSymbol: "-")
        {
        }

        // ---------------------------------------

        public override Result<Dimensions> CalcDimensions(IList<Dimensions> argDimensions, IList<double?> argConstants)
        {
            if (argDimensions.Count != 2) return Result<Dimensions>.Bad(Name + " must have 2 arguments");

            if (argDimensions[0] == null && argDimensions[1] == null) return Result<Dimensions>.Good(null);

            if (!Dimensions.AreEqual(argDimensions[0], argDimensions[1])) return Result<Dimensions>.Bad("Inconsistent Dimensions");

            return Result<Dimensions>.Good(argDimensions[0]);
        }

        public override CalcQuantity Calculate_Here(IList<CalcQuantity> args, UOMSet uomSet)
        {
            if (args.Count != 2) return new BadCalcQuantity(Name + " must have 2 arguments");

            if ((args[0].CalcStatus != CalcStatus.Good) || (args[1].CalcStatus != CalcStatus.Good)) return new BadCalcQuantity("Missing or bad arguments");

            if (!AnonUOM.EqualOrNull(args[0].AnonUOM,args[1].AnonUOM)) return new BadCalcQuantity("Inconsistent Units");

            try
            {
                CalcQuantity result = CalcQuantity.NewMathResult(
                    value: (args[0].Value - args[1].Value),
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

            if (varNames.Count != 2) return Result<string>.Bad(Name + " must have 2 arguments");

            if (varRequired == 0)
            {
                rearrangment = $"{resultName} + {varNames[1]}";
            }
            else if (varRequired == 1)
            {
                rearrangment = $"{varNames[0]} - {resultName}";
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
