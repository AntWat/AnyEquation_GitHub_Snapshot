using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.FormsBook.Toolkit;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.EquationParser;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.Model.Info;
using static AnyEquation.Equations.Model.Dimensions;
using AnyEquation.Equations.Database;
using static AnyEquation.Equations.Model.ContentManager;
using AnyEquation.Common;

// ----------------------------------

namespace AnyEquation.Equations.ViewModels
{
    public class VmPreviewEquation : ViewModelBase
    {
        #region ------------ Constructors and Life Cycle ------------

        public VmPreviewEquation(ContentManager contentManager, EqnCalc currentEquationCalc, Action okAction, Action cancelAction)
        {
            try
            {
                _contentManager = contentManager;
                _uOMSet = ContentManager.UOMSet_SI;

                SetCurrentEquationCalc(EqnCalc.IsolateFirstVariableIfRequired(currentEquationCalc, contentManager));

                _okAction = okAction;
                _cancelAction = cancelAction;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Commands

        Action _okAction;
        Action _cancelAction;

        #endregion ------------ Commands


        #region ------------ Fields and Properties ------------

        // -------------- IsBusy
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (SetProperty(ref _isBusy, value)) { }
            }
        }

        // -------------- ContentManager
        private ContentManager _contentManager;
        public ContentManager ContentManager
        {
            get { return _contentManager; }
        }

        private UOMSet _uOMSet;     // The UOMSet used for calculations


        // -------------- CurrentEquationCalc
        private EqnCalc _currentEquationCalc;
        public EqnCalc CurrentEquationCalc
        {
            get { return _currentEquationCalc; }
        }

        public string CurrentEqCalcDescription { get { return _currentEquationCalc?.Description ?? ""; } }

        // -------------- EqPreviewVariables
        private ObservableCollection<VmPreviewEqVar> _eqPreviewVariables = new ObservableCollection<VmPreviewEqVar>();
        public ObservableCollection<VmPreviewEqVar> EqPreviewVariables { get { return _eqPreviewVariables; } }


        // -------------- MaxVarLength: Allowing binding to the max Var and Uom lengths, for column width calculations
        // (probably not needed anymore)

        public event EventHandler MaxVarLengthChanged;

        private int _maxVarLength;      // Max number of characters used for a variable name for the CurrentEquationCalc
        public int MaxVarLength { get { return _maxVarLength; } }

        private int _maxUOMLength;      // Max number of characters used for a UOM for the CurrentEquationCalc
        public int MaxUOMLength { get { return _maxUOMLength; } }

        // -------------- Error and Info Messages
        private string _errorMessage = "";
        public string ErrorMessage { get { return _errorMessage; } }
        public bool ErrorMessageExists { get { return (_errorMessage?.Length > 0); } }

        private void OnMessagesChanged()
        {
            OnPropertyChanged("ErrorMessage");
            OnPropertyChanged("ErrorMessageExists");
            OnPropertyChanged("InfoMessage");
        }

