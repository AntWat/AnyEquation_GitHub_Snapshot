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
    class JongFunctionGet : JongFunction
    {
        /**
         * Constructor
         */
        public JongFunctionGet(Dictionary<String, JongFunctionAssign> functionlist)
        {
            functionlist_ = functionlist;
        }

        /**
         * Constructor
         */
        public JongFunctionGet(Dictionary<String, JongFunctionAssign> functionlist,
                                String name)
        {
            functionlist_ = functionlist;
            setName(name);
        }

        /**
         * Set the name of the JongFunction
         */
        public void setName(String name)
        {
            clear();
            name_ = name;
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
         * Get the name of the JongFunction
         */
        public String getName()
        {
            return name_;
        }

        /**
         * Evaluate the JongFunction
         */
        public override double eval() //throws JongError
        {
            // check if the JongFunction exists in the functionlist
            if (!functionlist_.ContainsKey(name_))
            {
                throw new JongError(102, name_);
            }

            // evaluate the JongFunction, and provide the parameter values for the 
            // JongFunction
            return functionlist_[name_].eval(values_);
        }

        /**
         * Get the name of the class
         */
        public override String name()
        {
            return "FunctionGet";
        }

        // functionlist_ points to the list with JongFunction assignments
        private Dictionary<String, JongFunctionAssign> functionlist_ =
            new Dictionary<String, JongFunctionAssign>();
        private String name_;  // JongFunction name

    }
}
