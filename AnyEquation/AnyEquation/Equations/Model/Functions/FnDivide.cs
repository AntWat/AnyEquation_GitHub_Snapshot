using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model.Functions
{
    class FnDivide : NormalFunction
    {
        public const string StrFunctionName = "Divide";

        public FnDivide() :
            base(isOperator: true, name: StrFunctionName, funcLayout: FuncLayout.Term_OverOp_Term, symbol: "/", asciiSymbol: "/")
        {
        }

        // ---------------------------------------
        public override Result<Dimensions> CalcDimensions(IList<Dimensions> argDimensions, IList<double?> argConstants)
        {
            if (argDimensions.Count != 2) return Result<Dimensions>.Bad(Name + " must have 2 arguments");

            Dimensions resDim = Dimensions.SubtractDimensions(argDimensions[0], argDimensions[1]);
            return Result<Dimensions>.Good(resDim);
        }

        public override CalcQuantity Calculate_Here(IList<CalcQuantity> args, UOMSet uomSet)
        {
            if (args.Count != 2) return new BadCalcQuantity(Name + " must have 2 arguments");

            if ((args[0].CalcStatus != CalcStatus.Good) || (args[1].CalcStatus != CalcStatus.Good)) return new BadCalcQuantity("Missing or bad arguments");

            try
            {
                Dimensions resDim = Dimensions.SubtractDimensions(args[0]?.AnonUOM?.Dimensions, args[1]?.AnonUOM?.Dimensions);
                AnonUOM resAnonUOM = new AnonUOM(resDim, uomSet);

                CalcQuantity result = CalcQuantity.NewMathResult(
                    value: (args[0].Value / args[1].Value),
                    anonUOM: resAnonUOM);
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
                rearrangment = $"{resultName} * {varNames[1]}";
            }
            else if (varRequired == 1)
            {
                rearrangment = $"{varNames[0]} / {resultName}";
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
