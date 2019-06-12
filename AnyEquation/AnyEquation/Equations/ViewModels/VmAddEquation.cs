using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static AnyEquation.Equations.Model.ContentManager;

namespace AnyEquation.Equations.ViewModels
{
    public class VmAddEquation : ViewModelBase
    {
        #region ------------ Statics ------------

        #endregion ------------ Statics ------------

        #region ------------ Constructors and Life Cycle ------------

        private IEquationsUiService _equationsUiService;
        public IEquationsUiService EquationsUiService { get { return _equationsUiService; } }

        public VmAddEquation(IEquationsUiService equationsUiService, ContentManager contentManager)
        {
            _equationsUiService = equationsUiService;

            ContentManager = contentManager;
            SelectionCancelled = false;

            EquationMsg = DefaultEquationMsg;
            ParamTypeMsg = DefaultParamTypeMsg;

            RecordSyntaxNotes();

        }

        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Fields and Properties ------------

        public bool OkToFinish 
        {
            get {
                bool okToFinish = true;

                //TODO: Proper generic system for validation checks and recording validation errors.

                if (!ParamTypeMsg.Equals(DefaultParamTypeMsg))
                {
                    okToFinish = false;
                }
                return okToFinish;
            }
        }

    // ------------------------------

    private ContentManager _contentManager;
        public ContentManager ContentManager
        {
            get { return _contentManager; }
            set
            {
                if (SetProperty(ref _contentManager, value))
                {
                    // TODO
                    //RefreshParams();

                    // OnPropertyChanged("MathExpression");
                }
            }
        }

        // ------------------------------

        public bool SelectionCancelled { get; set; }

        private EqnCalc _equationCalc;
        public EqnCalc EquationCalc
        {
            get { return _equationCalc; }
            set
            {
                if (SetProperty(ref _equationCalc, value))
                {
                    SetEqCalcVarInfos();

                    OnPropertyChanged("EqDescription");
                }
            }
        }

        private string _EqDescription;
        public string EqDescription
        {
            get { return _EqDescription; }
            set
            {
                if (SetProperty(ref _EqDescription, value))
                {
                    //OnPropertyChanged("EqDescription");
                }
            }
        }


        private ObservableCollection<VmEqCalcVarInfo> _EqCalcVarInfos = new ObservableCollection<VmEqCalcVarInfo>();
        public ObservableCollection<VmEqCalcVarInfo> EqCalcVarInfos { get { return _EqCalcVarInfos; } set { _EqCalcVarInfos = value; } }


        // ------------------------------
        private int _maxVarLength;      // Max number of characters used for a variable name for the CurrentEquationCalc
        public int MaxVarLength
        {
            get { return _maxVarLength; }
            set
            {
                if (SetProperty(ref _maxVarLength, value))
                {
                }
            }
        }

        private int _maxParamLength;      // Max number of characters used for a Param for the CurrentEquationCalc
        public int MaxParamLength
        {
            get { return _maxParamLength; }
            set
            {
                if (SetProperty(ref _maxParamLength, value))
                {
                    MaxVarLengthChanged?.Invoke(this, null);
                }
            }
        }


        private const string DefaultEquationMsg = "...";   //"No equation yet";       //"Input equation";

        private string _EquationMsg;
        public string EquationMsg
        {
            get { return _EquationMsg; }
            set
            {
                if (SetProperty(ref _EquationMsg, value))
                {
                }
            }
        }

        private const string DefaultParamTypeMsg = "";      //"Describe variables and choose parameter types (optional)";

        public bool ParamTypeMsgExists { get { return !_ParamTypeMsg.Equals(DefaultParamTypeMsg); } }

        private string _ParamTypeMsg;
        public string ParamTypeMsg
        {
            get { return _ParamTypeMsg; }
            set
            {
                if (SetProperty(ref _ParamTypeMsg, value))
                {
                    OnPropertyChanged("ParamTypeMsgExists");
                }
            }
        }

        private string _UserEquationString;
        public string UserEquationString
        {
            get { return _UserEquationString; }
            set
            {
                if (SetProperty(ref _UserEquationString, value))
                {
                }
            }
        }

