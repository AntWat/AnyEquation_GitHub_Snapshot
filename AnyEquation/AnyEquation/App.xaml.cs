using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.FormsBook.Toolkit;
using AnyEquation.Common;
using AnyEquation.Equations.Views;

namespace AnyEquation
{
    public partial class App : Application
    {
        #region ------------ Fields and Properties ------------

        // -----------------------
        public static string Key_BackgroundColor { get; } = "mainBackgroundColor";
        public static string Key_TextColor { get; } = "textColor";
        public static string Key_ContrastColor { get; } = "contrastColor";
        public static string Key_EntryBackgroundColor { get; } = "entryBackgroundColor";
        public static string Key_EntryTextColor { get; } = "entryTextColor";
        public static string Key_NavBarBackColor { get; } = "navBarBackColor";
        public static string Key_NavBarTextColor { get; } = "navBarTextColor";
        public static string Key_DisabledTextColor { get; } = "disabledTextColor";

        public static string Key_backgroundTextBlend05PcColor { get; } = "backgroundTextBlend05PcColor";
        public static string Key_backgroundTextBlend10PcColor { get; } = "backgroundTextBlend10PcColor";
        public static string Key_backgroundTextBlend25PcColor { get; } = "backgroundTextBlend25PcColor";
        public static string Key_backgroundTextBlend50PcColor { get; } = "backgroundTextBlend50PcColor";
        public static string Key_backgroundTextBlend75PcColor { get; } = "backgroundTextBlend75PcColor";
        public static string Key_backgroundTextBlend85PcColor { get; } = "backgroundTextBlend85PcColor";
        public static string Key_backgroundTextBlend95PcColor { get; } = "backgroundTextBlend95PcColor";

        // --------------------- From PanGesture sample
        public static double ScreenWidth;
        public static double ScreenHeight;

        // ---------------------
        const string strMainBackgroundColorName = "MainBackgroundColorName";
        const string strTextColorName = "TextColorName";

        private string _mainBackgroundColorName = "";
        public string MainBackgroundColorName
        {
            get { return _mainBackgroundColorName; }
            set
            {
                _mainBackgroundColorName = value;
                Properties[strMainBackgroundColorName] = _mainBackgroundColorName;
            }
        }

        private string _textColorName = "";
        public string TextColorName
        {
            get { return _textColorName; }
            set
            {
                _textColorName = value;
                Properties[strTextColorName] = _textColorName;
            }
        }

        #endregion ------------ Fields and Properties ------------

        public App()
        {
            Xamarin.FormsBook.Toolkit.Toolkit.Init();       // This is an empty routine that forces the lib to be recognised, otherwise pure xaml use of the lib doesn't compile

            InitializeComponent();

            // ------------------- Show the launch page, which immediately shows the Equations page
            App.Current.Resources["ShowAppBusy"] = true;        // Set as busy before anything loads
            Utils.IncrementAppIsBusy();

            VwLaunchPage mcp = new VwLaunchPage();
            NavigationPage navPage = new NavigationPage(mcp);

            MainPage = navPage;

            Utils.DecrementAppIsBusy();     // Make sure we refresh the busy state here, if nothing else does
        }

        public void ResetColors()
        {
            if (Application.Current.Resources.ContainsKey("defaultMainBackgroundColor"))
            {
                _mainBackgroundColorName = (OnPlatform<String>)Resources["defaultMainBackgroundColor"];
            }
            if (Application.Current.Resources.ContainsKey("defaultTextColor"))
            {
                _textColorName = (OnPlatform<String>)Resources["defaultTextColor"];
            }

            SetColoursFromNames();
        }

        protected override void OnStart()
        {
            //Logger.WriteLine("OnStart: Pt-1");

            IPlatformInfo platformInfo = DependencyService.Get<IPlatformInfo>();
            Resources["AppVersion"] = platformInfo.GetAppVersion();

            // -------------------
            SetFirstColors();
        }

        protected override void OnResume()
        {
            //Logger.WriteLine("OnResume: Pt-1");
            SetFirstColors();
        }

        protected override void OnSleep()
        {
            //Logger.WriteLine("OnSleep: Pt-1");
            //Logger.WriteLine(string.Format("Saving property: {0}={1}", strMainBackgroundColorName,_mainBackgroundColorName));
            Properties[strMainBackgroundColorName] = _mainBackgroundColorName;
            Properties[strTextColorName] = _textColorName;

            App.Current.SavePropertiesAsync();
        }

