using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.EquationParser;
using AnyEquation.Equations.Model.Functions;

namespace AnyEquation.Equations.Model
{
    public class EqnCalc : FunctionCalc
    {
        #region ------------ Constructors

        public EqnCalc(ContentManager cm) :
            base(cm.Functions[FnEquals.StrFunctionName])
        {
        }

        public EqnCalc(ContentManager cm, SingleResult lhs, SingleResult rhs) :
            base(cm.Functions[FnEquals.StrFunctionName], new List<SingleResult>() { lhs, rhs })
        {

        }

        #endregion ------------ Constructors


        #region ------------ Static: Rearrange Equation

        /// <summary>
        /// If the lhs is not already a variable try to rearrange the equation to place a variable on the lhs.
        /// </summary>
        public static EqnCalc IsolateFirstVariableIfRequired(EqnCalc eqOriginal, ContentManager cm)
        {
            try
            {
                if (eqOriginal.Inputs[0] is Variable) { return eqOriginal; }   // Nothing required

                IList<SingleValue> eqVars = eqOriginal?.FindAllVariables();

                Result<EqnCalc> r_result = RearrangeEquation(eqOriginal, eqVars[0].Name, cm);

                if (r_result.IsNotGood())
                {
                    Logging.LogMessage(r_result.Message);
                    return eqOriginal;
                }

                return r_result.Value;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                return eqOriginal;
            }


        }

        public static Result<EqnCalc> RearrangeEquation(EqnCalc eqOriginal, string varRequired, ContentManager cm)
        {
            try
            {
                // Copy the structure of eqOriginal so we don't mess it up
                Result<EqnCalc> r_eqCalc = CreateStructureCopy(eqOriginal, cm);
                if (r_eqCalc.IsNotGood()) { return Result<EqnCalc>.Bad(r_eqCalc.Message); }

                EqnCalc eqCalc = r_eqCalc.Value;

                int varDepth = -1;
                while (true)
                {
                    // --------- Find varRequired and check it only appears once
                    IList<Foundlocation> found = FindVariable(eqCalc, varRequired);
                    if ((found == null) || (found.Count == 0)) { return Result<EqnCalc>.Bad($"Variable '{varRequired}' was not found in the equation"); }
                    if (found.Count > 1)
                    {
                        return Result<EqnCalc>.Bad(
                                $"Cannot rearrange equation because it contains multiple instances of Variable '{varRequired}'");
                    }

                    // --------- Swap sides if it was found on the right
                    if (found[0].EqSide == EqSide.Right)
                    {
                        SingleResult side = eqCalc.Inputs[1];
                        eqCalc.Inputs[1] = eqCalc.Inputs[0];
                        eqCalc.Inputs[0] = side;
                    }

                    // --------- Check we are making progress
                    if (found[0].Depth == 1)   // Success!  
                    {
                        // Try to simplify the equation before returning it
                        Result<bool> r_changes = SimplifyEquation(eqCalc, cm);
                        if (r_changes.IsNotGood()) { return Result<EqnCalc>.Bad(r_changes.Message); }
                        bool changesWereMade = r_changes.Value;

                        // Successful return
                        return Result<EqnCalc>.Good(eqCalc);
                    }
                    else if (varDepth < 0)
                    {
                        varDepth = found[0].Depth;
                    }
                    else if (found[0].Depth >= varDepth)
                    {
                        return Result<EqnCalc>.Bad($"Last rearrangement did not achieve anything");
                    }
                    else if (found[0].Depth < 1)
                    {
                        return Result<EqnCalc>.Bad($"Unexpected error: found[0].Depth < 1");
                    }

                    // ------------ Rearrange once
                    Result<bool> r_resOnce = RearrangeOnce(eqCalc, found[0].TopBranchindex, cm);
                    if (r_resOnce.IsNotGood()) { return Result<EqnCalc>.Bad(r_resOnce.Message); }
                }
            }
            catch (Exception ex)
            {
                return Result<EqnCalc>.Bad(ex);
            }
        }