        public string EquationString
        {
            get {
                string sText = FunctionCalc.MathExpressionAsText(EquationCalc);
                return sText;
            }
        }

        public IList<VarInfo> GetVarInfoList()
        {
            IList<VarInfo> varInfos = new List<VarInfo>();

            foreach (var ei in EqCalcVarInfos)
            {
                if ((ei.Name!=null) && (ei.Name.Length>0))
                {
                    varInfos.Add(new VarInfo(ei.Name, ei.Description, ei.ParamType));
                }
            }

            return varInfos;
        }

        // ------------------------------
        private ObservableCollection<CSyntax> _syntaxes = new ObservableCollection<CSyntax>();
        public ObservableCollection<CSyntax> Syntaxes { get { return _syntaxes; } set { _syntaxes = value; } }


        #endregion ------------ Fields and Properties ------------


        #region ------------ Misc ------------

        private void RecordSyntaxNotes()
        {
            Syntaxes.Add(new CSyntax("=", " = ", "y = x", "Define the equation"));

            Syntaxes.Add(new CSyntax("+", " + ", "x + y", "Add"));
            Syntaxes.Add(new CSyntax("-", " - ", " -x, x - y", "Subtract or unary minus"));
            Syntaxes.Add(new CSyntax("*", " * ", "x * y", "Multiply"));
            Syntaxes.Add(new CSyntax("/", " / ", "x / y", "Divide"));
            Syntaxes.Add(new CSyntax("^", "^", "x^y", "Power"));

            Syntaxes.Add(new CSyntax("(", "(", "", "Left Bracket"));
            Syntaxes.Add(new CSyntax(")", ")", "", "Right Bracket"));

            Syntaxes.Add(new CSyntax("Number", "", "5, 5.12", "Any number"));
            Syntaxes.Add(new CSyntax("Variable", "", "xyz123", "Alpha-numeric, case sensitive, no spaces, cannot begin with a number"));

            Syntaxes.Add(new CSyntax("Square root", "Sqrt(", "Sqrt(x)", "Square root of x.  Same as x^0.5"));
            Syntaxes.Add(new CSyntax("Logarithm", "Ln(", "Ln(x)", "Natural log of x"));
            Syntaxes.Add(new CSyntax("PI", "PI", "PI", "Constant: PI: 3.14159265359"));

            // Add blank rows to demonstrate scrolling (TODO: Equations: Debug Test)
            for (int i = 0; i < 10; i++)
            {
                Syntaxes.Add(new CSyntax("", "", "", ""));
            }
        }

        public void RefreshEquationCalc(string eqnString)
        {
            UserEquationString = eqnString;

            if (eqnString?.Length > 0)
            {
                JongErrWarn errW = null;
                FunctionCalc funCalc = ContentManager.CreateFunctionCalcFromExpression(eqnString, null, null, out errW);

                EquationCalc = funCalc as EqnCalc;

                EquationMsg = errW?.ErrWarnString;
            }
            else
            {
                EquationCalc = null;
                EquationMsg = DefaultEquationMsg;
            }

            OnPropertyChanged("UserEquationString");
            OnPropertyChanged("EquationCalc");
            OnPropertyChanged("EquationMsg");

        }


        // ------------------------------
        private void SetMaxParamLength()
        {
            int maxParamLength = 0;

            foreach (var item in EqCalcVarInfos)
            {
                if (item?.ParamSymbol?.Length > maxParamLength)
                {
                    maxParamLength = item.ParamSymbol.Length;
                }
            }

            MaxParamLength = maxParamLength;

        }

        // ------------------------------

