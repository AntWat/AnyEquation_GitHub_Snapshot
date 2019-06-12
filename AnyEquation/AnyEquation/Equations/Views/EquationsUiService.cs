using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.Model.Info;
using AnyEquation.Equations.ViewModels;
using AnyEquation.Equations.User_Controls;
using AnyEquation.Common;
using AnyEquation.Equations.EquationParser;
using System.Diagnostics;

namespace AnyEquation.Equations.Views
{
    class EquationsUiService : IEquationsUiService
    {

        #region ------------ Statics ------------

        private static bool _isInitialised = false;

        public static async Task<bool> StartEquations(INavigation navigationIn = null)
        {
            Utils.IncrementAppIsBusy();

            INavigation navigation2 = navigationIn;

            EquationsUiService equationsUiService = new EquationsUiService(navigation2);
            VmEquations vmEquations = new VmEquations(equationsUiService);

            Result<EquationsSystemProblems> r_equationsSystemProblems = null;

            r_equationsSystemProblems = await InitEquationSystem(vmEquations,
                async (x) => 
                {
                    return await ShowVwEquationsPage(navigation2, vmEquations, x);
                });

            equationsUiService.Navigation = navigation2;

            Utils.DecrementAppIsBusy();

            return true;
        }

        private static async Task<Result<EquationsSystemProblems>> InitEquationSystem(VmEquations vmEquations,
                    Func<Result<EquationsSystemProblems>, Task<bool>> finishedAction)
        {
            // Note: This cannot be called on a separate thread because it would refresh Observable collections, 
            //       and thus cause a thread marshall error due to updates on a non-UI thread.
            //       Instead, it now creates a thread for the main content loading. This error only manifested itself in UWP
            //       A possible alternative is presented in: https://www.codeproject.com/Articles/64936/Multithreaded-ObservableImmutableCollection

            try
            {
                Result<EquationsSystemProblems> r_equationsSystemProblems = Result<EquationsSystemProblems>.Good(null);

                if (!_isInitialised)
                {
                    r_equationsSystemProblems = await Task.Run(async () => await ContentManager.LoadContent());
                }
                else
                {
                    await Task.Run(async () => await Task.Delay(100));      // TODO: By trial and error I found that this was necessary, otherwise the calculation grid doesn't show the second time the equation page is shown
                }

                vmEquations.SetContentManager(ContentManager.gContentManager);

                await finishedAction(r_equationsSystemProblems);

                _isInitialised = true;
                return r_equationsSystemProblems;

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                return Result<EquationsSystemProblems>.Bad(ex);
            }
        }

        // +++++++++++++++++++++++++++++++++++++++

        private static async Task<bool> ShowVwEquationsPage(INavigation navigation2, VmEquations vmEquations, Result<EquationsSystemProblems> r_equationsSystemProblems)
        {
            VwEquationsPage2 vwEquationsPage = null;

            vwEquationsPage = new AnyEquation.Equations.Views.VwEquationsPage2(vmEquations);

            if (navigation2 == null)
            {
                navigation2 = vwEquationsPage.Navigation;
            }
            else
            {
                await navigation2.PushAsync(vwEquationsPage, true);
            }

            // TODO: Report conflicts and/or allow the user to choose which to use?
            if ((r_equationsSystemProblems != null) && (r_equationsSystemProblems.Value != null))
            {
                EquationsSystemProblems equationsSystemProblems = r_equationsSystemProblems.Value;
                if (equationsSystemProblems.NumParamTypeInconsistencies>0 || equationsSystemProblems.NumUOMInconsistencies>0 
                    || equationsSystemProblems.NumEqLibInconsistencies > 0 )
                {
                    await vwEquationsPage.DisplayAlert("Data Loading Errors", 
                        $"{equationsSystemProblems.NumParamTypeInconsistencies + equationsSystemProblems.NumUOMInconsistencies + equationsSystemProblems.NumEqLibInconsistencies} inconsistencies were found while reading data from the databases.", "Ok");
                }
            }

            return true;
        }

        #endregion ------------ Statics ------------

        #region ------------ Constructors and Life Cycle

        public INavigation Navigation { get; set; }

        public EquationsUiService(INavigation navigation)
        {
            Navigation = navigation;
        }

