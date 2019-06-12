using Syncfusion.SfDataGrid.XForms.UWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AnyEquation.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            // --------------------- From PanGesture sample
            AnyEquation.App.ScreenWidth = Window.Current.Bounds.Width;
            AnyEquation.App.ScreenHeight = Window.Current.Bounds.Height;

            // --------------------- For SyncFusion Datagrid
            SfDataGridRenderer.Init();

            // ---------------------
            LoadApplication(new AnyEquation.App());
        }
    }
}
