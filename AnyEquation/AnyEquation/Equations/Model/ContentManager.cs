using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Common;
using AnyEquation.Equations.Database;
using AnyEquation.Equations.EquationParser;
using AnyEquation.Equations.Model.Functions;
using AnyEquation.Equations.Model.Info;
using AnyEquation.Equations.Common;
using SQLite;
using System.IO;
using System.Reflection;
using static AnyEquation.Common.Utils;
using static AnyEquation.Equations.Common.CsvRow;

namespace AnyEquation.Equations.Model
{
    public class EquationsSystemProblems
    {
        public int NumParamTypeInconsistencies { get; set; }
        public int NumUOMInconsistencies { get; set; }
        public int NumEqLibInconsistencies { get; set; }

        public IDictionary<ParamType, ConflictSet<ParamType, TblParamTypes_Row>> ParamTypeConflictSets { get; set; }
        public IDictionary<KnownUOM, ConflictSet<KnownUOM, TblUOM_Row>> KnownUOMConflictSets { get; set; }

    }


    /// <summary>
    /// Singleton class to manage all the content used by the Application
    /// </summary>
    public partial class ContentManager : ICheckModelName
    {

        #region ------------ Constants and Statics ------------

        public enum Popularities
        { None = 0, LessCommon = 1, Common = 2 }

        // The content is loaded async, so check it is fully loaded before allowing access to any of these properties... See VmEquations.InitEquationSystem
        private static ContentManager _gContentManager = new ContentManager();
        public static ContentManager gContentManager { get { _gContentManager.ThrowIfNotContentLoaded(); return _gContentManager; } }

        public static string DescribePopularity(int popularity)
        {
            if (popularity == (int)ContentManager.Popularities.None) { return "Others"; }
            else if (popularity == (int)ContentManager.Popularities.LessCommon) { return "Less common"; }
            else if (popularity == (int)ContentManager.Popularities.Common) { return "Common"; }
            else if (popularity > (int)ContentManager.Popularities.Common) { return "Very Popular"; }
            else { return "Unknown"; }
        }

        public const string CorePackageName = "AnyEquation.Core";

        public static ParamType DimensionLessParamType { get; }
            = new ParamType(CorePackageName, "Dimensionless", "Dimensionless", "", new Dimensions(), (int)Popularities.Common);


        public static async Task<Result<EquationsSystemProblems>> LoadContent()
        {
            return await _gContentManager.LoadAllContent();
        }

        #endregion ------------ Constants and Statics ------------


        #region ------------ Constructor

        private ContentManager() { }        // Private constructor enforcing singleton

        #endregion ------------ Constructor


        #region ------------ Fields and Properties ------------

        public bool IsContentLoaded { get; private set; }

        private Dictionary<string, Function> _functions = new Dictionary<string, Function>();
        public Dictionary<string, Function> Functions { get { return _functions; } }

        private Dictionary<string, ParamType> _ParamTypes = new Dictionary<string, ParamType>();
        public Dictionary<string, ParamType> ParamTypes { get { return _ParamTypes; } }

        private Dictionary<string, KnownUOM> _knownUOMs = new Dictionary<string, KnownUOM>();
        public Dictionary<string, KnownUOM> KnownUOMs { get { return _knownUOMs; } }

        private Dictionary<string, UOMSet> _UOMSets = new Dictionary<string, UOMSet>();
        public Dictionary<string, UOMSet> UOMSets { get { return _UOMSets; } }

        private Dictionary<string, Constant> _Constants = new Dictionary<string, Constant>();
        public Dictionary<string, Constant> Constants { get { return _Constants; } }

        private Dictionary<string, EquationLibrary> _EquationLibraries = new Dictionary<string, EquationLibrary>();
        public Dictionary<string, EquationLibrary> EquationLibraries { get { return _EquationLibraries; } }

        // ---------------------
        private JongParser _jongParser = new JongParser();

        // ---------------------
        private AnyUOMSet _defaultUOMSet;
        public AnyUOMSet DefaultUOMSet { get { return _defaultUOMSet; } set { _defaultUOMSet = value; } }

        #endregion ------------ Fields and Properties ------------


        #region ------------ LoadContent ------------

        private void ThrowIfNotContentLoaded()
        {
            if (!IsContentLoaded) throw new UnspecifiedException("Accessing content before it is fully loaded");
        }

        private async Task<Result<EquationsSystemProblems>> LoadAllContent()
        {
            try
            {
                AddCoreContent();

                Result<EquationsSystemProblems> r_equationsSystemProblems = await ReadDataBases();

                CreateSortedUomDict(recreate:true);

                if (r_equationsSystemProblems.IsGood())
                {
                    IsContentLoaded = true;
                }

                return r_equationsSystemProblems;
            }
            catch (Exception ex)
            {
                return Result<EquationsSystemProblems>.Bad(ex);
            }
        }


        #endregion ------------ LoadContent ------------


        #region ------------ Libraries and conflicts ------------

        // ------------------------------------------

        private IDictionary<ParamType, ConflictSet<ParamType, TblParamTypes_Row>> _paramTypeConflictSets = new Dictionary<ParamType, ConflictSet<ParamType, TblParamTypes_Row>>();

        private IDictionary<KnownUOM, ConflictSet<KnownUOM, TblUOM_Row>> _knownUOMConflictSets = new Dictionary<KnownUOM, ConflictSet<KnownUOM, TblUOM_Row>>();

        private IDictionary<Constant, ConflictSet<Constant, TblConstant_Row>> _constantConflictSets = new Dictionary<Constant, ConflictSet<Constant, TblConstant_Row>>();

        #endregion ------------ Libraries and conflicts ------------


        #region ------------ Databases ------------

