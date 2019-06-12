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
    /// JongFunction assignment
    class JongFunctionAssign : JongFunction
    {
        /**
         * Constructor
         */
        public JongFunctionAssign(Dictionary<String, JongFunctionAssign> functionlist)
        {
            functionlist_ = functionlist;
        }

        /**
         * Constructor
         */
        public JongFunctionAssign(Dictionary<String, JongFunctionAssign> functionlist,
                              String name,
                              List<JongLocalVariable> variables,
                              JongFunction expr)
        {
            functionlist_ = functionlist;
            set(name, variables, expr);
        }

        /**
         * Clear all settings: name, variables, expression
         */
        public override void clear()
        {
            name_ = "";
            variables_.Clear();
            values_.Clear();
        }

        /**
         * Set JongFunction name, variables, and expression
         * For example the JongFunction f(x,y)=x^y+2 whould be inserted as:
         * @param name          A string with the JongFunction name, for example "f"
         * @param variables     A vector containing the local variables x and y
         * @param expr          The JongFunction expression, like x^y+2
         */
        public void set(String name,
                        List<JongLocalVariable> variables,
                        JongFunction expr)
        {
            set(expr); // must be executed first, this clears the JongFunction too.
            setName(name);
            setVariables(variables);
        }

        /**
         * Set the name of the JongFunction
         */
        public void setName(String name)
        {
            name_ = name;
        }

        /**
         * Returns the name of the JongFunction
         */
        public String getName()
        {
            return name_;
        }

        /**
         * Set JongFunction variables
         */
        public void setVariables(List<JongLocalVariable> variables)
        {
            variables_ = variables;
        }

        /**
         * Get JongFunction variables
         */
        public List<JongLocalVariable> getVariables()
        {
            return variables_;
        }

        /**
         * Evaluate the JongFunction assignment. This will register the JongFunction
         * in the functionlist
         */
        public override double eval()
        {
            functionlist_[name_]=this;

            // TODO: Equations: return a message?
            return 0.0;
        }

        /**
         * Evaluate the JongFunction for the provided parameter values.
         * @param values    The parameter values that will be filled in as
         *                  local variables.
         */
        public double eval(List<JongFunction> values) //throws JongError
        {
            if (values_.Count() != 1)
            {
                // expression of the JongFunction is undefined
                throw new JongError(301, name());
            }

            if (values.Count() != variables_.Count())
            {
                // wrong number of parameter values provided
                throw new JongError(8, name_);
            }

            // link the JongFunction variables to the provided parameters
            for (int i = 0; i < variables_.Count(); i++)
            {
                variables_.ElementAt(i).set(values.ElementAt(i));
            }

            // evaluate the JongFunction
            return values_.ElementAt(0).eval();
        }

        /**
         * Returns the name of the class
         */
        public override String name()
        {
            return "FunctionAssign";
        }

        // functionlist_ points to the list with JongFunction assignments
        private Dictionary<String, JongFunctionAssign> functionlist_ =
            new Dictionary<String, JongFunctionAssign>();
        private String name_;
        private List<JongLocalVariable> variables_ = new List<JongLocalVariable>();

    }
}
