using AnyEquation.Common;
using AnyEquation.Equations.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: Equations: Override == and != and GetHashCode

namespace AnyEquation.Equations.Model
{
    /// This class and its sub-classes should be immutable once created, so instances can be safely shared for efficiency
    public class KnownUOM : UOM, IContent
    {
        public KnownUOM(string packageName, Dimensions Dimensions, 
                        string Name, string Description, string Symbol, 
                        bool IsBase, KnownUOM BaseUOM,
                        double AFactor, double BFactor, double CFactor, double DFactor) :
            base(Dimensions)
        {
            PackageName = packageName;

            _Name = Name;
            _Description = Description;
            _Symbol = Symbol;
            _IsBase = IsBase;

            _BaseUOM = BaseUOM;

            _AFactor = AFactor;
            _BFactor = BFactor;
            _CFactor = CFactor;
            _DFactor = DFactor;
        }


        public string PackageName { get; }

        private string _Name;
        public string Name { get { return _Name; } }

        private string _Description;
        public string Description { get { return _Description; } }

        private string _Symbol;
        public string Symbol { get { return _Symbol; } }

        private bool _IsBase;
        public bool IsBase { get { return _IsBase; } }

        private KnownUOM _BaseUOM;
        public KnownUOM BaseUOM { get { return _BaseUOM; }}

        // Conversion TO base unit = (A + BX) / (C + DX)
        private double _AFactor;
        public double AFactor { get { return _AFactor; } }

        private double _BFactor;
        public double BFactor { get { return _BFactor; } }

        private double _CFactor;
        public double CFactor { get { return _CFactor; } }

        private double _DFactor;
        public double DFactor { get { return _DFactor; } }


        #region ------------ Testing conflicts when data is imported from a database

        public static UOMConflictInfo TestConflict(string packageName, TblUOM_Row t1, KnownUOM t2)
        {
            if (!t1.Name.Equals(t2.Name)) return null;

            if (!t1.Dimensions.Equals(t2.Dimensions))
            {
                return new UOMConflictInfo(packageName, t1, ConflictType.InConsistent, UOMConflictReason.Dimensions, "Dimensions", "Inconsistent dimensions");
            }

            // TODO: Tolerance:
            if ( (t1.Factor_A!=t2.AFactor) || (t1.Factor_B != t2.BFactor) || (t1.Factor_C != t2.CFactor) || (t1.Factor_D != t2.DFactor) )
            {
                return new UOMConflictInfo(packageName, t1, ConflictType.InConsistent, UOMConflictReason.Factors, "Factors", "Inconsistent factors");
            }

            if (!t1.Description.Equals(t2.Description))
            {
                return new UOMConflictInfo(packageName, t1, ConflictType.Consistent, UOMConflictReason.Description, "Description", "Different description");
            }

            return new UOMConflictInfo(packageName, t1, ConflictType.Duplicate, UOMConflictReason.Duplicate, "Duplicate", "Duplicate");
        }

        /// <summary>
        /// Create a new content item from the database row
        /// </summary>
        /// <param name="knownSoFar">Used to find the baseUOM, not to check for consistency (which is done outside this routine)</param>
        public static KnownUOM NewContentItem(string packageName, TblUOM_Row t1, Dictionary<string, KnownUOM> knownSoFar)
        {
            bool isBase = t1.IsBase;
            KnownUOM baseUOM = null;

            if (!isBase)
            {
                foreach (var item in knownSoFar)
                {
                    if (Dimensions.AreEqual(item.Value.Dimensions, t1.Dimensions))
                    {
                        baseUOM = item.Value;
                        break;
                    }
                }
                if (baseUOM==null)
                {
                    int iDbg = 0;
                    throw new UnspecifiedException();
                }
            }

            KnownUOM knownUOM = new KnownUOM(packageName, t1.Dimensions,
                        t1.Name, t1.Description, t1.Symbol,
                        isBase, baseUOM,
                        t1.Factor_A, t1.Factor_B, t1.Factor_C, t1.Factor_D);

            return knownUOM;
        }

        #endregion ------------ Testing conflicts when data is imported from a database
    }

    #region ------------ Recording conflicts when data is imported from a database

    public enum UOMConflictReason
    {
        None = 0, Duplicate, Description, Dimensions, Factors
    }

    public class UOMConflictInfo : ConflictInfo<TblUOM_Row>
    {
        public UOMConflictInfo(string packageName, TblUOM_Row conflictingItem, ConflictType conflictType, 
                                    UOMConflictReason uOMConflictReason, string conflictReason, string explanation)
            : base(packageName, conflictingItem, conflictType, conflictReason, explanation)
        {
            UOMConflictReason = uOMConflictReason;
        }

        public UOMConflictReason UOMConflictReason { get; }
    }

    public class UOMConflictSet : ConflictSet<KnownUOM, TblUOM_Row>
    {
        public UOMConflictSet(KnownUOM mainItem) : base(mainItem)
        {
        }
    }

    #endregion ------------ Recording conflicts when data is imported from a database
}
