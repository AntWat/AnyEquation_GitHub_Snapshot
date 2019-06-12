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
    public partial class VwSelectStringsFromGroupedList : AwaitableContentPage
    {
        public bool IsModal { get; set; }
        private bool _ignoreEvent;
        public bool MultiSelect { get; }
        private IEquationsUiService EquationsUiService { get; }
        private IDictionary<string, /*selected*/bool> _groupExpansions;

        public ObservableCollection<GroupedString> GroupedStrings { get; } = new ObservableCollection<GroupedString>();
        private Func<IList<GroupedString>, bool /*Result: not used*/> OkCallback { get; }

        public VwSelectStringsFromGroupedList(IEquationsUiService equationsUiService, IDictionary<string, /*selected*/bool> groupExpansions, IList<GroupedString> groupedStrings, 
                                                bool multiSelect, string title, string subTitle,
                                                Func<IList<GroupedString>, bool /*Result: not used*/> okCallback,
                                                bool isModal)
        {
            MultiSelect = multiSelect;
            EquationsUiService = equationsUiService;
            _groupExpansions = groupExpansions;

            foreach (var item in groupedStrings)
            {
                GroupedStrings.Add(item);
            }

            OkCallback = okCallback;

            IsModal = isModal;
            InitializeComponent();
            BindingContext = this;
        }

        // -------------------------

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
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
            OkClose();
        }

        private async void OkClose()
        {
            OkCallback(GroupedStrings);

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
            ShowItemsAndDeselect();
        }

        private void ShowItemsAndDeselect()
        {
            try
            {
                IDictionary<string, /*selected*/bool> itemsAndSelections = new Dictionary<string, /*selected*/bool>();

                foreach (var item in GroupedStrings)
                {
                    if (item.IsSelected)
                    {
                        itemsAndSelections.Add(item.Item, item.IsSelected);
                    }
                }

                EquationsUiService.SelectStringsFromList(itemsAndSelections, "Currently selected items", "Deselect items to remove them", AdjustSelections);

                return;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private bool AdjustSelections(IDictionary<string, /*selected*/bool> itemsAndSelections)
        {
            bool oldIgnoreEvent = _ignoreEvent;
            try
            {
                _ignoreEvent = true;

                stringsGrid.SelectedItems.Clear();
                foreach (var item in GroupedStrings)
                {
                    if (itemsAndSelections.ContainsKey(item.Item))
                    {
                        item.IsSelected = itemsAndSelections[item.Item];
                        if (itemsAndSelections[item.Item])
                        {
                            stringsGrid.SelectedItems.Add(item);
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

            return true;
        }

        private void StringsGrid_GridTapped(object sender, GridTappedEventArgs e)
        {
            try
            {
                var rowIndex = e.RowColumnIndex.RowIndex;
                var rowData = e.RowData;
                var columnIndex = e.RowColumnIndex.ColumnIndex;

                if (rowIndex == 0)
                {
                    string colName = stringsGrid.Columns[columnIndex].MappingName;

                    stringsGrid.GroupColumnDescriptions.Clear();

                    if (colName.Equals("Group"))
                    {
                        SelectStringsGroupConverter._selectStringsGrouping = enSelectStringsGrouping.Group;
                    }
                    else if (colName.Equals("Parameter"))
                    {
                        SelectStringsGroupConverter._selectStringsGrouping = enSelectStringsGrouping.None;
                    }

                    if (SelectStringsGroupConverter._selectStringsGrouping != enSelectStringsGrouping.None)
                    {
                        stringsGrid.GroupColumnDescriptions.Add(new GroupColumnDescription()
                        {
                            ColumnName = colName,
                            Converter = (ChooseMultiUomGroupConverter)this.Resources["groupConverter"],
                        });
                    }
                }
                else if (rowData is GroupedString rowInfo)
                {
                    //TidyUpSelections(rowInfo);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        private async void StringsGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            bool oldIgnoreEvent = _ignoreEvent;
            try
            {
                if (_ignoreEvent) return;

                // Record the the selected items
                foreach (var item in GroupedStrings)
                {
                    if (stringsGrid.SelectedItems.Contains(item))
                    {
                        item.IsSelected = true;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }

                if (!MultiSelect)
                {
                    OkClose();
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

        private void StringsGrid_GridLoaded(object sender, GridLoadedEventArgs e)
        {
            bool oldIgnoreEvent = _ignoreEvent;
            try
            {
                _ignoreEvent = true;

                // Define grid selections
                stringsGrid.SelectedItems.Clear();
                foreach (var item in GroupedStrings)
                {
                    if (item.IsSelected)
                    {
                        stringsGrid.SelectedItems.Add(item);
                    }
                }

                // Expand groups if required
                foreach (Group group in stringsGrid.View.Groups)
                {
                    if ((group.Source.Count > 0) && (group.Source[0] is GroupedString row))
                    {
                        if (_groupExpansions.ContainsKey(row.Group) && _groupExpansions[row.Group])
                        {
                            stringsGrid.ExpandGroup(group);
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

    public enum enSelectStringsGrouping
    {
        None, Group
    }

    public class SelectStringsGroupConverter : IValueConverter
    {
        public static enSelectStringsGrouping _selectStringsGrouping = enSelectStringsGrouping.Group; //TODO: Static?

        public SelectStringsGroupConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var groupedString = value as GroupedString;
            if (groupedString == null) return null;

            switch (_selectStringsGrouping)
            {
                case enSelectStringsGrouping.None:
                    return "";
                case enSelectStringsGrouping.Group:
                    //return $"{groupedString.ParamGroup}:               {groupedString.Parameter}";
                    return $"{groupedString.GroupDescription}";
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
