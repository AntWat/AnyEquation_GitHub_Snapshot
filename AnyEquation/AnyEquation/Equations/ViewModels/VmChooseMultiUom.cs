using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.FormsBook.Toolkit;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using System.Windows.Input;
using Xamarin.Forms;

namespace AnyEquation.Equations.ViewModels
{
    public class VmChooseMultiUom : ViewModelBase
    {

        #region ------------ Constructors and Life Cycle ------------

        private IEquationsUiService _equationsUiService;
        public IEquationsUiService EquationsUiService { get { return _equationsUiService; } }

        public VmChooseMultiUom(ContentManager contentManager, IEquationsUiService equationsUiService, 
                                    IList<UOMSet> uomSets, int uomSetSelectedIndex, IList<KnownUOM> uomSelections)
        {
            ContentManager = contentManager;
            _equationsUiService = equationsUiService;

            RefreshUomSetNames(uomSets, uomSetSelectedIndex);
            InitUomSelections(uomSelections);

            RefreshParamTypes();
            RefreshRowInfo();

            SelectionCancelled = false;
        }

        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Fields and Properties ------------

        // ------------------------------

        public ContentManager ContentManager { get; }

        // ------------------------------

        public enum enChooseMultiUomGrouping
        {
            None, Group, Parameter
        }

        public enChooseMultiUomGrouping ChooseMultiUomGrouping { get; set; } = enChooseMultiUomGrouping.Group;

        public bool SelectionCancelled { get; set; }

        public IList<UOMSet> UOMSets { get; } = new List<UOMSet>();
        private ObservableCollection<string> _uomSetNames = new ObservableCollection<string>();
        public ObservableCollection<string> UomSetNames { get { return _uomSetNames; } set { _uomSetNames = value; } }

        public int UomSetSelectedIndex { get; set; }
        public UOMSet SelectedUomSet { get { return UOMSets[UomSetSelectedIndex];  } }

        private ObservableCollection<KnownUOM> _UomSelections = new ObservableCollection<KnownUOM>();
        public ObservableCollection<KnownUOM> UomSelections { get { return _UomSelections; } set { _UomSelections = value; } }

        private ObservableCollection<ChooseMultiUomRowInfo> _RowInfoCollection = new ObservableCollection<ChooseMultiUomRowInfo>();
        public ObservableCollection<ChooseMultiUomRowInfo> RowInfoCollection { get { return _RowInfoCollection; } set { _RowInfoCollection = value; } }

        private IList<List<ParamType>> _paramDisplayGroups = new List<List<ParamType>>();

        private ObservableCollection<ParamType> _ParamTypes = new ObservableCollection<ParamType>();
        public ObservableCollection<ParamType> ParamTypes { get { return _ParamTypes; } set { _ParamTypes = value; } }

        private ObservableCollection<string> _ParamTypeNames = new ObservableCollection<string>();
        public ObservableCollection<string> ParamTypeNames { get { return _ParamTypeNames; } set { _ParamTypeNames = value; } }

        /// <param name="selectedItems">Collection of selected ChooseMultiUomRowInfo items</param>
        public void UpdateSelectedItems(ObservableCollection<object> selectedItems)
        {
            try
            {
                UomSelections.Clear();
                foreach (ChooseMultiUomRowInfo item in selectedItems)
                {
                    if (!UomSelections.Contains(item.Uom))
                    {
                        UomSelections.Add(item.Uom);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        public IList<ChooseMultiUomRowInfo>GetSelectedItems()
        {
            try
            {
                IList<ChooseMultiUomRowInfo> selectedItems = new List<ChooseMultiUomRowInfo>();
                foreach (var uom in UomSelections)
                {
                    foreach (var row in _RowInfoCollection)
                    {
                        if (row.Uom==uom)
                        {
                            selectedItems.Add(row);
                        }
                    }
                }
                return selectedItems;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void RefreshUomSetNames(IList<UOMSet> uomSets, int uomSetSelectedIndex)
        {
            UOMSets.Clear();
            UomSetNames.Clear();
            foreach (var item in uomSets)
            {
                UOMSets.Add(item);
                UomSetNames.Add(item.Name);
            }

            UomSetSelectedIndex = uomSetSelectedIndex;
        }

        private void InitUomSelections(IList<KnownUOM> uomSelections)
        {
            foreach (var item in uomSelections)
            {
                UomSelections.Add(item);
            }
        }

        private void RefreshParamTypes()
        {
            _ParamTypes.Clear();
            _ParamTypeNames.Clear();

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

        private void RefreshRowInfo()
        {
            int group = 0;
            int order = 0;

            IList<ParamType> paramTypes = _ParamTypes.Select(x => x).ToList();

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
            List<ParamType> pTypes = null;
            if (pt != null)
            {
                pTypes = new List<ParamType>();
                _paramDisplayGroups.Add(pTypes);
                pTypes.Add(pt);
            }

            foreach (var uom in uoms)
            {
                order++;
                ChooseMultiUomRowInfo rowInfo = new ChooseMultiUomRowInfo()
                {
                    Parent = this,
                    Group = $"{group:000}",
                    Order = $"{order:000000}",
                    Parameter = $"{paramName}",
                    Unit = uom.Name,

                    ParamGroup = paramGroup,
                    Uom = uom,
                    ParamTypes = pTypes,

                    IsSelected = false,
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
                if (Dimensions.AreEqual(pt.Dimensions, pt2.Dimensions))
                {
                    ptGroup.Add(pt);

                    // RowInfo items will be renamed later
                    //foreach (var rowInfo in RowInfoCollection)
                    //{
                    //    if (rowInfo.ParamTypes == ptGroup)
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
                    if (pgName != null)
                    {
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

            ParamType paramType = null;
            if (ParamTypes.Count > 0)
            {
                paramType = ParamTypes[_paramTypeIndex];
            }

            foreach (var item in ContentManager.GetKnownUOMs(paramType))
            {
                _UOMs.Add(item);
            }
        }

        // -------------------------


        #endregion ------------ Fields and Properties ------------


        #region ------------ Command Bindings ------------

        public void ShowItemsAndDeselect(string title, string subTitle, Func<IDictionary<string, /*selected*/bool>, bool /*Result: not used*/> okCallback)
        {
            try
            {
                IDictionary<string, /*selected*/bool> itemsAndSelections = new Dictionary<string, /*selected*/bool>();

                foreach (var item in UomSelections)
                {
                    itemsAndSelections.Add(item.Name, true);
                }
                _equationsUiService.SelectStringsFromList(itemsAndSelections, title, subTitle, okCallback);

                return;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        #endregion ------------ Command Bindings ------------

        // -------------------- Private class

        public class ChooseMultiUomRowInfo
        {
            public VmChooseMultiUom Parent { get; set; }
            public string Group { get; set; }
            public string Order { get; set; }
            public string Parameter { get; set; }
            public string Unit { get; set; }

            public string ParamGroup { get; set; }
            public KnownUOM Uom { get; set; }
            public IList<ParamType> ParamTypes { get; set; }     // The list will be shared between rows

            public bool IsSelected;
        }
    }
}
