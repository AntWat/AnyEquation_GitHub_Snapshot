using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnyEquation.Equations.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VwLaunchPage : ContentPage
    {
        public VwLaunchPage()
        {
            InitializeComponent();

            this.BindingContext = this;
        }

        private bool _justLoaded=false;
        public bool JustLoaded
        {
            get => _justLoaded;
            set
            {
                if (_justLoaded!=value)
                {
                    _justLoaded = value;
                    OnPropertyChanged("JustLoaded");
                }
            }
        }

        protected override void OnAppearing()
        {
            JustLoaded = true;
            OnPropertyChanged("JustLoaded");
            base.OnAppearing();
        }

        private async void Switch_OnJustLoaded(object sender, ToggledEventArgs e)
        {
            if (JustLoaded)
            {
                JustLoaded = false;

                StartEquations();
            }
        }

        private async void StartEquations()
        {
            // Show the main equation page
            await EquationsUiService.StartEquations(Navigation);
        }


    }
}