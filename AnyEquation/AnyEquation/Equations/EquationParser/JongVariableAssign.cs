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
    class JongVariableAssign : JongFunction
    {
        /**
         * Constructor
         */
        public JongVariableAssign(Dictionary<String, Double> variablelist)
        {
            variablelist_ = variablelist;
        }

        /**
         * Constructor
         */
        public JongVariableAssign(Dictionary<String, Double> variablelist,
                    String name,
                    JongFunction value)
        {
            variablelist_ = variablelist;
            //setName(name);       
            set(value);
            name_ = name;         //This has to be here like this, otherwise the name is cleared by the Set
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
            if (values_.Count() != 1)
            {
                throw new JongError(403, name_); // uninitialized value
            }

            // evaluate the variable 
            double value = values_.ElementAt(0).eval();

            // assign the new value
            variablelist_[name_]=value;

            return value;
        }

        public override String name()
        {
            return "VariableAssign";
        }

        // variablelist_ points to the list with variable assignments
        private Dictionary<String, Double> variablelist_ =
            new Dictionary<String, Double>();
        private String name_;    // variable name
    }
}