        private static Result<bool> RearrangeOnce(EqnCalc eqCalc, int argIndexToPreserve, ContentManager cm)
        {
            FunctionCalc lhsFunCalc = eqCalc.Inputs[0] as FunctionCalc;
            Function lhsFunction = lhsFunCalc?.Function;
            if (lhsFunction == null) { return Result<bool>.Bad($"Unexpected error: lhs is not a funcalc"); }

            SingleResult argToPreserve = lhsFunCalc.Inputs[argIndexToPreserve];

            // Create placeholders that the function rearrangements will treat as variables
            IList<string> argPlaceholders = new List<string>();
            IDictionary<string, SingleResult> placeFillers = new Dictionary<string, SingleResult>();

            for (int i = 0; i < lhsFunCalc.Inputs.Count; i++)
            {
                string placeholder = $"v{i}";
                argPlaceholders.Add(placeholder);
                if (i != argIndexToPreserve)
                {
                    placeFillers.Add(placeholder, lhsFunCalc.Inputs[i]);
                }
            }
            string resPlaceHolder = "res";
            placeFillers.Add(resPlaceHolder, eqCalc.Inputs[1]);

            // Ask the function definition for a rearrangement that isolates the required variable
            Result<string> r_rearrangement = lhsFunction.GetRearrangement(argPlaceholders, resPlaceHolder, argIndexToPreserve);

            if (r_rearrangement.IsNotGood() || string.IsNullOrEmpty(r_rearrangement.Value))
            {
                return Result<bool>.Bad($"Rearrangement failed: {r_rearrangement.Message}");
            }

            // Parse the rearrangement expression
            JongErrWarn errW = null;
            FunctionCalc newRhs = cm.CreateFunctionCalcFromExpression(r_rearrangement.Value, "", null, out errW);

            if (newRhs == null)
            {
                return Result<bool>.Bad($"Rearrangement cannot be used: {errW?.ErrWarnString}");
            }

            // Subsitute for the placeholders
            Result<bool> r_ok = SubsituteForPlaceholders(newRhs, placeFillers);

            if (r_ok.IsNotGood()) { return Result<bool>.Bad($"Error substituting for placeholders: {r_ok.Message}"); }

            // Update the eqCalc with the rearrangement
            eqCalc.Inputs[1] = newRhs;
            eqCalc.Inputs[0] = argToPreserve;

            return Result<bool>.Good(true);
        }



        public enum EqSide { Left, Right, }

        private class Foundlocation
        {
            public EqSide EqSide { get; set; }
            public int Depth { get; set; }      // How many levels below '='
            public int TopBranchindex { get; set; }  // Which top-level branch it is under (-1 if found directly at level 1)
        }

        private static IList<Foundlocation> FindVariable(EqnCalc eqCalc, string varName)
        {
            IList<Foundlocation> foundVars = new List<Foundlocation>();

            AddFoundVars(eqCalc.Inputs[0], varName, EqSide.Left, -1, 1, foundVars);
            AddFoundVars(eqCalc.Inputs[1], varName, EqSide.Right, -1, 1, foundVars);

            return foundVars;
        }

        private static Result<bool> AddFoundVars(SingleResult singleResult, string varName, EqSide eqSide,
                                                 int topBranchindex, int depth, IList<Foundlocation> foundVars)
        {
            if (singleResult is SingleValue)
            {
                if ((singleResult is Literal) || (singleResult is Constant)) { }
                else if ((singleResult is Variable var))
                {
                    if (var.Name.Equals(varName, StringComparison.Ordinal))      // Case sensitive
                    {
                        foundVars.Add(new Foundlocation()
                        {
                            Depth = depth,
                            EqSide = eqSide,
                            TopBranchindex = topBranchindex,
                        });
                    };
                }
                else { return Result<bool>.Bad(ErrorUtils.UnspecifiedErrorMsg2("Unknown SingleValue sub-type")); }
            }
            else if (singleResult is FunctionCalc funCalc)
            {
                for (int i = 0; i < funCalc.Inputs.Count; i++)
                {
                    var input = funCalc.Inputs[i];

                    int nextTopBranchindex = ((topBranchindex < 0) ? i : topBranchindex);       // Only gets set at the very first level, because that is what we will rearrange
                    AddFoundVars(input, varName, eqSide, nextTopBranchindex, depth + 1, foundVars);
                }
            }
            else { return Result<bool>.Bad(ErrorUtils.UnspecifiedErrorMsg2("Unknown SingleResult sub-type")); }

            return null;
        }


