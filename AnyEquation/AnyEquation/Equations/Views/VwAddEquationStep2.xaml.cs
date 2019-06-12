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
using AnyEquation.Equations.ViewModels;
using static AnyEquation.Equations.ViewModels.VmAddEquation;

namespace AnyEquation.Equations.Views
{

#if DEBUG
    // Don't use XamlCompilation on UWP because it stops the debug variables appearing. See: https://bugzilla.xamarin.com/show_bug.cgi?id=52605
#else
    [XamlCompilation(XamlCompilationOptions.Compile)]
#endif
    public partial class VwAddEquationStep2 : AwaitableContentPage
    {
        private VmAddEquation _vmAddEquation;
        public bool IsModal { get; set; }
        private double avgCharWidth = 12;

        public VwAddEquationStep2(VmAddEquation vmAddEquation, bool isModal)
        {
            _vmAddEquation = vmAddEquation;
            IsModal = isModal;
            BindingContext = vmAddEquation;

            vmAddEquation.MaxVarLengthChanged += OnMaxVarLengthChanged;

            SetColWidths();

            InitializeComponent();
        }

        // --------------------
        void OnMaxVarLengthChanged(object sender, EventArgs args)
        {
            SetColWidths();
        }

        // --------------------
        private void SetColWidths()
        {
            try
            {
                int numChars_Col0 = _vmAddEquation?.MaxVarLength ?? 0;
                int numChars_Col1 = _vmAddEquation?.MaxParamLength ?? 0;

                if (numChars_Col0 < 1) numChars_Col0 = 2;
                if (numChars_Col1 < 1) numChars_Col1 = 7;

                SetResourceVal_GridLength("Variables_col0Width", numChars_Col0);
                SetResourceVal_GridLength("Variables_col1Width", (numChars_Col1 + 2));

                SetResourceVal_Dbl("ParamBtn_Width", (numChars_Col1 + 2));

            }
            catch (Exception ex)
            {
                //throw;
            }
        }

        // --------------------
        private void SetResourceVal_GridLength(string resKey, int numChars)
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

        private void SetResourceVal_Dbl(string resKey, int numChars)
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

        // Stop the listview item being selected
        private void variablesList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            // don't do anything if we just de-selected the row
            if (e.Item == null) return;
            // do something with e.SelectedItem
            ((ListView)sender).SelectedItem = null; // de-select the row
        }

        private async void btnBack_Clicked(object sender, EventArgs e)
        {
            _vmAddEquation.SelectionCancelled = true;
            if (IsModal)
            {
                await PopAwaitableModalAsync();
            }
            else
            {
                await PopAwaitableAsync();
            }
        }
        private async void btnNext_Clicked(object sender, EventArgs e)
        {
            variablesGrid.EndEdit();

            if (!_vmAddEquation.OkToFinish)
            {
                await DisplayAlert("Errors exist", "You must correct any errors before you can continue.", "Ok");
                return;
            }

            _vmAddEquation.SelectionCancelled = false;
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

}
