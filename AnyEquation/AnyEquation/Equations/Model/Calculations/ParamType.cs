using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Database;
using AnyEquation.Common;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{
    public class ParamType : IContent
    {
        public ParamType(string packageName, string name, string description, string siBaseUom, Dimensions dimensions, int popularity)
        {
            Name = name;
            Description = description;
            Dimensions = dimensions;
            Popularity = popularity;
            PackageName = packageName;

            if (!Dimensions.IsDimenionless(dimensions))
            {
                Result<KnownUOM> r_SIBaseUom = ContentManager.SiBaseUomFromDimensions(dimensions, siBaseUom);
                if (r_SIBaseUom.IsGood())
                {
                    SIBaseUom = r_SIBaseUom.Value;
                }
                else
                {
                    throw new UnspecifiedException(r_SIBaseUom.Message);
                }
            }
        }

        public string PackageName { get; }
        public string Name { get; }
        public string Description { get; set; }
        public Dimensions Dimensions { get; set; }

        public int Popularity { get; set; }
        /* Higher numbers will be more popular, so appear higher in selection lists.
                Rough categories:
                0 or empty: default
                1 = Less Common
                2 = Common
                3+ = Very Popular etc.*/

        public KnownUOM SIBaseUom { get; }


        #region ------------ Testing conflicts when data is imported from a database

        public static ParamTypeConflictInfo TestConflict(string packageName, TblParamTypes_Row t1, ParamType t2)
        {
            if (!t1.Name.Equals(t2.Name,StringComparison.OrdinalIgnoreCase)) return null;   // TODO: Case sensitive?

            if (!t1.Dimensions.Equals(t2.Dimensions))
            {
                return new ParamTypeConflictInfo(packageName, t1, ConflictType.InConsistent, ParamTypeConflictReason.Dimensions, "Dimensions", "Inconsistent dimensions");
            }

            if (!t1.Description.Equals(t2.Description))
            {
                return new ParamTypeConflictInfo(packageName, t1, ConflictType.Consistent, ParamTypeConflictReason.Description, "Description", "Different description");
            }

            return new ParamTypeConflictInfo(packageName, t1, ConflictType.Duplicate, ParamTypeConflictReason.Duplicate, "Duplicate", "Duplicate");
        }

        /// <summary>
        /// Create a new content item from the database row
        /// </summary>
        /// <param name="baseUomRows">Used to store the base uom definitions that are found within the param type rows</param>
        public static ParamType NewContentItem(string packageName, TblParamTypes_Row t1)
        {
            ParamType paramType = new ParamType(packageName, t1.Name, t1.Description, t1.BaseUom, t1.Dimensions, t1.Popularity);

            return paramType;
        }
        
        #endregion ------------ Testing conflicts when data is imported from a database
    }

    #region ------------ Recording conflicts when data is imported from a database

    public enum ParamTypeConflictReason
    {
        None = 0, Duplicate, Description, Dimensions
    }

    public class ParamTypeConflictInfo : ConflictInfo<TblParamTypes_Row>
    {
        public ParamTypeConflictInfo(string packageName, TblParamTypes_Row conflictingItem, ConflictType conflictType, ParamTypeConflictReason paramTypeConflictReason, string conflictReason, string explanation)
            : base(packageName, conflictingItem, conflictType, conflictReason, explanation)
        {
            ParamTypeConflictReason = paramTypeConflictReason;
        }

        public ParamTypeConflictReason ParamTypeConflictReason { get; }
    }

    public class ParamTypeConflictSet : ConflictSet<ParamType, TblParamTypes_Row>
    {
        public ParamTypeConflictSet(ParamType mainItem) : base(mainItem)
        {
        }
    }

    #endregion ------------ Recording conflicts when data is imported from a database
}
