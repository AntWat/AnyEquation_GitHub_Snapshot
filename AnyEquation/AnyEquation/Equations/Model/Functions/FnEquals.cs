using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{

    /// <summary>
    /// Represents the = sign in an equation.  
    /// When calculated it assigns the unknown value to the other one (e.g. makes left hand side = right hand side).
    /// It then returns the imbalance between the two sides, and an associated status.
    /// The status is Good if there is no imbalance.
    /// </summary>
    public class FnEquals : Function
    {
        public const string StrFunctionName = "Equals";

        public FnEquals() :
            base(isOperator:true, name: StrFunctionName, funcLayout: FuncLayout.Term_Op_Term, symbol:"=", asciiSymbol:"=", asciiLayout: FuncLayout.Term_Op_Term)
        {
        }

        public override bool IsNormalFunction { get { return false; } }      // If true, calculation will only be called if all the args are known, and we don't expect the calculation to change any of the args

        // ---------------------------------------

        public override Result<Dimensions> CalcDimensions(IList<Dimensions> argDimensions, IList<double?> argConstants)
        {
            if (argDimensions.Count != 2) return Result<Dimensions>.Bad(Name + " must have 2 arguments");

            if (argDimensions[0] == null && argDimensions[1] == null) return Result<Dimensions>.Good(null);

            if (!Dimensions.AreEqual(argDimensions[0], argDimensions[1])) return Result<Dimensions>.Bad("Inconsistent Dimensions");

            return Result<Dimensions>.Good(argDimensions[0]);
        }


        /// This is the function implemented by each concrete class to actually do the calculation.
        /// Args will be converted to a consistent UOMSet, unless there are all UOMless, before calling the actual function calc.
        /// If the function requires any other specific UOM it must do the conversion itself.
        public override CalcQuantity Calculate_Here(IList<CalcQuantity> args, UOMSet uomSet)
        {
            if (args.Count != 2) return new BadCalcQuantity(Name + " must have 2 arguments");

            if ((args[0].CalcStatus == CalcStatus.Bad) || (args[1].CalcStatus == CalcStatus.Bad)) return new BadCalcQuantity("Bad arguments");

            if ((args[0].CalcStatus == CalcStatus.Good) && (args[1].CalcStatus == CalcStatus.Good))
            {
                if (args[0].Value == args[1].Value)     // TODO: Equations: Tolerance?
                {
                    return new CalcQuantity(0, CalcStatus.Good,"", args[0].AnonUOM);
                }
                else
                {
                    return new CalcQuantity((args[0].Value - args[1].Value), CalcStatus.Bad, "Both sides are known but not equal", args[0].AnonUOM);     // Return imbalance
                }
            }
            else if ((args[0].CalcStatus == CalcStatus.Good) && (args[1].CalcStatus == CalcStatus.Uknown))
            {
                args[1].CalcStatus = args[0].CalcStatus;
                args[1].AnonUOM = args[0].AnonUOM;
                args[1].Value = args[0].Value;
                return new CalcQuantity(0, CalcStatus.Good, "", args[0].AnonUOM);
            }
            else if ((args[1].CalcStatus == CalcStatus.Good) && (args[0].CalcStatus == CalcStatus.Uknown))
            {
                args[0].CalcStatus = args[1].CalcStatus;
                args[0].AnonUOM = args[1].AnonUOM;
                args[0].Value = args[1].Value;
                return new CalcQuantity(0, CalcStatus.Good, "", args[0].AnonUOM);
            }
            else if ((args[0].CalcStatus == CalcStatus.Uknown) && (args[1].CalcStatus == CalcStatus.Uknown))
            {
                return new CalcQuantity(0, CalcStatus.Uknown, "", args[0].AnonUOM);
            }
            return new BadCalcQuantity("Unexpected failure");       // Should never happen
        }

        // ---------------------------------------

        public override Result<string> GetRearrangement(IList<string> varNames, string resultName, int varRequired)
        {
            return null;        // Nothing required
        }

    }
}
