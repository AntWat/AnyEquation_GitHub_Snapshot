using System;
using Xamarin.Forms;
using AnyEquation.Common;
using AnyEquation.Equations.ViewModels;
using System.Globalization;
using static AnyEquation.Equations.ViewModels.VmChooseMultiUom;
using Syncfusion.SfDataGrid.XForms;
using Syncfusion.Data;
using System.Collections.ObjectModel;
using AnyEquation.Equations.Common;
using System.Collections.Generic;

namespace AnyEquation.Equations.Views
{

#if DEBUG
    // Don't use XamlCompilation on UWP because it stops the debug variables appearing. See: https://bugzilla.xamarin.com/show_bug.cgi?id=52605
#else
    [XamlCompilation(XamlCompilationOptions.Compile)]
#endif
    public partial class VwChooseDefaultUomPage : AwaitableContentPage
    {
        private VmChooseMultiUom _vmChooseMultiUom = null;
        public VmChooseMultiUom VmChooseMultiUom
        {
            get { return _vmChooseMultiUom; }
            set { _vmChooseMultiUom = value; }
        }

        public bool IsModal { get; set; }
        private bool _ignoreEvent;

        public VwChooseDefaultUomPage(VmChooseMultiUom vmChooseMultiUom, bool isModal)
        {
            _vmChooseMultiUom = vmChooseMultiUom;
            IsModal = isModal;
            InitializeComponent();
            BindingContext = vmChooseMultiUom;
        }

        // -------------------------

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            _vmChooseMultiUom.SelectionCancelled = true;

            if (IsModal)
            {
                await PopAwaitableModalAsync();
            }
            else
            {
                await PopAwaitableAsync();
            }
        }

        private async void OK_Clicked(object sender, EventArgs e)
        {
            _vmChooseMultiUom.SelectionCancelled = false;

            if (IsModal)
            {
                await PopAwaitableModalAsync();
            }
            else
            {
                await PopAwaitableAsync();
            }
        }

        private void ShowSelected_Clicked(object sender, EventArgs e)
        {
            _vmChooseMultiUom.ShowItemsAndDeselect("Currently selected default units", "De-select items if you want to remove them:", AdjustSelections);
        }

