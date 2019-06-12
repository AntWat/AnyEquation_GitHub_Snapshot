using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;
using AnyEquation.Common;

namespace AnyEquation.Equations.Model
{
    public class FunctionCalc : SingleResult
    {
        public FunctionCalc()
        {
            Inputs = new List<SingleResult>();
        }

        public FunctionCalc(Function function)
        {
            Function = function;
            Inputs = new List<SingleResult>();
        }

        public FunctionCalc(Function function, IList<SingleResult> inputs)
        {
            Function = function;
            Inputs = inputs;
        }


        #region ------------ Fields and Properties

        public Function Function { get; }
        public IList<SingleResult> Inputs { get; set; }

        public IDictionary</*variable name*/string, Dimensions> ExpectedDimensions { get; set; }       // Used to check calculation works as expected
        public IDictionary</*variable name*/string, double> TestValues { get; set; }       // Used to check calculation works as expected

        public CalcStatus GetCalcStatus()
        {
            return CalcQuantity?.CalcStatus ?? CalcStatus.Bad;
        }
        public void SetCalcStatus(CalcStatus calcStatus, string message = null)
        {
            if (CalcQuantity == null)
            {
                CalcQuantity = new CalcQuantity();
            }
            CalcQuantity.CalcStatus = calcStatus;
            if (message != null)
            {
                CalcQuantity.Message = message;
            }
        }

        #endregion ------------ Fields and Properties


        #region ------------ Calculate

        public CalcStatus CalculateAll()
        {
            try
            {
                IList<SingleResult> callStack = new List<SingleResult>();
                Calculate_Recursive(callStack);
            }
            catch (Exception ex)
            {
                SetCalcStatus(CalcStatus.Bad, $"Unexpected error in CalculateAll: {ex.Message}");
            }

            return GetCalcStatus();
        }

        /// <summary>
        /// Recursively calculate all sub-levels and this level.
        /// Determine all the results and information possible.
        /// Guard against a mistake in the tree that causes infinite recursion.
        /// </summary>
        /// <returns></returns>
        private void Calculate_Recursive(IList<SingleResult> callStack)
        {
            foreach (SingleResult input in Inputs)
            {
                if (input is FunctionCalc)
                {
                    FunctionCalc subFunctionCalc = (input as FunctionCalc);

                    if (callStack.IndexOf(input)>=0)
                    {
                        throw new UnspecifiedException("Infinite recursion in Expression Tree");
                    }
                    callStack.Add(input);
                    subFunctionCalc.Calculate_Recursive(callStack);
                    callStack.Remove(input);
                }
            }

            Calculate_Here();
        }

        /// <summary>
        /// Calculate this level as musch as possible, using whatever information is currently known.
        /// </summary>
        /// <returns></returns>
        private void Calculate_Here()
        {
            List<CalcQuantity> args = new List<CalcQuantity>();

            foreach (SingleResult item in Inputs)
            {
                args.Add(item.CalcQuantity);
            }

            CalcQuantity result = Function.Calculate(args);

            CalcQuantity = result;
        }


        #endregion ------------ Calculate


        #region ------------ Check Dimensions

        public CalcStatus CheckDimensionsAll()
        {
            Result<Dimensions> resDim = null;

            try
            {
                IList<SingleResult> callStack = new List<SingleResult>();
                resDim = CalcDimensions_Recursive(callStack);
            }
            catch (Exception ex)
            {
                resDim = Result<Dimensions>.Bad(ErrorUtils.UnspecifiedErrorMsg("CheckDimensionsAll", ex));
            }

            return resDim.CalcStatus;
        }

