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
using AnyEquation.Equations.Common;
using AnyEquation.Equations.ViewModels;
using static AnyEquation.Equations.ViewModels.VmAddEquation;
using Syncfusion.SfDataGrid.XForms;

namespace AnyEquation.Equations.Views
{

#if DEBUG
    // Don't use XamlCompilation on UWP because it stops the debug variables appearing. See: https://bugzilla.xamarin.com/show_bug.cgi?id=52605
#else
    [XamlCompilation(XamlCompilationOptions.Compile)]
#endif
    public partial class VwAddEquation : AwaitableContentPage
    {
        private IEquationsUiService _equationsUiService;
        public IEquationsUiService EquationsUiService { get { return _equationsUiService; } }

        private VmAddEquation _vmAddEquation;
        public bool IsModal { get; set; }

        public VwAddEquation(IEquationsUiService equationsUiService, VmAddEquation vmAddEquation, bool isModal)
        {
            _equationsUiService = equationsUiService;
            _vmAddEquation = vmAddEquation;
            IsModal = isModal;
            InitializeComponent();
            BindingContext = vmAddEquation;
        }


        void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            _vmAddEquation.RefreshEquationCalc(args.NewTextValue);
        }

        void OnEntryCompleted(object sender, EventArgs args)
        {
        }

        private void SyntaxGrid_GridTapped(object sender, GridTappedEventArgs e)
        {
            try
            {
                var rowIndex = e.RowColumnIndex.RowIndex;
                var rowData = e.RowData;
                var columnIndex = e.RowColumnIndex.ColumnIndex;

                CSyntax syntax = rowData as CSyntax;

                if (rowIndex != 0)
                {
                    if (syntax != null)
                    {
                        txtEquation.Unfocus();
                        txtEquation.TextToInsert = syntax.TextToInsert;
                        txtEquation.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
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
            await EquationsUiService.ShowAddEquation_Step2(_vmAddEquation);

            if (_vmAddEquation.SelectionCancelled==false)       // Selection has finished
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
        }

        private void SyntaxGrid_GridLoaded(object sender, GridLoadedEventArgs e)
        {
            int iDbg = 0;
        }

        void EquationTapped(object sender, EventArgs args)
        {
            txtEquation.Focus();
        }
    }

}
