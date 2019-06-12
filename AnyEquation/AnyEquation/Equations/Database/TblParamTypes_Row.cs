using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Model;

namespace AnyEquation.Equations.Database
{
    public class TblParamTypes_Row : ImportedRow
    {
        public override bool OkToUse()
        {
            return (Status == 1);
        }

        /// This will be called after the table row has been imported, to allow any post-processing needed
        public override void DoneImport()
        {
            // Create the Dimensions object etc.
            Dimensions = new Dimensions(mass:Dim_Mass, length:Dim_Length, time:Dim_Time, electricCurrent:Dim_ElectricCurrent, 
                temperature: Dim_Temperature, quantityOfSubstance:Dim_QuantityOfSubstance, 
                luminosity: Dim_Luminosity, planeAngle: Dim_PlaneAngle,  solidAngle: Dim_SolidAngle,
                currency: Dim_Currency
);
        }

        #region ------------ Fields Read directly from the SqLite Database ------------


        //public int Index { get; set; }
        public int Status { get; set; }     // 1=OK, any other value: do not use

        public string Name { get; set; }
        public int Popularity { get; set; }
        /* Higher numbers will be more popular, so appear higher in selection lists.
                Rough categories:
                0 or empty: default
                1 = Common
                2 = Very common
                10 = User favorite*/

        public string Group { get; set; }   // Defines a set of param types that are equivalent (possibly even just alternative names)
        public string Description { get; set; }

        public string BaseUom { get; set; }     // SI Base UOM
        public string ParsedUOM { get; set; }

        public int Dim_Mass { get; set; }
        public int Dim_Length { get; set; }
        public int Dim_Time { get; set; }
        public int Dim_ElectricCurrent { get; set; }
        public int Dim_Temperature { get; set; }
        public int Dim_QuantityOfSubstance { get; set; }
        public int Dim_Luminosity { get; set; }
        public int Dim_PlaneAngle { get; set; }
        public int Dim_SolidAngle { get; set; }
        public int Dim_Currency { get; set; }


        #endregion ------------ Fields Read directly from the SqLite Database ------------


        #region ------------ Fields Filled after the row is read ------------

        public Dimensions Dimensions { get; set; }

        #endregion ------------ Fields Filled after the row is read ------------

    }

    #region ------------ Stuff for CSV import

    /// <summary>
    ///  Mapping for content columns
    /// </summary>
    public class CsvParamTypeLibraryMapping
    {
        public int StatusColumn { get; set; }
        public int NameColumn { get; set; }
        public int PopularityColumn { get; set; }
        public int GroupColumn { get; set; }
        public int DescriptionColumn { get; set; }
        public int BaseUOMColumn { get; set; }
        public int ParsedUOMColumn { get; set; }
        public int Dim_MassColumn { get; set; }
        public int Dim_LengthColumn { get; set; }
        public int Dim_TimeColumn { get; set; }
        public int Dim_ElectricCurrentColumn { get; set; }
        public int Dim_TemperatureColumn { get; set; }
        public int Dim_QuantityOfSubstanceColumn { get; set; }
        public int Dim_LuminosityColumn { get; set; }
        public int Dim_PlaneAngleColumn { get; set; }
        public int Dim_SolidAngleColumn { get; set; }
        public int Dim_CurrencyColumn { get; set; }
    }

    #endregion ------------ Stuff for CSV import

}