        private static Result<bool> SubsituteForPlaceholders(FunctionCalc funCalc, IDictionary<string, SingleResult> placeFillers)
        {
            for (int i = 0; i < funCalc.Inputs.Count; i++)
            {
                var input = funCalc.Inputs[i];

                if (input is Variable var)
                {
                    foreach (var item in placeFillers)
                    {
                        if (var.Name.Equals(item.Key, StringComparison.Ordinal))      // Case sensitive
                        {
                            funCalc.Inputs[i] = item.Value;
                            break;
                        }
                    }
                }
                else if (input is FunctionCalc subFunCalc)
                {
                    Result<bool> r_ok = SubsituteForPlaceholders(subFunCalc, placeFillers);
                }
            }

            return Result<bool>.Good(true);
        }


        #endregion ------------ Static: Rearrange Equation


        #region ------------ Static: Copy Equation

        /// <summary>
        /// Create a copy of srcEqnCalc that shares all leaf objects and dimensions etc. but has its own structure,
        /// so we can rearrange it without messing up the original.
        /// </summary>
        private static Result<EqnCalc> CreateStructureCopy(EqnCalc srcEqnCalc, ContentManager cm)
        {
            Result<SingleResult> r_tgtLhs = CreateAStructureCopy(srcEqnCalc.Inputs[0], cm);
            if (r_tgtLhs.IsNotGood()) { return Result<EqnCalc>.Bad(r_tgtLhs.Message); }

            Result<SingleResult> r_tgtRhs = CreateAStructureCopy(srcEqnCalc.Inputs[1], cm);
            if (r_tgtRhs.IsNotGood()) { return Result<EqnCalc>.Bad(r_tgtRhs.Message); }

            EqnCalc tgtEqnCalc = new EqnCalc(cm, r_tgtLhs.Value, r_tgtRhs.Value);
            tgtEqnCalc.CalcQuantity = srcEqnCalc.CalcQuantity;  // TODO: Not sure about this one?
            tgtEqnCalc.Description = srcEqnCalc.Description;
            tgtEqnCalc.ExpectedDimensions = srcEqnCalc.ExpectedDimensions;
            tgtEqnCalc.Name = srcEqnCalc.Name;
            tgtEqnCalc.ParamType = srcEqnCalc.ParamType;
            tgtEqnCalc.TestValues = srcEqnCalc.TestValues;

            return Result<EqnCalc>.Good(tgtEqnCalc);
        }

        private static Result<SingleResult> CreateAStructureCopy(SingleResult srcSingleResult, ContentManager cm)
        {
            SingleResult tgtSingleResult = srcSingleResult;

            if (srcSingleResult is SingleValue) { /* No change required */ }
            else if (srcSingleResult is FunctionCalc srcFunCalc)
            {
                FunctionCalc tgtFunCalc = new FunctionCalc(srcFunCalc.Function);

                tgtFunCalc.CalcQuantity = srcFunCalc.CalcQuantity;  // TODO: Not sure about this one?
                tgtFunCalc.Description = srcFunCalc.Description;
                tgtFunCalc.ExpectedDimensions = srcFunCalc.ExpectedDimensions;
                tgtFunCalc.Name = srcFunCalc.Name;
                tgtFunCalc.ParamType = srcFunCalc.ParamType;
                tgtFunCalc.TestValues = srcFunCalc.TestValues;

                for (int i = 0; i < srcFunCalc.Inputs.Count; i++)
                {
                    var input = srcFunCalc.Inputs[i];
                    Result<SingleResult> r_tgtInput = CreateAStructureCopy(input, cm);

                    if (r_tgtInput.IsNotGood()) { return Result<SingleResult>.Bad(r_tgtInput.Message); }
                    tgtFunCalc.Inputs.Add(r_tgtInput.Value);
                }

                tgtSingleResult = tgtFunCalc;
            }
            else { return Result<SingleResult>.Bad(ErrorUtils.UnspecifiedErrorMsg2("Unknown SingleResult sub-type")); }

            return Result<SingleResult>.Good(tgtSingleResult);
        }

        #endregion ------------ Static: Copy Equation


        #region ------------ Static: Simplify Equation

