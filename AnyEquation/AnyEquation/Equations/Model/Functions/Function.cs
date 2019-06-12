using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{
    /// <summary>
    /// Class implementing a function calculation but not holding any results.
    /// These should effectively be singletons, but this will not be enforced (for efficiency and flexibility when testing).
    /// 
    /// </summary>
    public abstract class Function
    {
        public Function(bool isOperator, string name, FuncLayout funcLayout, string symbol, string asciiSymbol, FuncLayout asciiLayout)
        {
            IsOperator = isOperator;
            Name = name;
            FuncLayout = funcLayout;
            Symbol = symbol;

            AsciiLayout = asciiLayout;
            AsciiSymbol = asciiSymbol;
        }

        public abstract bool IsNormalFunction { get; }      // If true, calculation will only be called if all the args are known, and we don't expect the calculation to change any of the args

        public bool IsOperator { get; set; }
        public string Name { get; set; }
        public FuncLayout FuncLayout { get; set; }
        public string Symbol { get; set; }      // For fancy symbols use "\u22C5" etc., which is Hex column in tables at : https://www.w3schools.com/charsets/ref_utf_math.asp

        public FuncLayout AsciiLayout { get; set; }     // Alternative layout when we need a text representation
        public string AsciiSymbol { get; set; }     // Safe symbol to use in pure text scenarios

        // ---------------------------
        /// <summary>
        /// Calculate the function as much as possible.
        /// For 'NormalFunction' objects we expect that the result is calculated from the args, and we do not change the args.
        /// However, this is not enforced at this time.
        /// Special sub-types, such as 'FnEquals', behave differently.
        /// 
        /// Args will be converted to a consistent UOMSet, unless they are all UOMless, before calling the actual function calc.
        /// If the function requires any other specific UOM it must do the conversion itself.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public CalcQuantity Calculate(IList<CalcQuantity> args)
        {
            CalcQuantity result = new CalcQuantity();

            try
            {
                // ------------------- Loop through all the args, checking and converting
                UOMSet uomSet = null;

                IList<CalcQuantity> convertedArgs = new List<CalcQuantity>();

                bool bStarted = false;
                for (int i = 0; i < args.Count; i++)
                {
                    CalcQuantity arg = args[i];

                    // ------------------- Check for a Bad or missing value
                    if (IsNormalFunction)       // Otherwise more flexibility is allowed so we can't demand that all args are known before calculation
                    {
                        switch (arg.CalcStatus)
                        {
                            case CalcStatus.Uknown:
                            case CalcStatus.Bad:
                                result.CalcStatus = arg.CalcStatus; result.Message = "Missing or bad arguments";
                                return result;

                            default:
                                break;
                        }
                    }


                    // ------------------- Convert the arg to the same UOMSET
                    CalcQuantity convertedArg = new CalcQuantity(arg);
                    bool bDimLess = Dimensions.IsDimenionless(convertedArg?.AnonUOM?.Dimensions);

                    if (!bDimLess)
                    {
                        if ((uomSet == null) & (!bStarted || !IsNormalFunction) )
                        {
                            bStarted = true;
                            uomSet = convertedArg?.AnonUOM?.UOMSet;
                        }
                        else if (IsNormalFunction && (convertedArg?.AnonUOM?.UOMSet != uomSet))
                        {
                            if ((uomSet == null) || (convertedArg?.AnonUOM?.UOMSet == null))
                            {
                                result.CalcStatus = CalcStatus.Bad; result.Message = "Some arguments have units while others do not";       // They must all have a specified UOMSet, or none of them have
                                return result;
                            }
                            else
                            {
                                // Convert to the uomSet used by other args
                                AnonUOM argUOM = convertedArg.AnonUOM;
                                convertedArg.Value = argUOM.UOMSet.Convert(arg.Value, argUOM.Dimensions, uomSet);
                                convertedArg.AnonUOM = new AnonUOM(argUOM.Dimensions, uomSet);
                            }
                        }
                    }

                    convertedArgs.Add(convertedArg);
                }

                
                // ------------------- Call the concrete implementations to actually do the calcualtion
                result = Calculate_Here(convertedArgs, uomSet);

                if (!IsNormalFunction)      // Then some args might have been changed by the calculation (e.g. assignment in FnEquals)
                {
                    for (int i = 0; i < convertedArgs.Count; i++)
                    {
                        convertedArgs[i].CopyMe(args[i]);
                    }
                }
            }
            catch (Exception)
            {
                return new CalcQuantity(0, CalcStatus.Bad, "Unexpected failure", null);
            }

            return result;
        }


        /// This is the function implemented by each concrete class to actually do the calculation.
        /// Args will be converted to a consistent UOMSet, unless there are all UOMless, before calling the actual function calc.
        /// If the function requires any other specific UOM it must do the conversion itself.
        public abstract CalcQuantity Calculate_Here(IList<CalcQuantity> args, UOMSet uomSet);


        // ---------------------------
        /// This is the function implemented by each concrete class to check cthat the function arguments have consistent dimensions
        /// <param name="argConstants">Needed for exponents to check that they are dimensionles and to calculate the result dimensions</param>
        public abstract Result<Dimensions> CalcDimensions(IList<Dimensions> argDimensions, IList<double?> argConstants);

        // ---------------------------

        /// Used to invert a function fo we can rearrange an equation
        /// <param name="varNames">Names to use for the current input variables</param>
        /// <param name="resultName">Name for the current righ hand side</param>
        /// <param name="varRequired"></param>
        /// <returns>A text representation of the new right hand side.
        /// Return null or empty string if the rearragnment is not possible</returns>
        public virtual Result<string> GetRearrangement(IList<string> varNames, string resultName, int varRequired) { return null; }

    }

    // -------------------------------------------------------------------------

    public enum FuncLayout
    {
        Unspecified
,        FuncLayout,
        Term_Op_Term,
        Term_OverOp_Term,
        Term_Superscript_Term,
        Op_Term
    }

}