        private void SetEqCalcVarInfos()
        {
            if (EquationCalc==null)
            {
                return; //Might just be a temporary syntax error, so we don't want to lose all our variable definitions
            }

            IList<SingleValue> eqVars = EquationCalc?.FindAllVariables() ?? (new List<SingleValue>());

            // -------------
            int maxVarLen = 0;
            foreach (var item in eqVars)
            {
                if (item?.Name?.Length > maxVarLen)
                {
                    maxVarLen = item.Name.Length;
                }
            }
            MaxVarLength = maxVarLen;
            MaxVarLengthChanged?.Invoke(this, null);

            // -------------
            IList<VmEqCalcVarInfo> toRemove = new List<VmEqCalcVarInfo>();

            // ------------ Remove the hacks
            for (int i=EqCalcVarInfos.Count-1; i>=0; i--)
            {
                VmEqCalcVarInfo ei = EqCalcVarInfos[i];
                if (ei.Name == null)
                {
                    EqCalcVarInfos.Remove(ei);
                }
            }
            foreach (var ei in EqCalcVarInfos)
            {
                if (ei.Name==null)
                {
                    EqCalcVarInfos.Remove(ei);
                }
            }
            foreach (var ei in EqCalcVarInfos)
            {
                toRemove.Add(ei);
            }

            // ------------ Add new items and note those still needed
            for (int i = 0; i < eqVars.Count; i++)
            {
                var sv = eqVars[i];
                sv.ClearAllVarVals();

                bool bFound = false;
                foreach (var ei in toRemove)
                {
                    if (ei.Name.Equals(sv.Name))
                    {
                        bFound = true;
                        toRemove.Remove(ei);    //still needed
                        break;
                    }
                }

                if (!bFound)
                {
                    EqCalcVarInfos.Add(new VmEqCalcVarInfo(this, sv));
                }
            }

            // ------------ Remove any no longer needed
            foreach (var ei in toRemove)
            {
                EqCalcVarInfos.Remove(ei);
            }

        }

        private void RefreshEqCalcVarInfos()
        {
            foreach (var item in EqCalcVarInfos)
            {
                if (item != null)
                {
                    item.RefreshThisData();
                }
            }
        }

        public event EventHandler MaxVarLengthChanged;

        // ------------------------------

        public void CheckParamTypes()
        {
            // Assign AnonUOM to each variable based on the param type
            UpdateInputsDimensions();

            // Check the consistency of dimensions
            CalcStatus calcStatus = EquationCalc.CheckDimensionsAll();

            if (calcStatus == CalcStatus.Bad)
            {
                ParamTypeMsg = "Parameter types have inconsisent dimensions";
            }
            else
            {
                ParamTypeMsg = DefaultParamTypeMsg;
            }
        }

        private void UpdateInputsDimensions()
        {
            UOMSet uomSetSI = ContentManager.UOMSets["SI"];

            IList<SingleValue>  variables = EquationCalc.FindAllVariables();

            foreach (var ei in EqCalcVarInfos)
            {
                string vName = ei.Name;
                Dimensions vDims = ei?.ParamType?.Dimensions;

                foreach (var v in variables)
                {
                    if (v.Name.Equals(vName))
                    {
                        v.CalcQuantity.AnonUOM = new AnonUOM(vDims, uomSetSI);
                        break;
                    }
                }
            }
        }


        #endregion ------------ Misc ------------

        // -------------------- Private class

        public class CSyntax
        {
            public CSyntax(string item, string textToInsert, string syntax, string note)
            {
                Item = item;
                TextToInsert = textToInsert;
                Syntax = syntax;
                Note = note;
            }

            public string Item { get; set; }
            public string TextToInsert { get; set; }
            public string Syntax { get; set; }
            public string Note { get; set; }

        }

        // -------------------- Private class

        public class VmEqCalcVarInfo : ViewModelBase
        {

            #region ------------ Constructors and Life Cycle ------------

            VmAddEquation _parent;

            public VmEqCalcVarInfo(VmAddEquation parent, SingleValue singleValue)
            {
                _parent = parent;

                // ----------------------
                Name = singleValue?.Name;
                Description = singleValue?.Description;

                RefreshThisData();

                // -------------------
                ChooseParamType = new Command(
                    execute: async () => await ChooseAParamType());
            }

            public static IList<ParamType> RecentParamTypeSelections { get; set; } = new List<ParamType>();     // TODO: Persist these values

