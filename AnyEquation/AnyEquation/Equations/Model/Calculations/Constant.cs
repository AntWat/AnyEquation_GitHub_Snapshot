using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Database;
using static AnyEquation.Common.Utils;
using AnyEquation.Common;

namespace AnyEquation.Equations.Model
{
    public class Constant : SingleValue, IContent
    {
        public const string ConstantPrefix = "_";

        public Constant(string packageName, string name, string description, ParamType paramType, double value, AnonUOM anonUOM, bool enforceNameFormat=true) :
            base( new CalcQuantity(value, CalcStatus.Good, "", anonUOM), name, description, paramType)
        {
            if (enforceNameFormat && !name.Substring(0,1).Equals(ConstantPrefix)) { throw new UnspecifiedException($"Constant names must begin with '{ConstantPrefix}' but {name} does not"); }
            PackageName = packageName;
        }

        public override string Text { get { return Name; } }

        public string PackageName { get; }

        // -----------------------

        private const int ConstantPrefixLength = 1;

        public static string RemovePrefix(string name)
        {
            if (name == null) { return name; }
            if (name.Length < ConstantPrefixLength) return name;
            if (name.Substring(0, ConstantPrefixLength).Equals(ConstantPrefix)) return name.Substring(ConstantPrefixLength);
            return name;
        }

        #region ------------ Testing conflicts when data is imported from a database

        public static ConstantConflictInfo TestConflict(string packageName, TblConstant_Row t1, Constant t2)
        {
            if (!t1.Name.Equals(t2.Name)) return null;

            if (!t2.ParamType.Name.Equals(t1.ParamType))
            {
                return new ConstantConflictInfo(packageName, t1, ConflictType.InConsistent, ConstantConflictReason.ParamType, "ParamType", "Different ParamTypes");
            }

            // TODO: Tolerance:
            if ((t1.Value != t2.CalcQuantity.Value))
            {
                return new ConstantConflictInfo(packageName, t1, ConflictType.InConsistent, ConstantConflictReason.Value, "Value", "Different values");
            }

            if (!t1.Description.Equals(t2.Description))
            {
                return new ConstantConflictInfo(packageName, t1, ConflictType.Consistent, ConstantConflictReason.Description, "Description", "Different description");
            }

            return new ConstantConflictInfo(packageName, t1, ConflictType.Duplicate, ConstantConflictReason.Duplicate, "Duplicate", "Duplicate");
        }

        /// <summary>
        /// Create a new content item from the database row
        /// </summary>
        public static Constant NewContentItem(string packageName, TblConstant_Row t1, IList<ParamType> paramTypes, IList<UOMSet> uomSets)
        {
            ParamType paramType = FindInList<ParamType>(paramTypes, (item) => item.Name.Equals(t1.ParamType));
            if (paramType == null) { return null; }

            // -------------
            UOMSet uomSet = FindInList<UOMSet>(uomSets, (item) => item.Name.Equals(t1.UnitSet));
            if (uomSet == null) { return null; }

            AnonUOM anonUOM = new AnonUOM(paramType.Dimensions, uomSet);

            // -------------
            Constant constant = new Constant(packageName, t1.Name, t1.Description, paramType, t1.Value, anonUOM);

            return constant;
        }

        #endregion ------------ Testing conflicts when data is imported from a database
    }

    #region ------------ Recording conflicts when data is imported from a database

    public enum ConstantConflictReason
    {
        None = 0, Duplicate, ParamType, Value, Description
    }

    public class ConstantConflictInfo : ConflictInfo<TblConstant_Row>
    {
        public ConstantConflictInfo(string packageName, TblConstant_Row conflictingItem, ConflictType conflictType,
                                    ConstantConflictReason constantConflictReason, string conflictReason, string explanation)
            : base(packageName, conflictingItem, conflictType, conflictReason, explanation)
        {
            ConstantConflictReason = constantConflictReason;
        }

        public ConstantConflictReason ConstantConflictReason { get; }
    }

    public class ConstantConflictSet : ConflictSet<Constant, TblConstant_Row>
    {
        public ConstantConflictSet(Constant mainItem) : base(mainItem)
        {
        }
    }

    #endregion ------------ Recording conflicts when data is imported from a database

}