        enum SimplificationRule
        {
            None=0,
            DivideRule1,    //  (a/b)/(c/d) => (a*d)/(b*c)
            DivideRule2,    //  (a/b)/c => a/(b*c)
            DivideRule3,    //  a/(b/c) => (a*c)/b
            PowerRule1,     //  (a^b)^c => a^(b*c)
            SquareRootRule1, // a^(1/2)=>Sqrt(a)
            SquareRootRule2, // a^0.5=>Sqrt(a)

            MultBy1Rule1,   // 1*a=>a
            MultBy1Rule2,   // a*1=>a
            DivideBy1Rule1,   // a/1=>a
            MultByLiteralRule1,   // a*Literal=>Literal*a  (e.g. a*2=>2*a)

            // No Rules yet: 
            //  (a^(b^c) => ??
        }

        private class SimplificationMatch
        {
            public FunctionCalc TopFunCalc { get; set; }    // Top of the match

            public FunctionCalc ParentFunCalc { get; set; } // Parent to TopFunCalc if there is one
            public int ParentArgIndex { get; set; }

            public SimplificationRule SimplificationRule { get; set; }
            public int Depth { get; set; }      // How far down the tree.  We will do the deepest first
        }

        /// <summary>
        /// Simplify the equation to make it display better: 
        /// </summary>
        /// <returns>Result true: if any changes were made</returns>
        private static Result<bool> SimplifyEquation(EqnCalc eqCalc, ContentManager cm)
        {
            try
            {
                bool changesWereMade = false;
                bool tryAgain = true;
                int maxLoop = 999;
                int loopCount = 0;

                while (tryAgain && (loopCount < maxLoop))
                {
                    Result<bool> r_Change = SimplifyTheEquation(eqCalc, cm);
                    if (r_Change.IsNotGood()) { return r_Change; }

                    if (!changesWereMade) { changesWereMade = r_Change.Value; }
                    tryAgain = r_Change.Value;
                    loopCount++;
                }

                if (loopCount >= maxLoop) { return Result<bool>.Bad("Loop count exceeded"); }

                return Result<bool>.Good(changesWereMade);
            }
            catch (Exception ex)
            {
                return Result<bool>.Bad(ex);
            }
        }

        private static Result<bool> SimplifyTheEquation(EqnCalc eqCalc, ContentManager cm)
        {
            SingleResult sr1 = eqCalc.Inputs[0];
            Result<bool> r_LhsChange = SimplifyFunctionCalc(ref sr1, cm);
            if (r_LhsChange.IsNotGood()) { return r_LhsChange; }
            eqCalc.Inputs[0] = sr1;

            SingleResult sr2 = eqCalc.Inputs[1];
            Result<bool> r_RhsChange = SimplifyFunctionCalc(ref sr2, cm);
            if (r_RhsChange.IsNotGood()) { return r_RhsChange; }
            eqCalc.Inputs[1] = sr2;

            return Result<bool>.Good(r_LhsChange.Value || r_RhsChange.Value);

        }

        /// <returns>Result true: if a simplificatiuon was found</returns>
        private static Result<bool> SimplifyFunctionCalc(ref SingleResult singleResult, ContentManager cm)
        {
            Result<SimplificationMatch> r_match = FindLowestSimplification(singleResult, 0, null, 0);
            if (r_match.IsNotGood()) { return Result<bool>.Bad(r_match.Message); } // Error

            SimplificationMatch match = r_match.Value;
            if (match==null) { return Result<bool>.Good(false); } // No match found

            FunctionCalc topFC = match.TopFunCalc;

            Function fnMultiply = cm.Functions[FnMultiply.StrFunctionName];
            Function fnSquareRoot = cm.Functions[FnSquareRoot.StrFunctionName];

            SingleResult term_a; SingleResult term_b; SingleResult term_c; SingleResult term_d;
            switch (match.SimplificationRule)
            {
                case SimplificationRule.DivideRule1:        //  (a/b)/(c/d) => (a*d)/(b*c)
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 0, 0 });
                    term_b = GetTermByRelativeIndex(topFC, new int[] { 0, 1 });
                    term_c = GetTermByRelativeIndex(topFC, new int[] { 1, 0 });
                    term_d = GetTermByRelativeIndex(topFC, new int[] { 1, 1 });

                    topFC.Inputs[0] = new FunctionCalc(fnMultiply, new List<SingleResult> { term_a, term_d });
                    topFC.Inputs[1] = new FunctionCalc(fnMultiply, new List<SingleResult> { term_c, term_b});
                    return Result<bool>.Good(true);

