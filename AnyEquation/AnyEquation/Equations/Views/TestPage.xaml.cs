using AnyEquation.Common;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.FormsBook.Toolkit;

namespace AnyEquation.Equations.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        private VmEquations _vmEquations;

        public TestPage(VmEquations vmEquations)
        {
            try
            {
                InitializeComponent();

                InitData();

                _vmEquations = vmEquations;

                this.BindingContext = this;
                //this.BindingContext = vmEquations;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }

        }

        private void InitData()
        {
            try
            {
                TreeNode topNode = new TreeNode()
                {
                    Name = $"Top",
                    Parent = null,
                };

                IList<TreeNode> topNodes = InitTree(topNode, "", 1, 5);

                SelectedTreeLevels.Clear();

                SelectedTreeLevels.Add(new SelectedTreeLevel(null, topNode));
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        private IList<TreeNode> InitTree(TreeNode parent, string ParentPostfix, int thisLevel, int maxLevels)
        {
            if (thisLevel == maxLevels) return null;

            IList<TreeNode> treeNodes = new List<TreeNode>();

            for (int i = 0; i < 10; i++)
            {
                string nextPostfix = $"{ParentPostfix}_{i+1}";

                TreeNode treeNode = new TreeNode()
                {
                    Name = $"Item{nextPostfix}",
                    Parent = parent,
                };
                treeNodes.Add(treeNode);
                parent?.ChildTreeNodes?.Add(treeNode);

                InitTree(treeNode, nextPostfix, thisLevel + 1, maxLevels);
            }

            return treeNodes;
        }

        //private void InitSubLevels(SelectedTreeLevel parent, string ParentPostfix, int thisLevel, int maxLevels)
        //{
        //    if (thisLevel == maxLevels) return;

        //    for (int i = 0; i < 20; i++)
        //    {
        //        string nextPostfix = $"{ParentPostfix}_{i}";

        //        TreeNode treeNode = new TreeNode()
        //        {
        //            Name = $"Item{nextPostfix}",
        //            Parent = parent?.TreeNode,
        //        };

        //        SelectedTreeLevel selectedTreeLevel = new SelectedTreeLevel()
        //        {
        //            Parent = parent,
        //            TreeNode = treeNode,
        //        };

        //        parent?.ChildTreeLevels?.Add(selectedTreeLevel);

        //        InitSubLevels(selectedTreeLevel, nextPostfix, thisLevel + 1, maxLevels);
        //    }
        //}


        private ObservableCollection<SelectedTreeLevel> _selectedTreeLevels = new ObservableCollection<SelectedTreeLevel>();
        public ObservableCollection<SelectedTreeLevel> SelectedTreeLevels { get { return _selectedTreeLevels; } set { _selectedTreeLevels = value; } }

        private SelectedTreeLevel CurrentlySelectedTreeLevel { get; set; }

        private IDictionary<string, TreeNode> _selectedLevelChildren = new Dictionary<string, TreeNode>();
        public ObservableCollection<string> SelectedLevelChildNames { get; set; } = new ObservableCollection<string>();

        private TreeNode _selectedLevelSelectedChild = null;


        private ObservableCollection<EqnCalc> _equationCalcs = new ObservableCollection<EqnCalc>();
        public ObservableCollection<EqnCalc> EquationCalcs { get
            {
                // TODO: Very inefficinet, just for prototyping
                _equationCalcs.Clear();
                foreach (var item in _vmEquations.EquationLibraries[0].GetAllEqnCalcs())
                {
                    _equationCalcs.Add(item);
                }

                return _equationCalcs;
            } }

        public EqnCalc CurrentEquationCalc { get; private set; }


        public string SelectedLevelSelectedChildName {
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
                    else if ( (_selectedLevelSelectedChild == null) || (!_selectedLevelSelectedChild.Name.Equals(value)) )
                    {
                        _selectedLevelSelectedChild = _selectedLevelChildren[(string)value];
                        CurrentlySelectedTreeLevel.SelectedChildNode = _selectedLevelSelectedChild;

                        CurrentEquationCalc = null;
                        OnPropertyChanged("CurrentEquationCalc");

                        //picker.IsOpen = false;
                        lstLevels.IsVisible = false;
                        lstEquations.IsVisible = false;
                        lstTreeLevels.IsVisible = true;
                        equationDisplay.IsVisible = lstTreeLevels.IsVisible;

                        RefreshSelectedTreeLevels();
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }
        }

        private void RefreshSelectedTreeLevels()
        {
            try
            {
                int lastGoodItem = 0;
                TreeNode selectedChildNode = SelectedTreeLevels[0].SelectedChildNode;
                for (int i = 1; i < SelectedTreeLevels.Count; i++)
                {
                    if ((selectedChildNode == null) || (SelectedTreeLevels[i].TreeNode!= selectedChildNode))
                    {
                        break;      // This selection, and all those below is no longer consistent with the selections above
                    }
                    lastGoodItem = i;
                    selectedChildNode = SelectedTreeLevels[i].SelectedChildNode;
                }

                int j = SelectedTreeLevels.Count - 1;
                while (j > lastGoodItem)
                {
                    SelectedTreeLevels.RemoveAt(j);
                    j--;
                }

                // Add the selection point at the end
                SelectedTreeLevel lastSelectedTreeLevel = SelectedTreeLevels[SelectedTreeLevels.Count - 1];
                SelectedTreeLevels.Add(new SelectedTreeLevel(lastSelectedTreeLevel, lastSelectedTreeLevel.SelectedChildNode));
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void Item_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                SelectedTreeLevel selectedTreeLevel = btn?.BindingContext as SelectedTreeLevel;
                SelectTreeLevel(selectedTreeLevel);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        private void SelectTreeLevel(SelectedTreeLevel selectedTreeLevel)
        {
            try
            {
                CurrentlySelectedTreeLevel = selectedTreeLevel;
                CurrentEquationCalc = null;
                OnPropertyChanged("CurrentEquationCalc");

                if (CurrentlySelectedTreeLevel == null) throw new UnspecifiedException("CurrentlySelectedTreeLevel should not be null");

                SelectedLevelChildNames.Clear();
                _selectedLevelChildren.Clear();
                _selectedLevelSelectedChild = null;

                foreach (var node in CurrentlySelectedTreeLevel?.TreeNode?.ChildTreeNodes)
                {
                    SelectedLevelChildNames.Add(node.Name);
                    _selectedLevelChildren.Add(node.Name, node);
                }

                //picker.IsOpen = true;
                lstTreeLevels.IsVisible = false;
                equationDisplay.IsVisible = lstTreeLevels.IsVisible;

                if (SelectedLevelChildNames.Count == 0)
                {
                    lstLevels.IsVisible = false;
                    lstEquations.IsVisible = true;
                }
                else
                {
                    lstLevels.IsVisible = true;
                    lstEquations.IsVisible = false;
                }

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        // ------------------------------
        private void lstEquations_ItemTap(object sender, ItemTappedEventArgs e)
        {
            CurrentEquationCalc = e.Item as EqnCalc;
            OnPropertyChanged("CurrentEquationCalc");

            lstLevels.IsVisible = false;
            lstEquations.IsVisible = false;
            lstTreeLevels.IsVisible = true;
            equationDisplay.IsVisible = lstTreeLevels.IsVisible;
        }

        private void lstTreeLevels_ItemTap(object sender, ItemTappedEventArgs e)
        {
            try
            {
                SelectedTreeLevel selectedTreeLevel = e.Item as SelectedTreeLevel;
                SelectTreeLevel(selectedTreeLevel);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        public class SelectedTreeLevel : ViewModelBase
        {
            public SelectedTreeLevel(SelectedTreeLevel parent, TreeNode treeNode)
            {
                Parent = parent;
                TreeNode = treeNode;

                //foreach (var item in treeNode.ChildTreeNodes)
                //{
                //    ChildNames.Add(item.Name);
                //    junkChildNames.Add(item.Name);
                //}
            }

            public SelectedTreeLevel Parent { get; set; }
            public TreeNode TreeNode { get; set; }

            private TreeNode _selectedChildNode;
            public TreeNode SelectedChildNode
            {
                get { return _selectedChildNode; }
                set
                {
                    _selectedChildNode = value;
                    OnPropertyChanged("DescriptiveText");
                }
            }

            //public ObservableCollection<string> ChildNames { get; set; } = new ObservableCollection<string>();

            public string DescriptiveText { get
            {
                if (SelectedChildNode == null)
                {
                    if (TreeNode.ChildTreeNodes.Count == 0)
                    {
                        return "Select an Equation...";
                    }
                    else if (Parent == null)
                    {
                        return "Select a Library...";
                    }
                    else if (Parent.Parent == null)
                    {
                        return "Select a Section...";
                    }
                    return "Select a Sub-section...";
                }
                else
                {
                    return SelectedChildNode.Name;
                }
            } }

        }

        public class TreeNode
        {
            public TreeNode Parent { get; set; }
            public  string Name { get; set; }

            public IList<TreeNode> ChildTreeNodes { get; set; } = new List<TreeNode>();

        }

    }
}