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

/// The class JongLocalVariableGet links to a local function variable

namespace AnyEquation.Equations.EquationParser
{
    class JongLocalVariableGet : JongFunction
    {
        /**
         * Constructor
         */
        public JongLocalVariableGet()
        {
        }

        /**
         * Constructor
         */
        public JongLocalVariableGet(JongLocalVariable localvar)
        {
            set(localvar);
        }

        /**
         * Set a new local variable link
         */
        public void set(JongLocalVariable localvar)
        {
            clear();
            values_.Add(localvar);
        }

        /**
         * Evaluate the linked local variable
         */
        public override double eval() //throws JongError
        {
            if (values_.Count() != 1)
            {
                throw new JongError(402, "");  // local variable uninitialized
            }

            return values_.ElementAt(0).eval();
        }

        /**
         * Return the name of this class
         */
        public override String name()
        {
            return "LocalVariableGet";
        }
    }
}
