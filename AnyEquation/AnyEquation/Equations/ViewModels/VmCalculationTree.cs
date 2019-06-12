using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.FormsBook.Toolkit;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.ViewModels
{
    public class VmCalculationTree : ViewModelBase
    {
        #region ------------ Statics ------------

        #endregion ------------ Statics ------------

        #region ------------ Constructors and Life Cycle ------------

        public IEquationsUiService EquationsUiService { get; set; }

        public VmCalculationTree(IEquationsUiService equationsUiService, EqnCalc equationCalc, string numberFormat)
        {
            EquationsUiService = equationsUiService;
            NumberFormat = numberFormat;
            CurrentEquationCalc = equationCalc;
        }

        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Fields and Properties ------------

        public string NumberFormat { get; set; }

        private EqnCalc _currentEquationCalc;
        public EqnCalc CurrentEquationCalc
        {
            get { return _currentEquationCalc; }
            set
            {
                if (SetProperty(ref _currentEquationCalc, value))
                {
                    GetCalculationTreeRows(CurrentEquationCalc, CalculationTreeRows, sIndent: "   ", NumberFormat: NumberFormat);
                }
            }
        }

        // ------------------------------

        private ObservableCollection<CalcTreeRowInfo> _CalculationTreeRows = new ObservableCollection<CalcTreeRowInfo>();
        public ObservableCollection<CalcTreeRowInfo> CalculationTreeRows { get { return _CalculationTreeRows; } set { _CalculationTreeRows = value; } }

        #endregion ------------ Fields and Properties ------------


        #region ------------ GetCalculationTreeRows

        /// <summary>
        /// Return information about how the expression was calculated
        /// </summary>
        /// <param name="sIndent">Indentation used for subsequent levels of calculationTree</param>
        /// <param name="calculationTree">A string representation of how the calculation was conducted, and the errors at different levels</param>
        private void GetCalculationTreeRows(FunctionCalc fnCalc, IList<CalcTreeRowInfo> calculationTreeRows, string sIndent, string NumberFormat)
        {
            calculationTreeRows.Clear();

            GetCalculationTreeRows2(fnCalc, calculationTreeRows, "", sIndent, NumberFormat);
        }

        private static void GetCalculationTreeRows2(FunctionCalc fnCalc, IList<CalcTreeRowInfo> calculationTreeRows, string sCurrentIndent, string sIndentStep, string NumberFormat)
        {
            CalcTreeRowInfo rowInfo = new CalcTreeRowInfo();
            rowInfo.MathExpression = fnCalc;

            if (fnCalc.Function is FnEquals)
            {
                rowInfo.DisplayText = string.Format("{0}{1} ({2}) {3}\n",
                                    sCurrentIndent, fnCalc.Function.Name, fnCalc.Function.AsciiSymbol,
                                    (fnCalc.CalcQuantity.Message.Length == 0) ? "" : (" // " + fnCalc.CalcQuantity.Message));
            }
            else
            {
                rowInfo.DisplayText = string.Format("{0}{1} ({2}) = {3}{4}\n",
                                    sCurrentIndent, fnCalc.Function.Name, fnCalc.Function.AsciiSymbol,
                                    ((fnCalc.GetCalcStatus() == CalcStatus.Good) ?
                                       string.Format("{0:" + NumberFormat + "}", fnCalc.CalcQuantity.Value) :
                                       "!"),
                                    (fnCalc.CalcQuantity.Message.Length == 0) ? "" : (" // " + fnCalc.CalcQuantity.Message));
            }

            calculationTreeRows.Add(rowInfo);

            // ----------------------
            foreach (SingleResult input in fnCalc.Inputs)
            {
                if (input is FunctionCalc)
                {
                    GetCalculationTreeRows2(((FunctionCalc)input), calculationTreeRows, (sCurrentIndent + sIndentStep), sIndentStep, NumberFormat);
                }
                else
                {
                    GetCalculationTreeRows2(input, calculationTreeRows, (sCurrentIndent + sIndentStep), NumberFormat);
                }
            }
        }

        private static void GetCalculationTreeRows2(SingleResult input, IList<CalcTreeRowInfo> calculationTreeRows, string sCurrentIndent, string NumberFormat)
        {
            CalcTreeRowInfo rowInfo = new CalcTreeRowInfo();
            rowInfo.MathExpression = input;

            rowInfo.DisplayText = string.Format("{0}{1}{2}{3}\n",
                                sCurrentIndent,
                                (input?.Name?.Length == 0) ? "" : string.Format("{0} = ", input?.Name),
                                (
                                 (input?.CalcQuantity?.CalcStatus == CalcStatus.Good) ?
                                   string.Format("{0:" + NumberFormat + "}", input?.CalcQuantity?.Value) :
                                   ""       /*Missing or bad value*/
                                ),
                                (input?.CalcQuantity?.Message?.Length == 0) ? "" : (" // " + input?.CalcQuantity?.Message));

            calculationTreeRows.Add(rowInfo);
        }

        #endregion ------------ GetCalculationTreeRows


        // -------------------- Private class

        public class CalcTreeRowInfo
        {
            public string DisplayText { get; set; }
            public SingleResult MathExpression { get; set; }
        }

    }
}
