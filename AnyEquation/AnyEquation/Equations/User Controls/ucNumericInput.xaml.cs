using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using AnyEquation.Common;

namespace AnyEquation.Equations.User_Controls
{
    public partial class ucNumericInput : ContentView
    {
        public ucNumericInput()
        {
            InitializeComponent();

            topContainer.BindingContext = this;
        }



        public double? NumVal
        {
            get { return (double)GetValue(NumValProperty); }
            set { SetValue(NumValProperty, value); }
        }

        // Using a BindableProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty NumValProperty =
            BindableProperty.Create(
                "NumVal",
                typeof(double?),
                typeof(ucNumericInput),
                null,	//Change this to 0 for int etc.
                BindingMode.Default,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    ucNumericInput uc = bindable as ucNumericInput;

                    if (uc!=null)
                    {
                        double nv = Utils.parseToDouble(newValue, double.NaN);
                        if (double.IsNaN(nv))
                        {
                            uc.numEntry.Text = "";
                            uc.lblText.Text = "";
                        }
                        else
                        {
                            uc.numEntry.Text = string.Format("{0}", nv);
                            uc.lblText.Text = uc.FormattedResult(nv);
                        }
                    }
                });


        // ------------
        public string NumFormat
        {
            get { return (string)GetValue(NumFormatProperty); }
            set { SetValue(NumFormatProperty, value); }
        }

        // Using a BindableProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty NumFormatProperty =
            BindableProperty.Create(
                "NumFormat",
                typeof(string),
                typeof(ucNumericInput),
                "0.###",	//Change this to 0 for int etc.
                BindingMode.Default,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    ucNumericInput uc = bindable as ucNumericInput;

                    if (uc != null)
                    {
                        string nv = (string)newValue;
                        uc.OnPropertyChanged("NumVal");
                    }
                });

        // ------------
        private string FormattedResult(double result)
        {
            string formattedRes = string.Format(("{0:" + NumFormat + "}"), result);
            return formattedRes;
        }



        // -------------------

        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        // Using a BindableProperty as the backing store for ReadOnly.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty ReadOnlyProperty =
            BindableProperty.Create(
                "ReadOnly",
                typeof(bool),
                typeof(ucNumericInput),
                false,	//Change this to 0 for int etc.
                BindingMode.Default,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    bool nv = (bool)newValue;

                    ((ucNumericInput)bindable).numEntry.IsVisible = !nv;
                    ((ucNumericInput)bindable).lblText.IsVisible = nv;
                });

        // -------------------

        public ICommand TextChangedCommand
        {
            get { return (ICommand)GetValue(TextChangedCommandProperty); }
            set { SetValue(TextChangedCommandProperty, value); }
        }

        // Using a BindableProperty as the backing store for TextChangedCommand.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty TextChangedCommandProperty =
            BindableProperty.Create(
                "TextChangedCommand",
                typeof(ICommand),
                typeof(ucNumericInput),
                null,	//Change this to 0 for int etc.
                BindingMode.Default,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    ICommand nv = (ICommand)newValue;
                });


        void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (TextChangedCommand!=null)
            {
                TextChangedCommand.Execute(new Tuple<object, TextChangedEventArgs>(sender, args));
            }
        }

    }
}
