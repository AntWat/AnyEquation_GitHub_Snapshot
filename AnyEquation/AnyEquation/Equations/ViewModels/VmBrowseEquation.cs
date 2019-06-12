using AnyEquation.Common;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.FormsBook.Toolkit;

namespace AnyEquation.Equations.ViewModels
{
    public class VmBrowseEquation1 : ViewModelBase
    {

        #region ------------ Constructors and Life Cycle ------------

        private VmEquations _vmEquations;

        public VmBrowseEquation1(VmEquations vmEquations)
        {
            try
            {
                _vmEquations = vmEquations;
                InitData();

                // -------------------
                SelectEquation = new Command(
                    execute: () =>
                    {
                        _vmEquations.EquationsUiService.SelectEqnCalcFromList(5, GetEquationCalcs(),
                            (eqn) => { CurrentEquationCalc = eqn; return true; });
                    });
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }

        }

        public ICommand SelectEquation { private set; get; }

        public string SelectEquationText { get; set; } = "Select an equation";

        #endregion ------------ Constructors and Life Cycle ------------

        private void InitData()
        {
            try
            {
                TreeNode1 topNode = new TreeNode1(null, $"Top", null);
                topNode.NumDescendantEquations = 0;

                SelectedTreeLevels.Clear();
                CurrentlySelectedTreeLevel = new SelectedTreeLevel1(null, topNode, "");
                SelectedTreeLevels.Add(CurrentlySelectedTreeLevel);

                // Recursively build the tree information from the libraries
                foreach (var eqbLib in _vmEquations.EquationLibraries)
                {
                    TreeNode1 libTn = new TreeNode1(topNode, eqbLib.Name, null);
                    libTn.NumDescendantEquations = 0 ;

                    foreach (var sn in eqbLib.TopSectionNodes)
                    {
                        TreeNode1 tn = new TreeNode1(libTn, sn.Name, sn);

                        AddChildNodes(sn, tn);

                        libTn.NumDescendantEquations += tn.NumDescendantEquations;
                    }

                    topNode.NumDescendantEquations += libTn.NumDescendantEquations;
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void AddChildNodes(SectionNode parentSn, TreeNode1 parentTn)
        {
            int numChildDescendantEquations = 0;
            foreach (var sn in parentSn.Children)
            {
                TreeNode1 childTn = new TreeNode1(parentTn, sn.Name, sn);

                AddChildNodes(sn, childTn);
                numChildDescendantEquations += childTn.NumDescendantEquations;
            }

            parentTn.NumDescendantEquations = numChildDescendantEquations + parentSn.EqnCalcs.Count;
        }

        #region ------------ Fields and Properties ------------

        private ObservableCollection<SelectedTreeLevel1> _selectedTreeLevels = new ObservableCollection<SelectedTreeLevel1>();
        public ObservableCollection<SelectedTreeLevel1> SelectedTreeLevels { get { return _selectedTreeLevels; } set { _selectedTreeLevels = value; } }

        private SelectedTreeLevel1 CurrentlySelectedTreeLevel { get; set; }

        private IDictionary<string, TreeNode1> _selectedLevelChildren = new Dictionary<string, TreeNode1>();

        private TreeNode1 _selectedLevelSelectedChild = null;

        IList<IGrouping<string, EqnCalc>> GetEquationCalcs()
        {
            IList<IGrouping<string, EqnCalc>> equationCalcs = new List<IGrouping<string, EqnCalc>>();

            if ( (CurrentlySelectedTreeLevel.Parent==null) && (CurrentlySelectedTreeLevel?.SelectedChildNode==null) )        // All libraries
            {
                foreach (var tn in CurrentlySelectedTreeLevel.TreeNode.ChildTreeNodes)
                {
                    AddEquationCalcs(tn, equationCalcs);
                }
            }
            else
            {
                AddEquationCalcs(CurrentlySelectedTreeLevel?.SelectedChildNode, equationCalcs);
            }

            return equationCalcs;
        }
        void AddEquationCalcs(TreeNode1 treeNode, IList<IGrouping<string, EqnCalc>> equationCalcs)
        {
            if (treeNode == null) return;

            if (treeNode?.SectionNode != null)
            {

                IEnumerable<IGrouping<string, EqnCalc>> groups =
                                            treeNode.SectionNode.EqnCalcs
                                            //.OrderBy(x => x.SortString)
                                            .GroupBy(x => GetNodePath(treeNode, includeNode:true, excludeTop:true));

                foreach (var item in groups) { equationCalcs.Add(item); }
            }

            foreach (var tn in treeNode.ChildTreeNodes)
            {
                AddEquationCalcs(tn, equationCalcs);
            }
        }

        public void AcceptCurrentEquationCalc()
        {
            _vmEquations.CurrentEquationCalc = CurrentEquationCalc;
        }

        private EqnCalc _currentEquationCalc;
        public EqnCalc CurrentEquationCalc { get { return _currentEquationCalc; }
            set
            {
                if (_currentEquationCalc != value)
                {
                    _currentEquationCalc = value;
                    OnPropertyChanged("CurrentEquationCalc");

                    RefreshVisibles();
                }
            }
        }

        public bool equationDisplay_IsVisible { get { return (_currentEquationCalc != null); } }
        private void RefreshVisibles()
        {
            OnPropertyChanged("equationDisplay_IsVisible");
        }

        public string SelectedLevelSelectedChildName
        {
            get { return _selectedLevelSelectedChild?.Name; }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        _selectedLevelSelectedChild = null;
                    }
                    else if (CurrentlySelectedTreeLevel == null)
                    {
                        throw new UnspecifiedException("");
                    }
                    else if ((_selectedLevelSelectedChild == null) || (!_selectedLevelSelectedChild.Name.Equals(value)))
                    {
                        _selectedLevelSelectedChild = _selectedLevelChildren[(string)value];
                        CurrentlySelectedTreeLevel.SelectedChildNode = _selectedLevelSelectedChild;

                        CurrentEquationCalc = null;
                        OnPropertyChanged("CurrentEquationCalc");

                        RefreshVisibles();

                        RefreshSelectedTreeLevels(_selectedLevelSelectedChild);
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }
        }

        private void RefreshSelectedTreeLevels(TreeNode1 lastTnToKeep)
        {
            try
            {
                int lastGoodItem = 0;
                for (int i = 0; i < SelectedTreeLevels.Count; i++)
                {
                    if (SelectedTreeLevels[i]?.SelectedChildNode == lastTnToKeep)
                    {
                        lastGoodItem = i;
                        break;
                    }
                }

                int j = SelectedTreeLevels.Count - 1;
                while (j > lastGoodItem)
                {
                    SelectedTreeLevels.RemoveAt(j);
                    j--;
                }

                // Add the selection point at the end
                SelectedTreeLevel1 lastSelectedTreeLevel = SelectedTreeLevels[SelectedTreeLevels.Count - 1];
                if (lastSelectedTreeLevel?.SelectedChildNode?.ChildTreeNodes?.Count>0)
                {
                    SelectedTreeLevel1 newSelectedTreeLevel = new SelectedTreeLevel1(lastSelectedTreeLevel, lastSelectedTreeLevel.SelectedChildNode, lastSelectedTreeLevel.Indent + "  ");
                    SelectedTreeLevels.Add(newSelectedTreeLevel);
                }
                else
                {
                    SelectEquation.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        public void SelectTreeLevel(SelectedTreeLevel1 selectedTreeLevel)
        {
            try
            {
                CurrentlySelectedTreeLevel = selectedTreeLevel;
                CurrentEquationCalc = null;
                OnPropertyChanged("CurrentEquationCalc");

                if (CurrentlySelectedTreeLevel == null) throw new UnspecifiedException("CurrentlySelectedTreeLevel should not be null");

                _selectedLevelChildren.Clear();
                _selectedLevelSelectedChild = null;

                IList<string> itemsToShow = new List<string>();
                IDictionary<string, string> itemsToReturn = new Dictionary<string, string>();
                foreach (var node in CurrentlySelectedTreeLevel?.TreeNode?.ChildTreeNodes)
                {
                    string itemToShow = node.Name + $" ({node.NumDescendantEquations} equations)";
                    itemsToShow.Add(itemToShow);
                    itemsToReturn.Add(itemToShow, node.Name);

                    _selectedLevelChildren.Add(node.Name, node);
                }

                if (itemsToShow.Count != 0)
                {
                    _vmEquations.EquationsUiService.SelectStringFromList(itemsToShow,
                        (s) => { SelectedLevelSelectedChildName = itemsToReturn[s]; return true; } );
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        #endregion ------------ Fields and Properties ------------


        #region ------------ Nested Classes ------------

        public class SelectedTreeLevel1 : ViewModelBase
        {
            public SelectedTreeLevel1(SelectedTreeLevel1 parent, TreeNode1 treeNode, string indent)
            {
                Parent = parent;
                TreeNode = treeNode;
                Indent = indent;
            }

            public SelectedTreeLevel1 Parent { get; set; }
            public TreeNode1 TreeNode { get; set; }

            private TreeNode1 _selectedChildNode;
            public TreeNode1 SelectedChildNode
            {
                get { return _selectedChildNode; }
                set
                {
                    _selectedChildNode = value;
                    OnPropertyChanged("DescriptiveText");
                }
            }

            public string Indent { get; set; }

            public string DescriptiveText
            {
                get
                {
                    string descriptiveText;
                    if (SelectedChildNode == null)
                    {
                        if (TreeNode?.ChildTreeNodes?.Count == 0)
                        {
                            descriptiveText = "Select an Equation...";      // TODO: Should never happen
                        }
                        else if (Parent == null)
                        {
                            descriptiveText = "Click here to select a Library...";
                        }
                        else if (Parent.Parent == null)
                        {
                            descriptiveText = "Select a Section...";
                        }
                        else
                        {
                            descriptiveText = "Select a Section...";
                        }
                        return Indent + descriptiveText;        // + $"  ({TreeNode.NumDescendantEquations})";
                    }
                    else
                    {
                        descriptiveText = SelectedChildNode.Name;
                        return Indent + descriptiveText + $"  ({SelectedChildNode.NumDescendantEquations})";
                    }


                }
            }

        }

        public class TreeNode1
        {
            public TreeNode1(TreeNode1 parent, string name, SectionNode sectionNode)
            {
                Parent = parent;
                Name = name;
                SectionNode = sectionNode;

                parent?.ChildTreeNodes?.Add(this);
            }

            public TreeNode1 Parent { get; set; }
            public string Name { get; set; }
            public SectionNode SectionNode { get; set; }

            public IList<TreeNode1> ChildTreeNodes { get; set; } = new List<TreeNode1>();

            public int NumDescendantEquations { get; set; }
        }


        public string GetNodePath(TreeNode1 tn, bool includeNode, bool excludeTop)
        {
            return GetNodePathRecursive(tn, excludeTop) + (includeNode? $"{tn.Name}" : "");
        }
        private string GetNodePathRecursive(TreeNode1 tn, bool excludeTop)
        {
            if (tn.Parent != null)
            {
                if (!excludeTop || (tn.Parent.Parent!=null))
                {
                    return GetNodePathRecursive(tn.Parent, excludeTop) + $"{tn.Parent.Name} > ";
                }
            }
            return "";
        }

        #endregion ------------ Nested Classes ------------

    }
}
