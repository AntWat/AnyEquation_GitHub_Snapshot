using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AnyEquation.Common;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.ViewModels;
using AnyEquation.Equations.Views;

namespace AnyEquation.Equations.Views
{

#if DEBUG
    // Don't use XamlCompilation on UWP because it stops the debug variables appearing. See: https://bugzilla.xamarin.com/show_bug.cgi?id=52605
#else
    [XamlCompilation(XamlCompilationOptions.Compile)]
#endif
    public partial class VwSettings : ContentPage
    {
        #region ------------ Statics ------------

        #endregion ------------ Statics ------------

        #region ------------ Constructors and Life Cycle ------------

        public VwSettings()
        {
            InitializeComponent();

            this.BindingContext = this;

            InitLinks();
        }
        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Fields and Properties ------------

        private ObservableCollection<IGrouping<string, CPagelInk>> _groupedLinks = new ObservableCollection<IGrouping<string, CPagelInk>>();
        public ObservableCollection<IGrouping<string, CPagelInk>> GroupedLinks { get { return _groupedLinks; } set { _groupedLinks = value; } }

        #endregion ------------ Fields and Properties ------------


        #region ------------ Event handlers that are specific to this GUI implementation, so are not implemented as ICommand objects  ------------

        // ------------------------

        // Note: Based on the Xanarin help description below, I decided to use ItemTapped not ItemSelected event...
        //       ...ListView supports selection of one item at a time. Selection is on by default. When a user taps an item, two events are fired: ItemTapped and ItemSelected. Note that tapping the same item twice will not fire multiple ItemSelected events, but will fire multiple ItemTapped events. Also note that ItemSelected will be called if an item is deselected.

        private async void lstLinks_ItemTap(object sender, ItemTappedEventArgs e)
        {
            try
            {
                CPagelInk pl = e.Item as CPagelInk;

                if (pl == null)
                {
                    return;
                }

                ((ListView)sender).SelectedItem = null; // de-select the row

                await InvokePageLink(pl);

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private async Task<bool> InvokePageLink(CPagelInk pl)
        {
            Utils.IncrementAppIsBusy();
            await pl.InvokeMe();
            Utils.DecrementAppIsBusy();

            return true;
        }

        // --------------------

        void OnToolBarClicked(object sender, EventArgs args)
        {
            ToolbarItem tb = sender as ToolbarItem;

            if (tb != null)
            {
                //string msgText = "You clicked: " + tb.Text;
                //DisplayAlert("Test", msgText, "Ok");


                //if (tb.Name.Equals("tbiAbout"))       //TODO: Name doesn't work as i expect
                if (tb.Text.Equals("about"))
                {
                    Navigation.PushAsync(new AnyEquation.Options.About.About());
                }
            }

        }


        #endregion ------------ Event handlers that are specific to this GUI implementation, so are not implemented as ICommand objects  ------------


        #region ------------ Misc ------------

        void InitLinks()
        {
            List<CPagelInk> optLinks = new List<CPagelInk>();
            InitOptionsCollection(optLinks);

            List<CPagelInk> allLinks = new List<CPagelInk>();
            foreach (var item in optLinks) { allLinks.Add(item); }

            IEnumerable<IGrouping<string, CPagelInk>> groups =
                                        allLinks
                                        .OrderBy(x => x.SortString)
                                        .GroupBy(x => x.SLinkType);

            foreach (var item in groups) { _groupedLinks.Add(item); }

        }

        void InitOptionsCollection(List<CPagelInk> optLinks)
        {
            optLinks.Clear();

            CPagelInk pl;

            pl = new CPagelInk(enLinkType.Option, "Number Format",
                async () => {
                    await DisplayAlert("Sorry, this feature is not yet impemented", "Number Format", "Ok");}
                , "", "");
            optLinks.Add(pl);

            pl = new CPagelInk(enLinkType.Option, "Page and Text Colors",
                () => { return Navigation.PushAsync(new AnyEquation.Options.Css3Colors.Css3ColorsPage()); }
                , "", "");
            optLinks.Add(pl);

            pl = new CPagelInk(enLinkType.Option, "About",
                () => { return Navigation.PushAsync(new AnyEquation.Options.About.About()); }
                , "", "");
            optLinks.Add(pl);

        }


        #endregion ------------ Misc ------------

        // -------------------- Private class

        public enum enLinkType
        {
            App = 0, Sample, Option
        }

        public class CPagelInk
        {
            public CPagelInk(enLinkType linkType, string title, InvokeMeTask invokeMe,
                                string urlSource, string infoText)
            {
                _linkType = linkType;
                _title = title;
                _invokeMe = invokeMe;
                _urlSource = urlSource;
                _infoText = infoText;
            }

            // ---------------------
            public delegate Task InvokeMeTask();

            // ---------------------
            private enLinkType _linkType = 0;
            public enLinkType LinkType { get { return _linkType; } set { _linkType = value; } }

            public string SLinkType { get { return _linkType.ToString() + "s"; } }

            private string _urlSource = "";
            public string UrlSource { get { return _urlSource; } set { _urlSource = value; } }

            private string _infoText = "";
            public string InfoText { get { return _infoText; } set { _infoText = value; } }

            public string SortString       //Allows us to control the order the groups are displayed
            {
                get
                {
                    string sPrefix = "";

                    switch (_linkType)
                    {
                        case enLinkType.Option:
                            sPrefix = "a";
                            break;
                        case enLinkType.App:
                            sPrefix = "b";
                            break;
                        case enLinkType.Sample:
                            sPrefix = "c";
                            break;
                        default:
                            break;
                    }

                    return (sPrefix + Title);
                }
            }

            private InvokeMeTask _invokeMe = null;
            public InvokeMeTask InvokeMe { get { return _invokeMe; } set { _invokeMe = value; } }

            private string _title = "";
            public string Title { get { return _title; } set { _title = value; } }

            private int _property = 0;
            public int Property { get { return _property; } set { _property = value; } }
        }
    }
}