        /// <summary>
        /// Recursively calc param types for all sub-levels and this level.
        /// Guard against a mistake in the tree that causes infinite recursion.
        /// </summary>
        /// <returns></returns>
        private Result<Dimensions> CalcDimensions_Recursive(IList<SingleResult> callStack)
        {
            try
            {
                IList<Dimensions> argDimensions = new List<Dimensions>();
                IList<double?> argConstants = new List<double?>();

                foreach (SingleResult input in Inputs)
                {
                    double? constVal = null;
                    
                    if (input is SingleValue)
                    {
                        argDimensions.Add(input.CalcQuantity?.GetUOM()?.Dimensions);

                        if ((input is Literal) || (input is Constant))
                        {
                            constVal = input.CalcQuantity.Value;
                        }
                    }
                    else if (input is FunctionCalc)
                    {
                        FunctionCalc subFunctionCalc = (input as FunctionCalc);

                        if (callStack.IndexOf(input) >= 0)
                        {
                            return Result<Dimensions>.Bad("Infinite recursion in Expression Tree");
                        }

                        callStack.Add(input);
                        Result<Dimensions> subDims = subFunctionCalc.CalcDimensions_Recursive(callStack);
                        callStack.Remove(input);

                        if (subDims.IsBad()) { return Result<Dimensions>.Bad(subDims.Message); }

                        argDimensions.Add(subDims.Value);
                    }
                    else
                    {
                        return Result<Dimensions>.Bad(ErrorUtils.UnspecifiedErrorMsg("CalcDimensions_Recursive", ""));
                    }

                    argConstants.Add(constVal);
                }

                Result<Dimensions> resDims = Function.CalcDimensions(argDimensions, argConstants);

                return resDims;
            }
            catch (Exception ex)
            {
                return Result<Dimensions>.Bad(ErrorUtils.UnspecifiedErrorMsg("CalcDimensions_Recursive", ex));
            }
        }


        #endregion ------------ Calculate



        #region ------------ GetErrors