        private async Task<Result<EquationsSystemProblems>> ReadDataBases()
        {
            try
            {
                //Task.Delay(15000).Wait();        //For Debugging Task management


                // -------------- Read ParamType Database
                const string STR_tblParamTypes = "tblParamTypes";

                SqLiteContentDatabase paramTypeDatabase = await SqLiteContentDatabase
                                                                .LoadDatabase("AnyEquation_Core_ParamTypes.sqlite3");
                IList<TblParamTypes_Row> paramTypeRows = SqLiteContentTable<TblParamTypes_Row>
                                                                .ReadContentTable(paramTypeDatabase, STR_tblParamTypes);

                string paramTypePackageName = paramTypeDatabase.GetInfoVal(SqLiteContentDatabase.STR_PackageName);

                IList<ConflictInfo<TblParamTypes_Row>> paramTypeConflicts = new List<ConflictInfo<TblParamTypes_Row>>();

                int numParamTypeInconsistencies = 0;
                AddDatabaseItems<ParamType, TblParamTypes_Row>(paramTypePackageName,
                                paramTypeRows,
                                ParamTypes,
                                paramTypeConflicts,
                                _paramTypeConflictSets,
                                ParamType.TestConflict, (pName, tRow) => ParamType.NewContentItem(pName, tRow),
                                ref numParamTypeInconsistencies);

                // -------------- Copy from UOMSet_SI to KnownUOMs in this instance
                foreach (var item in UOMSet_SI.KnownUOMs)
                {
                    bool isFound = false;
                    foreach (var item2 in KnownUOMs)
                    {
                        if (item2.Equals(item))
                        {
                            isFound=true;
                            break;
                        }
                    }

                    if (!isFound)
                    {
                        if (KnownUOMs.ContainsKey(item.Name))
                        {
                            int iDbg = 0;
                        }
                        KnownUOMs.Add(item.Name, item);
                    }
                }


                // -------------- AddCoreConstants, now we have the units they need
                AddCoreConstants();

                // -------------- Read UOM Database
                int numUOMInconsistencies = 0;
                const string STR_tblUOM = "tblUOM";

                SqLiteContentDatabase uomDatabase = await SqLiteContentDatabase
                                                                .LoadDatabase("AnyEquation_Core_UOM.sqlite3");
                IList<TblUOM_Row> uomRows = SqLiteContentTable<TblUOM_Row>
                                                                .ReadContentTable(uomDatabase, STR_tblUOM);

                string uomPackageName = uomDatabase.GetInfoVal(SqLiteContentDatabase.STR_PackageName);
                IList<ConflictInfo<TblUOM_Row>> uomConflicts = new List<ConflictInfo<TblUOM_Row>>();

                IEnumerable<TblUOM_Row> uomSortedRows = uomRows.OrderBy(
                                                                    (uomRow) => (uomRow.IsBase ? 0 : 1))     // Do the base ones first so they can be found when creating the non-base
                                                                    .ToList();

                AddDatabaseItems<KnownUOM, TblUOM_Row>(uomPackageName,
                                uomSortedRows,
                                KnownUOMs,
                                uomConflicts,
                                _knownUOMConflictSets,
                                KnownUOM.TestConflict, (pName, tRow) => KnownUOM.NewContentItem(pName, tRow, KnownUOMs),
                                ref numUOMInconsistencies);

                // -------------- More definitions
                IList<ConflictInfo<TblConstant_Row>> constantConflicts = new List<ConflictInfo<TblConstant_Row>>();

                // TODO: Close the SqLite connections

                // -------------- Read Equation Libraries
                int numConstantInconsistencies = 0;
                int numEqLibInconsistencies = 0;

                numEqLibInconsistencies += ReadLibraryFromCsv("AnyEq_Info.csv", 
                    paramTypeConflicts, ref numParamTypeInconsistencies, uomConflicts, ref numUOMInconsistencies,
                    constantConflicts, ref numConstantInconsistencies);
                //numEqLibInconsistencies += ReadLibraryFromCsv("Equations - LargeDummy__Info.csv",
                //    paramTypeConflicts, ref numParamTypeInconsistencies, uomConflicts, ref numUOMInconsistencies,
                //    constantConflicts, ref numConstantInconsistencies);
                //numEqLibInconsistencies += ReadEquationLibraryFromCsv("Equations - Geick__Info.csv");

                numEqLibInconsistencies += await AddEquationLibrary("EqualtionLibrary_Gieck.sqlite3");

                // ---------------------
                IList<EquationLibrary> testEquationLibraries = TestEquations.CreateTestEquationLibraries(this);
                foreach (var item in testEquationLibraries)
                {
                    _EquationLibraries.Add(item.Name, item);
                }

                // -------------- Return any problems for later reporting
                EquationsSystemProblems equationsSystemProblems = new EquationsSystemProblems()
                {
                    NumParamTypeInconsistencies = numParamTypeInconsistencies,
                    NumUOMInconsistencies = numUOMInconsistencies,
                    NumEqLibInconsistencies = numEqLibInconsistencies,

                    ParamTypeConflictSets = _paramTypeConflictSets,
                    KnownUOMConflictSets = _knownUOMConflictSets,
                };

                // -------------- Finished
                return Result<EquationsSystemProblems>.Good(equationsSystemProblems);
            }
            catch (Exception ex)
            {
                return Result<EquationsSystemProblems>.Bad(ex);
            }

        }

        /// <returns>Number of inconsistencies</returns>
        private async Task<int> AddEquationLibrary(string sqLiteDbFilename)
        {
            const string STR_tblSectionNode = "tblSectionNode";
            const string STR_tblEquation = "tblEquation";
            const string STR_tblVariable = "tblVariable";

            SqLiteContentDatabase database = await SqLiteContentDatabase
                                                            .LoadDatabase(sqLiteDbFilename);
            IList<TblEqLibSectionNode_Row> sectionNodeRows = SqLiteContentTable<TblEqLibSectionNode_Row>
                                                            .ReadContentTable(database, STR_tblSectionNode);
            IList<TblEqLibEquation_Row> equationRows = SqLiteContentTable<TblEqLibEquation_Row>
                                                            .ReadContentTable(database, STR_tblEquation);
            IList<TblEqLibVariable_Row> variableRows = SqLiteContentTable<TblEqLibVariable_Row>
                                                            .ReadContentTable(database, STR_tblVariable);

            EquationLibrary equationLibrary = EquationLibrary.CreateLibraryFromDatabase(this,
                                                                    database, sectionNodeRows, equationRows, variableRows);

            _EquationLibraries.Add(equationLibrary.Name, equationLibrary);

            // TODO: Error handling, reporting conflicts or errors
            int numInconsistencies = 0;

            return numInconsistencies;
        }

        private int ReadLibraryFromCsv(string csvFilename, 
                    IList<ConflictInfo<TblParamTypes_Row>> paramTypeConflicts, ref int numParamTypeInconsistencies,
                    IList<ConflictInfo<TblUOM_Row>> uomConflicts, ref int numUOMInconsistencies,
                    IList<ConflictInfo<TblConstant_Row>> constantConflicts, ref int numConstantInconsistencies)
        {
            // ------------ Read library info
            Assembly assembly = this.GetType().GetTypeInfo().Assembly;
            string resourceNamespace = "AnyEquation.Equations.Databases";
            
            CsvLibraryImport info = ReadLibraryInfoFromCsv(assembly, resourceNamespace, csvFilename);

            // ------------ Read References  // TODO

            // ------------ Read UOM
            ReadCsvUomFiles(assembly, resourceNamespace, info, uomConflicts, ref numUOMInconsistencies);

            // ------------ Read ParamTypes
            ReadCsvParamTypeFiles(assembly, resourceNamespace, info, paramTypeConflicts, ref numParamTypeInconsistencies);

            // ------------ Read Constants
            ReadCsvConstantFiles(assembly, resourceNamespace, info, constantConflicts, ref numConstantInconsistencies);

            // ------------ Read Equation data
            ReadCsvEquationFiles(assembly, resourceNamespace, info);

            // TODO: Error handling, reporting conflicts or errors
            int numInconsistencies = 0;

            return numInconsistencies;
        }


