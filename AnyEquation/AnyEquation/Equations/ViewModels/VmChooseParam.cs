using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.FormsBook.Toolkit;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;

namespace AnyEquation.Equations.ViewModels
{
    public class VmChooseParam : ViewModelBase
    {

        #region ------------ Constructors and Life Cycle ------------

        public VmChooseParam(ContentManager contentManager)
        {
            ContentManager = contentManager;
            SelectionCancelled = false;
        }

        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Fields and Properties ------------

        // ------------------------------

        private ContentManager _contentManager;
        public ContentManager ContentManager
        {
            get { return _contentManager; }
            set
            {
                if (SetProperty(ref _contentManager, value))
                {
                    RefreshParams();

                    // OnPropertyChanged("MathExpression");
                }
            }
        }

        // ------------------------------

        public bool SelectionCancelled { get; set; }
        public ParamType SelectedParam { get; set; }

        private ObservableCollection<ParamType> _Params = new ObservableCollection<ParamType>();
        public ObservableCollection<ParamType> Params { get { return _Params; } set { _Params = value; } }

        private void RefreshParams()
        {
            _Params.Clear();
            foreach (var item in _contentManager.ParamTypes)
            {
                _Params.Add(item.Value);
            }
        }

        #endregion ------------ Fields and Properties ------------

    }
}
