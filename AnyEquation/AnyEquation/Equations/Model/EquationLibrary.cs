using AnyEquation.Common;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Database;
using AnyEquation.Equations.EquationParser;
using AnyEquation.Equations.Model.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    #region ------------ EquationLibrary class

    public class EquationLibrary
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public IList<SectionNode> TopSectionNodes { get; } = new List<SectionNode>();

        public IList<EqnCalc> GetAllEqnCalcs()
        {
            IList<EqnCalc> eqnCalcs = new List<EqnCalc>();

            foreach (var sn in TopSectionNodes)
            {
                AddEqnCalcs(eqnCalcs, sn);
            }
            return eqnCalcs;
        }

        private void AddEqnCalcs(IList<EqnCalc> eqnCalcs, SectionNode sn)
        {
            foreach (var item in sn.EqnCalcs)
            {
                eqnCalcs.Add(item);
            }

            foreach (var child in sn.Children)
            {
                AddEqnCalcs(eqnCalcs, child);
            }
        }

        #region ------------ Create from Database tables

        public static EquationLibrary CreateLibraryFromDatabase(ContentManager contentManager,
                                                SqLiteContentDatabase database, IList<TblEqLibSectionNode_Row> sectionNodeRows, 
                                                IList<TblEqLibEquation_Row> equationRows, IList<TblEqLibVariable_Row> variableRows)
        {
            EquationLibrary equationLibrary = new EquationLibrary();
            equationLibrary.CreateFromDatabase(contentManager, database, sectionNodeRows,
                                                equationRows, variableRows);
            return equationLibrary;
        }

        public void CreateFromDatabase(ContentManager contentManager, 
                                                SqLiteContentDatabase database, IList<TblEqLibSectionNode_Row> sectionNodeRows,
                                                IList<TblEqLibEquation_Row> equationRows, IList<TblEqLibVariable_Row> variableRows)
        {
            Name = database.GetInfoVal("EquationLibraryName");
            Description = database.GetInfoVal("EquationLibraryDescription");

            // ----------- Read all the section nodes
            IDictionary<int, Tuple<TblEqLibSectionNode_Row,SectionNode>> allSectionNodes
                                    = new Dictionary<int, Tuple<TblEqLibSectionNode_Row, SectionNode>>();      // Key is TblEqLibSectionNode_Row.SectionNodeID
            foreach (var nodeRow in sectionNodeRows)
            {
                SectionNode sectionNode = new SectionNode(nodeRow.Name) { Description = nodeRow.Description };
                allSectionNodes.Add(nodeRow.SectionNodeID, new Tuple<TblEqLibSectionNode_Row, SectionNode>(nodeRow,sectionNode));
            }

            // ----------- Connect the parents
            foreach (var nodeTpl in allSectionNodes)
            {
                TblEqLibSectionNode_Row nodeRow = nodeTpl.Value.Item1;
                SectionNode sectionNode = nodeTpl.Value.Item2;

                if (nodeRow.ParentNodeID>0)
                {
                    int parentNodeID = nodeRow.ParentNodeID;
                    if (allSectionNodes.ContainsKey(parentNodeID))
                    {
                        sectionNode.Parent = allSectionNodes[parentNodeID].Item2;
                    }
                    else
                    {
                        int iDbg = 0;   // TODO
                    }
                }
                else
                {
                    TopSectionNodes.Add(sectionNode);
                }
            }

            // ----------- Check for duplications or loops... TODO

            // ----------- Store the EquationDetails
            IDictionary<int, Tuple<TblEqLibEquation_Row, EquationDetails>> allEquationDetails
                                    = new Dictionary<int, Tuple<TblEqLibEquation_Row, EquationDetails>>();      // Key is TblEqLibEquation_Row.EquationID

            foreach (var eqnRow in equationRows)
            {
                EquationDetails ed = new EquationDetails(eqnRow.Name, eqnRow.EquationAsText);
                allEquationDetails.Add(eqnRow.EquationID, new Tuple<TblEqLibEquation_Row, EquationDetails>(eqnRow, ed));
            }

            // ----------- Add the variables to the EquationDetails
            foreach (var varRow in variableRows)
            {
                string paramTypName = varRow.ParamType?.ToLower();
                ParamType paramType = null;
                if (contentManager.ParamTypes.ContainsKey(paramTypName))
                {
                    paramType = contentManager.ParamTypes[paramTypName];
                }
                else
                {
                    int iDbg = 0;   // TODO
                }

                VarInfo vi = new VarInfo(varRow.Name, varRow.Description, paramType);

                int equationID = varRow.EquationID;
                if (allEquationDetails.ContainsKey(equationID))
                {
                    allEquationDetails[equationID].Item2.VariableInfos.Add(vi); // TODO: Check for duplicate names
                }
                else
                {
                    int iDbg = 0;   // TODO
                }
            }

            // ----------- Create the EquationCalcs in the section nodes
            foreach (var eqTpl in allEquationDetails)
            {
                TblEqLibEquation_Row eqnRow = eqTpl.Value.Item1;
                EquationDetails ed = eqTpl.Value.Item2;

                int sectionNodeID = eqnRow.SectionNodeID;
                if (allSectionNodes.ContainsKey(sectionNodeID))
                {
                    AddEquationToSectionNode(contentManager, allSectionNodes[sectionNodeID].Item2, ed);
                }
                else
                {
                    int iDbg = 0;   // TODO
                }
            }

        }

        #endregion ------------ Create from Database tables



        #region ------------ Create from CSV

        public static EquationLibrary CreateLibraryFromCsv(ContentManager contentManager, string libraryName, 
                                                                string libraryDescription, IList<CsvEquationImport> csvEquationImports)
        {
            EquationLibrary equationLibrary = new EquationLibrary();
            equationLibrary.CreateFromCsv(contentManager, libraryName, libraryDescription, csvEquationImports);
            return equationLibrary;
        }

        public void CreateFromCsv(ContentManager contentManager, string libraryName,
                                                                string libraryDescription, IList<CsvEquationImport> csvEquationImports)
        {
            Name = libraryName;
            Description = libraryDescription;

            IDictionary<string, SectionNode> allSectionNodes = new Dictionary<string, SectionNode>();

            foreach (var csvEI in csvEquationImports)
            {
                try
                {
                    SectionNode sectionNode = CreateSectionNodeFromCsv(csvEI.SectionNode, allSectionNodes);

                    if ((csvEI.Status == 0) || (csvEI.Status == 1))
                    {
                        EquationDetails ed = CreateEquationDetailsFromCsv(contentManager, csvEI);
                        AddEquationToSectionNode(contentManager, sectionNode, ed);
                    }
                    else
                    {
                        int iDbg = 0;
                    }
                }
                catch (Exception ex)
                {
                    // Log but then continue with the rest of them
                    Logging.LogMessage($"Error processing equation '{csvEI.EquationAsText}': {ex.Message}");
                }
            }
        }

        private SectionNode CreateSectionNodeFromCsv(string sectionNodePath, IDictionary<string, SectionNode> allSectionNodes)
        {
            string[] sectionNodes = sectionNodePath.Split('>');

            SectionNode parentNode = null;
            SectionNode sectionNode = null;

            string sPath = "";

            foreach (var strNode in sectionNodes)
            {
                sPath += $">{strNode}"; 
                if (allSectionNodes.ContainsKey(sPath))
                {
                    sectionNode = allSectionNodes[sPath];
                }
                else
                {
                    sectionNode = new SectionNode(parentNode, strNode);
                    allSectionNodes.Add(sPath, sectionNode);
                    if (parentNode==null)
                    {
                        TopSectionNodes.Add(sectionNode);
                    }
                }
                parentNode = sectionNode;
            }

            return sectionNode;
        }

        private EquationDetails CreateEquationDetailsFromCsv(ContentManager contentManager, CsvEquationImport csvEI)
        {
            IList<VarInfo> variableInfos = new List<VarInfo>();
            foreach (var strVarInfo in csvEI.Variables)
            {
                variableInfos.Add(CreateVariableFromCsv(contentManager, strVarInfo));
            }

            IList<double> testValues = new List<double>();
            foreach (var strTestVal in csvEI.TestVariables)
            {
                double dVal;
                if (double.TryParse(strTestVal, out dVal))
                {
                    testValues.Add(dVal);
                }
            }

            string eqName = (string.IsNullOrEmpty(csvEI.EquationName) ? csvEI.EquationRef : csvEI.EquationName);
            EquationDetails ed = new EquationDetails(eqName, csvEI.EquationAsText, variableInfos);
            ed.TestValues = testValues;

            return ed;
        }

        private VarInfo CreateVariableFromCsv(ContentManager contentManager, string strVarInfo)
        {
            try
            {
                string[] info = strVarInfo.Split(':');

                string strParamType = info[2].ToLower();

                ParamType paramType = null;
                if (contentManager.ParamTypes.ContainsKey(strParamType))
                {
                    paramType = contentManager.ParamTypes[strParamType];
                }
                else
                {
                    int iDbg = 0;   // TODO
                }

                VarInfo vi = new VarInfo(info[0], info[1], paramType);
                return vi;
            }
            catch (Exception ex)
            {
                throw;      // Will be caught by the level above
            }
        }

        #endregion ------------ Create from CSV



        #region ------------ Create Equations from temporary definitions

        public static void AddEquationsToSectionNode(ContentManager contentManager, SectionNode sectionNode, IList<EquationDetails> equationDetails)
        {
            foreach (var ed in equationDetails)
            {
                AddEquationToSectionNode(contentManager, sectionNode, ed);
            }
        }

        public static void AddEquationToSectionNode(ContentManager contentManager, SectionNode sectionNode, EquationDetails ed)
        {
            string description = "";
            if (!string.IsNullOrEmpty(ed.Name)) { description += ((string.IsNullOrEmpty(description) ? "" : " ") + ed.Name); }
            if (!string.IsNullOrEmpty(ed.Reference)) { description += ((string.IsNullOrEmpty(description) ? "" : " ") + ed.Reference); }
            if (!string.IsNullOrEmpty(ed.Description)) { description += ((string.IsNullOrEmpty(description) ? "" : " ") + ed.Description); }

            // ----------------------
            JongErrWarn errW = null;
            FunctionCalc funCalc = contentManager.CreateFunctionCalcFromExpression(ed.EquationAsText, description, ed.VariableInfos, out errW);

            // ----------------------
            if (funCalc != null)
            {
                // ------------- Record the test values if there are any
                if ((ed.TestValues!=null) && (ed.TestValues.Count>0))
                {
                    funCalc.TestValues = new Dictionary</*variable name*/string, double>();

                    for (int i = 0; i < ed.VariableInfos.Count; i++)
                    {
                        VarInfo vi = ed.VariableInfos[i];
                        funCalc.TestValues.Add(vi.Name, ed.TestValues[i]);
                    }
                }
                funCalc.ExpectedDimensions = new Dictionary</*variable name*/string, Dimensions>();
                if (ed.VariableInfos!=null)
                {
                    foreach (VarInfo vi in ed.VariableInfos)
                    {
                        funCalc.ExpectedDimensions.Add(vi.Name, vi.ParamType?.Dimensions);
                    }
                }

                // -------------
                sectionNode.EqnCalcs.Add((EqnCalc)funCalc);
            }
        }

        #endregion ------------ Create Equations from temporary definitions
    }

    #endregion ------------ EquationLibrary class

    #region ------------ SectionNode class

    public class SectionNode
    {
        public SectionNode(SectionNode parent, string name)
        {
            Parent = parent;
            Name = name;
        }
        public SectionNode(string name)
        {
            Name = name;
        }

        SectionNode _parent=null;
        public SectionNode Parent {
            get { return _parent; }
            set
            {
                _parent = value;
                _parent?.Children?.Add(this);
            }
        }
        public IList<SectionNode> Children { get; set; } = new List<SectionNode>();


        public string Name { get; set; }
        public string Description { get; set; }

        public IList<EqnCalc> EqnCalcs { get; } = new List<EqnCalc>();
    }

    #endregion ------------ SectionNode class


    #region ------------ Temporary classes used during library creation

    public class EquationDetails
    {
        public EquationDetails(string name, string equationAsText, IList<VarInfo> variableInfos)
        {
            Name = name;
            EquationAsText = equationAsText;
            VariableInfos = variableInfos;
        }
        public EquationDetails(string name, string equationAsText)
        {
            Name = name;
            EquationAsText = equationAsText;
            VariableInfos = new List<VarInfo>();
        }

        public string Name { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public string EquationAsText { get; set; }

        public IList<VarInfo> VariableInfos { get; set; }
        public IList<double> TestValues { get; set; }

    }

    #endregion ------------ Temporary classes used during library creation

}
