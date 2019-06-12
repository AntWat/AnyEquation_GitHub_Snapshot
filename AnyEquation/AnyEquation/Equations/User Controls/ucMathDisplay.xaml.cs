using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using AnyEquation.Common;
using AnyEquation.Equations.Model;
using AnyEquation.Equations.Model.Functions;
using System.Globalization;
using System.ComponentModel;
using Xamarin.FormsBook.Toolkit;

namespace AnyEquation.Equations.User_Controls
{
    /// <summary>
    /// Displays a mathematical expression, such as an equation, equation in a graphical format
    /// </summary>
    public partial class ucMathDisplay : ContentView
    {

        #region ------------ Statics ------------

        #endregion ------------ Statics ------------

        #region ------------ Constructors and Life Cycle ------------
        public ucMathDisplay()
        {
            InitializeComponent();

            topContainer.BindingContext = this;
        }

        #endregion ------------ Constructors and Life Cycle ------------


        #region ------------ Fields and Properties ------------


        private int _lineDepth = 2;
        public int LineDepth { get { return _lineDepth; } set { _lineDepth = value; } }

        // -------------------
        public SingleResult MathExpression
        {
            get { return (SingleResult)GetValue(MathExpressionProperty); }
            set { SetValue(MathExpressionProperty, value); }
        }

        // Using a BindableProperty as the backing store for MathExpression.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty MathExpressionProperty =
            BindableProperty.Create(
                "MathExpression",
                typeof(SingleResult),
                typeof(ucMathDisplay),
                null,	//Change this to 0 for int etc.
                BindingMode.Default,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    SingleResult nv = (SingleResult)newValue;
                    ((ucMathDisplay)bindable).CreateContent();
                    ((ucMathDisplay)bindable).OnPropertyChanged("MathExpression");
                });

        
        // -------------------
        public bool AllowHorizonatalScroll
        {
            get { return (bool)GetValue(AllowHorizonatalScrollProperty); }
            set { SetValue(AllowHorizonatalScrollProperty, value); }
        }

