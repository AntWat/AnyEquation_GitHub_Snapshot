using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AnyEquation.Common;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.ViewModels;
using System.Globalization;
using static AnyEquation.Equations.ViewModels.VmChooseUom;
using Syncfusion.SfDataGrid.XForms;
using Syncfusion.Data;

namespace AnyEquation.Equations.Views
{

#if DEBUG
    // Don't use XamlCompilation on UWP because it stops the debug variables appearing. See: https://bugzilla.xamarin.com/show_bug.cgi?id=52605
#else
    [XamlCompilation(XamlCompilationOptions.Compile)]
#endif
    public partial class VwChooseUomPage : AwaitableContentPage
    {
        private VmChooseUom _vmChooseUom = null;
        public VmChooseUom VmChooseUom
        {
            get { return _vmChooseUom; }
            set { _vmChooseUom = value; }
        }

        public bool IsModal { get; set; }

        public VwChooseUomPage(VmChooseUom vmChooseUom, bool isModal)
        {
            _vmChooseUom = vmChooseUom;
            IsModal = isModal;
            InitializeComponent();
            BindingContext = vmChooseUom;
        }

        // -------------------------

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            _vmChooseUom.SelectionCancelled = true;

            if (IsModal)
            {
                await PopAwaitableModalAsync();
            }
            else
            {
                await PopAwaitableAsync();
            }
        }

        private void UnitsGrid_GridTapped(object sender, GridTappedEventArgs e)
        {
            try
            {
                var rowIndex = e.RowColumnIndex.RowIndex;
                var rowData = e.RowData;
                var columnIndex = e.RowColumnIndex.ColumnIndex;

                if (rowIndex==0)
                {
                    string colName = unitsGrid.Columns[columnIndex].MappingName;

                    unitsGrid.GroupColumnDescriptions.Clear();

                    if (colName.Equals("Unit"))
                    {
                        VmChooseUom.ChooseUomGrouping = enChooseUomGrouping.None;
                    }
                    else if (colName.Equals("Group"))
                    {
                        VmChooseUom.ChooseUomGrouping = enChooseUomGrouping.Group;
                    }
                    else if (colName.Equals("Parameter"))
                    {
                        VmChooseUom.ChooseUomGrouping = enChooseUomGrouping.Parameter;
                    }

                    if (VmChooseUom.ChooseUomGrouping != enChooseUomGrouping.None)
                    {
                        unitsGrid.GroupColumnDescriptions.Add(new GroupColumnDescription()
                        {
                            ColumnName = colName,
                            Converter = (ChooseUomGroupConverter)this.Resources["groupConverter"],
                        });
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void UnitsGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            // Gets the selected item 
            var selectedItem = e.AddedItems[0];

            ChooseUomRowInfo uomRowInfo = selectedItem as ChooseUomRowInfo;

            if (uomRowInfo!=null)
            {
                _vmChooseUom.SelectedUOM = uomRowInfo.Uom;

                if (IsModal)
                {
                    await PopAwaitableModalAsync();
                }
                else
                {
                    await PopAwaitableAsync();
                }
            }
        }

        private void UnitsGrid_GridLoaded(object sender, GridLoadedEventArgs e)
        {
            // Expand special items
            foreach (Group item in unitsGrid.View.Groups)
            {
                string gKey = item.Key.ToString();
                if ( (gKey.IndexOf("recent", StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (gKey.IndexOf("Unspecified", StringComparison.OrdinalIgnoreCase) >= 0) )
                {
                    unitsGrid.ExpandGroup(item);
                }
            }

            //Expand the last group if there is only one Param
            if (_vmChooseUom.ParamTypes!=null && _vmChooseUom.ParamTypes.Count==1)
            {
                Group group = (unitsGrid.View.Groups[unitsGrid.View.Groups.Count-1] as Group);
                unitsGrid.ExpandGroup(group);
            }
        }

    }


    public class ChooseUomGroupConverter : IValueConverter
    {
        public ChooseUomGroupConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var chooseUomRowInfo = value as ChooseUomRowInfo;
            if (chooseUomRowInfo == null) return null;

            enChooseUomGrouping chooseUomGrouping = chooseUomRowInfo.Parent.ChooseUomGrouping;

            switch (chooseUomGrouping)
            {
                case enChooseUomGrouping.None:
                    return "";
                case enChooseUomGrouping.Group:
                    return $"{chooseUomRowInfo.ParamGroup}:               {chooseUomRowInfo.Parameter}";
                case enChooseUomGrouping.Parameter:
                    return $"{chooseUomRowInfo.Parameter}";
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
