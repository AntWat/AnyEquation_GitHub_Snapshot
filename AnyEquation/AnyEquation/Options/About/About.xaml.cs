using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace AnyEquation.Options.About
{
    public partial class About : ContentPage
    {
        public About()
        {
            InitializeComponent();
            this.BindingContext = this;
            DefineSources();
        }


        private ObservableCollection<CodeSource> _sources = new ObservableCollection<CodeSource>();
        public ObservableCollection<CodeSource> Sources { get { return _sources; } set { _sources = value; } }

        private void DefineSources()
        {
            _sources.Add(new CodeSource("Book: Creating Mobile Apps with Xamarin.Forms","https://developer.xamarin.com/guides/xamarin-forms/creating-mobile-apps-xamarin-forms/"));
            _sources.Add(new CodeSource("Xamarin.Forms Samples","https://developer.xamarin.com/samples/xamarin-forms/"));
            _sources.Add(new CodeSource("Xamarin Help Community Site", "https://xamarinhelp.com/"));
            _sources.Add(new CodeSource("Xamarin-Forms-Labs", "https://github.com/XLabs/Xamarin-Forms-Labs"));
            _sources.Add(new CodeSource("Some of additons and modifications of my own", ""));

            OnPropertyChanged("Sources");
        }


        // -------------------- Private class

        public class CodeSource
        {

            public CodeSource(string title, string uri)
            {
                Title = title;
                Uri = uri;
            }

            public string Title { get; private set; }
            public string Uri { get; private set; }

        }
    }

}

//<!--Some of my own.\n
//Book: Creating Mobile Apps with Xamarin.Forms: https://developer.xamarin.com/guides/xamarin-forms/creating-mobile-apps-xamarin-forms/\n
//https://developer.xamarin.com/guides/xamarin-forms/creating-mobile-apps-xamarin-forms/\n
//https://developer.xamarin.com/samples/xamarin-forms/\n
//https://xamarinhelp.com/ -->

