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
    public class VmBrowseEquation2 : ViewModelBase
    {

        #region ------------ Constructors and Life Cycle ------------

        private VmEquations _vmEquations;

        public VmBrowseEquation2(VmEquations vmEquations)
        {
            try
            {
                _vmEquations = vmEquations;
                InitData();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }

        }

        public void SelectEquation(Action finishAction)
        {
            _vmEquations.EquationsUiService.SelectEqnCalcFromList(5, GetEquationCalcs(),
                (eqn) => { PreviewEquation(eqn, finishAction);  return true; });
        }

        #endregion ------------ Constructors and Life Cycle ------------

        EquationLibraryContentNode _topNode = new EquationLibraryContentNode(null, $"Top", null);

        private void InitData()
        {
            try
            {
                _topNode.NumDescendantEquations = 0;
                _topNode.Expanded = true;

                // Recursively build the tree information from the libraries
                foreach (var item in _vmEquations.ContentManager.EquationLibraries)
                {
                    EquationLibrary eqbLib = item.Value;

                    EquationLibraryContentNode libTn = new EquationLibraryContentNode(_topNode, eqbLib.Name, null);
                    libTn.NumDescendantEquations = 0;

                    foreach (var sn in eqbLib.TopSectionNodes)
                    {
                        EquationLibraryContentNode tn = new EquationLibraryContentNode(libTn, sn.Name, sn);

                        AddChildNodes(sn, tn);

                        libTn.NumDescendantEquations += tn.NumDescendantEquations;
                    }

                    _topNode.NumDescendantEquations += libTn.NumDescendantEquations;
                }

                RefreshEquationLibraryDisplayTree();
                if (EquationLibraryDisplayTree?.Count>0)
                {
                    ExpandTreeLevel(EquationLibraryDisplayTree[0]);
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void RefreshEquationLibraryDisplayTree()
        {
            // Create the first level
            EquationLibraryDisplayTree.Clear();
            AddEquationLibraryDisplayLevels(true, null, _topNode);
        }

        private void AddEquationLibraryDisplayLevels(bool isTop, EquationLibraryDisplayNode parentNode, EquationLibraryContentNode contentNode)
        {
            EquationLibraryDisplayNode displayNode = null;

            if (!isTop)
            {
                displayNode = new EquationLibraryDisplayNode
                                                (parentNode, contentNode, ((parentNode == null) ? "" : parentNode.Indent + "  "));
                EquationLibraryDisplayTree.Add(displayNode);
            }

            if (contentNode.Expanded)
            {
                foreach (var childContentNode in contentNode.ChildTreeNodes)
                {
                    AddEquationLibraryDisplayLevels(false, displayNode, childContentNode);
                }
            }
        }


        private void AddChildNodes(SectionNode parentSn, EquationLibraryContentNode parentTn)
        {
            int numChildDescendantEquations = 0;
            foreach (var sn in parentSn.Children)
            {
                EquationLibraryContentNode childTn = new EquationLibraryContentNode(parentTn, sn.Name, sn);

                AddChildNodes(sn, childTn);
                numChildDescendantEquations += childTn.NumDescendantEquations;
            }

            parentTn.NumDescendantEquations = numChildDescendantEquations + parentSn.EqnCalcs.Count;
        }

        #region ------------ Fields and Properties ------------

        private ObservableCollection<EquationLibraryDisplayNode> _equationLibraryDisplayTree = new ObservableCollection<EquationLibraryDisplayNode>();
        public ObservableCollection<EquationLibraryDisplayNode> EquationLibraryDisplayTree { get { return _equationLibraryDisplayTree; } set { _equationLibraryDisplayTree = value; } }

        private EquationLibraryDisplayNode CurrentDisplayNode { get; set; } = null;

        //private IDictionary<string, EquationLibraryContentNode> _selectedLevelChildren = new Dictionary<string, EquationLibraryContentNode>();

        //private EquationLibraryContentNode _selectedLevelSelectedChild = null;

        IList<IGrouping<string, EqnCalc>> GetEquationCalcs()
        {
            IList<IGrouping<string, EqnCalc>> equationCalcs = new List<IGrouping<string, EqnCalc>>();
            AddEquationCalcs(CurrentDisplayNode?.ContentNode, equationCalcs);

            //if ((CurrentlySelectedTreeLevel.Parent == null) && (CurrentlySelectedTreeLevel?.SelectedChildNode == null))        // All libraries
            //{
            //    foreach (var tn in CurrentlySelectedTreeLevel.ContentNode.ChildTreeNodes)
            //    {
            //        AddEquationCalcs(tn, equationCalcs);
            //    }
            //}
            //else
            //{
            //    AddEquationCalcs(CurrentlySelectedTreeLevel?.SelectedChildNode, equationCalcs);
            //}

            return equationCalcs;
        }
        void AddEquationCalcs(EquationLibraryContentNode treeNode, IList<IGrouping<string, EqnCalc>> equationCalcs)
        {
            if (treeNode == null) return;

            if (treeNode?.SectionNode != null)
            {

                IEnumerable<IGrouping<string, EqnCalc>> groups =
                                            treeNode.SectionNode.EqnCalcs
                                            //.OrderBy(x => x.SortString)
                                            .GroupBy(x => GetNodePath(treeNode, includeNode: true, excludeTop: true));

                foreach (var item in groups) { equationCalcs.Add(item); }
            }

            foreach (var tn in treeNode.ChildTreeNodes)
            {
                AddEquationCalcs(tn, equationCalcs);
            }
        }

        private void PreviewEquation(EqnCalc eqnCalc, Action finishAction)
        {
            CurrentEquationCalc = eqnCalc;
            _vmEquations.EquationsUiService.PreviewEquation(_vmEquations.ContentManager, CurrentEquationCalc,
                okAction: ()=> { AcceptCurrentEquationCalc(); finishAction(); },
                cancelAction: ()=> { SelectEquation(finishAction); });
        }

        private void AcceptCurrentEquationCalc()
        {
            _vmEquations.CurrentEquationCalc = CurrentEquationCalc;
        }

        private EqnCalc _currentEquationCalc;
        public EqnCalc CurrentEquationCalc
        {
            get { return _currentEquationCalc; }
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

        //public string SelectedLevelSelectedChildName
        //{
        //    get { return _selectedLevelSelectedChild?.Name; }
        //    set
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(value))
        //            {
        //                _selectedLevelSelectedChild = null;
        //            }
        //            else if (CurrentlDisplayNode == null)
        //            {
        //                throw new UnspecifiedException("");
        //            }
        //            else if ((_selectedLevelSelectedChild == null) || (!_selectedLevelSelectedChild.Name.Equals(value)))
        //            {
        //                _selectedLevelSelectedChild = _selectedLevelChildren[(string)value];
        //                //CurrentlySelectedTreeLevel.SelectedChildNode = _selectedLevelSelectedChild;

        //                CurrentEquationCalc = null;
        //                OnPropertyChanged("CurrentEquationCalc");

        //                RefreshVisibles();

        //                RefreshSelectedTreeLevels(_selectedLevelSelectedChild);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Logging.LogException(ex);
        //            throw;
        //        }
        //    }
        //}

        //private void RefreshSelectedTreeLevels(EquationLibraryContentNode lastTnToKeep)
        //{
        //    try
        //    {
        //        int lastGoodItem = 0;
        //        for (int i = 0; i < EquationLibraryDisplayTree.Count; i++)
        //        {
        //            if (EquationLibraryDisplayTree[i]?.SelectedChildNode == lastTnToKeep)
        //            {
        //                lastGoodItem = i;
        //                break;
        //            }
        //        }

        //        int j = EquationLibraryDisplayTree.Count - 1;
        //        while (j > lastGoodItem)
        //        {
        //            EquationLibraryDisplayTree.RemoveAt(j);
        //            j--;
        //        }

        //        // Add the selection point at the end
        //        EquationLibraryDisplayNode lastSelectedTreeLevel = EquationLibraryDisplayTree[EquationLibraryDisplayTree.Count - 1];
        //        if (lastSelectedTreeLevel?.SelectedChildNode?.ChildTreeNodes?.Count > 0)
        //        {
        //            EquationLibraryDisplayNode newSelectedTreeLevel = new EquationLibraryDisplayNode(lastSelectedTreeLevel, lastSelectedTreeLevel.SelectedChildNode, lastSelectedTreeLevel.Indent + "  ");
        //            EquationLibraryDisplayTree.Add(newSelectedTreeLevel);
        //        }
        //        else
        //        {
        //            SelectEquation.Execute(null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.LogException(ex);
        //        throw;
        //    }
        //}


        /// <returns>Number of children</returns>
        public int ExpandTreeLevel(EquationLibraryDisplayNode displayNode)
        {
            try
            {
                if (CurrentDisplayNode!=null)
                {
                    CurrentDisplayNode.IsSelected = false;
                }

                EquationLibraryContentNode selectedContentNode = displayNode.ContentNode;
                selectedContentNode.Expanded = !selectedContentNode.Expanded;

                RefreshEquationLibraryDisplayTree();

                CurrentDisplayNode = FindDisplayNode(selectedContentNode);
                CurrentEquationCalc = null;
                OnPropertyChanged("CurrentEquationCalc");

                if (CurrentDisplayNode == null) throw new UnspecifiedException("CurrentDisplayNode should not be null");
                CurrentDisplayNode.IsSelected = true;

                return selectedContentNode.ChildTreeNodes.Count;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private EquationLibraryDisplayNode FindDisplayNode(EquationLibraryContentNode contentNode)
        {
            foreach (var item in EquationLibraryDisplayTree)
            {
                if (item.ContentNode== contentNode)
                {
                    return item;
                }
            }
            return null;
        }


        #endregion ------------ Fields and Properties ------------


        #region ------------ Nested Classes ------------

        public class EquationLibraryDisplayNode : ViewModelBase
        {
            public EquationLibraryDisplayNode(EquationLibraryDisplayNode parent, EquationLibraryContentNode contentNode, string indent)
            {
                Parent = parent;
                ContentNode = contentNode;
                Indent = indent;
            }

            public EquationLibraryDisplayNode Parent { get; set; }
            public EquationLibraryContentNode ContentNode { get; set; }

            public string Indent { get; set; }

            public string DescriptiveText
            {
                get
                {
                    string descriptiveText;
                    descriptiveText = ContentNode.Name;
                    return Indent + descriptiveText + $"  ({ContentNode.NumDescendantEquations})";
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }

        }

        public class EquationLibraryContentNode
        {
            public EquationLibraryContentNode(EquationLibraryContentNode parent, string name, SectionNode sectionNode)
            {
                Parent = parent;
                Name = name;
                SectionNode = sectionNode;

                parent?.ChildTreeNodes?.Add(this);
            }

            public EquationLibraryContentNode Parent { get; set; }
            public string Name { get; set; }
            public SectionNode SectionNode { get; set; }

            public IList<EquationLibraryContentNode> ChildTreeNodes { get; set; } = new List<EquationLibraryContentNode>();

            public int NumDescendantEquations { get; set; }

            public bool Expanded { get; set; }
        }


        public string GetNodePath(EquationLibraryContentNode tn, bool includeNode, bool excludeTop)
        {
            return GetNodePathRecursive(tn, excludeTop) + (includeNode ? $"{tn.Name}" : "");
        }
        private string GetNodePathRecursive(EquationLibraryContentNode tn, bool excludeTop)
        {
            if (tn.Parent != null)
            {
                if (!excludeTop || (tn.Parent.Parent != null))
                {
                    return GetNodePathRecursive(tn.Parent, excludeTop) + $"{tn.Parent.Name} > ";
                }
            }
            return "";
        }

        #endregion ------------ Nested Classes ------------

    }
}
