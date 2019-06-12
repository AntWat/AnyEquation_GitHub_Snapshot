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
    class JongVariableGet : JongFunction
    {
        /**
         * Constructor
         */
        JongVariableGet(Dictionary<String, Double> variablelist)
        {
            variablelist_ = variablelist;
        }

        /**
         * Constructor
         */
        public JongVariableGet(Dictionary<String, Double> variablelist, String name)
        {
            variablelist_ = variablelist;
            setName(name);
        }

        /**
         * Set a variable name
         */
        public void setName(String name)
        {
            clear();
            name_ = name;
        }

        /**
         * Get the variable name
         */
        public String getName()
        {
            return name_;
        }

        /**
         * Clear all settings: name, values
         */
        public override void clear()
        {
            name_ = "";
            values_.Clear();
        }

        public override double eval() //throws JongError
        {
            // check if this variable exists in the variablelist
            if (!variablelist_.ContainsKey(name_))
            {
                throw new JongError(103, name_);
            }

            // return the value of the variable
            return variablelist_[name_];
        }

        public override String name()
        {
            return "VariableGet";
        }

        // variablelist_ points to the list with variable assignments
        private Dictionary<String, Double> variablelist_ =
            new Dictionary<String, Double>();
        private String name_;    // variable name
    }
}
