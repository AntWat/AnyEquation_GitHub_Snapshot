using Syncfusion.SfDataGrid.XForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AnyEquation.Equations.Views
{
    public class SfChooseUomGridStyle : DataGridStyle
    {
        public SfChooseUomGridStyle()
        {
        }

        public override Color GetHeaderBackgroundColor()
        {
            return (Color)Application.Current.Resources[App.Key_backgroundTextBlend25PcColor];
        }

        public override Color GetHeaderForegroundColor()
        {
            return (Color)Application.Current.Resources[App.Key_BackgroundColor];
        }

        public override Color GetRecordBackgroundColor()
        {
            return Color.Transparent;
        }

        public override Color GetRecordForegroundColor()
        {
            return (Color)Application.Current.Resources[App.Key_TextColor];
        }

        public override Color GetSelectionBackgroundColor()
        {
            return (Color)Application.Current.Resources[App.Key_TextColor];
        }

        public override Color GetSelectionForegroundColor()
        {
            return (Color)Application.Current.Resources[App.Key_BackgroundColor];
        }

        public override Color GetCaptionSummaryRowBackgroundColor()
        {
            return (Color)Application.Current.Resources[App.Key_backgroundTextBlend25PcColor];
        }

        public override Color GetCaptionSummaryRowForeGroundColor()     // TODO: Obsolete?
        {
            return (Color)Application.Current.Resources[App.Key_BackgroundColor];
        }

        public override Color GetBorderColor()
        {
            return (Color)Application.Current.Resources[App.Key_TextColor];
        }

        //public override Color GetLoadMoreViewBackgroundColor()
        //{
        //    return Color.FromRgb(242, 242, 242);
        //}

        //public override Color GetLoadMoreViewForegroundColor()
        //{
        //    return Color.FromRgb(34, 31, 31);
        //}

        //public override Color GetAlternatingRowBackgroundColor()
        //{
        //    return Color.Yellow;
        //}
    }
}
