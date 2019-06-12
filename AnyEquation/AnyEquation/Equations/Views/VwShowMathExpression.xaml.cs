using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AnyEquation.Equations.Model.Dimensions;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnyEquation.Equations.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VwShowMathExpression : ContentPage
    {
        public IEquationsUiService EquationsUiService  { get; set; }
        public SingleResult MathExpression { get; set; }

        public VwShowMathExpression(IEquationsUiService equationsUiService, SingleResult mathExpression)
        {
            EquationsUiService = equationsUiService;
            MathExpression = mathExpression;

            InitializeComponent();

            InitInfoRows();

            this.BindingContext = this;
        }

        private void InitInfoRows()
        {
            InfoRows.Add(new InfoRow()
            {
                Label = "Value : ",
                Text = " " +((MathExpression?.CalcQuantity?.CalcStatus == CalcStatus.Good) ?
                                   $"{(MathExpression?.CalcQuantity?.Value)}" :
                                   ""),
            });
            InfoRows.Add(new InfoRow()
            {
                Label = "Units Set : ",
                Text = " " + MathExpression?.CalcQuantity?.AnonUOM?.UOMSet?.Name,
            });

            Dimensions dims = MathExpression?.CalcQuantity?.AnonUOM?.Dimensions;
            string dimText = "unknown";
            if (dims!=null)
            {
                dimText = IsDimenionless(dims) ? "dimensionless" : $"{dims?.ToString()}     // {Dimensions.GetExplanationLine()}";
            }

            InfoRows.Add(new InfoRow()
            {
                Label = "Dimensions : ",
                Text = " " + dimText,
            });
            //InfoRows.Add(new InfoRow()
            //{
            //    Label = "Units : ",
            //    Text = "?",
            //});
            if (MathExpression?.CalcQuantity?.Message?.Length>0)
            {
                InfoRows.Add(new InfoRow()
                {
                    Label = "Message : ",
                    Text = " " + MathExpression?.CalcQuantity?.Message,
                });
            }
        }

        private ObservableCollection<InfoRow> _InfoRows = new ObservableCollection<InfoRow>();
        public ObservableCollection<InfoRow> InfoRows { get { return _InfoRows; } }

        public class InfoRow
        {
            public string Label { get; set; }
            public string Text { get; set; }
        }
    }
}