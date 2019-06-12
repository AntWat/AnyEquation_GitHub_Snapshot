using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    public abstract class SingleValue : SingleResult
    {
        public SingleValue(CalcQuantity quantity, string name, string description, ParamType paramType)
        {
            CalcQuantity = quantity;
            Name = name;
            Description = description;
            ParamType = paramType;
        }

        public abstract string Text { get; }

    }
}
