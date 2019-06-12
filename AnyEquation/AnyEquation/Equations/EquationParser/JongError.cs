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
    class JongError : Exception
    {

        /**
         * Create an error with given message id and fill in given string in message
         * @param id    id of the message
         * @param str   a string which will be filled in in the message
         */
        public JongError(int id, String str)
        {
            row_ = -1;
            col_ = -1;
            id_ = id;

            msg_ = string.Format(errorMsg(id_), str);
        }

        /**
         * Create an error with given message id and fill in given string in message
         * @param id    id of the message
         */
        public JongError(int id)
        {
            row_ = -1;
            col_ = -1;
            id_ = id;

            msg_ = errorMsg(id_);
        }

        /**
         * Create an error with given message id and fill in given string in message
         * @param id    id of the message
         * @param str   a string which will be filled in in the message
         * @param row   row where the error occured
         * @param col   column where the error occured 
         */
        public JongError(int id, String str, int row, int col)
        {
            row_ = row;
            col_ = col;
            id_ = id;

            msg_ = string.Format(errorMsg(id_), str);
        }

        /**
         * Create an error with given message id and fill in given string in message
         * @param id    id of the message
         * @param row   row where the error occured 
         * @param col   column where the error occured 
         */
        public JongError(int id, int row, int col)
        {
            row_ = row;
            col_ = col;
            id_ = id;

            msg_ = errorMsg(id_);
        }

        /**
        * Returns the error message, including line and column number
        */
        public String get()
        {
            String res;
            if (row_ == -1)
            {
                if (col_ == -1)
                {
                    res = string.Format("Error: {0}", msg_);
                }
                else
                {
                    res = string.Format("Error: {0} (col {1})", msg_, col_);
                }
            }
            else
            {
                res = string.Format("Error: {0} (ln {1}, col {2})", msg_, row_, col_);
            }

            return res;
        }

        public int get_id()
        {
            return id_;
        }

        /// Private functions

        /**
         * Returns a pointer to the message description for the given message id.
         * Returns "Unknown error" if id was not recognized.
         */
        private String errorMsg(int id)
        {
            switch (id)
            {
                case 1: return "Syntax error in part {0}";
                case 2: return "Syntax error";
                case 3: return "Parenthesis ) missing";
                case 4: return "Empty expression";
                case 5: return "Unexpected part {0}";
                case 6: return "Unexpected end of expression";
                case 7: return "Value expected";
                case 8: return "Wrong number of parameters in function {0}";
                case 9: return "Parenthesis ( expected";

                // wrong or unknown operators, functions, variables
                case 101: return "Unknown operator {0}";
                case 102: return "Unknown function {0}";
                case 103: return "Unknown variable {0}";

                // domain errors
                case 200: return "Too long expression, maximum number of characters exceeded";

                // error in assignments of variables or functions
                case 300: return "Defining variable failed";
                case 301: return "Undefined function {0}";
                case 302: return "Link undefined";
                case 303: return "Illegal variable name {0}";

                // error in functions
                case 400: return "Integer value expected in function {0}";
                case 401: return "Division by zero";
                case 402: return "Local variable {0} uninitialized";
                case 403: return "Variable {0} uninitialized";
            }

            return "Unknown error";
        }

        /// Data  
        private int row_;    /// row where the error occured
        private int col_;    /// column (position) where the error occured
        private int id_;     /// id of the error
        private String msg_;

    }
}