        private void SetFirstColors()
        {
            //Logger.WriteLine("SetFirstColors: Pt-1");
            ResetColors();

            //Logger.WriteLine("SetFirstColors: Pt-2");
            if (Properties.ContainsKey(strMainBackgroundColorName))
            {
                //Logger.WriteLine("Setting: _mainBackgroundColorName");
                _mainBackgroundColorName = (string)Properties[strMainBackgroundColorName];
            }
            if (Properties.ContainsKey(strTextColorName))
            {
                //Logger.WriteLine("Setting: _textColorName");
                _textColorName = (string)Properties[strTextColorName];
            }

            SetColoursFromNames();
        }

        public void SetColoursFromNames()
        {
            NavigationPage navPage = (NavigationPage)Application.Current.MainPage;
            NamedColor namedBackgroundColor = null;
            NamedColor namedTextColor = null;

            if (_mainBackgroundColorName.Length > 0)
            {
                namedBackgroundColor = NamedColor.All.First(x => x.Name.Equals(_mainBackgroundColorName));
            }
            if (_textColorName.Length > 0)
            {
                namedTextColor = NamedColor.All.First(x => x.Name.Equals(_textColorName));
            }

            if (namedBackgroundColor != null)
            {
                Application.Current.Resources[Key_BackgroundColor] = namedBackgroundColor.Color;

                if (namedTextColor != null)
                {
                    if (namedTextColor.Brightness > 191)
                    {
                        Application.Current.Resources[Key_NavBarBackColor] = NamedColor.CadetBlue;
                    }
                    else
                    {
                        Application.Current.Resources[Key_NavBarBackColor] = namedTextColor.Color;
                    }

                    navPage.BarBackgroundColor = (Color)Application.Current.Resources[Key_NavBarBackColor];
                }

            }


            if (namedTextColor != null)
            {
                Application.Current.Resources[Key_TextColor] = namedTextColor.Color;

                if (namedBackgroundColor != null)
                {
                    Application.Current.Resources[Key_NavBarTextColor] = namedBackgroundColor.Color;
                    navPage.BarTextColor = (Color)Application.Current.Resources[Key_NavBarTextColor];
                }
            }

            App.SetDependentColors(Application.Current);
        }

        public static void SetDependentColors(Application app)
        {
            Color mainBackgroundColor = (Color)app.Resources[Key_BackgroundColor];
            Color textColor = (Color)app.Resources[Key_TextColor];
            app.Resources[Key_ContrastColor] = NamedColor.BlackOrWhiteForContrast(mainBackgroundColor);

            app.Resources[Key_DisabledTextColor] = NamedColor.BlendColors(mainBackgroundColor, textColor, 0.5);

            app.Resources[Key_backgroundTextBlend05PcColor] = NamedColor.BlendColors(mainBackgroundColor, textColor, 0.05);
            app.Resources[Key_backgroundTextBlend10PcColor] = NamedColor.BlendColors(mainBackgroundColor, textColor, 0.10);
            app.Resources[Key_backgroundTextBlend25PcColor] = NamedColor.BlendColors(mainBackgroundColor, textColor, 0.25);
            app.Resources[Key_backgroundTextBlend50PcColor] = NamedColor.BlendColors(mainBackgroundColor, textColor, 0.50);
            app.Resources[Key_backgroundTextBlend75PcColor] = NamedColor.BlendColors(mainBackgroundColor, textColor, 0.75);
            app.Resources[Key_backgroundTextBlend85PcColor] = NamedColor.BlendColors(mainBackgroundColor, textColor, 0.85);
            app.Resources[Key_backgroundTextBlend95PcColor] = NamedColor.BlendColors(mainBackgroundColor, textColor, 0.95);

            if (mainBackgroundColor == NamedColor.Black)
            {
                app.Resources[Key_EntryBackgroundColor] = NamedColor.White;
                app.Resources[Key_EntryTextColor] = NamedColor.Black;
            }
            else
            {
                app.Resources[Key_EntryBackgroundColor] = mainBackgroundColor;
                app.Resources[Key_EntryTextColor] = app.Resources[Key_TextColor];
            }
        }

    }
}
