using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model.Functions
{
    abstract class FnOneArg : NormalFunction
    {
        public FnOneArg(string name, Dimensions argDimensions, Dimensions resultDimensions) :
            base(false, name, FuncLayout.FuncLayout, name, name)
        {
            ArgDimensions = argDimensions;
            ResultDimensions = resultDimensions;
        }

        protected abstract double CalcValue(double angle);
        protected abstract string InverseName();

        public Dimensions ArgDimensions { get; }
        public Dimensions ResultDimensions { get; }

        // ---------------------------------------

        public override Result<Dimensions> CalcDimensions(IList<Dimensions> argDimensions, IList<double?> argConstants)
        {
            if (argDimensions.Count != 1) return Result<Dimensions>.Bad(Name + " must have 1 argument");

            if (!Dimensions.Equals(argDimensions[0], ArgDimensions)) return Result<Dimensions>.Bad("Bad argument dimensions");

            return Result<Dimensions>.Good(ResultDimensions);
        }

        public override CalcQuantity Calculate_Here(IList<CalcQuantity> args, UOMSet uomSet)
        {
            if (args.Count != 1) return new BadCalcQuantity(Name + " must have 1 argument");

            if ((args[0].CalcStatus != CalcStatus.Good)) return new BadCalcQuantity("Missing or bad arguments");

            if (!Dimensions.Equals(args[0]?.AnonUOM?.Dimensions, ArgDimensions)) return new BadCalcQuantity("Bad argument dimensions");

            try
            {
                AnonUOM resAnonUOM = new AnonUOM(ResultDimensions, uomSet);

                CalcQuantity result = CalcQuantity.NewMathResult(
                    value: CalcValue(args[0].Value),
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

            if (varNames.Count != 1) return Result<string>.Bad(Name + " must have 1 argument");

            if (varRequired == 0)
            {
                rearrangment = $"{InverseName()}({resultName})";
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
