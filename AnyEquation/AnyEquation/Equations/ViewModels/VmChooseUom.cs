using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.FormsBook.Toolkit;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;

namespace AnyEquation.Equations.ViewModels
{
    public class VmChooseUom : ViewModelBase
    {
        #region ------------ Statics ------------

        #endregion ------------ Statics ------------

        #region ------------ Constructors and Life Cycle ------------

        public VmChooseUom(ContentManager contentManager, ParamType paramType)
        {
            ContentManager = contentManager;

            RefreshParamTypes(paramType);
            RefreshRowInfo();

            SelectionCancelled = false;
        }

        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Fields and Properties ------------

        // ------------------------------

        public ContentManager ContentManager { get; }

        // ------------------------------

        public enum enChooseUomGrouping
        {
            None, Group, Parameter
        }

        public enChooseUomGrouping ChooseUomGrouping { get; set; } = enChooseUomGrouping.Group;

        public bool SelectionCancelled { get; set; }

        public static IList<KnownUOM> RecentSelections { get; set; } = new List<KnownUOM>();

        private KnownUOM _selectedUOM;
        public KnownUOM SelectedUOM
        {
            get { return _selectedUOM; }
            set {
                if (value!=null)
                {
                    RecentSelections.Remove(value);
                    RecentSelections.Insert(0, value);
                }
                _selectedUOM = value;
            }
        }

        private ObservableCollection<ChooseUomRowInfo> _RowInfoCollection = new ObservableCollection<ChooseUomRowInfo>();
        public ObservableCollection<ChooseUomRowInfo> RowInfoCollection { get { return _RowInfoCollection; } set { _RowInfoCollection = value; } }

        private IList<List<ParamType>> _paramDisplayGroups = new List<List<ParamType>>();

        private ObservableCollection<ParamType> _ParamTypes = new ObservableCollection<ParamType>();
        public ObservableCollection<ParamType> ParamTypes { get { return _ParamTypes; } set { _ParamTypes = value; } }

        private ObservableCollection<string> _ParamTypeNames = new ObservableCollection<string>();
        public ObservableCollection<string> ParamTypeNames { get { return _ParamTypeNames; } set { _ParamTypeNames = value; } }

        bool _allowUnspecified = false;

        private void RefreshParamTypes(ParamType paramType)
        {
            _allowUnspecified = (paramType == null);

            _ParamTypes.Clear();
            _ParamTypeNames.Clear();

            if (paramType == null)
            {
                IList<ParamType> paramTypes = ContentManager.ParamTypes.Select(kvp => kvp.Value)
                                                .OrderByDescending(pt => pt.Popularity)
                                                .ThenBy(pt => pt.Name)
                                                .ToList();

                foreach (var item in paramTypes)
                {
                    _ParamTypes.Add(item);
                    _ParamTypeNames.Add(item.Name);
                }
            }
            else
            {
                _ParamTypes.Add(paramType);
                _ParamTypeNames.Add(paramType.Name);
            }
        }

        private void RefreshRowInfo()
        {
            int group = 0;
            int order = 0;

            IList<ParamType> paramTypes = _ParamTypes.Select(x=>x).ToList();

            // Only show recent items that match the param types
            IList<KnownUOM> filteredRecent = new List<KnownUOM>();

            if (ParamTypes==null || ParamTypes.Count==0)
            {
                filteredRecent = RecentSelections;      // No filtering
            }
            else
            {
                foreach (var item1 in RecentSelections)
                {
                    bool bShow = false;
                    foreach (var item2 in ParamTypes)
                    {
                        if (Dimensions.AreEqual(item1.Dimensions, item2.Dimensions))
                        {
                            bShow = true;
                            break;
                        }
                    }
                    if (bShow)
                    {
                        filteredRecent.Add(item1);
                    }
                }
            }

            if (_allowUnspecified)
            {
                group++;
                AddUnspecifiedItem(group, ref order);
            }

            group++;
            AddDiplayGroup("_Recent", filteredRecent, group, 999, "_Recent", null, ref order);

            foreach (var pt in _ParamTypes)
            {
                if (!AddToExistingDisplayGroup(pt))
                {
                    IList<KnownUOM> uoms = ContentManager.GetKnownUOMs(pt);
                    if (uoms?.Count > 0)
                    {
                        string paramGroup = ((_ParamTypes.Count == 1) ? "Param Type is " : ContentManager.DescribePopularity(pt.Popularity));
                        group++;
                        AddDiplayGroup(paramGroup, uoms, group, pt.Popularity, pt.Name, pt, ref order);
                    }
                }
            }

            RenameRowInfoParams();
        }