                case SimplificationRule.DivideRule2:    //  (a/b)/c => a/(b*c)
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 0, 0 });
                    term_b = GetTermByRelativeIndex(topFC, new int[] { 0, 1 });
                    term_c = GetTermByRelativeIndex(topFC, new int[] { 1 });

                    topFC.Inputs[0] = term_a;
                    topFC.Inputs[1] = new FunctionCalc(fnMultiply, new List<SingleResult> { term_c, term_b });
                    return Result<bool>.Good(true);

                case SimplificationRule.DivideRule3:    //  a/(b/c) => (a*c)/b
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 0 });
                    term_b = GetTermByRelativeIndex(topFC, new int[] { 1, 0 });
                    term_c = GetTermByRelativeIndex(topFC, new int[] { 1, 1 });

                    topFC.Inputs[0] = new FunctionCalc(fnMultiply, new List<SingleResult> { term_a, term_c });
                    topFC.Inputs[1] = term_b;
                    return Result<bool>.Good(true);

                case SimplificationRule.PowerRule1:
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 0, 0 });
                    term_b = GetTermByRelativeIndex(topFC, new int[] { 0, 1 });
                    term_c = GetTermByRelativeIndex(topFC, new int[] { 1 });

                    topFC.Inputs[0] = term_a;
                    topFC.Inputs[1] = new FunctionCalc(fnMultiply, new List<SingleResult> { term_b, term_c });
                    return Result<bool>.Good(true);

                case SimplificationRule.SquareRootRule1:
                case SimplificationRule.SquareRootRule2:
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 0 });
                    FunctionCalc fcSqrRoot = new FunctionCalc(fnSquareRoot, new List<SingleResult> { term_a });

                    return ReplaceParentArg(match, ref singleResult, fcSqrRoot);

                case SimplificationRule.MultBy1Rule1:
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 1 });
                    return ReplaceParentArg(match, ref singleResult, term_a);
                case SimplificationRule.MultBy1Rule2:
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 0 });
                    return ReplaceParentArg(match, ref singleResult, term_a);
                case SimplificationRule.DivideBy1Rule1:
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 0 });
                    return ReplaceParentArg(match, ref singleResult, term_a);

                case SimplificationRule.MultByLiteralRule1:
                    term_a = GetTermByRelativeIndex(topFC, new int[] { 0 });
                    term_b = GetTermByRelativeIndex(topFC, new int[] { 1 });

                    topFC.Inputs[0] = term_b;
                    topFC.Inputs[1] = term_a;

                    return Result<bool>.Good(true);

                default:
                    return Result<bool>.Bad(ErrorUtils.UnspecifiedErrorMsg2("Unrecognised SimplificationRule"));
            }
        }

        private static Result<bool> ReplaceParentArg(SimplificationMatch match, ref SingleResult oldSingleResult, SingleResult newSingleResult)
        {
            if (match.ParentFunCalc == null)
            {
                oldSingleResult = newSingleResult;
            }
            else
            {
                match.ParentFunCalc.Inputs[match.ParentArgIndex] = newSingleResult;
            }
            return Result<bool>.Good(true);
        }

        private static Result<SimplificationMatch> FindLowestSimplification(SingleResult singleResult, int depth, FunctionCalc parentFunCalc, int parentArgIndex)
        {
            Result<SimplificationMatch> r_match = FindSimplificationHere(singleResult, depth, parentFunCalc, parentArgIndex);
            if (r_match.IsNotGood()) { return r_match; } // Error

            SimplificationMatch lowest_match = r_match.Value;

            if (singleResult is FunctionCalc funCalc)
            {
                for (int i = 0; i < funCalc.Inputs.Count; i++)
                {
                    var input = funCalc.Inputs[i];
                    Result<SimplificationMatch> r_match2 = FindLowestSimplification(input, depth+1, funCalc, i);
                    if (r_match2.IsNotGood()) { return r_match2; } // Error

                    if ((r_match2.Value!=null) && ((lowest_match==null) || (r_match2.Value.Depth > lowest_match.Depth)))
                    {
                        lowest_match = r_match2.Value;
                    }
                }
            }

            return Result<SimplificationMatch>.Good(lowest_match);
        }

        private static Result<SimplificationMatch> FindSimplificationHere(SingleResult singleResult, int depth, FunctionCalc parentFunCalc, int parentArgIndex)
        {
            if (singleResult is FunctionCalc funCalc)
            {
                if (funCalc.Inputs.Count != 2) { Result<SimplificationMatch>.Good(null); }        //All rules expect 2 inputs

                Function topFunction = funCalc.Function;

                FunctionCalc[] childFunCalc = new FunctionCalc[funCalc.Inputs.Count];
                Function[] childFunction = new Function[funCalc.Inputs.Count];

                for (int i = 0; i < funCalc.Inputs.Count; i++)
                {
                    childFunCalc[i] = funCalc.Inputs[i] as FunctionCalc;
                    childFunction[i] = childFunCalc[i]?.Function;
                }

                SimplificationRule rule = SimplificationRule.None;

                if (topFunction.Name.Equals(FnDivide.StrFunctionName))
                {
                    if (funCalc.Inputs[1] is Literal lit1)
                    {
                        if ((lit1.CalcQuantity.Value == 1))
                        {
                            rule = SimplificationRule.DivideBy1Rule1;
                        }
                    }

                    else if (childFunction[0]?.Name?.Equals(FnDivide.StrFunctionName) ?? false)
                    {
                        if (childFunction[1]?.Name?.Equals(FnDivide.StrFunctionName) ?? false)
                        {
                            rule = SimplificationRule.DivideRule1;
                        }
                        else
                        {
                            rule = SimplificationRule.DivideRule2;
                        }
                    }
                    else if (childFunction[1]?.Name?.Equals(FnDivide.StrFunctionName) ?? false)
                    {
                        rule = SimplificationRule.DivideRule3;
                    }
                }
                else if (topFunction.Name.Equals(FnPower.StrFunctionName))
                {
                    if (childFunction[0]?.Name?.Equals(FnPower.StrFunctionName) ?? false)
                    {
                        rule = SimplificationRule.PowerRule1;
                    }

                    else if (childFunction[1]?.Name?.Equals(FnDivide.StrFunctionName) ?? false)
                    {
                        if ((childFunCalc[1].Inputs[0] is Literal lit1) && (childFunCalc[1].Inputs[1] is Literal lit2))
                        {
                            if ((lit1.CalcQuantity.Value == 1) && (lit2.CalcQuantity.Value == 2))
                            {
                                rule = SimplificationRule.SquareRootRule1;
                            }
                        }
                    }

                    else if (funCalc.Inputs[1] is Literal lit1)
                    {
                        if ((lit1.CalcQuantity.Value == 0.5))
                        {
                            rule = SimplificationRule.SquareRootRule2;
                        }
                    }
                }

                else if (topFunction.Name.Equals(FnMultiply.StrFunctionName))
                {
                    if (funCalc.Inputs[0] is Literal lit0)
                    {
                        if ((lit0.CalcQuantity.Value == 1))
                        {
                            rule = SimplificationRule.MultBy1Rule1;
                        }
                    }
                    else if (funCalc.Inputs[1] is Literal lit1)
                    {
                        if ((lit1.CalcQuantity.Value == 1))
                        {
                            rule = SimplificationRule.MultBy1Rule2;
                        }
                        else
                        {
                            rule = SimplificationRule.MultByLiteralRule1;
                        }
                    }
                }

                if (rule != SimplificationRule.None)
                {
                    return Result<SimplificationMatch>.Good(
                        new SimplificationMatch
                        {
                            Depth = depth,
                            SimplificationRule = rule,
                            TopFunCalc = funCalc,
                            ParentFunCalc = parentFunCalc,
                            ParentArgIndex = parentArgIndex,
                        });
                }
            }

            return Result<SimplificationMatch>.Good(null);
        }

        private static SingleResult GetTermByRelativeIndex(FunctionCalc topFC, int[] indeces)
        {
            FunctionCalc fc = topFC;
            SingleResult sr = null;

            foreach (int index in indeces)
            {
                if (fc == null) return null;
                if (fc.Inputs.Count<index+1) return null;

                sr = fc.Inputs[index];
                fc = fc.Inputs[index] as FunctionCalc;
            }

            return sr;
        }

        #endregion ------------ Static: Simplify Equation

    }

}

