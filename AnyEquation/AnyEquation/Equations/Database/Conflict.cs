using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Model;

namespace AnyEquation.Equations.Database
{
    public enum ConflictType
    {
        None=0, Duplicate, Consistent, InConsistent
    }

    public class ConflictInfo<TableRow>
    {
        public ConflictInfo(string packageName, TableRow conflictingItem, ConflictType conflictType, string conflictReason, string explanation)
        {
            PackageName = packageName;
            ConflictingItem = conflictingItem;
            ConflictType = conflictType;
            ConflictReason = conflictReason;
            Explanation = explanation;
        }

        public string PackageName { get; }
        public TableRow ConflictingItem { get; }
        public ConflictType ConflictType { get; }
        public string ConflictReason { get; }
        public string Explanation { get; }
    }

    public class ConflictSet<T_Type, TableRow> where T_Type : IContent
    {
        public ConflictSet(T_Type mainItem)
        {
            MainItem = mainItem;
        }

        public T_Type MainItem { get; set; }
        
        private IList<ConflictInfo<TableRow>> _conflictingItems = new List<ConflictInfo<TableRow>>();
        public IList<ConflictInfo<TableRow>> ConflictingItems { get { return _conflictingItems; } set { _conflictingItems = value; } }

        // ------------------------------

        public static ConflictInfo<TableRow> AddIfNoConflict(string newPackageName, 
                                                    IDictionary<string, T_Type> MainItems, TableRow newItem,
                                                    IDictionary<T_Type, ConflictSet<T_Type, TableRow>> conflictSets,
                                                    Func<string /*packageName*/, TableRow /*t1*/, T_Type /*t2*/, ConflictInfo<TableRow>> testConflict,
                                                    Func<string /*packageName*/, TableRow /*t1*/, T_Type> newContentItem)
        {
            foreach (var item in MainItems)
            {
                T_Type mainItem = item.Value;
                ConflictInfo<TableRow> conflictInfo = testConflict(newPackageName, newItem, mainItem);

                if ((conflictInfo!=null) && (conflictInfo.ConflictType!=ConflictType.None) )
                {
                    if (!conflictSets.ContainsKey(mainItem))
                    {
                        conflictSets.Add(mainItem, new ConflictSet<T_Type, TableRow>(mainItem));
                    }
                    conflictSets[mainItem].ConflictingItems.Add(conflictInfo);
                    return conflictInfo;
                }
            }

            T_Type newMainItem = newContentItem(newPackageName, newItem);

            MainItems.Add(newMainItem.Name, newMainItem);
            return null;
        }

    }
}
