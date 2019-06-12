using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    /// <summary>
    /// Represents a complete, consistent set of UOM, such as 'SI Units' or 'British Units'.
    /// None of the contents should have identical dimensions.
    /// Every unit will be a base unit, made up of the core base units for each dimension (e.g. Power must be W, cannot be kW or MW).
    /// </summary>
    public sealed class UOMSet : AnyUOMSet
    {
        public UOMSet(string name) : base(name)
        {
        }

        public override bool IsConsistent()
        {
            return true;
        }

    }
}
