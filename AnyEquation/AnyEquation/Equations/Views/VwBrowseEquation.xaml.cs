using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static AnyEquation.Equations.ViewModels.VmBrowseEquation1;

namespace AnyEquation.Equations.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class VwBrowseEquation : ContentPage
	{
        private VmBrowseEquation1 _vmBrowseEquation;

        public VwBrowseEquation (VmBrowseEquation1 vmBrowseEquation)
		{
            _vmBrowseEquation = vmBrowseEquation;

            InitializeComponent ();

            this.BindingContext = _vmBrowseEquation;
        }

        private void lstTreeLevels_ItemTap(object sender, ItemTappedEventArgs e)
        {
            try
            {
                SelectedTreeLevel1 selectedTreeLevel = e.Item as SelectedTreeLevel1;
                ChangeSelectTreeLevel(selectedTreeLevel);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void ChangeSelectTreeLevel(SelectedTreeLevel1 selectedTreeLevel)
        {
            _vmBrowseEquation.SelectTreeLevel(selectedTreeLevel);
        }

        private void Level_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                SelectedTreeLevel1 selectedTreeLevel = btn.BindingContext as SelectedTreeLevel1;
                ChangeSelectTreeLevel(selectedTreeLevel);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        private void Ok_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                _vmBrowseEquation.AcceptCurrentEquationCalc();
                this.Navigation.PopAsync();
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

                this.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

    }
}