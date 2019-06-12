using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    /// <summary>
    /// Represents any set of UOM, which do not have to be consistent (e.g. you can mix km and m^2).
    /// None of the contents should have identical dimensions.
    /// </summary>
    public class AnyUOMSet
    {
        public AnyUOMSet(string name)
        {
            Name = name;
        }

        public virtual bool IsConsistent()
        {
            return false;
        }

        public UOMSet ParentUomSet { get; set; }       // This will be used to find UOMs if ones don't exist here, and we don't ask to create them.  This will typically be the SI set or similar.

        public string Name { get; set; }

        private IList<KnownUOM> _KnownUOMs = new List<KnownUOM>();

        public IList<KnownUOM> KnownUOMs
        {
            get { return _KnownUOMs; }
            set { _KnownUOMs = value; }
        }


        public KnownUOM FindFromName(string name)
        {
            foreach (var item in KnownUOMs)
            {
                if (item.Name.Equals(name))
                {
                    return item;
                }
            }
            return null;
        }

        public KnownUOM FindFromParamType(ParamType ParamType)
        {
            Dimensions dimensions = ParamType?.Dimensions;

            return FindFromDimensions(dimensions);
        }

        /// Find the KownUOM that has the dimensions
        public KnownUOM FindFromDimensions(Dimensions dimensions)
        {
            bool WasCreated;

            return FindFromDimensions(dimensions, false, out WasCreated);
        }

        public KnownUOM FindFromDimensions(Dimensions dimensions, bool bAllowCreate, out bool wasCreated, string name=null)
        {
            if (bAllowCreate && !IsConsistent())
            {
                bAllowCreate = false;       // Only makes sense to create base units in a consistent set
            }

            wasCreated = false;

            if (dimensions == null) return null;

            foreach (var item in KnownUOMs)
            {
                if (item.Dimensions.Equals(dimensions))
                {
                    return item;
                }
            }

            if (bAllowCreate && IsConsistent() && !Dimensions.IsDimenionless(dimensions))
            {
                wasCreated = true;

                string sName = name;

                if (string.IsNullOrEmpty(sName))
                {
                    string sTop = "";
                    string sBottom = "";

                    AddUomStringPart(DimType.Mass, dimensions.Mass, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.Length, dimensions.Length, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.Time, dimensions.Time, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.ElectricCurrent, dimensions.ElectricCurrent, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.Temperature, dimensions.Temperature, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.QuantityOfSubstance, dimensions.QuantityOfSubstance, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.Luminosity, dimensions.Luminosity, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.PlaneAngle, dimensions.PlaneAngle, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.SolidAngle, dimensions.SolidAngle, ref sTop, ref sBottom);
                    AddUomStringPart(DimType.Currency, dimensions.Currency, ref sTop, ref sBottom);

                    sName = string.Format("{0}{1}{2}", (sTop.Length == 0 ? "1" : sTop), (sBottom.Length == 0 ? "" : "/"), (sBottom.Length == 0 ? "" : sBottom));
                }

                KnownUOM uom = new KnownUOM("", dimensions, Name: sName, Description: sName, Symbol: sName,
                    IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

                KnownUOMs.Add(uom);
                return uom;
            }
            else
            {
                return ParentUomSet?.FindFromDimensions(dimensions, bAllowCreate, out wasCreated);
            }

            return null;
        }

        private void AddUomStringPart(DimType dimType, double dimPartVal, ref string sTop, ref string sBottom)
        {
            if (dimPartVal == 0) return;

            KnownUOM baseUom;

            baseUom = this.FindFromDimensions(new Dimensions(dimType, 1));

            if (dimPartVal > 0)
            {
                sTop += baseUom.Symbol;
                if (dimPartVal != 1) sTop += string.Format("^{0}", dimPartVal);
            }
            else if (dimPartVal<0)
            {
                sBottom += baseUom.Symbol;
                if (dimPartVal != -1) sBottom += string.Format("^{0}",Math.Abs(dimPartVal));
            }
        }

        /// Convert the value with the dimensions from this AnyUOMSet to tgtUOMSet
        public double Convert(double theValue, Dimensions dimensions, AnyUOMSet tgtUOMSet)
        {
            KnownUOM srcKownUOM = this.FindFromDimensions(dimensions);
            KnownUOM tgtKownUOM = tgtUOMSet.FindFromDimensions(dimensions);

            return UOM.Convert(theValue, srcKownUOM, tgtKownUOM);
        }

    }
}
