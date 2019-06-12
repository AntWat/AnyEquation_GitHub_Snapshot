using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Database
{

    public class TblEqLibSectionNode_Row : ImportedRow
    {
        public override bool OkToUse()
        {
            return true;
        }

        /// This will be called after the table row has been imported, to allow any post-processing needed
        public override void DoneImport()
        {
        }

        #region ------------ Fields Read directly from the SqLite Database ------------

        public int SectionNodeID { get; set; }
        public int ParentNodeID { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        #endregion ------------ Fields Read directly from the SqLite Database ------------


        #region ------------ Fields Filled after the row is read ------------

        #endregion ------------ Fields Filled after the row is read ------------
    }


    public class TblEqLibEquation_Row : ImportedRow
    {
        public override bool OkToUse()
        {
            return true;
        }

        /// This will be called after the table row has been imported, to allow any post-processing needed
        public override void DoneImport()
        {
        }

        #region ------------ Fields Read directly from the SqLite Database ------------

        public int EquationID { get; set; }
        public int SectionNodeID { get; set; }

        public string Name { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public string EquationAsText { get; set; }

        #endregion ------------ Fields Read directly from the SqLite Database ------------


        #region ------------ Fields Filled after the row is read ------------

        #endregion ------------ Fields Filled after the row is read ------------
    }


    public class TblEqLibVariable_Row : ImportedRow
    {
        public override bool OkToUse()
        {
            return true;
        }

        /// This will be called after the table row has been imported, to allow any post-processing needed
        public override void DoneImport()
        {
        }

        #region ------------ Fields Read directly from the SqLite Database ------------

        public int VariableID { get; set; }
        public int EquationID { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ParamType { get; set; }

        #endregion ------------ Fields Read directly from the SqLite Database ------------


        #region ------------ Fields Filled after the row is read ------------

        #endregion ------------ Fields Filled after the row is read ------------
    }

    public class CsvLibraryImport
    {
        public string CsvFormatNumber { get; set; }
        public string PackageName { get; set; }
        public string LibraryName { get; set; }
        public string LibraryDescription { get; set; }

        public string EquationLibraryName { get; set; }
        public string EquationLibraryDescription { get; set; }
        public string LibraryUrl { get; set; }

        public IList<string> ReferencesFiles { get; set; } = new List<string>();
        public IList<string> ParamTypesFiles { get; set; } = new List<string>();
        public IList<string> UomFiles { get; set; } = new List<string>();
        public IList<string> ConstantsFiles { get; set; } = new List<string>();
        public IList<string> EquationFiles { get; set; } = new List<string>();
    }

    /// <summary>
    ///  Mapping for content columns
    /// </summary>
    public class CsvEquationLibraryMapping
    {
        public int SourceColumn { get; set; }       
        public int StatusColumn { get; set; }
        public int SectionNodeColumn { get; set; }
        public int EquationNameColumn { get; set; }
        public int EquationRefColumn { get; set; }
        public int DescriptionColumn { get; set; }
        public int EquationAsTextColumn { get; set; }
        public int ResultVariableColumn { get; set; }

        public IList<int> InputVariableColumns { get; set; } = new List<int>();

        public int TestCalulationColumn { get; set; }
        public IList<int> TestInputColumns { get; set; } = new List<int>();
    }

    public class CsvEquationImport
    {
        public int Source { get; set; }     // Which reference material did the equation come from
        public int Status { get; set; }
        public string SectionNode { get; set; }
        public string EquationName { get; set; }
        public string EquationRef { get; set; }
        public string Description { get; set; }
        public string EquationAsText { get; set; }
        public IList<string> Variables { get; } = new List<string>();

        public IList<string> TestVariables { get; } = new List<string>();
    }
}
