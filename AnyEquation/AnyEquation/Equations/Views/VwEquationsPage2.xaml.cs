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
using static AnyEquation.Equations.ViewModels.VmEquations;

namespace AnyEquation.Equations.Views
{
    public partial class VwEquationsPage2 : ContentPage
    {
        private VmEquations _vmEquations;
        private double avgCharWidth = 12;

        public VwEquationsPage2(VmEquations vmEquations)
        {
            try
            {
                // ----------------
                _vmEquations = vmEquations;

                this.BindingContext = vmEquations;

                vmEquations.MaxVarLengthChanged += OnMaxVarLengthChanged;
                vmEquations.CalculationFinished += OnCalculationFinished;

                // ----------------
                SetColWidths();

                // ----------------
                InitializeComponent();

                // ----------------
                // If we are using XAML compilation, then DisplayBinding must be set in code due to a bug. See: https://www.syncfusion.com/forums/130637/cannot-set-displaybinding-on-column-from-xaml
                calcGrid.Columns[1].DisplayBinding = new Binding() { Path = "Value", Converter = new NumericConverter() };

                // ----------------
                if (Device.OS == TargetPlatform.Windows)
                {
                    calcGrid.Columns[2].Width = 200;        // Auto-sizing doen't seem to work for uwp
                }
                else
                {
                    calcGrid.Columns[2].ColumnSizer = ColumnSizer.Auto;
                }

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
                int numChars_Col0 = _vmEquations?.MaxVarLength ?? 0;
                int numChars_Col2 = _vmEquations?.MaxUOMLength ?? 0;

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

        private async void btnMenu_Clicked(object sender, EventArgs e)
        {
            try
            {
                // --------------- Refresh the calc, in case any of the choices depend on the result
                RefreshCalc();

                // --------------- Show the menu
                const string sCancel = "Cancel";
                const string sChangeDependantVariable = "Rearrange / Solve For...";
                const string sChooseEquation = "Choose Equation";
                const string sAddEquation = "Add Equation";
                const string sShowCalculationTree = "Show Calculation Tree";
                const string sDefaultUnits = "Default Units";
                const string sSettings = "Settings and About";

                var action = await DisplayActionSheet("Options:", sCancel, null, sChangeDependantVariable, sChooseEquation, sAddEquation, sShowCalculationTree, sDefaultUnits, sSettings);

                if (action==null)
                {
                    // Nothing to do
                }
                else if (action.Equals(sChangeDependantVariable))
                {
                    _vmEquations.ChangeDependantVariable(
                        (s)=>{ DisplayAlert("Failure in 'Rearrange / Solve For...'", s, "Ok"); return true; } );
                }
                else if (action.Equals(sChooseEquation))
                {
                    _vmEquations.ChooseEquation.Execute(null);
                }
                else if (action.Equals(sShowCalculationTree))
                {
                    _vmEquations.ShowCalculationTree.Execute(null);
                }
                else if (action.Equals(sAddEquation))
                {
                    _vmEquations.AddEqn.Execute(null);
                }
                else if (action.Equals(sDefaultUnits))
                {
                    _vmEquations.ChooseDefaultUnits.Execute(null);
                }
                else if (action.Equals(sSettings))
                {
                    _vmEquations.ShowSettings.Execute(null);
                }

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        private void CalcGrid_GridViewCreated(object sender, GridViewCreatedEventArgs e)
        {
            //(sender as SfDataGrid).GridStyle = new CustomGridStyle();
            int iDbg = 0;
        }

        private void CalcGrid_GridLoaded(object sender, GridLoadedEventArgs e)
        {
            int iDbg = 0;
        }

        private void CalcGrid_CurrentCellBeginEdit(object sender, GridCurrentCellBeginEditEventArgs e)
        {
            var rowIndex = e.RowColumnIndex.RowIndex;
            var columnIndex = e.RowColumnIndex.ColumnIndex;

            if (e.RowColumnIndex.ColumnIndex == 1)       //Value column: 
            {
                VmEqVar vmEqCalcVariable = calcGrid.GetRecordAtRowIndex(rowIndex) as VmEqVar;
                if (vmEqCalcVariable == null || vmEqCalcVariable.IsCalculated)
                {
                    e.Cancel=true;
                    return;
                }

                _vmEquations.ClearResults();     // Clear the current result in case it doesn't get refreshed properly after the edit
            }
        }

        private void CalcGrid_CurrentCellEndEdit(object sender, GridCurrentCellEndEditEventArgs e)
        {
            if (e.RowColumnIndex.ColumnIndex==1)
            {
                _vmEquations.UpdateVariableValue(e.RowColumnIndex.RowIndex-1, e.NewValue);
            }
        }

        private void btnRefresh_Clicked(object sender, EventArgs e)
        {
            RefreshCalc();
        }

        private void RefreshCalc()
        {
            calcGrid.EndEdit();
            _vmEquations.Recalculate();
        }

        private void CalcGrid_GridTapped(object sender, GridTappedEventArgs e)
        {
            try
            {
                var rowIndex = e.RowColumnIndex.RowIndex;
                var rowData = e.RowData;
                var columnIndex = e.RowColumnIndex.ColumnIndex;

                VmEqVar vmEqCalcVariable = rowData as VmEqVar;

                if (columnIndex == 1)
                {
                    if (vmEqCalcVariable==null || vmEqCalcVariable.IsCalculated)
                    {
                        return;
                    }

                    calcGrid.EndEdit();     // Finish previous edit if there was one.
                    calcGrid.BeginEdit(rowIndex, columnIndex);      // Start new one.
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }


    }

    // ----------------------

    public class CalcGridcolorConverter : IValueConverter
    {
        /// <param name="value">A boolean, that determines if the row shows a calculated value</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCalculated = (bool)value;
            
            if (!isCalculated)
            {
                int iParam;
                if (int.TryParse((string)parameter, out iParam))
                {
                    if (iParam==1)
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
    }   // class CalcGridcolorConverter

}
