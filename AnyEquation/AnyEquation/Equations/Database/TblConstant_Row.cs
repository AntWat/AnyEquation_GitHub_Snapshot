using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Model;

namespace AnyEquation.Equations.Database
{
    public class TblConstant_Row : ImportedRow
    {
        public override bool OkToUse()
        {
            return (Status == 1);
        }

        /// This will be called after the table row has been imported, to allow any post-processing needed
        public override void DoneImport()
        {
        }

        #region ------------ Fields Read directly from the SqLite Database, or CVSV ------------

        //public string Index { get; set; }
        public int Status { get; set; }
        public string Name { get; set; }
        public int Popularity { get; set; }
        public string ParamType { get; set; }
        public string Description { get; set; }
        public string UnitSet { get; set; }
        public double Value { get; set; }

        #endregion ------------ Fields Read directly from the SqLite Database, or CVSV ------------


        #region ------------ Fields Filled after the row is read ------------

        #endregion ------------ Fields Filled after the row is read ------------


    }

    #region ------------ Stuff for CSV import

    /// <summary>
    ///  Mapping for content columns
    /// </summary>
    public class CsvConstantLibraryMapping
    {
        //public int IndexColumn { get; set; }
        public int StatusColumn { get; set; }
        public int NameColumn { get; set; }
        public int PopularityColumn { get; set; }
        public int ParamTypeColumn { get; set; }
        public int DescriptionColumn { get; set; }
        public int UnitSetColumn { get; set; }
        public int ValueColumn { get; set; }
    }

    #endregion ------------ Stuff for CSV import

}
