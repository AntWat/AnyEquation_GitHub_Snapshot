using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.EquationParser
{
    /**
     * Base class JongFunction. All JongFunctions (like Sin, Abs, Add, Subtract, etc.)
     * inherit from this JongFunction
     */
    public abstract class JongFunction
    {
        /*
         * Code from the source below, modified by Ant Waters:
         * No license required for this code
         */

        /***
        Expression parser
        author: Jos de Jong, 2010-09-26
        site:   www.speqmath.com
         */

        /**
         * Constructor
         */
        public JongFunction()
        {
        }

        /**
         * Delete and cleanup all parameters set in this JongFunction
         */
        public virtual void clear()
        {
            // When you override this JongFunction, you have to clear values_ yourself.       
            values_.Clear();
        }

        /**
         * Set one parameter for this JongFunction
         * @param value    The parameter for the JongFunction
         */
        public void set(JongFunction value)
        {
            clear();
            values_.Add(value);
        }

        /**
         * Set two parameter for this JongFunction
         * @param value1    First parameter for the JongFunction
         * @param value2    Second parameter for the JongFunction
         */
        public void set(JongFunction value1, JongFunction value2)
        {
            clear();

            values_.Add(value1);
            values_.Add(value2);
        }

        /**
         * Set a list of parameters for this JongFunction
         * @param values    A vector containing parameters for the JongFunction
         */
        public void set(List<JongFunction> values)
        {
            clear();
            for (int i = 0; i < values.Count(); i++)
            {
                values_.Add(values.ElementAt(i));
            }
        }

        /**
         * Returns the list with parameters present in this JongFunction
         * You can use this JongFunction to traverse an expression tree
         * @return 			The vector with JongFunction parameters
         */
        public List<JongFunction> get() {
            return values_;
        }

        // eval() and name() are abstract, and must be implemented by all subclasses
        public abstract double eval(); //throws JongError;
        public abstract String name();

        protected List<JongFunction> values_ = new List<JongFunction>();

    }
}
