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
using AnyEquation.Equations.ViewModels;
using static AnyEquation.Equations.ViewModels.VmCalculationTree;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Views
{

#if DEBUG
    // Don't use XamlCompilation on UWP because it stops the debug variables appearing. See: https://bugzilla.xamarin.com/show_bug.cgi?id=52605
#else
    [XamlCompilation(XamlCompilationOptions.Compile)]
#endif
    public partial class VwCalculationTree : ContentPage
    {
        VmCalculationTree _vmCalculationTree;
        IEquationsUiService _equationsUiService;

        public VwCalculationTree(VmCalculationTree vmCalculationTree, IEquationsUiService equationsUiService)
        {
            _vmCalculationTree = vmCalculationTree;
            _equationsUiService = equationsUiService;

            InitializeComponent();
            this.BindingContext = _vmCalculationTree;
        }


        private void lstTreeLevels_ItemTap(object sender, ItemTappedEventArgs e)
        {
            try
            {
                CalcTreeRowInfo calcTreeRowInfo = e.Item as CalcTreeRowInfo;
                _equationsUiService.ShowMathExpression(calcTreeRowInfo.MathExpression);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

    }

}
