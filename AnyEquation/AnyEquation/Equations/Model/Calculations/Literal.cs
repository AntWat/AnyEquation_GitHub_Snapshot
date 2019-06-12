using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{
    /// <summary>
    /// A literal, dimensionless number.  Anything that needs dimensions must be a Constant or a Variable.
    /// </summary>
    public class Literal : SingleValue
    {
        public Literal(double value) :
            base(new CalcQuantity(value,CalcStatus.Good,"",new AnonUOM(Dimensions.DimensionLessInstance, null)), "", "", ContentManager.DimensionLessParamType)
        {
        }

        public override string Text { get { return CalcQuantity?.Value.ToString()??""; } }

    }
}