        public string InfoMessage
        {
            get
            {
                try
                {
                    return FormattedInfoMessage(_currentEquationCalc?.GetCalcStatus() ?? CalcStatus.Uknown);
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }
        }

        private string FormattedInfoMessage(CalcStatus calcStatus)
        {
            if (_errorMessage.Length > 0)
            {
                string sMsg = _errorMessage?.Trim();

                if ((sMsg?.Length > 0) && !(sMsg.Substring(sMsg.Length - 1).Equals("!")))
                {
                    sMsg += "!";
                }
                return sMsg;
            }
            switch (calcStatus)
            {
                case CalcStatus.Good:
                    return "OK";
                case CalcStatus.Uknown:
                case CalcStatus.Bad:
                    return "Unknown failure!";
                default:
                    return "";
            }
        }

        // -------------- NumberFormat
        private string _numberFormat = "0.###";
        public string NumberFormat
        {
            get { return _numberFormat; }
            set
            {
                if (SetProperty(ref _numberFormat, value))
                {
                    //OnPropertyChanged("ErrorMessageExists");
                }
            }
        }

        #endregion ------------ Fields and Properties ------------


        #region ------------ SetCurrentEquationCalc

        private void SetCurrentEquationCalc(EqnCalc eqnCalc)
        {
            if (_currentEquationCalc != eqnCalc)
            {
                _currentEquationCalc = eqnCalc;

                SetEqPreviewVariables();
                Recalculate();

                OnPropertyChanged("CurrentEqCalcDescription");
            }
        }
        
        private void SetEqPreviewVariables()
        {
            try
            {
                IList<SingleValue> eqVars = _currentEquationCalc?.FindAllVariables() ?? (new List<SingleValue>());
                IList<SingleValue> eqConstants = _currentEquationCalc?.FindAllConstants() ?? (new List<SingleValue>());

                // -------------
                SetMaxVarLength(eqVars, eqConstants);

                // -------------
                EqPreviewVariables.Clear();

                // -------------
                Random rnd = new Random();
                const int rndMax = 1000000;
                const double rndDivide = (double)(rndMax / 10);

                // -------------
                for (int i = 0; i < eqVars.Count; i++)
                {
                    var item = eqVars[i];
                    item.ClearAllVarVals();

                    bool isCalculated = false;
                    if (i == 0) { isCalculated = true; }

                    VmPreviewEqVar eqVar = new VmPreviewEqVar(this, item, isCalculated);
                    EqPreviewVariables.Add(eqVar);

                    AssignTestData(eqVar, isCalculated, item, rnd, rndMax, rndDivide);
                }

                for (int i = 0; i < eqConstants.Count; i++)
                {
                    var item = eqConstants[i];
                    EqPreviewVariables.Add(new VmPreviewEqVar(this, item, true));
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }

        }

        private void AssignTestData(VmPreviewEqVar eqVar, bool isCalculated, SingleValue sngVal, Random rnd, int rndMax, double rndDivide)
        {
            if (!isCalculated && (sngVal is Variable))
            {
                if ((_currentEquationCalc.TestValues != null) && _currentEquationCalc.TestValues.ContainsKey(sngVal.Name))
                {
                    eqVar.Value = _currentEquationCalc.TestValues[sngVal.Name];
                }
                else
                {
                    // Assign randomn values for the test calculation
                    int rndNext = rnd.Next(rndMax);
                    double varVal = ((double)rndNext) / rndDivide;       // Going to be a Number between 0 and 10
                    eqVar.Value = varVal;
                }
            }
            eqVar.SetExpectedDimensions((_currentEquationCalc.ExpectedDimensions.ContainsKey(sngVal.Name) ?
                                                    _currentEquationCalc.ExpectedDimensions[sngVal.Name] : null));
        }

        #endregion ------------ SetCurrentEquationCalc


        #region ------------ Calculation

        public event EventHandler CalculationFinished;

        private bool _bCalculating = false;

        public void ClearResults()       // For use before calculation and when the display may be inconsistent
        {
            _currentEquationCalc.SetCalcStatus(CalcStatus.Uknown);

            _errorMessage = "";

            foreach (var eqCalcVar in EqPreviewVariables)
            {
                if (eqCalcVar.IsCalculated && (eqCalcVar.GetSingleValue() is Variable))
                {
                    eqCalcVar.GetSingleValue()?.CalcQuantity?.Clear();
                    eqCalcVar.RefreshThisData();
                }
            }

            OnMessagesChanged();
        }

        // -------------------------------------
        public void Recalculate()
        {
            try
            {
                if (EqPreviewVariables.Count > 0)
                {
                    if (_bCalculating) return;
                    _bCalculating = true;

                    UpdateCalcInputs();
                    ClearResults();

                    CalcStatus calcStatus = _currentEquationCalc.CalculateAll();

                    IList<FunCalcError> lowestErrors = new List<FunCalcError>();
                    IList<FunCalcError> allErrors = new List<FunCalcError>();
                    _currentEquationCalc.GetErrors(ref lowestErrors, ref allErrors, NumberFormat: NumberFormat);
                    if (lowestErrors?.Count > 0)
                    {
                        _errorMessage = lowestErrors[0].Message;
                    }
                    else
                    {
                        foreach (var item in EqPreviewVariables)
                        {
                            if (item.UnExpectedDimensions)
                            {
                                _errorMessage = "Unexpected Dimensions in variables or results";
                            }
                        }
                    }
                    OnMessagesChanged();

                    RefreshEqPreviewVariables();

                    SetMaxUOMLength();
                    CalculationFinished?.Invoke(this, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
            finally
            {
                _bCalculating = false;
            }
        }

        // -------------------------------------
        /// <summary>
        /// Update the SingleValue items used by the calculation
        /// </summary>
        private void UpdateCalcInputs()
        {
            foreach (var item in EqPreviewVariables)
            {
                UpdateCalcInput(item);
            }
        }

        void UpdateCalcInput(VmPreviewEqVar wmEqVar)
        {
            try
            {
                SingleValue singleValue = wmEqVar.GetSingleValue();
                KnownUOM knownUOM = wmEqVar.GetKnownUOM();

                if (singleValue == null) return;
                if (singleValue is Constant) return;
                if (singleValue is Literal) return;

                // Check or update the calc UOM
                bool bUpdateCalcUom = false;
                if (knownUOM == null)
                {
                    singleValue.CalcQuantity.AnonUOM = null;
                }
                else if (singleValue.CalcQuantity.AnonUOM == null)
                {
                    bUpdateCalcUom = true;
                }
                else if (!knownUOM.Dimensions.Equals(singleValue.CalcQuantity.AnonUOM.Dimensions))
                {
                    bUpdateCalcUom = true;
                }

                if (bUpdateCalcUom)
                {
                    singleValue.CalcQuantity.AnonUOM = new AnonUOM(knownUOM.Dimensions, _uOMSet);
                }

                double dbl = (wmEqVar.Value ?? double.NaN);

                CalcStatus calcStatus;
                if (double.IsNaN(dbl) || double.IsInfinity(dbl))
                {
                    calcStatus = CalcStatus.Bad;
                }
                else
                {
                    calcStatus = CalcStatus.Good;
                }

                singleValue.CalcQuantity.CalcStatus = calcStatus;

                // Convert the input value to the calc UOM
                double convertedVal = dbl;

                if (knownUOM != null)
                {
                    convertedVal = UOM.Convert(dbl, fromUOM: knownUOM, anonToUOM: singleValue.CalcQuantity.AnonUOM);
                }

                singleValue.CalcQuantity.Value = convertedVal;

                SetMaxUOMLength();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        // -----------------------------
        private void RefreshEqPreviewVariables()
        {
            try
            {
                foreach (var item in EqPreviewVariables)
                {
                    if (item != null)
                    {
                        item.RefreshThisData();
                    }
                }

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        #endregion ------------ Calculation


        #region ------------ Called from VmPreviewEqVar

        public KnownUOM KnownUOMFromDimensions(Dimensions dimensions)
        {
            bool WasCreated = false;
            KnownUOM knownUOM = _uOMSet?.FindFromDimensions(dimensions, bAllowCreate: true, wasCreated: out WasCreated);

            if (WasCreated)
            {
                ContentManager.KnownUOMs.Add(knownUOM.Name, knownUOM);
            }

            return knownUOM;
        }

        #endregion ------------ Called from VmPreviewEqVar

        #region ------------ Called from the UI

        public void DoOkAction()
        {
            _okAction();
        }
        public void DoCancelAction()
        {
            _cancelAction();
        }

        #endregion ------------ Called from the UI


        #region ------------ Updating MaxVarLength and MaxUOMLength

        private void SetMaxVarLength(IList<SingleValue> eqVars, IList<SingleValue> eqConstants)
        {
            int maxVarLen = 0;
            foreach (var item in eqVars)
            {
                if (item?.Name?.Length > maxVarLen)
                {
                    maxVarLen = item.Name.Length;
                }
            }
            foreach (var item in eqConstants)
            {
                if (item?.Name?.Length > maxVarLen)
                {
                    maxVarLen = item.Name.Length;
                }
            }
            _maxVarLength = maxVarLen;
            OnPropertyChanged("MaxVarLength");
            MaxVarLengthChanged?.Invoke(this, null);
        }

        private void SetMaxUOMLength()
        {
            try
            {
                int maxUOMLength = 0;

                foreach (var item in EqPreviewVariables)
                {
                    if (item?.UomSymbol?.Length > maxUOMLength)
                    {
                        maxUOMLength = item.UomSymbol.Length;
                    }
                }

                _maxUOMLength = maxUOMLength;
                OnPropertyChanged("MaxUomLength");
                MaxVarLengthChanged?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        #endregion ------------ Updating MaxVarLength and MaxUOMLength


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // ++++++++++++++++++++++++ Private class, used above and as a Binding
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public class VmPreviewEqVar : ViewModelBase
        {
            #region ------------ Constructors and Life Cycle ------------

            VmPreviewEquation _parent;

            public VmPreviewEqVar(VmPreviewEquation parent, SingleValue singleValue, bool isCalculated, bool refreshNow = false)
            {
                try
                {
                    _parent = parent;
                    _singleValue = singleValue;

                    _IsCalculated = isCalculated;

                    _knownUOM = _parent.ContentManager.DefaultUOMSet.FindFromParamType(singleValue.ParamType);

                    // ------------- Refresh items that will not change after this instance is created
                    OnPropertyChanged("IsCalculated");
                    OnPropertyChanged("CanChangeUom");

                    // ------------- Refresh other items if required
                    if (refreshNow) { RefreshThisData(); }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }

            #endregion ------------ Constructors and Life Cycle ------------

            #region ------------ Fields and Properties ------------

            private SingleValue _singleValue;   // Holds the real calc IO.
            public SingleValue GetSingleValue() { return _singleValue; }    // Not a property so I don't have to worry about anthing binding to it

            private string _name;
            public string GetName() { return _name; }

            public string DisplayName
            {
                get
                {
                    if (_singleValue is Constant)
                    {
                        return Constant.RemovePrefix(_name);
                    }
                    else
                    {
                        return _name;
                    }
                }
            }

            private string _description;
            public string Description
            {
                get { return _description; }
            }

            private Dimensions _expectedDimensions;
            public void SetExpectedDimensions(Dimensions expectedDimensions) { _expectedDimensions = expectedDimensions; }

            public bool UnExpectedDimensions { get; set; } = false;

            private double? _value;
            public double? Value
            {
                get
                {       // Return null rather than nan or infinity
                    if (_value == null)
                    {
                        return null;
                    }
                    else
                    {
                        double val = (double)_value;
                        if (double.IsNaN(val) || double.IsInfinity(val))
                        {
                            return null;
                        }
                        return val;
                    }
                }
                set
                {
                    try
                    {
                        if (!_bRefreshing && !IsCalculated)
                        {
                            if (SetProperty(ref _value, value, propertyName: "Value"))
                            {
                                _parent.Recalculate();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(ex);
                        throw;
                    }
                }
            }

            private KnownUOM _knownUOM;
            public KnownUOM GetKnownUOM() { return _knownUOM; }

            public string UomSymbol
            {
                get
                {
                    try
                    {
                        string uomSymbol = "";

                        if (_singleValue == null)
                        {
                            uomSymbol = "";
                        }
                        else if (_knownUOM != null)
                        {
                            uomSymbol = _knownUOM.Symbol;
                        }
                        else
                        {
                            uomSymbol = "unit?";
                        }
                        return uomSymbol;
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(ex);
                        throw;
                    }
                }
            }

            public bool CanChangeUom
            {
                get
                {
                    if (IsDimenionless(_singleValue?.ParamType?.Dimensions))
                    {
                        return false;
                    }
                    else if ((_singleValue is Variable) || (_singleValue is Constant))
                    {
                        return true;
                    }
                    return false;
                }
            }

            private string _paramInfo;
            public string ParamInfo
            {
                get { return _paramInfo; }
            }

            private bool _IsCalculated = false;
            public bool IsCalculated
            {
                get { return _IsCalculated; }
            }

            #endregion ------------ Fields and Properties ------------

            #region ------------ Refresh

            private bool _bRefreshing = false;

            /// Refresh this data after a calculation or UOM change
            public void RefreshThisData()
            {
                bool old_bRefreshing = _bRefreshing;
                _bRefreshing = true;

                try
                {
                    if (_singleValue == null) return;

                    // ----------------------
                    _name = _singleValue?.Name;
                    _paramInfo = _singleValue?.ParamType?.Name;
                    _description = " " + _singleValue?.Description;

                    RefreshUOM();

                    // ------------ Set the value etc.
                    if (IsCalculated) { UpdateCalcOutput(); }

                    // ------------ Refresh the observers
                    OnPropertyChanged("DisplayName");
                    OnPropertyChanged("ParamInfo");
                    OnPropertyChanged("Description");
                    OnPropertyChanged("KnownUOM");
                    OnPropertyChanged("UomSymbol");
                    OnPropertyChanged("UnExpectedDimensions");
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
                finally
                {
                    _bRefreshing = old_bRefreshing;
                }
            }

            /// Sort out the UOM
            private void RefreshUOM()
            {
                UOM oUOM = _singleValue?.CalcQuantity?.GetUOM();
                KnownUOM calcUOM = null;

                if (oUOM is KnownUOM)
                {
                    calcUOM = (KnownUOM)oUOM;
                }
                else if (oUOM is AnonUOM anonUOM)       // Most likely
                {
                    calcUOM = _parent.KnownUOMFromDimensions(anonUOM.Dimensions);
                }

                if ((_knownUOM == null) ||
                    (IsCalculated && (_singleValue.CalcQuantity.CalcStatus == CalcStatus.Good) &&
                     !UOM.EqualDimensions(_knownUOM, calcUOM)))
                {
                    _knownUOM = calcUOM;
                }

                UnExpectedDimensions = ((_expectedDimensions != null) && (_knownUOM != null) &&
                                        !(_knownUOM.Dimensions.Equals(_expectedDimensions)));
            }

            /// Convert the calculated value to the required UOM
            private void UpdateCalcOutput()
            {
                try
                {
                    if (_singleValue == null) return;

                    // Convert the value to the calc UOM
                    double dbl = _singleValue.CalcQuantity.Value;

                    double convertedVal = dbl;
                    if (_knownUOM != null)
                    {
                        convertedVal = UOM.Convert(dbl, anonFromUOM: _singleValue.CalcQuantity.AnonUOM, toUOM: _knownUOM);
                    }

                    _value = convertedVal;

                    OnPropertyChanged("Value");
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }

            #endregion ------------ Misc ------------
        }
    }

}
