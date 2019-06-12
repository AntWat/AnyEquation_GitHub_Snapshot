using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    public enum DimType
    {
        Mass, Length, Time, ElectricCurrent, Temperature, QuantityOfSubstance, Luminosity, PlaneAngle, SolidAngle, Currency
    }

    // TODO: Equations: Override == and != and GetHashCode

    public class Dimensions
    {

        public static Dimensions DimensionLessInstance = new Dimensions();

        public Dimensions()
        {
            Initialise();
        }
        public Dimensions(double mass=0, double length = 0, double time = 0, double electricCurrent = 0, double temperature = 0, double quantityOfSubstance = 0, double luminosity = 0, double planeAngle = 0, double solidAngle = 0, double currency = 0)
        {
            Initialise();

            Mass = mass;
            Length = length;
            Time = time;
            ElectricCurrent = electricCurrent;
            Temperature = temperature;
            QuantityOfSubstance = quantityOfSubstance;
            Luminosity = luminosity;
            PlaneAngle = planeAngle;
            SolidAngle = solidAngle;
            Currency = currency;
        }

        public Dimensions(DimType dimType, double dimPartVal)
        {
            Initialise();
            _dims[dimType] = dimPartVal;
        }

        private void Initialise()        // _dims should always have an entry for every type
        {
            _dims.Clear();
            foreach (var dimType in Enum.GetValues(typeof(DimType)))
            {
                _dims.Add((DimType)dimType, 0);
            }
        }


        private IDictionary<DimType, double> _dims = new Dictionary<DimType, double>();

        public IDictionary<DimType, double> Dims
        {
            get { return _dims; }
        }


        public double Mass {
            get { return _dims[DimType.Mass]; }
            set { _dims[DimType.Mass] = value; } }
        public double Length
        {
            get { return _dims[DimType.Length]; }
            set { _dims[DimType.Length] = value; }
        }
        public double Time
        {
            get { return _dims[DimType.Time]; }
            set { _dims[DimType.Time] = value; }
        }
        public double ElectricCurrent
        {
            get { return _dims[DimType.ElectricCurrent]; }
            set { _dims[DimType.ElectricCurrent] = value; }
        }
        public double Temperature
        {
            get { return _dims[DimType.Temperature]; }
            set { _dims[DimType.Temperature] = value; }
        }
        public double QuantityOfSubstance
        {
            get { return _dims[DimType.QuantityOfSubstance]; }
            set { _dims[DimType.QuantityOfSubstance] = value; }
        }
        public double Luminosity
        {
            get { return _dims[DimType.Luminosity]; }
            set { _dims[DimType.Luminosity] = value; }
        }
        public double PlaneAngle
        {
            get { return _dims[DimType.PlaneAngle]; }
            set { _dims[DimType.PlaneAngle] = value; }
        }
        public double SolidAngle
        {
            get { return _dims[DimType.SolidAngle]; }
            set { _dims[DimType.SolidAngle] = value; }
        }
        public double Currency
        {
            get { return _dims[DimType.Currency]; }
            set { _dims[DimType.Currency] = value; }
        }


        /// <summary>
        /// Get the Dimensions for v1 * v2 by adding the component parts of the 2 sets of Dimensions.
        /// </summary>
        /// <returns>A newly created Dimensions instance</returns>
        public static Dimensions AddDimensions(Dimensions dims1, Dimensions dims2)
        {
            if ((dims1 == null) && (dims2 == null)) return null;
            if (dims1 == null) return null;
            if (dims2 == null) return null;

            Dimensions resDims = new Dimensions();

            foreach (var dimType in Enum.GetValues(typeof(DimType)))
            {
                DimType dt = (DimType)dimType;

                resDims.Dims[dt] = dims1.Dims[dt] + dims2.Dims[dt];
            }

            return resDims;
        }


        /// <summary>
        /// Get the Dimensions for v1 / v2 by subtracting the component parts of the 2 sets of Dimensions.
        /// </summary>
        /// <returns>A newly created Dimensions instance</returns>
        public static Dimensions SubtractDimensions(Dimensions dims1, Dimensions dims2)
        {
            if ((dims1 == null) && (dims2 == null)) return null;
            if (dims1 == null) return null;
            if (dims2 == null) return null;

            Dimensions resDims = new Dimensions();

            foreach (var dimType in Enum.GetValues(typeof(DimType)))
            {
                DimType dt = (DimType)dimType;

                resDims.Dims[dt] = dims1.Dims[dt] - dims2.Dims[dt];
            }

            return resDims;
        }


        /// <summary>
        /// Get the Dimensions for v1 ^ exponent by multiplying the component parts of the 1st set of Dimensions by the exponent.
        /// </summary>
        /// <returns>A newly created Dimensions instance</returns>
        public static Dimensions MultiplyDimensions(Dimensions dims1, double exponent)
        {
            if (dims1 == null) return null;

            Dimensions resDims = new Dimensions();

            foreach (var dimType in Enum.GetValues(typeof(DimType)))
            {
                DimType dt = (DimType)dimType;

                resDims.Dims[dt] = dims1.Dims[dt] * exponent;
            }

            return resDims;
        }


        /// <summary>
        /// See if dims1 is dimensionless
        /// </summary>
        /// <returns>A newly created Dimensions instance</returns>
        public static bool IsDimenionless(Dimensions dims1)
        {
            if (dims1 == null) return false;

            bool bRet = true;

            foreach (var dimType in Enum.GetValues(typeof(DimType)))
            {
                DimType dt = (DimType)dimType;

                if (dims1.Dims[dt] != 0.0)      // TODO: Equations: Tolerance
                { bRet = false; break; }
            }

            return bRet;
        }


        // --------------------
        static IDictionary<DimType, string> _prefixes = new Dictionary<DimType, string>()
            {
                { DimType.Mass, "M" },
                { DimType.Length, "L" },
                { DimType.Time, "t" },
                { DimType.ElectricCurrent, "A" },
                { DimType.Temperature, "T" },
                { DimType.QuantityOfSubstance, "q" },
                { DimType.Luminosity, "l" },
                { DimType.PlaneAngle, "a" },
                { DimType.SolidAngle, "s" },
                { DimType.Currency, "c" },
            };

        public static string GetExplanationLine()
        {
            string explanationLine = "";

            foreach (var dimType in Enum.GetValues(typeof(DimType)))
            {
                DimType dt = (DimType)dimType;

                explanationLine += $"{_prefixes[dt]}={dt.ToString()},  ";
            }
            return explanationLine;
        }

        public override string ToString()
        {
            string s = "";

            foreach (var dimType in Enum.GetValues(typeof(DimType)))
            {
                DimType dt = (DimType)dimType;

                if (Dims[dt] != 0.0)      // TODO: Equations: Tolerance
                {
                    s += string.Format("{0}^{1} ", _prefixes[dt], Dims[dt]);
                }
                s = s.Trim();
            }

            return s;
        }

        // --------------------

        public override bool Equals(Object obj)
        {
            Dimensions dims1 = obj as Dimensions;

            if (dims1 == null) return false;

            foreach (var dimType in Enum.GetValues(typeof(DimType)))
            {
                DimType dt = (DimType)dimType;

                if (dims1.Dims[dt] != this.Dims[dt])      // TODO: Equations: Tolerance
                { return false; }
            }

            return true;
        }

        public static bool AreEqual(Dimensions dims1, Dimensions dims2)
        {
            if ((dims1 == null) && (dims2 == null)) { return true; }
            else if (dims1 == null) { return false; }
            else if (dims2 == null) { return false; }

            return dims1.Equals(dims2);
        }

    }
}
