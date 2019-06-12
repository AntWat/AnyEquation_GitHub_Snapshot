using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.Model.Info;
using AnyEquation.Equations.ViewModels;

namespace AnyEquation.Equations.Common
{
    public interface IEquationsUiService
    {
        void ShowChooseEquation(VmEquations vmEquations);

        void ShowSettings();

        Task<Tuple<bool, KnownUOM>> ShowChooseUom(ContentManager contentManager, ParamType paramType);
        void ShowChooseDefaultUnits(ContentManager cm);

        Task<Tuple</*Cancelled?*/bool, UOMSet, IList<KnownUOM>>> ShowChooseDefaultUom(ContentManager contentManager, IList<UOMSet> uomSets, int uomSetSelectedIndex, IList<KnownUOM> uomSelections);

        //Task<Tuple<bool, ParamType>> ShowChooseParam(ContentManager contentManager);

        void ShowCalculationTree(EqnCalc equationCalc, string numberFormat);
        void ShowMathExpression(SingleResult mathExpression);

        /// <returns>Success, equationString, userEquationString, eqDescription, varInfos</returns>
        Task<Tuple<bool, string, string, string, IList<VarInfo>>> ShowAddEquation(ContentManager cm);

        Task<bool> ShowAddEquation_Step2(VmAddEquation vm);      //Page 2

        Task<EqnCalc> ShowCreateEquation(ContentManager cm);

        void SelectStringFromList(IList<string> itemsToShow, string title, string subTitle, Func<string /*selection*/, bool /*Result: not used*/> selectionCallback);

        void SelectEqnCalcFromList(int numGroupsPerPage, IList<IGrouping<string, EqnCalc>> itemsToShow, Func<EqnCalc /*selection*/, bool /*Result: not used*/> selectionCallback);

        void SelectStringsFromList(IDictionary<string, /*selected*/bool> itemsAndSelections, string title, string subTitle, Func<IDictionary<string, /*selected*/bool>, bool /*Result: not used*/> okCallback);

        Task<Tuple</*Cancelled?*/bool, IList<GroupedString>>> SelectStringsFromGroupedList(
                    IDictionary<string, /*selected*/bool> groupExpansions, IList<GroupedString> groupedStrings, bool multiSelect, string title, string subTitle);

        void PreviewEquation(ContentManager contentManager, EqnCalc equationCalc, Action okAction, Action cancelAction);
    }

    public class GroupedString
    {
        public string Group { get; set; }
        public string GroupDescription { get; set; }
        public string Item { get; set; }
        public bool IsSelected { get; set; }

        public object Data { get; set; }
    }
}
