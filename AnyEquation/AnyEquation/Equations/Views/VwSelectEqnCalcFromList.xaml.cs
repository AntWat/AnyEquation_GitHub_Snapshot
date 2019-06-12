using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AnyEquation.Equations.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VwSelectEqnCalcFromList : ContentPage
    {
        public VwSelectEqnCalcFromList(int numGroupsPage, IList<IGrouping<string, EqnCalc>> itemsToShow, Func<EqnCalc /*selection*/, bool /*not used*/> selectionCallback)
        {
            InitializeComponent();

            SelectionCallback = selectionCallback;
            _allItemsToShow = itemsToShow;

            NumGroupsPerPage = numGroupsPage;
            InitPageNumbering(_allItemsToShow?.Count??0);

            this.BindingContext = this;
        }

        Func<EqnCalc /*selection*/, bool /*not used*/> SelectionCallback;

        private IList<IGrouping<string, EqnCalc>> _allItemsToShow;

        public ObservableCollection<IGrouping<string, EqnCalc>> ItemsToShow { get; set; } = new ObservableCollection<IGrouping<string, EqnCalc>>();

        // -------------------------

        public int NumGroupsPerPage { get; set; }
        public int NumPages { get; set; } = 0;
        public int CurrentPage { get; set; } = 0;

        public string PageNumberText
        {
            get
            {
                return $"Page {CurrentPage} of {NumPages}";
            }
        }


        private void InitPageNumbering(int numToShow)
        {
            if (numToShow == 0) return;

            NumPages = numToShow / NumGroupsPerPage;
            if ((numToShow % NumGroupsPerPage)>0)
            {
                NumPages++;
            }
            DisplayPage(1);
        }

        private void DisplayPage(int nextPage)
        {
            if (NumPages == 0) return;

            CurrentPage = nextPage;
            if (CurrentPage > NumPages)
            {
                CurrentPage = 1;
            }
            else if (CurrentPage <1)
            {
                CurrentPage = NumPages;
            }

            int iFirst = ((CurrentPage-1)* NumGroupsPerPage);
            int iNextPage = iFirst + NumGroupsPerPage;
            if (iNextPage> _allItemsToShow.Count)
            {
                iNextPage = _allItemsToShow.Count;
            }

            ItemsToShow.Clear();
            for (int i = iFirst; i < iNextPage; i++)        // Remember these are groups, not individual items
            {
                ItemsToShow.Add(_allItemsToShow[i]);
            }

            OnPropertyChanged("PageNumberText");

        }

        public void ShowPreviousPage()
        {
            if (NumPages == 0) return;

            DisplayPage(CurrentPage-1);
        }

        public void ShowNextPage()
        {
            if (NumPages == 0) return;

            DisplayPage(CurrentPage+1);
        }

        // -------------------------

        public EqnCalc SelectedItem
        {
            set
            {
                try
                {
                    EqnCalc selection = value as EqnCalc;

                    if (selection != null)
                    {
                        this.Navigation.PopModalAsync();
                        SelectionCallback(selection);
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                    throw;
                }
            }
        }

        private void Back_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                ShowPreviousPage();

                //_vmBrowseEquation2.AcceptCurrentEquationCalc();
                //this.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void Next_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                ShowNextPage();

                //this.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                this.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

    }
}