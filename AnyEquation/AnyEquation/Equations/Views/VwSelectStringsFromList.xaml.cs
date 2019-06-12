using AnyEquation.Common;
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
    public partial class VwSelectStringsFromList : AwaitableContentPage
    {
        public VwSelectStringsFromList(IDictionary<string, /*selected*/bool> itemsAndSelections, string title, string subTitle, Func<IDictionary<string, /*selected*/bool>, bool /*Result: not used*/> okCallback)
        {
            InitializeComponent();

            DisplayTitle = title;
            DisplaySubTitle = subTitle;
            OkCallback = okCallback;

            foreach (var item in itemsAndSelections)
            {
                ItemsToShow.Add( new DisplayItem()
                {
                    TextToDisplay = item.Key,
                    IsSelected = item.Value,
                });
            }

            this.BindingContext = this;
        }

        public string DisplayTitle { get; set; }
        public string DisplaySubTitle { get; set; }

        public bool SelectionCancelled { get; private set; }
        Func<IDictionary<string, /*selected*/bool>, bool /*Result: not used*/> OkCallback;

        public ObservableCollection<DisplayItem> ItemsToShow { get; set; } = new ObservableCollection<DisplayItem>();

        public bool IsModal { get; } = true;

        // -------------------------------------
        public DisplayItem NullSelectedItem
        {
            get { return null;  /* We never want an item selected */ }
            set
            {
                OnPropertyChanged("NullSelectedItem");
            }
        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            SelectionCancelled = true;

            if (IsModal)
            {
                await PopAwaitableModalAsync();
            }
            else
            {
                await PopAwaitableAsync();
            }
        }

        private async void OK_Clicked(object sender, EventArgs e)
        {
            SelectionCancelled = false;
            if (OkCallback!=null)
            {
                IDictionary<string, /*selected*/bool> itemsAndSelections = new Dictionary<string, /*selected*/bool>();
                foreach (var item in ItemsToShow)
                {
                    itemsAndSelections.Add(item.TextToDisplay, item.IsSelected);
                }
                OkCallback(itemsAndSelections);
            }

            if (IsModal)
            {
                await PopAwaitableModalAsync();
            }
            else
            {
                await PopAwaitableAsync();
            }
        }

        public class DisplayItem
        {
            public bool IsSelected { get; set; }
            public string TextToDisplay { get; set; }
        }

    }
}