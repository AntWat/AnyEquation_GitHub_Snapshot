using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.FormsBook.Toolkit;
using AnyEquation.Common;

namespace AnyEquation.Options.Css3Colors
{
    public partial class Css3ColorsPage : ContentPage
    {
        public Css3ColorsPage()
        {
            InitializeComponent();
            this.BindingContext = this;
            Utils.IncrementAppIsBusy();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Device.StartTimer(TimeSpan.FromSeconds(.1), ()=> {
                InitColorOptions();
                Utils.DecrementAppIsBusy();
                return false;
                });
        }

        #region ------------ Fields and Properties ------------

        public enum enPropertyTarget
        {
            BackgroundColor=0, TextColor
        }

        private ObservableCollectionFast<ColorOption> _colorOptions = new ObservableCollectionFast<ColorOption>();
        public ObservableCollectionFast<ColorOption> ColorOptions { get { return _colorOptions; } set { _colorOptions = value; } }

        private ObservableCollectionFast<JumpPoint> _jumpPoints = new ObservableCollectionFast<JumpPoint>();
        public ObservableCollectionFast<JumpPoint> JumpPoints { get { return _jumpPoints; } set { _jumpPoints = value; } }

        // -------------------------------------
        public Color ContrastColor
        {
            get
            {
                return (Color)Application.Current.Resources[App.Key_ContrastColor];
            }
        }

        // ------------------------- Resource keys
        private string _backgroundColorKey = App.Key_BackgroundColor;

        private string _textColorKey = App.Key_TextColor;

        // -------------------------------------

        private ColorOption _backgroundColorOption = null;
        public ColorOption BackgroundColorOption
        {
            get { return _backgroundColorOption; }
            set
            {
                _backgroundColorOption = value;
                App app = Application.Current as App;

                if (_backgroundColorOption != null)
                {
                    app.MainBackgroundColorName = _backgroundColorOption.NamedColor.Name;
                    app.SetColoursFromNames();

                    OnPropertyChanged(App.Key_ContrastColor);
                    OnPropertyChanged("ColorOfTextSwitch");
                }
            }
        }

        // -------------------
        private ColorOption _textColorOption = null;
        public ColorOption TextColorOption
        {
            get { return _textColorOption; }
            set
            {
                ColorOption colorOption = value;

                _textColorOption = colorOption;
                if (_textColorOption != null)
                {
                    App app = Application.Current as App;
                    app.TextColorName = _textColorOption.NamedColor.Name;
                    app.SetColoursFromNames();
                }
            }
        }


        // -------------------
        public Color ColorOfTextSwitch
        {
            get
            {
                NamedColor namedColor = BackgroundColorOption?.NamedColor;

                if (namedColor == null)
                {
                    return NamedColor.Red;
                }
                else if (namedColor.Name.Equals("Black"))
                {
                    return NamedColor.DarkGrey;
                }
                else
                {
                    return (Color)Switch.BackgroundColorProperty.DefaultValue;
                }
            }
        }


        // -------------------------------------
        public ColorOption NullSelectedItem
        {
            get { return null;  /* We never want an item selected */ }
            set
            {
                OnPropertyChanged("NullSelectedItem");
            }
        }


        #endregion ------------ Fields and Properties ------------


        #region ------------ Misc ------------

        void InitColorOptions()
        {
            // ---------------------- Fill in _colorOptions

            List<ColorOption> list = new List<ColorOption>();

            foreach (var item in NamedColor.All)
            {
                ColorOption colorOption = new ColorOption(this, item);

                list.Add(colorOption);
            }

            var list2 = list.OrderBy(x => x.NamedColor.Name);

            _colorOptions.Clear();
            _colorOptions.AddRange(list2);


            // ---------------------- Create the Jump Points

            Dictionary<string, JumpPoint> dict = new Dictionary<string, JumpPoint>();

            foreach (var item in list2)
            {
                string cName = item.NamedColor.Name;
                string key = cName.Substring(0, 1);

                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, new JumpPoint(key, item));
                }
            }

            List<JumpPoint> jumps = new List<JumpPoint>();
            foreach (var item in dict) { jumps.Add(item.Value); }

            _jumpPoints.Clear();
            _jumpPoints.AddRange(jumps);

            OnPropertyChanged("ColorOptions");
            OnPropertyChanged("JumpPoints");

            // ----------------------
            if (Application.Current.Resources.ContainsKey(_backgroundColorKey))
            {
                Color color = (Color)Application.Current.Resources[_backgroundColorKey];
                BackgroundColorOption = FindColorOption(_colorOptions, color);
                if (_backgroundColorOption!=null)
                {
                    _backgroundColorOption.SetSelected(enPropertyTarget.BackgroundColor, true);
                }
            }

