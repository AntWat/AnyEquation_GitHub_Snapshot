using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AnyEquation.Equations.Model.Dimensions;

// TODO: Equations: Override == and != and GetHashCode

namespace AnyEquation.Equations.Model
{
    /// This class and its sub-classes should be immutable once created, so instances can be safely shared for efficiency
    public class AnonUOM : UOM
    {
        public AnonUOM(Dimensions dimensions, UOMSet uOMSet) :
            base(dimensions)
        {
            _uOMSet = uOMSet;
        }

        private UOMSet _uOMSet;
        public UOMSet UOMSet { get { return _uOMSet; } }

        // --------------------
        public override string ToString()
        {
            string s = _uOMSet?.Name??"";
            s += ":";
            s += Dimensions?.ToString() ?? "";

            return s;
        }


        public override bool Equals(Object obj)
        {
            AnonUOM anonUOM = obj as AnonUOM;
            if ((anonUOM == null) || (this.Dimensions == null) || (anonUOM.Dimensions == null)) return false;       // throw new InvalidOperationException("AnonUOM equality check involving null dimensions");

            if (IsDimenionless(this.Dimensions) && IsDimenionless(anonUOM.Dimensions)) return true;      // UOMSet doesn't matter

            if (this.UOMSet != anonUOM.UOMSet) return false;
            if (!this.Dimensions.Equals(anonUOM.Dimensions)) return false;

            return true;
        }

        public static bool EqualOrNull(AnonUOM an1, AnonUOM an2)
        {
            if ((an1 == null) && (an2 == null)) return true;

            if (an2 == null) return an1.Equals(an2);
            if (an1 == null) return an2.Equals(an1);

            return an1.Equals(an2);
        }

        public static bool HaveConsistentUomSets(AnonUOM an1, AnonUOM an2)
        {
            if ((an1 == null) && (an2 == null)) return true;

            if (IsDimenionless(an1?.Dimensions) || IsDimenionless(an2?.Dimensions))
            {
               return true;
            }

            if ((an1 == null) || (an2 == null)) return false;

            return an1.UOMSet==an2.UOMSet;
        }
    }

}

