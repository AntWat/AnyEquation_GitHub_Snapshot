using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model.Functions
{
    abstract class FnTrig : FnOneArg
    {
        public FnTrig(string name) :
            base(name, _planeAngleDim, _dimensionlessDim)
        { }

        public static Dimensions _dimensionlessDim { get; } = new Dimensions();
        public static Dimensions _planeAngleDim { get; } = new Dimensions(planeAngle: 1);
    }

    class FnSin : FnTrig
    {
        public const string StrFunctionName = "Sin";
        public FnSin() : base(name: StrFunctionName) { }
        protected override double CalcValue(double angle) { return Math.Sin(angle); }
        protected override string InverseName() { return "ASin"; }
    }

    class FnCos : FnTrig
    {
        public const string StrFunctionName = "Cos";
        public FnCos() : base(name: StrFunctionName) { }
        protected override double CalcValue(double angle) { return Math.Cos(angle); }
        protected override string InverseName() { return "ACos"; }
    }

    class FnTan : FnTrig
    {
        public const string StrFunctionName = "Tan";
        public FnTan() : base(name: StrFunctionName) { }
        protected override double CalcValue(double angle) { return Math.Tan(angle); }
        protected override string InverseName() { return "ATan"; }
    }

    // ++++++++++++++++++++++++++++++++++

    abstract class FnATrig : FnOneArg
    {
        public FnATrig(string name) :
            base(name, _dimensionlessDim, _planeAngleDim)
        { }

        public static Dimensions _dimensionlessDim { get; } = new Dimensions();
        public static Dimensions _planeAngleDim { get; } = new Dimensions(planeAngle: 1);
    }

    class FnASin : FnATrig
    {
        public const string StrFunctionName = "ASin";
        public FnASin() : base(name: StrFunctionName) { }
        protected override double CalcValue(double angle) { return Math.Asin(angle); }
        protected override string InverseName() { return "Sin"; }
    }

    class FnACos : FnATrig
    {
        public const string StrFunctionName = "ACos";
        public FnACos() : base(name: StrFunctionName) { }
        protected override double CalcValue(double angle) { return Math.Acos(angle); }
        protected override string InverseName() { return "Cos"; }
    }
    class FnATan : FnATrig
    {
        public const string StrFunctionName = "ATan";
        public FnATan() : base(name: StrFunctionName) { }
        protected override double CalcValue(double angle) { return Math.Atan(angle); }
        protected override string InverseName() { return "Tan"; }
    }

    // ++++++++++++++++++++++++++++++++++

}
