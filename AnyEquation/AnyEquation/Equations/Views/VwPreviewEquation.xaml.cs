using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using AnyEquation.Common;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.User_Controls;
using AnyEquation.Equations.ViewModels;
using Syncfusion.SfDataGrid.XForms;
using System.Globalization;
using static AnyEquation.Equations.ViewModels.VmPreviewEquation;

namespace AnyEquation.Equations.Views
{
    public partial class VwPreviewEquation : AwaitableContentPage
    {
        private VmPreviewEquation _vmPreviewEquation;
        private double avgCharWidth = 12;

        public VwPreviewEquation(VmPreviewEquation vmPreviewEquation)
        {
            try
            {
                // ----------------
                _vmPreviewEquation = vmPreviewEquation;

                this.BindingContext = vmPreviewEquation;

                vmPreviewEquation.MaxVarLengthChanged += OnMaxVarLengthChanged;
                vmPreviewEquation.CalculationFinished += OnCalculationFinished;

                // ----------------
                SetColWidths();

                // ----------------
                InitializeComponent();

                // ----------------
                // If we are using XAML compilation, then DisplayBinding must be set in code due to a bug. See: https://www.syncfusion.com/forums/130637/cannot-set-displaybinding-on-column-from-xaml
                //previewGrid.Columns[1].DisplayBinding = new Binding() { Path = "Value", Converter = new NumericConverter() };
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        // --------------------
        void OnMaxVarLengthChanged(object sender, EventArgs args)
        {
            SetColWidths();
        }

        void OnCalculationFinished(object sender, EventArgs args)
        {
            SetColWidths();
        }

        // --------------------
        private void SetColWidths()     // This is not currently needed but left in for any future problems
        {
            try
            {
                int numChars_Col0 = _vmPreviewEquation?.MaxVarLength ?? 0;
                int numChars_Col2 = _vmPreviewEquation?.MaxUOMLength ?? 0;

                if (numChars_Col0 < 1) numChars_Col0 = 2;
                if (numChars_Col2 < 1) numChars_Col2 = 4;

                SetResourceVal_GridLength("Equations_col0Width", numChars_Col0);
                SetResourceVal_GridLength("Equations_col2Width", (numChars_Col2 + 2));

                SetResourceVal_Dbl("UnitsBtn_Width", (numChars_Col2 + 2));

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        // --------------------
        private void SetResourceVal_GridLength(string resKey, int numChars)
        {
            try
            {
                GridLength gl = new GridLength((double)(numChars * avgCharWidth));
                if (Application.Current.Resources.ContainsKey(resKey))
                {
                    Application.Current.Resources[resKey] = gl;
                }
                else
                {
                    Application.Current.Resources.Add(resKey, gl);
                }

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void SetResourceVal_Dbl(string resKey, int numChars)
        {
            try
            {
                double val = (double)(numChars * avgCharWidth);
                if (Application.Current.Resources.ContainsKey(resKey))
                {
                    Application.Current.Resources[resKey] = val;
                }
                else
                {
                    Application.Current.Resources.Add(resKey, val);
                }

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        // Stop the listview item being selected
        private void variablesList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                // don't do anything if we just de-selected the row
                if (e.Item == null) return;
                // do something with e.SelectedItem
                ((ListView)sender).SelectedItem = null; // de-select the row

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        private void PreviewGrid_GridViewCreated(object sender, GridViewCreatedEventArgs e)
        {
            int iDbg = 0;
        }

        private void PreviewGrid_GridLoaded(object sender, GridLoadedEventArgs e)
        {
            //SfDataGrid grid = sender as SfDataGrid;
            //grid.SetBinding(SfDataGrid.ItemsSourceProperty, new Binding() { Source = _vmPreviewEquation.EqPreviewVariables });

            int iDbg = 0;
        }


        private void PreviewGrid_GridTapped(object sender, GridTappedEventArgs e)
        {
            try
            {
                var rowIndex = e.RowColumnIndex.RowIndex;
                var rowData = e.RowData;
                var columnIndex = e.RowColumnIndex.ColumnIndex;

                VmPreviewEqVar vmPreviewEqVar = rowData as VmPreviewEqVar;

                if (columnIndex == 1)
                {
                    if (vmPreviewEqVar == null || vmPreviewEqVar.IsCalculated)
                    {
                        return;
                    }

                    //previewGrid.EndEdit();     // Finish previous edit if there was one.
                    //previewGrid.BeginEdit(rowIndex, columnIndex);      // Start new one.
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }

        private void Ok_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                this.Navigation.PopModalAsync();
                _vmPreviewEquation.DoOkAction();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                this.Navigation.PopModalAsync();
                _vmPreviewEquation.DoCancelAction();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


    }

    // ----------------------

    public class PreviewGridcolorConverter : IValueConverter
    {
        /// <param name="value">A boolean, that determines if the row shows a calculated value</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFlagged = (bool)value;

            if (!isFlagged)
            {
                int iParam;
                if (int.TryParse((string)parameter, out iParam))
                {
                    if (iParam == 1)
                    {
                        return (Color)Application.Current.Resources[App.Key_BackgroundColor];
                    }
                    else
                    {
                        return (Color)Application.Current.Resources[App.Key_TextColor];
                    }
                }
            }
            else
            {
                int iParam2;
                if (int.TryParse((string)parameter, out iParam2))
                {
                    if (iParam2 == 1)
                    {
                        return (Color)Application.Current.Resources[App.Key_TextColor];
                    }
                    else
                    {
                        return (Color)Application.Current.Resources[App.Key_BackgroundColor];
                    }
                }
            }
            // Shouldn't get to here:
            return (Color)Application.Current.Resources[App.Key_BackgroundColor];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }   // class PreviewGridcolorConverter

}
