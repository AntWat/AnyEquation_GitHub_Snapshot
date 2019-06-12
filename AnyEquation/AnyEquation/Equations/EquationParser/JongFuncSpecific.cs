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
    class JongFuncSpecific : JongFunction
    {

        public JongFuncSpecific(String funcName)
        {
            _name = funcName;
            clear();
        }

        public JongFuncSpecific(String funcName, JongFunction arg1)
        {
            _name = funcName;
            clear();
            set(arg1);
        }

        public JongFuncSpecific(String funcName, JongFunction arg1, JongFunction arg2)
        {
            _name = funcName;
            clear();
            set(arg1, arg2);
        }

        public override double eval() //throws JongError
        {
            if (values_.Count() != 2)
            {
                throw new JongError(8, name()); // wrong number of parameters
            }

            return values_.ElementAt(0).eval() * values_.ElementAt(1).eval();
        }

        public override String name()
        {
            return _name;
        }

        private String _name;

    }
}
