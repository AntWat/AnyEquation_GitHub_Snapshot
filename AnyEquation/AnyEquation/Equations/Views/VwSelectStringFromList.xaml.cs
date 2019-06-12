using AnyEquation.Equations.Common;
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
    public partial class VwSelectStringFromList : ContentPage
    {
        public VwSelectStringFromList(IList<string> itemsToShow, string title, string subTitle, Func<string /*selection*/, bool /*not used*/> selectionCallback)
        {
            InitializeComponent();

            DisplayTitle = title;
            DisplaySubTitle = subTitle;
            SelectionCallback = selectionCallback;
            foreach (var item in itemsToShow)
            {
                ItemsToShow.Add(item);
            }

            this.BindingContext = this;
        }

        public string DisplayTitle { get; set; }
        public string DisplaySubTitle { get; set; }

        Func<string /*selection*/, bool /*not used*/> SelectionCallback;

        public ObservableCollection<string> ItemsToShow { get; set; } = new ObservableCollection<string>();

        public string SelectedItem {
            set
            {
                try
                {
                    string selection = (string)value;

                    if (selection!=null)
                    {
                        this.Navigation.PopModalAsync();
                        SelectionCallback(selection);
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }
        }

    }
}