        /// <summary>
        /// Return information about errors
        /// </summary>
        /// <param name="lowestErrors">Errors that are not caused by errors in their inputs</param>
        /// <param name="allErrors">All errors, at all levels</param>
        public void GetErrors(ref IList<FunCalcError> lowestErrors, ref IList<FunCalcError> allErrors, string NumberFormat)
        {
            try
            {
                lowestErrors.Clear();
                allErrors.Clear();
                IList<SingleResult> callStack = new List<SingleResult>();

                GetErrors2(this, ref lowestErrors, ref allErrors, NumberFormat, callStack);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <returns>true if any errors are found at this level or below</returns>
        /// Guard against a mistake in the tree that causes infinite recursion.
        private static bool GetErrors2(FunctionCalc fnCalc, ref IList<FunCalcError> lowestErrors, ref IList<FunCalcError> allErrors, string NumberFormat, IList<SingleResult> callStack)
        {
            bool bRet=false;
            FunCalcError funCalcError=null;

            // ----------------------
            if (fnCalc.GetCalcStatus() == CalcStatus.Bad)
            {
                funCalcError = new FunCalcError(fnCalc.CalcQuantity?.Message, fnCalc);
                allErrors.Add(funCalcError);
                bRet = true;
            }

            // ----------------------
            bool bFoundLower = false;
            foreach (SingleResult input in fnCalc.Inputs)
            {
                bool bFoundMore = false;
                if (input is FunctionCalc)
                {
                    if (callStack.IndexOf(input) >= 0)
                    {
                        throw new UnspecifiedException("Infinite recursion in Expression Tree");
                    }
                    callStack.Add(input);
                    bFoundMore = GetErrors2(((FunctionCalc)input), ref lowestErrors, ref allErrors, NumberFormat, callStack);
                    callStack.Remove(input);
                }
                else 
                {
                    bFoundMore = GetErrors3(input, ref lowestErrors, ref allErrors, NumberFormat);
                }

                if (bFoundMore) bFoundLower = true;
            }

            // ----------------------
            if (bFoundLower)
            {
                bRet = true;
            }
            else if (funCalcError!=null)
            {
                lowestErrors.Add(funCalcError);
            }

            return bRet;
        }

        private static bool GetErrors3(SingleResult input, ref IList<FunCalcError> lowestErrors, ref IList<FunCalcError> allErrors, string NumberFormat)
        {
            bool bRet = false;

            if (input?.CalcQuantity?.CalcStatus == CalcStatus.Bad)
            {
                FunCalcError funCalcError = new FunCalcError(input?.CalcQuantity?.Message, input);
                allErrors.Add(funCalcError);
                lowestErrors.Add(funCalcError);
                bRet = true;
            }

            return bRet;
        }

        #endregion ------------ GetErrors



        #region ------------ GetCalculationTree

        /// <summary>
        /// Return information about how the expression was calculated
        /// </summary>
        /// <param name="sIndent">Indentation used for subsequent levels of calculationTree</param>
        /// <param name="calculationTree">A string representation of how the calculation was conducted, and the errors at different levels</param>
        public string GetCalculationTreeText(string sIndent, string NumberFormat)
        {
            try
            {
                string calculationTree = "";
                IList<SingleResult> callStack = new List<SingleResult>();

                GetCalculationTreeText2(this, ref calculationTree, "", sIndent, NumberFormat, callStack);

                return calculationTree;
            }
            catch (Exception ex)
            {
                return ErrorUtils.UnspecifiedErrorMsg("GetCalculationTree", ex);
            }
        }

        /// Guard against a mistake in the tree that causes infinite recursion.
        private static void GetCalculationTreeText2(FunctionCalc fnCalc, ref string calculationTree, string sCurrentIndent, string sIndentStep, string NumberFormat, IList<SingleResult> callStack)
        {
            if (fnCalc.Function is FnEquals)
            {
                calculationTree += string.Format("{0}{1} ({2}) {3}\n",
                                    sCurrentIndent, fnCalc.Function.Name, fnCalc.Function.AsciiSymbol,
                                    (fnCalc.CalcQuantity.Message.Length == 0) ? "" : (" // " + fnCalc.CalcQuantity.Message));
            }
            else
            {
                calculationTree += string.Format("{0}{1} ({2}) = {3}{4}\n",
                                    sCurrentIndent, fnCalc.Function.Name, fnCalc.Function.AsciiSymbol,
                                    ((fnCalc.GetCalcStatus() == CalcStatus.Good) ?
                                       string.Format("{0:" + NumberFormat + "}", fnCalc.CalcQuantity.Value) :
                                       "!"),
                                    (fnCalc.CalcQuantity.Message.Length == 0) ? "" : (" // " + fnCalc.CalcQuantity.Message));
            }

            // ----------------------
            foreach (SingleResult input in fnCalc.Inputs)
            {
                if (input is FunctionCalc)
                {
                    if (callStack.IndexOf(input) >= 0)
                    {
                        throw new UnspecifiedException("Infinite recursion in Expression Tree");
                    }
                    callStack.Add(input);
                    GetCalculationTreeText2(((FunctionCalc)input), ref calculationTree, (sCurrentIndent + sIndentStep), sIndentStep, NumberFormat, callStack);
                    callStack.Remove(input);
                }
                else
                {
                    FunctionCalc.GetCalculationTreeText3(input, ref calculationTree, (sCurrentIndent + sIndentStep), NumberFormat);
                }
            }
        }

        private static void GetCalculationTreeText3(SingleResult input, ref string calculationTree, string sCurrentIndent, string NumberFormat)
        {
            calculationTree += string.Format("{0}{1}{2}{3}\n",
                                sCurrentIndent,
                                (input?.Name?.Length == 0) ? "" : string.Format("{0} = ", input?.Name),
                                (
                                 (input?.CalcQuantity?.CalcStatus == CalcStatus.Good) ?
                                   string.Format("{0:" + NumberFormat + "}", input?.CalcQuantity?.Value) :
                                   "Missing or bad value"
                                ),
                                (input?.CalcQuantity?.Message?.Length == 0) ? "" : (" // " + input?.CalcQuantity?.Message));
        }

        #endregion ------------ GetCalculationTree




        #region ------------ EquationAsText

        /// <summary>
        /// Get an expression in text form, using the same syntax as is used to enter an equation.
        /// This will be needed to save the equation to a database for re-use, so it is not just a cosmetic or debug function.
        /// </summary>
        public static string MathExpressionAsText(SingleResult expr)
        {
            string sText="";

            try
            {
                if (expr != null)
                {
                    IList<SingleResult> callStack = new List<SingleResult>();
                    sText = ExpressionAsText(expr, bSkipOpBrackets: true, lastFn: null, callStack: callStack);
                }
            }
            catch (Exception ex)
            {
                sText = ErrorUtils.UnspecifiedErrorMsg("MathExpressionAsText", ex);
            }

            return sText;
        }

        /// Guard against a mistake in the tree that causes infinite recursion.
        private static string ExpressionAsText(SingleResult expr, bool bSkipOpBrackets, Function lastFn, IList<SingleResult> callStack)
        {
            string sText;

            if (expr is FunctionCalc)
            {
                if (callStack.IndexOf(expr) >= 0)
                {
                    throw new UnspecifiedException("Infinite recursion in Expression Tree");
                }
                callStack.Add(expr);
                sText = FunctionCalcAsText((FunctionCalc) expr, bSkipOpBrackets, lastFn, callStack);
                callStack.Remove(expr);
            }
            else if (expr is SingleValue)
            {
                sText = SingleValueAsText((SingleValue) expr);
            }
            else
            {
                throw new UnspecifiedException("Error in MathExpressionAsText");
            }

            return sText;

        }

        // ------------------------

        private static string SingleValueAsText(SingleValue expr)
        {
            string sText = expr.Text;

            return sText;
        }

        // ------------------------

        private static string FunctionCalcAsText(FunctionCalc expr, bool bSkipOpBrackets, Function lastFn, IList<SingleResult> callStack)
        {
            const string pName = "FunctionCalcAsText";
            string sText = "";      // TODO: Perhaps replace this with StringBuilder?

            bool bThisSkipOpBrackets = bSkipOpBrackets;
            bool bNextSkipOpBrackets = false;

            Function fn = expr.Function;

            if (fn is FnEquals) bNextSkipOpBrackets = true;

            if (fn.AsciiLayout == FuncLayout.FuncLayout)
            {
                sText += fn.AsciiSymbol;

                sText += "(";

                AddExpressionInputs(ref sText, expr.Inputs, callStack);

                sText += ")";
            }
            else if (fn.AsciiLayout == FuncLayout.Op_Term)
            {
                bThisSkipOpBrackets = true;

                if (!bThisSkipOpBrackets)
                {
                    sText += "(";
                }

                sText += fn.AsciiSymbol;

                sText += ExpressionAsText(expr.Inputs[0], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn, callStack: callStack);

                if (!bThisSkipOpBrackets)
                {
                    sText += ")";
                }
            }
            else if ((fn.AsciiLayout == FuncLayout.Term_Op_Term) || (fn.AsciiLayout == FuncLayout.Term_Superscript_Term))
            {
                if (fn==lastFn)        // is Functions.FnMultiply)
                {
                    bThisSkipOpBrackets = true;
                }
                if (!bThisSkipOpBrackets)
                {
                    sText += "(";
                }

                sText += ExpressionAsText(expr.Inputs[0], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn, callStack: callStack);

                sText += fn.AsciiSymbol;

                sText += ExpressionAsText(expr.Inputs[1], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn, callStack: callStack);

                if (!bThisSkipOpBrackets)
                {
                    sText += ")";
                }
            }
            else if (fn.AsciiLayout == FuncLayout.Term_OverOp_Term)
            {
                    // The test below is commented out because although it is likely correct, it is not necessary...
                            //if (!(fn is Functions.FnDivide))
                            //{
                            //    throw new UnspecifiedException($"Error in {pName}");        // What else could it be? What do we show instead of a line?
                            //}

                if (!bThisSkipOpBrackets)
                {
                    sText += "(";
                }

                bNextSkipOpBrackets = true;     //Explicity added below

                bool bNumeratorBrackets = ((expr.Inputs[0] is SingleValue)?false:true);
                bool bDenominatorBrackets = ((expr.Inputs[1] is SingleValue) ? false : true);

                if (bNumeratorBrackets) { sText += "("; }

                sText += ExpressionAsText(expr.Inputs[0], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn, callStack: callStack);

                if (bNumeratorBrackets) { sText += ")"; }

                sText += fn.AsciiSymbol;

                if (bDenominatorBrackets) { sText += "("; }

                sText += ExpressionAsText(expr.Inputs[1], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn, callStack: callStack);

                if (bDenominatorBrackets) { sText += ")"; }

                if (!bThisSkipOpBrackets)
                {
                    sText += ")";
                }
            }
            //else if (fn.AsciiLayout == FuncLayout.Term_Superscript_Term)
            //{
            //    sText += ExpressionAsText(expr.Inputs[0], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn, callStack: callStack);

            //    if (expr.Inputs[1] is SingleValue)
            //    {
            //        sText += ((SingleValue)expr.Inputs[1]).Text;
            //    }
            //    else
            //    {
            //        // TODO: Equations
            //    }
            //}

            return sText;
        }


        private static void AddExpressionInputs(ref string sText, IList<SingleResult> inputs, IList<SingleResult> callStack)
        {
            bool bNextSkipOpBrackets = (inputs.Count == 1);

            for (int i = 0; i < inputs.Count; i++)
            {
                SingleResult sr = inputs[i];
                sText += ExpressionAsText(inputs[i], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: null, callStack: callStack);

                if (i > 0)
                {
                    sText += ",";
                }
            }
        }


        #endregion ------------ EquationAsText

    }



    // ----------------------

    public class FunCalcError
    {

        public FunCalcError(string message, SingleResult singleResult)
        {
            Message = message;
            SingleResult = singleResult;
        }

        public string Message { get; set; }
        public SingleResult SingleResult { get; set; }
    }





}