            if (Application.Current.Resources.ContainsKey(_textColorKey))
            {
                Color color = (Color)Application.Current.Resources[_textColorKey];
                _textColorOption = FindColorOption(_colorOptions, color);
                if (_textColorOption!=null)
                {
                    _textColorOption.SetSelected(enPropertyTarget.TextColor, true);
                }
            }
        }


        // ----------------------
        ColorOption FindColorOption(ObservableCollection<ColorOption>  coll, Color color)
        {
            foreach (var item in coll)
            {
                if (item.NamedColor.Color.Equals(color))
                {
                    return item;
                }
            }
            return null;
        }

        // ----------------------
        void ChangeSelection(enPropertyTarget propertyTarget, ColorOption colorOption, bool newValue)
        {
            if (newValue==false)
            {
                if ( ((propertyTarget == enPropertyTarget.BackgroundColor) && (BackgroundColorOption == colorOption)) ||
                     ((propertyTarget == enPropertyTarget.TextColor) && (TextColorOption == colorOption)) )
                {
                    // Can't deselect the current item, as we don't know what else to select instead
                    colorOption.OnPropertyChanged("BackgroundSelected");
                }
                else
                {
                    colorOption.SetSelected(propertyTarget, false);
                }
            }
            else if ((propertyTarget == enPropertyTarget.BackgroundColor) && (colorOption == TextColorOption) )
            {
                // Text and Background cannot be the same    
                //TODO: Show message?
                colorOption.OnPropertyChanged("BackgroundSelected");
            }
            else if ((propertyTarget == enPropertyTarget.TextColor) && (colorOption == BackgroundColorOption))
            {
                // Text and Background cannot be the same    
                //TODO: Show message?
                colorOption.OnPropertyChanged("TextSelected");
            }
            else
            {
                foreach (var item in _colorOptions)
                {
                    if (item != colorOption)
                    {
                        item.SetSelected(propertyTarget, false);
                    }
                }
                colorOption.SetSelected(propertyTarget, true);

                if (propertyTarget == enPropertyTarget.BackgroundColor)
                {
                    BackgroundColorOption = colorOption;
                }
                else if (propertyTarget == enPropertyTarget.TextColor)
                {
                    TextColorOption = colorOption;
                }
            }
        }

        #endregion ------------ Misc ------------

        #region ------------ Implement INotifyPropertyChanged ------------

        // TODO: These are defined in the parent so don't need redefining in any page class....
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    var handler = this.PropertyChanged;
        //    if (handler != null)
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //}

        #endregion ------------ Implement INotifyPropertyChanged ------------

        #region ------------ Event handlers that are specific to this GUI implementation, so are not implemented as ICommand objects  ------------

        private async void lstJumpPoints_ItemTap(object sender, ItemTappedEventArgs e)
        {
            JumpPoint jp = e.Item as JumpPoint;

            if (jp == null)
            {
                return;
            }

            ColorOption colorOption = jp.ColorOption;

            ((ListView)sender).SelectedItem = null; // de-select the row

            lstColors.ScrollTo(colorOption, ScrollToPosition.Center, false); ;
        }


        private void BtnReset_Click(object sender, EventArgs e)
        {
            ((App)Application.Current).ResetColors();
        }


        #endregion ------------ Event handlers that are specific to this GUI implementation, so are not implemented as ICommand objects  ------------

        // --------------------------------------------------------------
        public class JumpPoint
        {
            public JumpPoint(string text, ColorOption colorOption)
            {
                _text = text;
                _colorOption = colorOption;
            }

            private ColorOption _colorOption;
            public ColorOption ColorOption { get { return _colorOption; } set { _colorOption = value; } }

            private string _text = "";
            public string Text { get { return _text; } set { _text = value; } }
        }

        // ------------------------------
        public class ColorOption : INotifyPropertyChanged
        {
            public ColorOption(Css3ColorsPage parent, NamedColor namedColor)
            {
                _parent = parent;
                _namedColor = namedColor;
            }

            private Css3ColorsPage _parent = null;
            public Css3ColorsPage Parent { get { return _parent; } set { _parent = value; } }

            private NamedColor _namedColor = null;
            public NamedColor NamedColor { get { return _namedColor; } set { _namedColor = value; } }

            private bool _backgroundSelected = false;
            public bool BackgroundSelected
            {
                get { return _backgroundSelected; }
                set
                {
                    bool newValue = value;
                    if (newValue != _backgroundSelected)
                    {
                        _parent.ChangeSelection(enPropertyTarget.BackgroundColor, this, newValue);
                    }
                }
            }

            private bool _textSelected = false;
            public bool TextSelected
            {
                get { return _textSelected; }
                set
                {
                    bool newValue = value;
                    if (newValue != _textSelected)
                    {
                        _parent.ChangeSelection(enPropertyTarget.TextColor, this, newValue);
                    }
                }
            }

            public void SetSelected(enPropertyTarget propertyTarget, bool newValue)
            {
                if (propertyTarget == enPropertyTarget.BackgroundColor)
                {
                    if (_backgroundSelected != newValue)
                    {
                        _backgroundSelected = newValue;
                        OnPropertyChanged("BackgroundSelected");
                    }
                }
                else if (propertyTarget == enPropertyTarget.TextColor)
                {
                    if (_textSelected != newValue)
                    {
                        _textSelected = newValue;
                        OnPropertyChanged("TextSelected");
                    }
                }
            }

            public Color ColorOfBackgroundSwitch
            {
                get
                {
                    if (_namedColor == null)
                    {
                        return NamedColor.Red;
                    }
                    else if (_namedColor.Name.Equals("Black"))
                    {
                        return NamedColor.DarkGrey;
                    }
                    else
                    {
                        return (Color)Switch.BackgroundColorProperty.DefaultValue;
                    }
                }
            }


            #region ------------ Implement INotifyPropertyChanged ------------

            public event PropertyChangedEventHandler PropertyChanged;
            public virtual void OnPropertyChanged(string propertyName)
            {
                var handler = this.PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion ------------ Implement INotifyPropertyChanged ------------

        }

    }
}
