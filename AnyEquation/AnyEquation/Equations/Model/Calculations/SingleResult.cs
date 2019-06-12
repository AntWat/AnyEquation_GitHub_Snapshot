using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    public abstract class SingleResult
    {
        public CalcQuantity CalcQuantity { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ParamType ParamType { get; set; }

        // ---------------------

        public IList<SingleValue> FindAllVariables()
        {
            IList<SingleValue> allVars = new List<SingleValue>();
            GetAllVariables(allVars);

            return allVars;
        }

        public IList<SingleValue> FindAllConstants()
        {
            IList<SingleValue> allConstants = new List<SingleValue>();
            GetAllConstants(allConstants);

            return allConstants;
        }

        public IList<SingleValue> FindAllLiterals()
        {
            IList<SingleValue> allLiterals = new List<SingleValue>();
            GetAllLiterals(allLiterals);

            return allLiterals;
        }

        // ---------------------

        private void GetAllVariables(IList<SingleValue> allVars)
        {
            if (this is Variable)
            {
                SingleValue sv = (SingleValue)this;
                if (allVars.IndexOf(sv)==-1)
                {
                    allVars.Add((Variable)this);
                }
            }
            else if (this is FunctionCalc)
            {
                foreach (var item in ((FunctionCalc)this).Inputs)
                {
                    item.GetAllVariables(allVars);
                }
            }
        }

        private void GetAllConstants(IList<SingleValue> allConstants)
        {
            if (this is Constant)
            {
                SingleValue sv = (SingleValue)this;
                if (allConstants.IndexOf(sv) == -1)
                {
                    allConstants.Add((Constant)this);
                }
            }
            else if (this is FunctionCalc)
            {
                foreach (var item in ((FunctionCalc)this).Inputs)
                {
                    item.GetAllConstants(allConstants);
                }
            }
        }

        private void GetAllLiterals(IList<SingleValue> allLiterals)
        {
            if (this is Literal)
            {
                SingleValue sv = (SingleValue)this;
                if (allLiterals.IndexOf(sv) == -1)
                {
                    allLiterals.Add((Literal)this);
                }
            }
            else if (this is FunctionCalc)
            {
                foreach (var item in ((FunctionCalc)this).Inputs)
                {
                    item.GetAllLiterals(allLiterals);
                }
            }
        }

        // ---------------------

        public void ClearAllVarVals()
        {
            IList<SingleValue> eqVars = FindAllVariables();

            if (eqVars!=null)
            {
                foreach (var item in eqVars)
                {
                    item?.CalcQuantity?.Clear();
                }
            }
        }

    }
}