        #endregion ------------ Constructors and Life Cycle

        #region ------------ IEquationsUiService implementations

        public void ShowChooseEquation(VmEquations vmEquations)
        {
            try
            {
                //Navigation.PushAsync(new VwBrowseEquation(new VmBrowseEquation1(vmEquations)));
                Navigation.PushAsync(new VwBrowseEquation2(new VmBrowseEquation2(vmEquations)));
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        public void ShowSettings()
        {
            try
            {
                Navigation.PushAsync(new VwSettings());
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        public async Task<Tuple<bool, string, string, string, IList<VarInfo>>> ShowAddEquation(ContentManager cm)
        {
            VmAddEquation vm = new VmAddEquation(this, cm);
            VwAddEquation vw = new VwAddEquation(this, vm, isModal: true);

            await Navigation.PushModalAsync(vw);
            await vw.PageClosedTask; // Wait here until the Page is dismissed

            return new Tuple<bool, string, string, string, IList<VarInfo>>(vm.SelectionCancelled, vm.EquationString, vm.UserEquationString, vm.EqDescription, vm.GetVarInfoList());
        }

        public async Task<bool> ShowAddEquation_Step2(VmAddEquation vm)
        {
            VwAddEquationStep2 vw = new VwAddEquationStep2(vm, isModal: true);

            await Navigation.PushModalAsync(vw);
            await vw.PageClosedTask; // Wait here until the Page is dismissed

            return true;
        }


        public async Task<EqnCalc> ShowCreateEquation(ContentManager cm)
        {
            Tuple<bool, string, string, string, IList<VarInfo>> tpl;
            tpl = await ShowAddEquation(cm);

            try
            {
                bool bCancelled = tpl.Item1;
                string equationString = null;
                string userEquationString = null;
                string eqDescription = null;
                IList<VarInfo> varInfos = null;

                if (!bCancelled)
                {
                    equationString = tpl.Item2;
                    userEquationString = tpl.Item3;
                    eqDescription = tpl.Item4;
                    varInfos = tpl.Item5;

                    Debug.WriteLine("equationString={0}, userEquationString={1},eqDescription={2}");
                    Debug.WriteLine(varInfos);

                    JongErrWarn errW = null;
                    FunctionCalc funCalc = cm.CreateFunctionCalcFromExpression(equationString, eqDescription, varInfos, out errW);

                    EqnCalc equationCalc = funCalc as EqnCalc;

                    if (equationCalc == null)
                    {
                        // TODO
                    }
                    else
                    {
                        // TODO
                        //EquationLibraries.Add(equationCalc);
                        return equationCalc;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }

            return null;
        }

        // -----------------------------
        public async Task<Tuple<bool, KnownUOM>> ShowChooseUom(ContentManager contentManager, ParamType paramType)
        {
            VmChooseUom vmUom = new VmChooseUom(contentManager, paramType);
            VwChooseUomPage vw = new VwChooseUomPage(vmUom, isModal:true);

            await Navigation.PushModalAsync(vw);
            await vw.PageClosedTask; // Wait here until the Page is dismissed

            return new Tuple<bool, KnownUOM> (vmUom.SelectionCancelled, vmUom.SelectedUOM );
        }

        // -----------------------------
        public async Task<Tuple</*Cancelled?*/bool, UOMSet, IList<KnownUOM>>> ShowChooseDefaultUom(ContentManager contentManager, 
                                                        IList<UOMSet> uomSets, int uomSetSelectedIndex, IList<KnownUOM> uomSelections)
        {
            VmChooseMultiUom vm = new VmChooseMultiUom(contentManager, this, uomSets, uomSetSelectedIndex, uomSelections);
            VwChooseDefaultUomPage vw = new VwChooseDefaultUomPage(vm, isModal: true);

            await Navigation.PushModalAsync(vw);
            await vw.PageClosedTask; // Wait here until the Page is dismissed

            IList<KnownUOM> chosenUoms = vm.UomSelections;
            return new Tuple<bool, UOMSet, IList<KnownUOM>>(vm.SelectionCancelled, vm.SelectedUomSet, chosenUoms);
        }

        public async void ShowChooseDefaultUnits(ContentManager cm)
        {
            try
            {
                IList<UOMSet> uomSets = new List<UOMSet>();
                int uomSetSelectedIndex = 0;
                foreach (var item in cm.UOMSets)
                {
                    uomSets.Add(item.Value);
                }
                if (cm.DefaultUOMSet.ParentUomSet != null)
                {
                    uomSetSelectedIndex = uomSets.IndexOf(cm.DefaultUOMSet.ParentUomSet);
                }

                Tuple</*Cancelled?*/bool, UOMSet, IList<KnownUOM>> tpl = await ShowChooseDefaultUom(cm, uomSets, uomSetSelectedIndex, cm.DefaultUOMSet.KnownUOMs);
                if (!tpl.Item1)
                {
                    cm.DefaultUOMSet.ParentUomSet = tpl.Item2;

                    cm.DefaultUOMSet.KnownUOMs.Clear();
                    foreach (KnownUOM uom in tpl.Item3)
                    {
                        cm.DefaultUOMSet.KnownUOMs.Add(uom);
                    }

                    // TODO: Change units for the current calc?
                    // TODO: Persist the default unit choices
                }

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }


        // -----------------------------
        public void ShowCalculationTree(EqnCalc equationCalc, string numberFormat)
        {
            try
            {
                Navigation.PushAsync(new VwCalculationTree(new VmCalculationTree(this, equationCalc, numberFormat), this));
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        // -----------------------------
        public void ShowMathExpression(SingleResult mathExpression)
        {
            Navigation.PushAsync(new VwShowMathExpression(this, mathExpression));
        }

        // -----------------------------
        public void SelectStringFromList(IList<string> itemsToShow, string title, string subTitle, Func<string /*selection*/, bool /*not used*/> selectionCallback)
        {
            Navigation.PushModalAsync(new VwSelectStringFromList(itemsToShow, title, subTitle, selectionCallback));
        }

        // -----------------------------
        public void SelectEqnCalcFromList(int numGroupsPerPage, IList<IGrouping<string, EqnCalc>> itemsToShow, Func<EqnCalc /*selection*/, bool /*not used*/> selectionCallback)
        {
            Navigation.PushModalAsync(new VwSelectEqnCalcFromList(numGroupsPerPage, itemsToShow, selectionCallback));
        }

        // -----------------------------
        public void SelectStringsFromList(IDictionary<string, /*selected*/bool> itemsAndSelections, string title, string subTitle, Func<IDictionary<string, /*selected*/bool>, bool /*Result: not used*/> okCallback)
        {
            Navigation.PushModalAsync(new VwSelectStringsFromList(itemsAndSelections, title, subTitle, okCallback));
        }

        // -----------------------------

        IList<GroupedString> groupedStringSelections;
        public async Task<Tuple</*Cancelled?*/bool, IList<GroupedString>> > SelectStringsFromGroupedList(
                    IDictionary<string, /*selected*/bool> groupExpansions, IList<GroupedString> groupedStrings, bool multiSelect, string title, string subTitle)
        {
            groupedStringSelections = null;
            bool selectionCancelled = true;

            Func<IList<GroupedString>, bool /*Result: not used*/> okCallback = (g) => 
            {
                selectionCancelled = false;
                groupedStringSelections = g;
                return true;
            };

            VwSelectStringsFromGroupedList vw = new VwSelectStringsFromGroupedList(this, groupExpansions, groupedStrings, multiSelect, title, subTitle, 
                                                                                        okCallback, isModal: true);

            await Navigation.PushModalAsync(vw);
            await vw.PageClosedTask; // Wait here until the Page is dismissed

            return new Tuple<bool, IList<GroupedString>>(selectionCancelled, groupedStringSelections);
        }

        // -----------------------------
        public void PreviewEquation(ContentManager contentManager, EqnCalc equationCalc, Action okAction, Action cancelAction)
        {
            VmPreviewEquation vm = new VmPreviewEquation(contentManager, equationCalc, okAction, cancelAction);
            VwPreviewEquation vw = new VwPreviewEquation(vm);

            Navigation.PushModalAsync(vw);
            //await vw.PageClosedTask; // Wait here until the Page is dismissed
            //Navigation.PushAsync(vw);

            return;
        }
        
        #endregion ------------ IEquationsUiService implementations


    }
}
