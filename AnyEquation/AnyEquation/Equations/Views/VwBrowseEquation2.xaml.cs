using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static AnyEquation.Equations.ViewModels.VmBrowseEquation2;

namespace AnyEquation.Equations.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VwBrowseEquation2 : ContentPage
    {
        private VmBrowseEquation2 _vmBrowseEquation2;

        public VwBrowseEquation2(VmBrowseEquation2 vmBrowseEquation2)
        {
            _vmBrowseEquation2 = vmBrowseEquation2;

            InitializeComponent();

            this.BindingContext = _vmBrowseEquation2;
        }

        private void lstTreeLevels_ItemTap(object sender, ItemTappedEventArgs e)
        {
            try
            {
                EquationLibraryDisplayNode displayNode = e.Item as EquationLibraryDisplayNode;
                int NumChildren = _vmBrowseEquation2.ExpandTreeLevel(displayNode);

                if (NumChildren==0)     // Show the list of equations for this node
                {
                    _vmBrowseEquation2.SelectEquation(() => this.Navigation.PopAsync());
                }

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void SelectEquation_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                _vmBrowseEquation2.SelectEquation(()=>this.Navigation.PopAsync());
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

    // ----------------------

    public class ListItemColorConverter : IValueConverter
    {
        /// <param name="value">A boolean, that determines if the row shows a calculated value</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = (bool)value;

            if (isSelected)
            {
                int iParam;
                if (int.TryParse((string)parameter, out iParam))
                {
                    if (iParam == 1)    // Foreground
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
                    if (iParam2 == 1)    // Foreground
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