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

// ----------------------------------
/* TODO: Equations:

 *) Model:
       Auto-define UOM for km^2 etc? Or better still, create them dynamically when needed?
       Moles?

 *) C#
       Override == and != where required
       Make constants immutable

 *) Features:
        Number format

 *) Proper systems for error handling, logging, validation, Result

 *) Lots of error handling
  
 *) Get beta trial working again
  
 *) Rewrite classes that were copied from VmEqCalcVariable completely
  
    */

// ----------------------------------

namespace AnyEquation.Equations.ViewModels
{
    public class VmEquations : ViewModelBase
    {
        #region ------------ Constructors and Life Cycle ------------

        private IEquationsUiService _equationsUiService;
        public IEquationsUiService EquationsUiService { get { return _equationsUiService; } }

        public VmEquations(IEquationsUiService equationsUiService)
        {
            _equationsUiService = equationsUiService;
            DefineCommands();
        }

        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Commands

        public ICommand ChooseEquation { private set; get; }

        public ICommand ShowCalculationTree { private set; get; }

        public ICommand AddEqn { private set; get; }

        public ICommand ChooseDefaultUnits { private set; get; }

        public ICommand ShowSettings { private set; get; }


        private void DefineCommands()
        {
            // -------------------
            ChooseEquation = new Command(
                execute: () => { _equationsUiService.ShowChooseEquation(this); });

            // -------------------
            ShowSettings = new Command(
                execute: () => { _equationsUiService.ShowSettings(); });

            // -------------------
            ShowCalculationTree = new Command(
                execute: () => { _equationsUiService.ShowCalculationTree(CurrentEquationCalc, NumberFormat); });

            // -------------------
            AddEqn = new Command(
                execute: async () =>
                {
                    EqnCalc equationCalc = await _equationsUiService.ShowCreateEquation(ContentManager);
                    if (equationCalc != null) { CurrentEquationCalc = equationCalc; }
                });

            // -------------------
            ChooseDefaultUnits = new Command(
                execute: ()=>_equationsUiService.ShowChooseDefaultUnits(ContentManager));
        }

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
        public void SetContentManager(ContentManager contentManager)
        {
            _contentManager = contentManager;
            _uOMSet = ContentManager.UOMSet_SI;
            CurrentEquationCalc = _contentManager.EquationLibraries.ElementAt(0).Value.GetAllEqnCalcs()[0];
        }
        public ContentManager ContentManager
        {
            get { return _contentManager; }
        }
        
        private UOMSet _uOMSet;     // The UOMSet used for calculations


        // -------------- CurrentEquationCalc
        private EqnCalc _originalEquationCalc;  // CurrentEquationCalc from when it was last set by anything other than a rearrangement

        private EqnCalc _currentEquationCalc;
        public EqnCalc CurrentEquationCalc
        {
            get { return _currentEquationCalc; }
            set
            {
                SetCurrentEquationCalc(value);
                _originalEquationCalc = value;
            }
        }

        public string CurrentEqCalcDescription { get { return CurrentEquationCalc?.Description ?? ""; } }