        private CsvLibraryImport ReadLibraryInfoFromCsv(Assembly assembly, string resourceNamespace, string csvFilename)
        {
            CsvLibraryImport info = new CsvLibraryImport();

            using (Stream stream = assembly.GetManifestResourceStream($"{resourceNamespace}.{csvFilename}"))
            using (CsvFileReader reader = new CsvFileReader(stream))
            {
                int iRow = 0;
                CsvRow c = new CsvRow();

                IList<string> fileStems = new List<string>
                    { "ReferencesFile", "ParamTypesFile", "UomFile", "ConstantsFile", "EquationFile" };
                IList<IList<string>> fileLists = new List<IList<string>>
                    { info.ReferencesFiles, info.ParamTypesFiles, info.UomFiles, info.ConstantsFiles, info.EquationFiles };
                IList<int> stemLengths = new List<int>();
                foreach (var stem in fileStems) { stemLengths.Add(stem.Length); }

                while (reader.ReadRow(c))
                {
                    string valKey = (c.Count > 0 ? c[0] : "");
                    string valVal = (c.Count > 1 ? c[1] : "");

                    if (iRow == 0) { /*Header row*/ }
                    else if (string.IsNullOrEmpty(valKey)) { }

                    else if (valKey.Equals("CsvFormatNumber")) { info.CsvFormatNumber = valVal; }
                    else if (valKey.Equals("PackageName")) { info.PackageName = valVal; }
                    else if (valKey.Equals("LibraryName")) { info.LibraryName = valVal; }
                    else if (valKey.Equals("LibraryDescription")) { info.LibraryDescription = valVal; }
                    else if (valKey.Equals("EquationLibraryName")) { info.EquationLibraryName = valVal; }
                    else if (valKey.Equals("EquationLibraryDescription")) { info.EquationLibraryDescription = valVal; }
                    else if (valKey.Equals("LibraryUrl")) { info.LibraryUrl = valVal; }

                    else
                    {
                        bool wasFound = false;
                        for (int i = 0; i < fileStems.Count; i++)
                        {
                            string stem = fileStems[i];

                            if ((valKey.Length > stemLengths[i]) && valKey.Substring(0, stemLengths[i]).Equals(stem))
                            {
                                if (!string.IsNullOrEmpty(valVal))
                                {
                                    fileLists[i].Add(valVal);
                                }
                                wasFound = true; break;
                            }
                        }
                        if (!wasFound) { throw new UnspecifiedException($"Unrecognised key '{valKey}' in InfoTable for library {csvFilename}"); }
                    }

                    iRow++;
                }
            }

            return info;
        }

        // --------------------------------------------

        private void ReadCsvUomFiles(Assembly assembly, string resourceNamespace, CsvLibraryImport info,
                        IList<ConflictInfo<TblUOM_Row>> uomConflicts, ref int numUOMInconsistencies)
        {
            IList<TblUOM_Row> csvUomImports = new List<TblUOM_Row>();

            foreach (var UomFileName in info.UomFiles)
            {
                CsvUomLibraryMapping mappings = null;

                using (Stream stream = assembly.GetManifestResourceStream($"{resourceNamespace}.{UomFileName}"))
                using (CsvFileReader reader = new CsvFileReader(stream))
                {
                    int iRow = 0;
                    CsvRow c = new CsvRow();
                    while (reader.ReadRow(c))
                    {
                        if (iRow == 0)
                        {
                            mappings = ReadUomLibraryMappingFromCsv(assembly, resourceNamespace, c);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(c[mappings.NameColumn]))
                            {
                                TblUOM_Row csvUomImport = new TblUOM_Row()
                                {
                                    //Index = CsvIntVal(c, mappings.IndexColumn),
                                    Status = CsvIntVal(c, mappings.StatusColumn),
                                    Name = CsvStringVal(c, mappings.NameColumn),
                                    Symbol = CsvStringVal(c, mappings.SymbolColumn),
                                    Popularity = CsvIntVal(c, mappings.PopularityColumn),
                                    Description = CsvStringVal(c, mappings.DescriptionColumn),
                                    BaseUom = CsvStringVal(c, mappings.BaseUomColumn),
                                    Factor_A = CsvDoubleVal(c, mappings.Factor_AColumn),
                                    Factor_B = CsvDoubleVal(c, mappings.Factor_BColumn),
                                    Factor_C = CsvDoubleVal(c, mappings.Factor_CColumn),
                                    Factor_D = CsvDoubleVal(c, mappings.Factor_DColumn),
                                    Dim_Mass = CsvIntVal(c, mappings.Dim_MassColumn),
                                    Dim_Length = CsvIntVal(c, mappings.Dim_LengthColumn),
                                    Dim_Time = CsvIntVal(c, mappings.Dim_TimeColumn),
                                    Dim_ElectricCurrent = CsvIntVal(c, mappings.Dim_ElectricCurrentColumn),
                                    Dim_Temperature = CsvIntVal(c, mappings.Dim_TemperatureColumn),
                                    Dim_QuantityOfSubstance = CsvIntVal(c, mappings.Dim_QuantityOfSubstanceColumn),
                                    Dim_Luminosity = CsvIntVal(c, mappings.Dim_LuminosityColumn),
                                    Dim_PlaneAngle = CsvIntVal(c, mappings.Dim_PlaneAngleColumn),
                                    Dim_SolidAngle = CsvIntVal(c, mappings.Dim_SolidAngleColumn),
                                    Dim_Currency = CsvIntVal(c, mappings.Dim_CurrencyColumn),
                                };
                                csvUomImport.DoneImport();

                                csvUomImports.Add(csvUomImport);
                            }
                        }

                        iRow++;
                    }
                }
            }

            // ------------ Add the items read in

            AddDatabaseItems<KnownUOM, TblUOM_Row>(info.PackageName,
                            csvUomImports,
                            KnownUOMs,
                            uomConflicts,
                            _knownUOMConflictSets,
                            KnownUOM.TestConflict, (pName, tRow) => KnownUOM.NewContentItem(pName, tRow, KnownUOMs),
                            ref numUOMInconsistencies);
        }

        private CsvUomLibraryMapping ReadUomLibraryMappingFromCsv(Assembly assembly, string resourceNamespace, IList<string> headings)
        {
            CsvUomLibraryMapping mapping = new CsvUomLibraryMapping();

            for (int i = 0; i < headings.Count; i++)
            {
                string hdg = headings[i];

                if (string.IsNullOrEmpty(hdg)) { }

                else if (hdg.Equals("Index")) { /*Ignore*/ }
                else if (hdg.Equals("Status")) { mapping.StatusColumn = i; }
                else if (hdg.Equals("Name")) { mapping.NameColumn = i; }
                else if (hdg.Equals("Symbol")) { mapping.SymbolColumn = i; }
                else if (hdg.Equals("Popularity")) { mapping.PopularityColumn = i; }
                else if (hdg.Equals("Description")) { mapping.DescriptionColumn = i; }
                else if (hdg.Equals("BaseUom")) { mapping.BaseUomColumn = i; }
                else if (hdg.Equals("Factor_A")) { mapping.Factor_AColumn = i; }
                else if (hdg.Equals("Factor_B")) { mapping.Factor_BColumn = i; }
                else if (hdg.Equals("Factor_C")) { mapping.Factor_CColumn = i; }
                else if (hdg.Equals("Factor_D")) { mapping.Factor_DColumn = i; }
                else if (hdg.Equals("Dim_Mass")) { mapping.Dim_MassColumn = i; }
                else if (hdg.Equals("Dim_Length")) { mapping.Dim_LengthColumn = i; }
                else if (hdg.Equals("Dim_Time")) { mapping.Dim_TimeColumn = i; }
                else if (hdg.Equals("Dim_ElectricCurrent")) { mapping.Dim_ElectricCurrentColumn = i; }
                else if (hdg.Equals("Dim_Temperature")) { mapping.Dim_TemperatureColumn = i; }
                else if (hdg.Equals("Dim_QuantityOfSubstance")) { mapping.Dim_QuantityOfSubstanceColumn = i; }
                else if (hdg.Equals("Dim_Luminosity")) { mapping.Dim_LuminosityColumn = i; }
                else if (hdg.Equals("Dim_PlaneAngle")) { mapping.Dim_PlaneAngleColumn = i; }
                else if (hdg.Equals("Dim_SolidAngle")) { mapping.Dim_SolidAngleColumn = i; }
                else if (hdg.Equals("Dim_Currency")) { mapping.Dim_CurrencyColumn = i; }

                else
                {
                    throw new UnspecifiedException($"Unrecognised column '{hdg}' in csv");
                }
            }

            return mapping;
        }

