using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: Equations: Override == and != and GetHashCode

namespace AnyEquation.Equations.Model
{
    /// This class and its sub-classes should be immutable once created, so instances can be safely shared for efficiency
    public abstract class UOM
    {
        public UOM(Dimensions dimensions)
        {
            _dimensions = dimensions;
        }

        private Dimensions _dimensions;
        public Dimensions Dimensions { get { return _dimensions; } }

        /// Convert value from the UOM of this class to those of toUOM.
        /// Return the converted value, or Double.NaN on failure.
        //public abstract double Convert(double value, UOM toUOM);

        // ---------------------------

        public static double Convert(double theValue, KnownUOM fromUOM, KnownUOM toUOM)
        {
            /*
            Conversion To Base Unit: (A + BX) / (C + DX)

            To convert from unit1 (value x1) to unit2 (value x2):

            BaseVal: b = (A1 + B1.X1) / (C1 + D1.X1) = (A2 + B2.X2) / (C2 + D2.X2)

            So:

            b = (A1 + B1.X1) / (C1 + D1.X1)
            x2 = (A2 - C2.b) / (D2.b - B2)

            */

            if (fromUOM == null) return double.NaN;
            if (toUOM == null) return double.NaN;

            if (!fromUOM.Dimensions.Equals(toUOM.Dimensions)) return double.NaN;

            double x1 = theValue;
            double b = (fromUOM.AFactor + (fromUOM.BFactor * x1)) / (fromUOM.CFactor + (fromUOM.DFactor * x1));
            double x2 = (toUOM.AFactor - (toUOM.CFactor * b)) / ((toUOM.DFactor * b) - toUOM.BFactor);

            return x2;
        }

        // ---------------------------

        public static double Convert(double theValue, KnownUOM fromUOM, AnonUOM anonToUOM)
        {
            KnownUOM to_UOM = anonToUOM?.UOMSet?.FindFromDimensions(anonToUOM.Dimensions);
            return Convert(theValue, fromUOM, to_UOM);
        }

        public static double Convert(double theValue, AnonUOM anonFromUOM, KnownUOM toUOM)
        {
            KnownUOM from_UOM = anonFromUOM?.UOMSet?.FindFromDimensions(anonFromUOM.Dimensions);
            return Convert(theValue, from_UOM, toUOM);
        }

        public static double Convert(double theValue, AnonUOM anonFromUOM, AnonUOM anonToUOM)
        {
            KnownUOM from_UOM = anonFromUOM?.UOMSet?.FindFromDimensions(anonFromUOM.Dimensions);
            KnownUOM to_UOM = anonToUOM?.UOMSet?.FindFromDimensions(anonToUOM.Dimensions);
            return Convert(theValue, from_UOM, to_UOM);
        }

        // ---------------------------
        public static bool EqualDimensions(KnownUOM uom1, KnownUOM uom2)
        {
            if ((uom1 == null) && (uom2 == null)) return true;
            if ((uom1 == null) || (uom2 == null)) return false;

            if ((uom1.Dimensions == null) && (uom2.Dimensions == null)) return true;
            if ((uom1.Dimensions == null) || (uom2.Dimensions == null)) return false;

            return uom1.Dimensions.Equals(uom2.Dimensions);
        }

    }
}