        // -------------- EqCalcVariables
        private ObservableCollection<VmEqVar> _eqCalcVariables = new ObservableCollection<VmEqVar>();
        public ObservableCollection<VmEqVar> EqCalcVariables { get { return _eqCalcVariables; } }


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
                    switch (CurrentEquationCalc?.GetCalcStatus() ?? CalcStatus.Uknown)
                    {
                        case CalcStatus.Good:
                            return "OK";
                        case CalcStatus.Uknown:
                        case CalcStatus.Bad:
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
                                return "Enter values:";
                            }
                        default:
                            return "";
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }
        }


        // -------------- NumberFormat
        private string _numberFormat = "0.###";
        public string NumberFormat
        {
            get { return _numberFormat; }
            private set
            {
                if (SetProperty(ref _numberFormat, value))
                {
                    //OnMessagesChanged();
                }
            }
        }

        #endregion ------------ Fields and Properties ------------


        #region ------------ SetCurrentEquationCalc

        private void SetCurrentEquationCalc(EqnCalc eqnCalc)
        {
            if (_currentEquationCalc!=eqnCalc)
            {
                _currentEquationCalc = eqnCalc;

                SetEqCalcVariables();
                Recalculate();

                OnPropertyChanged("CurrentEquationCalc");
                OnPropertyChanged("CurrentEqCalcDescription");
            }
        }

        private void SetEqCalcVariables()
        {
            try
            {
                IList<SingleValue> eqVars = CurrentEquationCalc?.FindAllVariables() ?? (new List<SingleValue>());
                IList<SingleValue> eqConstants = CurrentEquationCalc?.FindAllConstants() ?? (new List<SingleValue>());

                // -------------
                SetMaxVarLength(eqVars, eqConstants);

                // -------------
                EqCalcVariables.Clear();

                for (int i = 0; i < eqVars.Count; i++)
                {
                    var item = eqVars[i];
                    item.ClearAllVarVals();

                    bool isCalculated = false;
                    if (i == 0) { isCalculated = true; }

                    EqCalcVariables.Add(new VmEqVar(this, item, isCalculated));
                }

                for (int i = 0; i < eqConstants.Count; i++)
                {
                    var item = eqConstants[i];
                    EqCalcVariables.Add(new VmEqVar(this, item, true));
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        #endregion ------------ SetCurrentEquationCalc


        #region ------------ Calculation

        public event EventHandler CalculationFinished;

        private bool _bCalculating = false;

        public void ClearResults()       // For use before calculation and when the display may be inconsistent
        {
            CurrentEquationCalc.SetCalcStatus(CalcStatus.Uknown);

            _errorMessage = "";

            foreach (var eqCalcVar in EqCalcVariables)
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
                if (EqCalcVariables.Count > 0)
                {
                    if (_bCalculating) return;
                    _bCalculating = true;

                    UpdateCalcInputs();
                    ClearResults();

                    CalcStatus calcStatus = CurrentEquationCalc.CalculateAll();

                    IList<FunCalcError> lowestErrors = new List<FunCalcError>();
                    IList<FunCalcError> allErrors = new List<FunCalcError>();
                    CurrentEquationCalc.GetErrors(ref lowestErrors, ref allErrors, NumberFormat: NumberFormat);
                    if (lowestErrors?.Count > 0)
                    {
                        _errorMessage = lowestErrors[0].Message;
                    }
                    OnMessagesChanged();

                    RefreshEqCalcVariables();

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
            foreach (var item in EqCalcVariables)
            {
                UpdateCalcInput(item);
            }
        }

        void UpdateCalcInput(VmEqVar wmEqVar)
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

        // -------------------------------------
        private void RefreshEqCalcVariables()
        {
            try
            {
                foreach (var item in EqCalcVariables)
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


        #region ------------ Called from VmEqVar

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

        #endregion ------------ Called from VmEqVar
        

        #region ------------ Called from the UI

        public void ChangeDependantVariable(Func<string/**/, bool/*result: Not used*/> displayAlertFunc)
        {
            IList<string> varNames = new List<string>();

            foreach (var item in EqCalcVariables)
            {
                varNames.Add(item.GetName());
            }
            _equationsUiService.SelectStringFromList(varNames,"Select the dependant variable","", 
                                            (s) => { ChangeDependantVariable(s, displayAlertFunc); return true; });
        }

        private void ChangeDependantVariable(string varRequired, Func<string/**/, bool/*result: Not used*/> displayAlertFunc)
        {
            Result<EqnCalc> r_result = EqnCalc.RearrangeEquation(_originalEquationCalc, varRequired, ContentManager);
            if (r_result.IsNotGood())
            {
                displayAlertFunc(r_result.Message);
            }
            else
            {
                SetCurrentEquationCalc(r_result.Value);
            }
        }

        // -----------------------

        public void UpdateVariableValue(int varIndex, object newValue)
        {
            try
            {
                EqCalcVariables[varIndex].SetVariableValue(newValue);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
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

                foreach (var item in EqCalcVariables)
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

        public class VmEqVar : ViewModelBase
        {
            #region ------------ Constructors and Life Cycle ------------

            VmEquations _parent;

            public VmEqVar(VmEquations parent, SingleValue singleValue, bool isCalculated, bool refreshNow=false)
            {
                try
                {
                    DefineCommands();

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

            #region ------------ Commands

            private void DefineCommands()
            {
                ChooseUom = new Command(execute: ChooseTheUom);
            }
            public ICommand ChooseUom { get; private set; }

            private async void ChooseTheUom()
            {
                try
                {
                    Tuple<bool, KnownUOM> tpl;
                    tpl = await _parent.EquationsUiService.ShowChooseUom(_parent.ContentManager, _singleValue?.ParamType);

                    if (!tpl.Item1)
                    {
                        _knownUOM = tpl.Item2;
                        RefreshThisData();
                        _parent.Recalculate();
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }

            #endregion ------------ Commands


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

            private double? _value;
            public double? Value
            {
                get
                {       // Return null rather than nan or infinity
                    if (_value==null)
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
                    // Don't set here because it messes up in UWP (gets called multiple times, later times being Nan)
                    // Instead we expect it to be updated explicitly via code calling "SetVariableValue".

                    // However, a Set operation needs to be available otherwise anything which thinks it has a 2-way 
                    // binding will fall over.
                }
            }

            public void SetVariableValue(object newValue)
            {
                try
                {
                    decimal? newDecVal = newValue as decimal?;
                    double? newVal = null;
                    try
                    {
                        newVal = (double?)newDecVal;
                    }
                    catch (Exception) { /* Ignore: I couldn't find a way to test the conversion without just trying it! */}

                    if (!_bRefreshing && !IsCalculated)
                    {
                        if (SetProperty(ref _value, newVal, propertyName: "Value"))
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
                            uomSymbol="";
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
                     !UOM.EqualDimensions(_knownUOM, calcUOM)) )
                {
                    _knownUOM = calcUOM;
                }
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

            #endregion ------------ Refresh
        }
    }

}