        // --------------------------------------------

        private void ReadCsvParamTypeFiles(Assembly assembly, string resourceNamespace, CsvLibraryImport info,
                        IList<ConflictInfo<TblParamTypes_Row>> paramTypeConflicts, ref int numParamTypeInconsistencies)
        {
            IList<TblParamTypes_Row> csvParamTypeImports = new List<TblParamTypes_Row>();

            foreach (var ParamTypeFileName in info.ParamTypesFiles)
            {
                CsvParamTypeLibraryMapping mappings = null;

                using (Stream stream = assembly.GetManifestResourceStream($"{resourceNamespace}.{ParamTypeFileName}"))
                using (CsvFileReader reader = new CsvFileReader(stream))
                {
                    int iRow = 0;
                    CsvRow c = new CsvRow();
                    while (reader.ReadRow(c))
                    {
                        if (iRow == 0)
                        {
                            mappings = ReadParamTypeLibraryMappingFromCsv(assembly, resourceNamespace, c);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(c[mappings.NameColumn]))
                            {
                                TblParamTypes_Row csvParamTypeImport = new TblParamTypes_Row()
                                {
                                    Status = CsvIntVal(c, mappings.StatusColumn),
                                    Name = CsvStringVal(c, mappings.NameColumn),
                                    Popularity = CsvIntVal(c, mappings.PopularityColumn),
                                    Group = CsvStringVal(c, mappings.GroupColumn),
                                    Description = CsvStringVal(c, mappings.DescriptionColumn),
                                    BaseUom = CsvStringVal(c, mappings.BaseUOMColumn),
                                    ParsedUOM = CsvStringVal(c, mappings.ParsedUOMColumn),
                                    Dim_Mass = CsvIntVal(c, mappings.Dim_MassColumn),
                                    Dim_Length = CsvIntVal(c, mappings.Dim_LengthColumn),
                                    Dim_Time = CsvIntVal(c, mappings.Dim_TimeColumn),
                                    Dim_ElectricCurrent = CsvIntVal(c, mappings.Dim_ElectricCurrentColumn),
                                    Dim_Temperature = CsvIntVal(c, mappings.Dim_TemperatureColumn),
                                    Dim_QuantityOfSubstance = CsvIntVal(c, mappings.Dim_QuantityOfSubstanceColumn),
                                    Dim_Luminosity = CsvIntVal(c, mappings.Dim_LuminosityColumn),
                                    Dim_PlaneAngle = CsvIntVal(c, mappings.Dim_PlaneAngleColumn),
                                    Dim_SolidAngle = CsvIntVal(c, mappings.Dim_SolidAngleColumn),
                                    Dim_Currency = CsvIntVal(c, mappings.Dim_CurrencyColumn),
                                };
                                csvParamTypeImport.DoneImport();

                                csvParamTypeImports.Add(csvParamTypeImport);
                            }
                        }

                        iRow++;
                    }
                }
            }

            // ------------ Add the items read in

