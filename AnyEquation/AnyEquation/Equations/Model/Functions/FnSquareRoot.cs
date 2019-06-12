using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model.Functions
{
    public class FnSquareRoot : NormalFunction
    {
        FnPower fnPower = new FnPower();
        CalcQuantity exponent = new CalcQuantity(0.5, CalcStatus.Good, "", new AnonUOM(Dimensions.DimensionLessInstance, null));

        public const string StrFunctionName = "SquareRoot";

        public FnSquareRoot() :
            base(isOperator: true, name: StrFunctionName, funcLayout: FuncLayout.Op_Term, symbol: "\u221A", asciiSymbol: "Sqrt", asciiLayout: FuncLayout.FuncLayout)
        {
        }

        // ---------------------------------------

        public override Result<Dimensions> CalcDimensions(IList<Dimensions> argDimensions, IList<double?> argConstants)
        {
            IList<Dimensions> argDimensions2 = new List<Dimensions>(argDimensions);
            IList<double?> argConstants2 = new List<double?>(argConstants);

            argDimensions2.Add(new Dimensions());       // DimensionLess
            argConstants2.Add(exponent.Value);

            return fnPower.CalcDimensions(argDimensions2, argConstants2);
        }

        public override CalcQuantity Calculate_Here(IList<CalcQuantity> args, UOMSet uomSet)
        {
            if (args.Count != 1) return new BadCalcQuantity(Name + " must have 1 argument");

            if (args[0].CalcStatus != CalcStatus.Good) return new BadCalcQuantity("Missing or bad arguments");

            try
            {
                CalcQuantity result = fnPower.Calculate_Here(new List<CalcQuantity>() { args[0], exponent }, uomSet);
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
                rearrangment = $"{resultName}^2";
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
