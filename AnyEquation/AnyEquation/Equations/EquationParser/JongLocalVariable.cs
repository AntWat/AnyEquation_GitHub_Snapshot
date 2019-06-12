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
    /** 
     * The class LocalVar stores local variables for an assigned function
     * This object will link to another object value, to be defined when evaluating
     * the function
     */
    class JongLocalVariable : JongFunction
    {
        /**
         * Constructor
         */
        public JongLocalVariable()
        {
        }

        /**
         * Constructor
         */
        public JongLocalVariable(String name)
        {
            setName(name);
        }

        /**
         * Set a variable name
         * Note that the variable value must be set via LocalVar.set(value);
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

        /**
         * Evaluate the linked function
         * Note that the variable value must be set via LocalVariable.set(value);
         */
        public override double eval() //throws JongError
        {
            if (values_.Count() != 1)
            {
                throw new JongError(402, name_);  // local variable uninitialized
            }

            return values_.ElementAt(0).eval();
        }

        /**
         * Return the name of this class
         */
        public override String name()
        {
            return "LocalVariable";
        }

        private String name_;
    }
}