            AddDatabaseItems<ParamType, TblParamTypes_Row>(info.PackageName,
                            csvParamTypeImports,
                            ParamTypes,
                            paramTypeConflicts,
                            _paramTypeConflictSets,
                            ParamType.TestConflict, (pName, tRow) => ParamType.NewContentItem(pName, tRow),
                            ref numParamTypeInconsistencies);
        }

        private CsvParamTypeLibraryMapping ReadParamTypeLibraryMappingFromCsv(Assembly assembly, string resourceNamespace, IList<string> headings)
        {
            CsvParamTypeLibraryMapping mapping = new CsvParamTypeLibraryMapping();

            for (int i = 0; i < headings.Count; i++)
            {
                string hdg = headings[i];

                if (string.IsNullOrEmpty(hdg)) { }

                else if (hdg.Equals("Index")) { /*Ignore*/ }
                else if (hdg.Equals("Status")) { mapping.StatusColumn = i; }
                else if (hdg.Equals("Name")) { mapping.NameColumn = i; }
                else if (hdg.Equals("Popularity")) { mapping.PopularityColumn = i; }
                else if (hdg.Equals("Group")) { mapping.GroupColumn = i; }
                else if (hdg.Equals("Description")) { mapping.DescriptionColumn = i; }
                else if (hdg.Equals("BaseUOM")) { mapping.BaseUOMColumn = i; }
                else if (hdg.Equals("ParsedUOM")) { mapping.ParsedUOMColumn = i; }
                else if (hdg.Equals("Dim_Mass")) { mapping.Dim_MassColumn = i; }
                else if (hdg.Equals("Dim_Length")) { mapping.Dim_LengthColumn = i; }
                else if (hdg.Equals("Dim_Time")) { mapping.Dim_TimeColumn = i; }
                else if (hdg.Equals("Dim_ElectricCurrent")) { mapping.Dim_ElectricCurrentColumn = i; }
                else if (hdg.Equals("Dim_Temperature")) { mapping.Dim_TemperatureColumn = i; }
                else if (hdg.Equals("Dim_QuantityOfSubstance")) { mapping.Dim_QuantityOfSubstanceColumn = i; }
                else if (hdg.Equals("Dim_Luminosity")) { mapping.Dim_LuminosityColumn = i; }
                else if (hdg.Equals("Dim_PlaneAngle")) { mapping.Dim_PlaneAngleColumn = i; }
                else if (hdg.Equals("Dim_SolidAngle")) { mapping.Dim_SolidAngleColumn = i; }
                else if (hdg.Equals("Dim_Currency")) { mapping.Dim_CurrencyColumn = i; }

                else
                {
                    throw new UnspecifiedException($"Unrecognised column '{hdg}' in csv");
                }
            }

            return mapping;
        }

        // --------------------------------------------

        private void ReadCsvConstantFiles(Assembly assembly, string resourceNamespace, CsvLibraryImport info,
                        IList<ConflictInfo<TblConstant_Row>> constantConflicts, ref int numConstantInconsistencies)
        {
            IList<TblConstant_Row> csvConstantImports = new List<TblConstant_Row>();

            foreach (var ConstantFileName in info.ConstantsFiles)
            {
                CsvConstantLibraryMapping mappings = null;

                using (Stream stream = assembly.GetManifestResourceStream($"{resourceNamespace}.{ConstantFileName}"))
                using (CsvFileReader reader = new CsvFileReader(stream))
                {
                    int iRow = 0;
                    CsvRow c = new CsvRow();
                    while (reader.ReadRow(c))
                    {
                        if (iRow == 0)
                        {
                            mappings = ReadConstantLibraryMappingFromCsv(assembly, resourceNamespace, c);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(c[mappings.NameColumn]))
                            {
                                TblConstant_Row csvConstantImport = new TblConstant_Row()
                                {
                                    //Index = CsvIntVal(c, mappings.IndexColumn),
                                    Status = CsvIntVal(c, mappings.StatusColumn),
                                    Name = CsvStringVal(c, mappings.NameColumn),
                                    Popularity = CsvIntVal(c, mappings.PopularityColumn),
                                    ParamType = CsvStringVal(c, mappings.ParamTypeColumn),
                                    Description = CsvStringVal(c, mappings.DescriptionColumn),
                                    UnitSet = CsvStringVal(c, mappings.UnitSetColumn),
                                    Value = CsvDoubleVal(c, mappings.ValueColumn),
                                };
                                csvConstantImport.DoneImport();

                                csvConstantImports.Add(csvConstantImport);
                            }
                        }

                        iRow++;
                    }
                }
            }

            // ------------ Add the items read in

            AddDatabaseItems<Constant, TblConstant_Row>(info.PackageName,
                            csvConstantImports,
                            Constants,
                            constantConflicts,
                            _constantConflictSets,
                            Constant.TestConflict, (pName, tRow) => Constant.NewContentItem(pName, tRow, 
                                                ParamTypes.Values.ToList(), UOMSets.Values.ToList()),
                            ref numConstantInconsistencies);
        }

        private CsvConstantLibraryMapping ReadConstantLibraryMappingFromCsv(Assembly assembly, string resourceNamespace, IList<string> headings)
        {
            CsvConstantLibraryMapping mapping = new CsvConstantLibraryMapping();

            for (int i = 0; i < headings.Count; i++)
            {
                string hdg = headings[i];

                if (string.IsNullOrEmpty(hdg)) { }

                else if (hdg.Equals("Index")) { /*Ignore*/ }
                else if (hdg.Equals("Status")) { mapping.StatusColumn = i; }
                else if (hdg.Equals("Name")) { mapping.NameColumn = i; }
                else if (hdg.Equals("Popularity")) { mapping.PopularityColumn = i; }
                else if (hdg.Equals("ParamType")) { mapping.ParamTypeColumn = i; }
                else if (hdg.Equals("Description")) { mapping.DescriptionColumn = i; }
                else if (hdg.Equals("UnitSet")) { mapping.UnitSetColumn = i; }
                else if (hdg.Equals("Value")) { mapping.ValueColumn = i; }

                else
                {
                    throw new UnspecifiedException($"Unrecognised column '{hdg}' in csv");
                }
            }

            return mapping;
        }

        // --------------------------------------------

        private void ReadCsvEquationFiles(Assembly assembly, string resourceNamespace, CsvLibraryImport info)
        {
            IList<CsvEquationImport> csvEquationImports = new List<CsvEquationImport>();

            foreach (var equationFileName in info.EquationFiles)
            {
                CsvEquationLibraryMapping mappings = null;

                using (Stream stream = assembly.GetManifestResourceStream($"{resourceNamespace}.{equationFileName}"))
                using (CsvFileReader reader = new CsvFileReader(stream))
                {
                    int iRow = 0;
                    CsvRow c = new CsvRow();
                    while (reader.ReadRow(c))
                    {
                        if (iRow == 0)
                        {
                            mappings = ReadEquationLibraryMappingFromCsv(assembly, resourceNamespace, c);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(c[mappings.EquationAsTextColumn]))
                            {
                                CsvEquationImport csvEquationImport = new CsvEquationImport()
                                {
                                    Source = CsvIntVal(c, mappings.SourceColumn),
                                    Status = CsvIntVal(c, mappings.StatusColumn),
                                    SectionNode = CsvStringVal(c, mappings.SectionNodeColumn),
                                    EquationName = CsvStringVal(c, mappings.EquationNameColumn),
                                    EquationRef = CsvStringVal(c, mappings.EquationRefColumn),
                                    Description = CsvStringVal(c, mappings.DescriptionColumn),
                                    EquationAsText = CsvStringVal(c, mappings.EquationAsTextColumn),
                                };

                                csvEquationImport.Variables.Add(c[mappings.ResultVariableColumn]);

                                foreach (int iCol in mappings.InputVariableColumns)
                                {
                                    string sVar = (c.Count > iCol ? c[iCol] : "");
                                    if (!string.IsNullOrEmpty(sVar))
                                    {
                                        csvEquationImport.Variables.Add(sVar);
                                    }
                                }

                                csvEquationImport.TestVariables.Add(c[mappings.TestCalulationColumn]);

                                foreach (int iCol in mappings.TestInputColumns)
                                {
                                    string sVar = (c.Count > iCol ? c[iCol] : "");
                                    if (!string.IsNullOrEmpty(sVar))
                                    {
                                        csvEquationImport.TestVariables.Add(sVar);
                                    }
                                }

                                csvEquationImports.Add(csvEquationImport);
                            }
                        }

                        iRow++;
                    }
                }
            }

            // ------------ Create the library
            EquationLibrary equationLibrary = EquationLibrary.CreateLibraryFromCsv(this, info.EquationLibraryName, info.EquationLibraryDescription, csvEquationImports);

            _EquationLibraries.Add(equationLibrary.Name, equationLibrary);
        }


        private CsvEquationLibraryMapping ReadEquationLibraryMappingFromCsv(Assembly assembly, string resourceNamespace, IList<string> headings)
        {
            CsvEquationLibraryMapping mapping = new CsvEquationLibraryMapping();

            for (int i = 0; i < headings.Count; i++)
            {
                string hdg = headings[i];

                if (string.IsNullOrEmpty(hdg)) { }

                else if (hdg.Equals("Source")) { mapping.SourceColumn = i; }
                else if (hdg.Equals("Status")) { mapping.StatusColumn = i; }
                else if (hdg.Equals("SectionNode")) { mapping.SectionNodeColumn = i; }
                else if (hdg.Equals("EquationName")) { mapping.EquationNameColumn = i; }
                else if (hdg.Equals("EquationRef")) { mapping.EquationRefColumn = i; }
                else if (hdg.Equals("Description")) { mapping.DescriptionColumn = i; }
                else if (hdg.Equals("EquationAsText")) { mapping.EquationAsTextColumn = i; }

                else if (hdg.Equals("ResultVariable")) { mapping.ResultVariableColumn = i; }
                else if ((hdg.Length > 13) && hdg.Substring(0, 13).Equals("InputVariable"))
                {
                    mapping.InputVariableColumns.Add(i);
                }

                else if (hdg.Equals("TestCalulation")) { mapping.TestCalulationColumn = i; }
                else if ((hdg.Length > 9) && hdg.Substring(0, 9).Equals("TestInput"))
                {
                    mapping.TestInputColumns.Add(i);
                }

                else
                {
                    throw new UnspecifiedException($"Unrecognised column '{hdg}' in csv");
                }
            }

            return mapping;
        }

        // --------------------------------------------

        void AddDatabaseItems<T_Type, TableRow>(string newPackageName,
                        IEnumerable<TableRow> sortedRows,
                        Dictionary<string, T_Type> MainItems,
                        IList<ConflictInfo<TableRow>> newConflicts,
                        IDictionary<T_Type, ConflictSet<T_Type, TableRow>> conflictSets,
                        Func<string /*packageName*/, TableRow /*t1*/, T_Type /*t2*/, ConflictInfo<TableRow>> testConflict,
                        Func<string /*packageName*/, TableRow /*t1*/, T_Type> newContentItem,
                        ref int numInconsistencies)

                        where T_Type : IContent
                        where TableRow : ImportedRow, new()
        {
            foreach (TableRow newItem in sortedRows)
            {
                if (newItem.OkToUse())
                {
                    ConflictInfo<TableRow> conflictInfo =
                            ConflictSet<T_Type, TableRow>.AddIfNoConflict(newPackageName,
                                        MainItems, newItem, conflictSets,
                                        testConflict, newContentItem);

                    if (conflictInfo != null)
                    {
                        if (conflictInfo.ConflictType == ConflictType.InConsistent)
                        {
                            numInconsistencies++;
                        }

                        // It has already been added to the conflict list, but we also record it for reporting or management in the calling routine
                        newConflicts.Add(conflictInfo);
                    }
                }
            }
        }


        #endregion ------------ Databases ------------


        #region ------------ Implementation of ICheckModelName

        public bool CheckModelName(String modelName, String libPackageName)
        {
            // TODO: Use or remove libPackageName

            foreach (var item in _functions)
            {
                if (item.Value.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase) ||
                    item.Value.AsciiSymbol.Equals(modelName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion ------------ Implementation of ICheckModelName


        #region ------------ Adding Content

        private void AddCoreContent()
        {
            AddCoreFunctions();

            AddCoreUOMSets();

            AddCoreParamTypes();

            _defaultUOMSet = new AnyUOMSet("DefaultUOMSet");
            _defaultUOMSet.ParentUomSet = UOMSet_SI;

            //AddCoreKnownUOMs();
        }

        // ------------------------------
        private void AddCoreFunctions()
        {
            AddCoreFunction(new FnEquals());
            AddCoreFunction(new FnAdd());
            AddCoreFunction(new FnSubtract());
            AddCoreFunction(new FnUnaryMinus());
            AddCoreFunction(new FnMultiply());
            AddCoreFunction(new FnDivide());

            AddCoreFunction(new FnPower());
            AddCoreFunction(new FnSquareRoot());

            AddCoreFunction(new FnSin());
            AddCoreFunction(new FnASin());
            AddCoreFunction(new FnCos());
            AddCoreFunction(new FnACos());
            AddCoreFunction(new FnTan());
            AddCoreFunction(new FnATan());

            AddCoreFunction(new FnLog());
            AddCoreFunction(new FnExp());
        }

        private void AddCoreFunction(Function function)
        {
            _functions.Add(function.Name, function);
        }

        // ------------------------------

        private void AddCoreParamTypes()
        {
            _ParamTypes.Add("dimensionless", DimensionLessParamType);
            AddKnownUOM("Dimensionless", UOMSet_SI,
                                new Dimensions(),
                                Name: "dimensionless", Description: "Dimensionless", Symbol: "Dimensionless",
                                IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            AddParamType("mass", Popularities.Common, "Mass", "kg", mass: 1);

            AddParamType("length", Popularities.Common, "Length", "m", length: 1);

            AddParamType("time", Popularities.Common, "Time", "s", time: 1);
            AddParamType("temperature", Popularities.Common, "Temperature", "K", temperature: 1);
            AddParamType("currency", Popularities.Common, "Currency", "$", currency: 1);
            AddParamType("planeAngle", Popularities.Common, "PlaneAngle", "rad", planeAngle: 1);
            AddParamType("electric current", Popularities.Common, "Electric current", "A", electricCurrent: 1);     //ampere

            AddParamType("quantityOfSubstance", Popularities.Common, "Quantity Of Substance", "mol", quantityOfSubstance: 1);
            AddParamType("luminosity", Popularities.Common, "Luminosity", "cd", luminosity: 1);     //candela

            // TODO: AddParamType("solidAngle", Popularities.Common, "Solid Angle", "?", solidAngle: 1);

            // TODO: Integer param type

            //AddParamType("gravitational constant", Popularities.Common, "gravitational constant", "gravitational constant",length:3, mass: -1, time:-2 );     // m^3/(kg.s^2)


        }

        // ------------------------------

        public const string STR_SI = "SI";
        public const string STR_British = "British";

        public static UOMSet UOMSet_SI { get; set; }

        private void AddCoreUOMSets()
        {
            _UOMSets.Add(STR_SI, new UOMSet(STR_SI));       // Core units are added below
            _UOMSets.Add(STR_British, new UOMSet(STR_British));     // TODO: Needs core units to be added to this, otherwise none can be derived

            UOMSet_SI = _UOMSets[STR_SI];
        }

        // ------------------------------
        private void AddCoreKnownUOMs()
        {
            return;

            KnownUOM baseDimLess = AddKnownUOM("Dimensionless", UOMSet_SI,
                        new Dimensions(),
                        Name: "Dimensionless", Description: "Dimensionless", Symbol: "Dimensionless",
                        IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            KnownUOM baseMass = AddKnownUOM("kilogram", UOMSet_SI,
                        new Dimensions(mass: 1),
                        Name: "kg", Description: "kilogram", Symbol: "kg",
                        IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            KnownUOM baseLength = AddKnownUOM("metre", UOMSet_SI,
                    new Dimensions(length: 1),
                    Name: "metre", Description: "metre", Symbol: "m",
                    IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            KnownUOM baseTime = AddKnownUOM("second", UOMSet_SI,
                    new Dimensions(time: 1),
                    Name: "second", Description: "second", Symbol: "s",
                    IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            KnownUOM baseTemperature = AddKnownUOM("centigrade", UOMSet_SI,
                    new Dimensions(temperature: 1),
                    Name: "centigrade", Description: "Degrees Centigrade", Symbol: "C",
                    IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            KnownUOM baseCurrency = AddKnownUOM("USD", UOMSet_SI,
                    new Dimensions(currency: 1),
                    Name: "USD", Description: "USD", Symbol: "$",
                    IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            KnownUOM baseAngle = AddKnownUOM("radian", UOMSet_SI,
                    new Dimensions(planeAngle: 1),
                    Name: "radian", Description: "radian (angle)", Symbol: "r",
                    IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            // TODO: Core KnownUOM for other dimension types

            // -------------------------------------
            AddKnownUOM("kilometre", UOMSet_SI,
                    new Dimensions(length: 1),
                    Name: "km", Description: "km", Symbol: "km",
                    IsBase: false, BaseUOM: baseLength, AFactor: 0, BFactor: 1000, CFactor: 1, DFactor: 0);

            AddKnownUOM("hour", UOMSet_SI,
                    new Dimensions(time: 1),
                    Name: "hour", Description: "hour", Symbol: "h",
                    IsBase: false, BaseUOM: baseTime, AFactor: 0, BFactor: 3600, CFactor: 1, DFactor: 0);

            // -------------------------------------
            // TODO: Remove the need for any KnownUOM that can be derived from ones already known (m/s, km/hour, km^2 etc.)
            //  Auto-define them? Or better still, create them dynamically when needed?

            KnownUOM baseLengthOverTime = AddKnownUOM("metre/second", UOMSet_SI,
                    new Dimensions(length: 1, time: -1),
                    Name: "m/s", Description: "metre/second", Symbol: "m/s",
                    IsBase: true, BaseUOM: null, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            // -------------------------------------
            AddKnownUOM("metre^2", UOMSet_SI,
                    new Dimensions(length: 2),
                    Name: "m^2", Description: "m^2", Symbol: "m^2",
                    IsBase: false, BaseUOM: baseLength, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);
            AddKnownUOM("metre^3", UOMSet_SI,
                    new Dimensions(length: 3),
                    Name: "m^3", Description: "m^3", Symbol: "m^3",
                    IsBase: false, BaseUOM: baseLength, AFactor: 0, BFactor: 1, CFactor: 1, DFactor: 0);

            AddKnownUOM("kilometre^2", UOMSet_SI,
                    new Dimensions(length: 2),
                    Name: "km^2", Description: "km^2", Symbol: "km^2",
                    IsBase: false, BaseUOM: baseLength, AFactor: 0, BFactor: Math.Pow(1000, 2), CFactor: 1, DFactor: 0);
            AddKnownUOM("kilometre^3", UOMSet_SI,
                    new Dimensions(length: 3),
                    Name: "km^3", Description: "km^3", Symbol: "km^3",
                    IsBase: false, BaseUOM: baseLength, AFactor: 0, BFactor: Math.Pow(1000, 3), CFactor: 1, DFactor: 0);

            AddKnownUOM("kilometre/second", UOMSet_SI,
                    new Dimensions(length: 1, time: -1),
                    Name: "km/s", Description: "km/s", Symbol: "km/s",
                    IsBase: false, BaseUOM: baseLengthOverTime, AFactor: 0, BFactor: 1000, CFactor: 1, DFactor: 0);

            AddKnownUOM("kilometre/hour", UOMSet_SI,
                    new Dimensions(length: 1, time: -1),
                    Name: "km/h", Description: "km/h", Symbol: "km/h",
                    IsBase: false, BaseUOM: baseLengthOverTime, AFactor: 0, BFactor: (double)(1000.0 / 3600.0), CFactor: 1, DFactor: 0);

        }


        // ------------------------------
        private void AddCoreConstants()
        {
            AddConstant("PI", "Mathematical constant, the ratio of a circle's circumference to its diameter", _ParamTypes["dimensionless"], 3.14159265359, null, enforceNameFormat:false);

            AddConstant("_c", "Speed of light in a vacuum", _ParamTypes["velocity"], 299792458, new AnonUOM(new Dimensions(length: 1, time: -1), UOMSet_SI));

            //AddConstant("g", "Acceleration due to gravity", _ParamTypes["acceleration linear"], 9.8, new AnonUOM(new Dimensions(length: 1, time: -2), UOMSet_SI));
            //AddConstant("G", "Gravitational Constant", _ParamTypes["gravitational constant"], 6.67E-11, new AnonUOM(new Dimensions(length: 3, mass:-1, time: -2), UOMSet_SI));

            //""
        }


        #endregion ------------ Adding Content


        #region ------------ Support Methods

        // ------------------------------
        private Function FindFunction(String modelName, String libPackageName)
        {
            // TODO: Use or remove libPackageName

            foreach (var item in _functions)
            {
                if (item.Value.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase) ||
                    item.Value.AsciiSymbol.Equals(modelName, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
                }
            }
            return null;
        }

        // ------------------------------
        private KnownUOM AddKnownUOM(string key, UOMSet uomSet,
                        Dimensions Dimensions,
                        string Name, string Description, string Symbol,
                        bool IsBase, KnownUOM BaseUOM,
                        double AFactor, double BFactor, double CFactor, double DFactor)
        {
            KnownUOM uom = new KnownUOM(CorePackageName, Dimensions, Name, Description, Symbol,
                                        IsBase, BaseUOM,
                                        AFactor, BFactor, CFactor, DFactor);

            _knownUOMs.Add(key, uom);
            if (uomSet != null)
            {
                uomSet.KnownUOMs.Add(uom);
            }

            return uom;
        }

        // -----------------
        private void AddParamType(string name, Popularities enPopularity, string description, string siBaseUom, double mass = 0, double length = 0, double time = 0, double electricCurrent = 0, double temperature = 0, double quantityOfSubstance = 0, double luminosity = 0, double planeAngle = 0, double solidAngle = 0, double currency = 0)
        {
            _ParamTypes.Add(name,
                new ParamType(CorePackageName, name, description, siBaseUom,
                    new Dimensions(mass: mass, length: length, time: time, electricCurrent: electricCurrent,
                                    temperature: temperature, quantityOfSubstance: quantityOfSubstance, luminosity: luminosity,
                                    planeAngle: planeAngle, solidAngle: solidAngle, currency: currency)
                    , (int)enPopularity));
        }

        // ------------------------------
        private void AddConstant(string name, string description, ParamType paramType, double value, AnonUOM anonUOM, bool enforceNameFormat=true)
        {
            if (anonUOM == null)
            {
                anonUOM = new AnonUOM(paramType?.Dimensions, UOMSet_SI);
            }

            _Constants.Add(name,
                new Constant(CorePackageName, name, description, paramType, value, anonUOM, enforceNameFormat));

        }

        public Constant FindConstant(String name, String libPackageName)
        {
            // TODO Return a deep copy in case something accidentally tries to change the value?  Or better, make Constant immutable?
            foreach (var item in _Constants)
            {
                if (item.Value.Name.Equals(name))
                {
                    return item.Value;
                }
            }
            return null;
        }

        // ------------------------------

        private IDictionary<string, IList<KnownUOM>> SortedUomDict { get; set; }

        private void CreateSortedUomDict(bool recreate=false)
        {
            if ((SortedUomDict != null) && !recreate) { return;  }

            IDictionary<string, IList<KnownUOM>> tmpSortedUomDict = new Dictionary<string, IList<KnownUOM>>();

            // Create the lists
            foreach (var item in KnownUOMs)
            {
                string key = item.Value.Dimensions.ToString();

                if (!tmpSortedUomDict.ContainsKey(key))
                {
                    tmpSortedUomDict.Add(key, new List<KnownUOM>());
                }
                IList<KnownUOM> UOMs = tmpSortedUomDict[key];

                UOMs.Add(item.Value);
            }

            // Store the sorted lists
            SortedUomDict = new Dictionary<string, IList<KnownUOM>>();
            foreach (var item in tmpSortedUomDict)
            {
                IList<KnownUOM> UOMs = item.Value.OrderBy(u => u.Name).ToList();
                SortedUomDict.Add(item.Key, UOMs);
            }
        }

        public IList<KnownUOM> GetKnownUOMs(ParamType paramType)
        {
            CreateSortedUomDict(recreate:false);

            string key = paramType.Dimensions.ToString();
            if (!SortedUomDict.ContainsKey(key)) { return new List<KnownUOM>(); }

            return SortedUomDict[key];
        }


        #endregion ------------ Support Methods


        #region ------------ Equation Parsing ------------


        // ---------------------------------------------------------------
        // Create various items from a text version of an expression
        // ---------------------------------------------------------------

        public FunctionCalc CreateFunctionCalcFromExpression(string expressionString, string description, IList<VarInfo> varInfos, out JongErrWarn errW)
        {
            errW = new JongErrWarn();

            string[] expressionSides = expressionString.Split('=');
            if (expressionSides.Length > 2) { throw new UnspecifiedException("Expression contains multiple '=' signs"); }

            SingleResult[] eqnSides = new SingleResult[expressionSides.Length];

            for (int i = 0; i < expressionSides.Length; i++)
            {
                JongFunction jongFunc = _jongParser.Parse(expressionSides[i], this, errW);

                if (jongFunc == null)
                {
                    //Debug.WriteLine(errW.ErrWarnString);
                    return null;
                }

                // ----- Recursively create the FunctionCalc structure
                eqnSides[i] = ContentManager.CreateFunctionCalc_From_JongFunction(jongFunc, this, errW);

                if (eqnSides[i]==null)
                {
                    IList<SingleResult> allLeavesFound = new List<SingleResult>();
                    eqnSides[i] = CreateTerm_From_JongFunction(jongFunc, this, allLeavesFound, errW);
                }

                if (eqnSides[i] == null) { return null; }
            }

            FunctionCalc funcalc = null;

            if (expressionSides.Length==1)
            {
                funcalc = eqnSides[0] as FunctionCalc;
            }
            else
            {
                funcalc = new EqnCalc(this, eqnSides[0], eqnSides[1]);
            }

            if (funcalc != null)
            {
                funcalc.Description = description;

                IList<SingleValue> eqVars = funcalc.FindAllVariables();

                if (varInfos != null)
                {
                    foreach (var vi in varInfos)
                    {
                        SingleValue sv = (from ev in eqVars
                                          where ev.Name.Equals(vi.Name)
                                          select ev).FirstOrDefault();
                        if (sv != null)
                        {
                            sv.Description = vi.Description;
                            sv.ParamType = vi.ParamType;
                        }
                    }

                }
            }

            return funcalc;
        }

        // Create a FunctionCalc object from an instance of JongFunction, which has probably been created by parsing an expression in text format
        public static FunctionCalc CreateFunctionCalc_From_JongFunction(JongFunction jongFunc, ContentManager contentManager, JongErrWarn errW)
        {
            IList<SingleResult> allLeavesFound = new List<SingleResult>();
            SingleResult term = CreateTerm_From_JongFunction(jongFunc, contentManager, allLeavesFound, errW);
            if (term == null) return null;  //Error should already have been logged

            if (term is FunctionCalc)
            {
                return (FunctionCalc)term;
            }

            return (FunctionCalc)ErrorReturnNullTerm(errW, String.Format("Item was not an equation or expression"));
        }

        // Create a Term object from an instance of JongFunction, which has probably been created by parsing an expression in text format.
        // Called recursively on all the sub-parts.
        private static SingleResult CreateTerm_From_JongFunction(JongFunction jongFunc, ContentManager contentManager, IList<SingleResult> allLeavesFoundAlready, JongErrWarn errW)
        {
            try
            {
                // ---------------------
                SingleResult retTerm = null;
                IList<SingleResult> theInputs = new List<SingleResult>();
                Function function = null;
                FunctionCalc funCalc = null;
                String modelName = "";

                // ----- If this is a variable, see if it has been created already
                String varName = "";

                if (jongFunc.name() == "VariableGet")
                { varName = ((JongVariableGet)jongFunc).getName(); }

                else if (jongFunc.name() == "VariableAssign")
                { varName = ((JongVariableAssign)jongFunc).getName(); }

                Variable foundVar = null;
                if (varName != "")
                {
                    foreach (var leafTerm in allLeavesFoundAlready)                     // See if we have it already, if not add it
                    {
                        if (leafTerm is Variable)
                        {
                            Variable lVar = (Variable)leafTerm;
                            if (lVar.Name.Equals(varName))      //,StringComparison.OrdinalIgnoreCase))
                            { foundVar = lVar; break; }
                        }
                    }
                }

                if (jongFunc.name() == "Value")
                {
                    retTerm = new Literal(jongFunc.eval());
                    allLeavesFoundAlready.Add(retTerm);
                }
                else if (jongFunc.name() == "VariableGet")
                {
                    if (varName == "null")      //Missing optional argument
                    {
                        return null;        //Not an error
                    }
                    else if (foundVar == null)
                    {
                        Constant constVal = contentManager.FindConstant(varName, "");

                        if (constVal != null)
                        {
                            retTerm = constVal;
                        }
                        else
                        {
                            retTerm = new Variable(varName);
                            allLeavesFoundAlready.Add(retTerm);
                        }
                    }
                    else
                    {
                        retTerm = foundVar;
                    }
                }
                else if (jongFunc.name() == "VariableAssign")
                {
                    function = contentManager.FindFunction("Equals", "");

                    Variable var = foundVar;
                    if (foundVar == null)
                    {
                        var = new Variable(varName);
                        theInputs.Add(var);
                        allLeavesFoundAlready.Add(var);
                    }
                    else
                    {
                        return ErrorReturnNullTerm(errW);
                    }

                    funCalc = new EqnCalc(contentManager);

                }
                else
                {
                    modelName = jongFunc.name();
                    int numArgs = jongFunc.get().Count;


                    // Special cases
                    if (modelName.Equals("-"))
                    {
                        if (numArgs == 1)
                        {
                            modelName = "UnaryMinus";
                        }
                        else
                        {
                            modelName = "Subtract";
                        }
                    }
                    function = (Function)contentManager.FindFunction(modelName, "");
                    if (function == null)
                    { return ErrorReturnNullTerm(errW, String.Format("Bad model name \"%s\"", modelName)); }

                    funCalc = new FunctionCalc(function);

                }

                if (funCalc != null)
                {
                    retTerm = funCalc;

                    // Recurse through the arguments
                    IList<JongFunction> argFuncs = jongFunc.get();

                    foreach (JongFunction argFunc in argFuncs)
                    {
                        SingleResult nextInput = CreateTerm_From_JongFunction(argFunc, contentManager, allLeavesFoundAlready, errW);

                        theInputs.Add(nextInput);

                    }

                    foreach (var item in theInputs)
                    {
                        funCalc.Inputs.Add(item);
                    }
                }

                return retTerm;

            }
            catch (JongError err)
            {
                errW.Init(ErrWStat.UNKNOWN_ERROR, err.get(), false);
                return null;
            }
        }

        public static SingleResult ErrorReturnNullTerm(JongErrWarn errW, String errMsg = "Unexpected error")
        {
            Logger.WriteLine(errMsg);
            errW.Init(ErrWStat.UNKNOWN_ERROR, errMsg, true);

            return null;
        }


        #endregion ------------ Equation Parsing ------------


        public static Result<KnownUOM> SiBaseUomFromDimensions(Dimensions dimensions, string siBaseUom)
        {
            try
            {
                KnownUOM knownUOM = UOMSet_SI.FindFromName(siBaseUom);

                if (knownUOM != null)
                {
                    if (dimensions.Equals(knownUOM.Dimensions))
                    {
                        return Result<KnownUOM>.Good(knownUOM);
                    }
                    else
                    {
                        return Result<KnownUOM>.Bad($"Inconsistent use of uom {siBaseUom}");
                    }
                }

                bool wasCreated = false;
                knownUOM = UOMSet_SI.FindFromDimensions(dimensions, /*bAllowCreate*/true, out wasCreated, name: siBaseUom);

                return Result<KnownUOM>.Good(knownUOM);
            }
            catch (Exception ex)
            {
                return Result<KnownUOM>.Bad(ex);
            }
        }


    }
}