            private async Task<bool> ChooseAParamType()
            {
                try
                {
                    IDictionary<string, /*selected*/bool> groupExpansions = new Dictionary<string, /*selected*/bool>();
                    IList<GroupedString> groupedStrings = new List<GroupedString>();
                    bool multiSelect = false;
                    string title = "Select a ParamType";
                    string subTitle = "";

                    Array popularityValues = Enum.GetValues(typeof(Popularities));

                    int group = 0;

                    if (RecentParamTypeSelections.Count>0)
                    {
                        group++;
                        string groupName = $"{group:000}";
                        foreach (var pt in RecentParamTypeSelections)
                        {
                            groupedStrings.Add(new GroupedString()
                            {
                                Item = pt.Name,
                                Group = groupName,
                                GroupDescription = "_Recent",
                                IsSelected = false,
                                Data = pt,
                            });
                        }
                        groupExpansions.Add(groupName, true);
                    }

                    for (int i = popularityValues.Length; i>=0; i--)
                    {
                        int popularity;
                        if (i == popularityValues.Length)
                        {
                            popularity = (int)Popularities.Common + 1;      // Popularity values can be any integer, so we group together any > Popularities.Common
                        }
                        else
                        {
                            popularity = (int)popularityValues.GetValue(i);
                        }

                        string popularityDescription = ContentManager.DescribePopularity(popularity);
                        group++;
                        foreach (var item in _parent.ContentManager.ParamTypes)
                        {
                            ParamType pt = item.Value;
                            if ((pt.Popularity == popularity) || ((i == popularityValues.Length) && (pt.Popularity > (int)Popularities.Common)))
                            {
                                groupedStrings.Add(new GroupedString()
                                {
                                    Item = item.Value.Name,
                                    Group = $"{group:000}",
                                    GroupDescription = popularityDescription,
                                    IsSelected = false,
                                    Data = pt,
                                });
                            }
                        }

                    }


                    Tuple</*Cancelled?*/bool, IList<GroupedString>> tpl2 = 
                                await _parent.EquationsUiService.SelectStringsFromGroupedList(
                                                                    groupExpansions, groupedStrings, multiSelect, title, subTitle);

                    if (!tpl2.Item1)
                    {
                        IList<GroupedString> groupedStrings2 = tpl2.Item2;

                        // Find the first selected item
                        foreach (var item in groupedStrings2)
                        {
                            if (item.IsSelected)
                            {
                                ParamType = item.Data as ParamType;

                                RecentParamTypeSelections.Remove(ParamType);
                                RecentParamTypeSelections.Insert(0, ParamType);

                                RefreshThisData();
                                break;
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }


            #endregion ------------ Constructors and Life Cycle ------------

            #region ------------ Fields and Properties ------------

            public ICommand ChooseParamType { private set; get; }

            private bool _bRefreshing = false;

            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    if (SetProperty(ref _name, value))
                    {
                    }
                }
            }

            private string _description;
            public string Description
            {
                get { return _description; }
                set
                {
                    if (SetProperty(ref _description, value))
                    {
                    }
                }
            }

            private ParamType _paramType;
            public ParamType ParamType
            {
                get { return _paramType; }
                set
                {
                    if (SetProperty(ref _paramType, value))
                    {
                        _parent.CheckParamTypes();
                    }
                }
            }

            public string ParamSymbol
            {
                get
                {
                    if (ParamType != null)
                    {
                        return ParamType.Name;
                    }
                    else
                    {
                        return "param?";
                    }
                }
            }

            public bool IsValid { get { return ((Name==null)?false:(Name.Length>0)); } }
            


            #endregion ------------ Fields and Properties ------------

            #region ------------ Misc ------------

            public void RefreshThisData()
            {
                bool old_bRefreshing = _bRefreshing;
                _bRefreshing = true;

                // ---------------------

                OnPropertyChanged("Name");
                OnPropertyChanged("Description");
                OnPropertyChanged("ParamType");
                OnPropertyChanged("ParamSymbol");

                _bRefreshing = old_bRefreshing;
            }


            #endregion ------------ Misc ------------
        }

    }
}