        // Using a BindableProperty as the backing store for MathExpression.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty AllowHorizonatalScrollProperty =
            BindableProperty.Create(
                "AllowHorizonatalScroll",
                typeof(bool),
                typeof(ucMathDisplay),
                true,	//Change this to 0 for int etc.
                BindingMode.Default,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    ((ucMathDisplay)bindable).OnPropertyChanged("MathExpression");
                });

        // -------------------
        public string EmptyText
        {
            get { return (string)GetValue(EmptyTextProperty); }
            set { SetValue(EmptyTextProperty, value); }
        }

        // Text to display while the equation is null
        public static readonly BindableProperty EmptyTextProperty =
            BindableProperty.Create(
                "EmptyText",
                typeof(string),
                typeof(ucMathDisplay),
                "No Equation",	//Change this to 0 for int etc.
                BindingMode.Default,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    ((ucMathDisplay)bindable).CreateContent();
                    ((ucMathDisplay)bindable).OnPropertyChanged("MathExpression");
                });

        #endregion ------------ Fields and Properties ------------


        #region ------------ Misc ------------

        // Create all the content to display the MathExpression
        void CreateContent()
        {
            // TODO: Equations: Remove previous content.  Any need to Dispose?

            // Create display
            topContainer.Children.Clear();

            View mainView = null;

            try
            {
                if (MathExpression!=null)
                {
                    mainView = DisplayMathExpression(MathExpression, bSkipOpBrackets: true, lastFn: null);
                }
            }
            catch (Exception ex)
            {
                mainView=DisplayText("Display error!");
            }

            if (mainView==null)
            {
                mainView=DisplayText(EmptyText);
            }

            if (mainView != null)
            {
                if (AllowHorizonatalScroll)
                {
                    ScrollView scrollView = new ScrollView
                    {
                        Orientation = ScrollOrientation.Horizontal,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                    };
                    StackLayout stackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        Padding = new Thickness(0, 0, 0, 10)     // Bottom Was 30!
                    };

                    stackLayout.Children.Add(mainView);
                    scrollView.Content = stackLayout;

                    topContainer.Children.Add(scrollView);
                }
                else
                {
                    topContainer.Children.Add(mainView);
                }
            }

            //DbgPrintVisualTree(topContainer);

            MakeInputTransparent(topContainer);
        }

        Label DisplayText(string text)
        {
            Label lbl = new Label() { Text= text };
            //topContainer.Children.Add(lbl);
            return lbl;
        }

        void MakeInputTransparent(View view)
        {
            view.InputTransparent = true;

            if (view is StackLayout)
            {
                StackLayout layout = (StackLayout)view;
                foreach (var item in layout.Children)
                {
                    MakeInputTransparent(item);
                }
            }
        }

        View DisplayMathExpression(SingleResult expr, bool bSkipOpBrackets, Function lastFn)
        {

            View vwExpr;

            if (expr is FunctionCalc)
            {
                vwExpr=DisplayFunctionCalc((FunctionCalc)expr, bSkipOpBrackets, lastFn);
            }
            else if (expr is SingleValue)
            {
                vwExpr=DisplaySingleValue((SingleValue)expr);
            }
            else
            {
                throw new ArgumentException();  //TODO: Equations
            }

            return vwExpr;

        }

        // ------------------------

        private View DisplaySingleValue(SingleValue expr)
        {
            string text = expr.Text;
            ResultLabel rLbl;

            if (expr is Constant)
            {
                rLbl = new ConstantResultLabel()
                {
                    Text = Constant.RemovePrefix(text),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    SingleResult = expr,
                    FontAttributes = FontAttributes.Italic,
                };

            } else
            {
                rLbl = new ValueResultLabel()
                {
                    Text = expr.Text,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    SingleResult = expr
                };
            }

            return rLbl;
        }

        // ------------------------

        private View DisplayFunctionCalc(FunctionCalc expr, bool bSkipOpBrackets, Function lastFn)
        {
            bool bThisSkipOpBrackets = bSkipOpBrackets;
            bool bNextSkipOpBrackets = false;

            StackLayout stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
            };

            Function fn = expr.Function;

            if (fn is FnEquals) bNextSkipOpBrackets = true;

            if (fn.FuncLayout==FuncLayout.FuncLayout)
            {
                stackLayout.Children.Add(new FuncNameResultLabel
                {
                    Text = fn.Symbol,
                    VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                    SingleResult = expr
                });

                stackLayout.Children.Add(new OpenBracketResultLabel
                {
                    Text = "(",
                    VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                    SingleResult = expr
                });

                AddExpressionInputs(stackLayout, expr.Inputs);
                
                stackLayout.Children.Add(new CloseBracketResultLabel
                {
                    Text = ")",
                    VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                    SingleResult = expr
                });
            }
            else if (fn.FuncLayout == FuncLayout.Op_Term)
            {
                bThisSkipOpBrackets = true;

                if (!bThisSkipOpBrackets)
                {
                    stackLayout.Children.Add(new OpenBracketResultLabel
                    {
                        Text = "(",
                        VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                        SingleResult = expr
                    });
                }

                stackLayout.Children.Add(new OpResultLabel
                {
                    Text = fn.Symbol,
                    VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                    SingleResult = expr
                });

                stackLayout.Children.Add(DisplayMathExpression(expr.Inputs[0], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn));

                if (!bThisSkipOpBrackets)
                {
                    stackLayout.Children.Add(new CloseBracketResultLabel
                    {
                        Text = ")",
                        VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                        SingleResult = expr
                    });
                }
            }
            else if (fn.FuncLayout == FuncLayout.Term_Op_Term)
            {
                if (fn == lastFn)        // is Functions.FnMultiply)
                {
                    bThisSkipOpBrackets = true;
                }
                if (!bThisSkipOpBrackets)
                {
                    stackLayout.Children.Add(new OpenBracketResultLabel
                    {
                        Text = "(",
                        VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                        SingleResult = expr
                    });
                }

                stackLayout.Children.Add(DisplayMathExpression(expr.Inputs[0], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn));

                stackLayout.Children.Add(new OpResultLabel
                {
                    Text = fn.Symbol,
                    VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                    SingleResult = expr
                });

                stackLayout.Children.Add(DisplayMathExpression(expr.Inputs[1], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn));

                if (!bThisSkipOpBrackets)
                {
                    stackLayout.Children.Add(new CloseBracketResultLabel
                    {
                        Text = ")",
                        VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                        SingleResult = expr
                    });
                }
            }
            else if (fn.FuncLayout == FuncLayout.Term_OverOp_Term)
            {
                if (!(fn is FnDivide))
                {
                    // TODO: Equations: What else could it be? What do we show instead of a line?
                }

                if (!bThisSkipOpBrackets)
                {
                    stackLayout.Children.Add(new OpenBracketResultLabel
                    {
                        Text = "(",
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        SingleResult = expr
                    });
                }

                StackLayout slVert = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                };
                stackLayout.Children.Add(slVert);

                View vwTop = DisplayMathExpression(expr.Inputs[0], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn);
                slVert.Children.Add(vwTop);

                BoxView boxView = new BoxView
                {
                    Color = (Color)Application.Current.Resources[App.Key_TextColor],
                    HeightRequest=_lineDepth,
                    VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                };

                slVert.Children.Add(boxView);

                View vwBottom = DisplayMathExpression(expr.Inputs[1], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn);
                slVert.Children.Add(vwBottom);

                MaxWidthBinder maxWidthBinder = new MaxWidthBinder(new List<View> { vwTop, vwBottom });
                boxView.SetBinding(BoxView.WidthRequestProperty, new Binding() { Source = maxWidthBinder, Path = "MaxWidth" });

                if (!bThisSkipOpBrackets)
                {
                    stackLayout.Children.Add(new CloseBracketResultLabel
                    {
                        Text = ")",
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        SingleResult = expr
                    });
                }
            }
            else if (fn.FuncLayout == FuncLayout.Term_Superscript_Term)
            {
                StackLayout slHoriz = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
                stackLayout.Children.Add(slHoriz);

                View vw = DisplayMathExpression(expr.Inputs[0], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: fn);
                slHoriz.Children.Add(vw);

                SuperscriptText ssText = new SuperscriptText()
                {
                    Text = FunctionCalc.MathExpressionAsText(expr.Inputs[1]),       // Not perfect for a complex exponent....
                    //VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                    BackgroundColor = (Color)Application.Current.Resources[App.Key_BackgroundColor],
                    TextColor = (Color)Application.Current.Resources[App.Key_TextColor],
                };

                ssText.SetBinding(SuperscriptText.FontSizeProperty, new Binding() { Source = vw, Path = "FontSize" });
                slHoriz.Children.Add(ssText);
            }

            return stackLayout;
        }


        private void AddExpressionInputs(StackLayout stackLayout, IList<SingleResult> inputs)
        {
            bool bNextSkipOpBrackets = (inputs.Count == 1);

            for (int i = 0; i < inputs.Count; i++)
            {
                SingleResult sr = inputs[i];
                stackLayout.Children.Add(DisplayMathExpression(inputs[i], bSkipOpBrackets: bNextSkipOpBrackets, lastFn: null));

                if (i>0)
                {
                    stackLayout.Children.Add(new OpResultLabel
                    {
                        Text = ",",
                        VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center,
                        SingleResult = sr
                    });
                }
            }
        }

        // -----------------------------------
        private string DbgPrintVisualTree(View view)
        {
            string visualTree = "";
            
            DbgPrintVisualTree2(view, "", "   ", ref visualTree, bAllLabelsAsLabel:true);
            return visualTree;
        }

        private void DbgPrintVisualTree2(View view, string CurrentIndent, string IndentStep, ref string visualTree, bool bAllLabelsAsLabel)
        {
            if ((view is Label) && bAllLabelsAsLabel)
            {
                visualTree += string.Format("{0}<Label ",
                                     CurrentIndent);
            }
            else
            {
                visualTree += string.Format("{0}<{1} ",
                                     CurrentIndent, view.GetType().Name);
            }


            DbgPrintProperties(view, "", ref visualTree);
            visualTree += string.Format(" >\n");

            if (view is StackLayout)
            {
                StackLayout layout = (StackLayout)view;
                foreach (var item in layout.Children)
                {
                    DbgPrintVisualTree2(item, (CurrentIndent + IndentStep), IndentStep, ref visualTree, bAllLabelsAsLabel);
                }
            }

            if ((view is Label) && bAllLabelsAsLabel)
            {
                visualTree += string.Format("{0}</Label",
                                     CurrentIndent);
            }
            else
            {
                visualTree += string.Format("{0}</{1}",
                                     CurrentIndent, view.GetType().Name);
            }
            visualTree += string.Format(" >\n");
        }

        private void DbgPrintProperties(View view, string CurrentIndent, ref string visualTree)
        {
            if (view is StackLayout)
            {
                StackLayout vw = (StackLayout)view;
                visualTree += string.Format("{0}Orientation=\"{1}\" HorizontalOptions=\"{2}\" VerticalOptions=\"{3}\"", 
                                         CurrentIndent, vw.Orientation.ToString(), LayoutToString(vw.HorizontalOptions), LayoutToString(vw.VerticalOptions));
            }
            else if (view is SuperscriptText)
            {
                SuperscriptText vw = (SuperscriptText)view;
                visualTree += string.Format("{0}Text=\"{1}\"",
                                         CurrentIndent, vw.Text);
                //, vw.BackgroundColor, vw.TextColor
            }
            else if (view is Label)
            {
                Label vw = (Label)view;
                visualTree += string.Format("{0}Text=\"{1}\"",
                                         CurrentIndent, vw.Text);
            }
            else if (view is BoxView)
            {
                BoxView vw = (BoxView)view;
                visualTree += string.Format("{0}HeightRequest=\"{1}\" HorizontalOptions=\"{2}\"",
                                         CurrentIndent, vw.HeightRequest, LayoutToString(vw.HorizontalOptions));
                //Color={1} , vw.Color
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private string LayoutToString(LayoutOptions options)
        {
            string sLocation = "";

            switch (options.Alignment)
            {
                case LayoutAlignment.Start:
                    sLocation = "Start";
                    break;
                case LayoutAlignment.Center:
                    sLocation = "Center";
                    break;
                case LayoutAlignment.End:
                    sLocation = "End";
                    break;
                case LayoutAlignment.Fill:
                    sLocation = "Fill";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (options.Expands)
            {
                return sLocation + "AndExpand";
            }
            else
            {
                return sLocation;
            }

        }

        #endregion ------------ Misc ------------

        private class MaxWidthBinder : ViewModelBase
        {
            public MaxWidthBinder(IList<View> views)
            {
                foreach (var view in views)
                {
                    Views.Add(view);
                    view.PropertyChanged += ViewPropertyChanged;
                }
                OnPropertyChanged("MaxWidth");
            }

            public IList<View> Views { get; } = new List<View>();
            public double MaxWidth { get
                {
                    double maxWidth = 0;
                    foreach (var item in Views)
                    {
                        if (item.Width > maxWidth) { maxWidth = item.Width; }
                    }
                    return maxWidth;
                }
            }

            void ViewPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("Width"))
                {
                    OnPropertyChanged("MaxWidth");
                }
            }
        }

    }
}
