using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using AnyEquation.Equations.Model;

namespace AnyEquation.Equations.User_Controls
{
    public abstract class ResultLabel : Label
    {

        public SingleResult SingleResult
        {
            get { return (SingleResult)GetValue(SingleResultProperty); }
            set { SetValue(SingleResultProperty, value); }
        }

        // Using a BindableProperty as the backing store for SingleResult.  This enables animation, styling, binding, etc...
        public static readonly BindableProperty SingleResultProperty =
            BindableProperty.Create(
                "SingleResult",
                typeof(SingleResult),
                typeof(ResultLabel),
                null,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    SingleResult nv = (SingleResult)newValue;
                });

    }

    // ---------------------------
    class FuncNameResultLabel : ResultLabel
    {
    }

    // ---------------------------
    class OpenBracketResultLabel : ResultLabel
    {
    }

    // ---------------------------
    class CloseBracketResultLabel : ResultLabel
    {
    }

    // ---------------------------
    class ValueResultLabel : ResultLabel
    {
    }

    // ---------------------------
    class ConstantResultLabel : ResultLabel
    {
    }

    // ---------------------------
    class OpResultLabel : ResultLabel
    {
    }

    // ---------------------------
    class EqualsResultLabel : OpResultLabel
    {
    }

    // ---------------------------
    class PowerResultLabel : ResultLabel
    {
    }

    // ---------------------------
    class CommaResultLabel : ResultLabel
    {
    }
}