        void AddDiplayGroup(string paramGroup, IList<KnownUOM> uoms, int group, int ptPopularity, string paramName, ParamType pt, ref int order)
        {
            List<ParamType> pTypes=null;
            if (pt != null)
            {
                pTypes = new List<ParamType>();
                _paramDisplayGroups.Add(pTypes);
                pTypes.Add(pt);
            }

            foreach (var uom in uoms)
            {
                order++;
                ChooseUomRowInfo rowInfo = new ChooseUomRowInfo()
                {
                    Parent = this,
                    Group = $"{group:000}",     //-{ptPopularity}
                    Order = $"{order:000000}",
                    Parameter = $"{paramName}",
                    Unit = uom.Name,

                    ParamGroup = paramGroup,
                    Uom = uom,
                    ParamTypes = pTypes,
                };

                RowInfoCollection.Add(rowInfo);
            }
        }

        /// Returns true if the ParamType matches one already added
        bool AddToExistingDisplayGroup(ParamType pt)
        {
            foreach (var ptGroup in _paramDisplayGroups)
            {
                ParamType pt2 = ptGroup[0];
                if (Dimensions.AreEqual(pt.Dimensions,pt2.Dimensions))
                {
                    ptGroup.Add(pt);

                    // RowInfo items will be renamed later
                    //foreach (var rowInfo in RowInfoCollection)
                    //{
                    //    if (rowInfo.ParamTypes==ptGroup)
                    //    {
                    //        rowInfo.Parameter += $", {pt.Name}";
                    //    }
                    //}

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Rename RowInfo Parameter info if there are multiple ParamTypes that match
        /// </summary>
        void RenameRowInfoParams()
        {
            foreach (var ptGroup in _paramDisplayGroups)
            {
                // Sort the items according to popularity
                ptGroup.OrderByDescending((p) => p.Popularity);

                string pgName = null;
                foreach (var pt in ptGroup)
                {
                    if (pgName!=null) {
                        pgName += ", ";
                    }
                    pgName += pt.Name;
                }

                foreach (var rowInfo in RowInfoCollection)
                {
                    if (rowInfo.ParamTypes == ptGroup)
                    {
                        rowInfo.Parameter = pgName;
                    }
                }
            }
        }

        void AddUnspecifiedItem(int group, ref int order)
        {
            RowInfoCollection.Add(new ChooseUomRowInfo()
            {
                Parent = this,
                Group = $"{group:000}",     //-{ptPopularity}
                Order = $"{order:000000}",
                Parameter = "_Unspecified",
                Unit = null,

                ParamGroup = null,
                Uom = null,
            });
        }

        private int _paramTypeIndex;
        public int ParamTypeIndex
        {
            get { return _paramTypeIndex; }
            set
            {
                if (SetProperty(ref _paramTypeIndex, value))
                {
                    RefreshUOMs();
                }
            }
        }

        private ObservableCollection<KnownUOM> _UOMs = new ObservableCollection<KnownUOM>();
        public ObservableCollection<KnownUOM> UOMs { get { return _UOMs; } set { _UOMs = value; } }

        private void RefreshUOMs()
        {
            _UOMs.Clear();

            ParamType paramType=null;
            if (ParamTypes.Count > 0)
            {
                paramType = ParamTypes[_paramTypeIndex];
            }

            foreach (var item in ContentManager.GetKnownUOMs(paramType))
            {
                _UOMs.Add(item);
            }
        }

        #endregion ------------ Fields and Properties ------------

        // -------------------- Private class

        public class ChooseUomRowInfo
        {
            public VmChooseUom Parent { get; set; }
            public string Group { get; set; }
            public string Order { get; set; }
            public string Parameter { get; set; }
            public string Unit { get; set; }

            public string ParamGroup { get; set; }
            public KnownUOM Uom { get; set; }
            public IList<ParamType> ParamTypes { get; set; }     // The list will be shared between rows

        }
    }
}
