using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Code from the source below, modified by Ant Waters:
 * No license required for this code
 */

/***
Expression parser
author: Jos de Jong, 2010-09-26
site:   www.speqmath.com
 */

namespace AnyEquation.Equations.EquationParser
{
    class JongValue : JongFunction
    {
        /**
         * Constructor
         */
        public JongValue()
        {
            value_ = 0.0;
        }

        /**
         * Constructor
         */
        public JongValue(double value)
        {
            setValue(value);
        }

        /**
         * Set a new value.
         */
        public void setValue(double value)
        {
            clear();
            value_ = value;
        }

        /**
         * Get the current value.
         */
        public double getValue()
        {
            return value_;
        }

        /**
         * Evaluate the value
         */
        public override double eval()
        {
            return value_;
        }

        /**
         * Return the name of the function
         */
        public override String name()
        {
            return "Value";
        }

        private double value_;
    }
}