        private bool AdjustSelections(IDictionary<string, /*selected*/bool> itemsAndSelections)
        {
            bool oldIgnoreEvent = _ignoreEvent;
            try
            {
                _ignoreEvent = true;

                unitsGrid.SelectedItems.Clear();

                foreach (var row in _vmChooseMultiUom.RowInfoCollection)
                {
                    if (itemsAndSelections.ContainsKey(row.Uom.Name) && itemsAndSelections[row.Uom.Name])
                    {
                        unitsGrid.SelectedItems.Add(row);
                    }
                }

                // Record the the selected items
                ObservableCollection<object> selectedItems = unitsGrid.SelectedItems;
                _vmChooseMultiUom.UpdateSelectedItems(selectedItems);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
            finally
            {
                _ignoreEvent = oldIgnoreEvent;
            }

            return true;
        }

        private void UnitsGrid_GridTapped(object sender, GridTappedEventArgs e)
        {
            try
            {
                var rowIndex = e.RowColumnIndex.RowIndex;
                var rowData = e.RowData;
                var columnIndex = e.RowColumnIndex.ColumnIndex;

                if (rowIndex == 0)
                {
                    string colName = unitsGrid.Columns[columnIndex].MappingName;

                    unitsGrid.GroupColumnDescriptions.Clear();

                    if (colName.Equals("Unit"))
                    {
                        VmChooseMultiUom.ChooseMultiUomGrouping = enChooseMultiUomGrouping.None;
                    }
                    else if (colName.Equals("Group"))
                    {
                        VmChooseMultiUom.ChooseMultiUomGrouping = enChooseMultiUomGrouping.Group;
                    }
                    else if (colName.Equals("Parameter"))
                    {
                        VmChooseMultiUom.ChooseMultiUomGrouping = enChooseMultiUomGrouping.Parameter;
                    }

                    if (VmChooseMultiUom.ChooseMultiUomGrouping != enChooseMultiUomGrouping.None)
                    {
                        unitsGrid.GroupColumnDescriptions.Add(new GroupColumnDescription()
                        {
                            ColumnName = colName,
                            Converter = (ChooseMultiUomGroupConverter)this.Resources["groupConverter"],
                        });
                    }
                }
                else if (rowData is ChooseMultiUomRowInfo rowInfo)
                {
                    TidyUpSelections(rowInfo);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void TidyUpSelections(ChooseMultiUomRowInfo rowInfo)
        {
            try
            {
                // Remove a previous selection that clashes with this new one
                ChooseMultiUomRowInfo selectionToRemove = null;
                foreach (ChooseMultiUomRowInfo item in unitsGrid.SelectedItems)
                {
                    if (item.Uom.Equals(rowInfo.Uom) && item.Uom.Dimensions.Equals(rowInfo.Uom.Dimensions))
                    {
                        selectionToRemove = item;
                        break;      // Should only ever be one that matches, otherwise there was a clash already
                    }
                }
                if (selectionToRemove != null)
                {
                    unitsGrid.SelectedItems.Remove(selectionToRemove);
                }

                // Select all identical appearances
                IList<ChooseMultiUomRowInfo> selectionsToAdd = new List<ChooseMultiUomRowInfo>();

                foreach (ChooseMultiUomRowInfo row in _vmChooseMultiUom.RowInfoCollection)
                {
                    if (row!=rowInfo && row.Uom.Equals(rowInfo.Uom))
                    {
                        unitsGrid.SelectedItems.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private async void UnitsGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            bool oldIgnoreEvent = _ignoreEvent;
            try
            {
                if (_ignoreEvent) return;

                // Record the the selected items
                ObservableCollection<object> selectedItems = unitsGrid.SelectedItems;
                _vmChooseMultiUom.UpdateSelectedItems(selectedItems);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
            finally
            {
                _ignoreEvent = oldIgnoreEvent;
            }
        }

        private void UnitsGrid_GridLoaded(object sender, GridLoadedEventArgs e)
        {
            bool oldIgnoreEvent = _ignoreEvent;
            try
            {
                _ignoreEvent = true;

                unitsGrid.SelectedItems.Clear();

                IList<string> groupsToExpand = new List<string>();
                foreach (var item in _vmChooseMultiUom.GetSelectedItems())
                {
                    unitsGrid.SelectedItems.Add(item);
                    groupsToExpand.Add(item.Group);
                }

                foreach (Group group in unitsGrid.View.Groups)
                {
                    if ( (group.Source.Count>0) && (group.Source[0] is ChooseMultiUomRowInfo row) )
                    {
                        if (groupsToExpand.Contains(row.Group))
                        {
                            unitsGrid.ExpandGroup(group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
            finally
            {
                _ignoreEvent = oldIgnoreEvent;
            }
        }
    }

    public class ChooseMultiUomGroupConverter : IValueConverter
    {
        public ChooseMultiUomGroupConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var chooseMultiUomRowInfo = value as ChooseMultiUomRowInfo;
            if (chooseMultiUomRowInfo == null) return null;

            enChooseMultiUomGrouping chooseMultiUomGrouping = chooseMultiUomRowInfo.Parent.ChooseMultiUomGrouping;

            switch (chooseMultiUomGrouping)
            {
                case enChooseMultiUomGrouping.None:
                    return "";
                case enChooseMultiUomGrouping.Group:
                    return $"{chooseMultiUomRowInfo.ParamGroup}:               {chooseMultiUomRowInfo.Parameter}";
                case enChooseMultiUomGrouping.Parameter:
                    return $"{chooseMultiUomRowInfo.Parameter}";
                default:
                    break;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

}
