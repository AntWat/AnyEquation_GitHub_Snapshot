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
    public class JongParser
    {

        ICheckModelName _checkModelName;

        /**
         * Constructor
         */
        public JongParser()
        {
            // initialize the JongParser
            init();
        }

        /**
         * Parse the given expression
         * @param new_expr   A string containing an expression, for example "2+3"
         * @return JongFunction
         */
        public JongFunction Parse(String new_expr, ICheckModelName checkModelName, JongErrWarn errW)
        {
            try
            {
                _checkModelName = checkModelName;

                // initialize all variables
                expr = new_expr;
                ans = 0.0;

                // Pare the expression into a function tree format
                JongFunction function = parse_start();

                //TODO: Equations: Commented out for C#
                //@SuppressWarnings("unused")


                String dbgString = FuncToString(function, "    ", "++++", errW);
                dbgString += "\n";

                //            System.out.print(dbgString);

                return function;

            }
            catch (JongError err)
            {
                errW.Init(ErrWStat.UNKNOWN_ERROR, err.get(), false);
                return null;
            }
        }

        /**
         * Parse the given expression
         * @param new_expr   A string containing an expression, for example "2+3"
         * @return ans       A string containing the answer, or an error when
         *                   an error occurred
         */
        public String ParseAndEval(String new_expr, ICheckModelName checkModelName, JongErrWarn errW)
        {
            try
            {
                _checkModelName = checkModelName;

                // initialize all variables
                expr = new_expr;
                ans = 0.0;

                // Pare the expression into a function tree format
                JongFunction function = parse_start();

                String dbgString = FuncToString(function, "    ", "----", errW);
                dbgString += "\n";

                //System.out.print(dbgString);

                // evaluate the expression
                ans = function.eval();


                if (function is JongFunctionAssign) {
                    // function assignment
                    JongFunctionAssign fa = (JongFunctionAssign)function;
                    String desc = "";
                    desc += fa.getName();
                    desc += "(";
                    for (int i = 0; i < fa.getVariables().Count(); i++)
                    {
                        if (i > 0)
                        {
                            desc += ", ";
                        }
                        desc += fa.getVariables().ElementAt(i).getName();
                    }
                    desc += ")";

                    ans_str = "Function " + desc + " defined";
                } else if (function is JongVariableAssign) {
                    // variable assignment
                    JongVariableAssign JongVariableAssign = (JongVariableAssign)function;
                    ans_str = string.Format("%s = %g",
                                            JongVariableAssign.getName(),
                                            ans);

                    // add the answer to memory as variable "Ans"
                    variablelist["ANS"]=ans;
                } else {
                    //DecimalFormat formatter = new DecimalFormat("#.##########");
                    ans_str = string.Format("{0:#.##########}",ans);

                    // add the answer to memory as variable "Ans"
                    variablelist["ANS"]=ans;
                }

            }
            catch (JongError err)
            {
                errW.Init(ErrWStat.UNKNOWN_ERROR, err.get(), false);
                ans_str = err.get();
                expr = "";
            }

            return ans_str;
        }


        /**
         * Get the next character from the expression.
         * The character is stored into the char t.
         * If the end of the expression is reached, the function puts null ('\0')
         * in t.
         */
        private void getChar()
        {
            t_pos++;
            if (t_pos < expr.Length)
            {
                t = expr[t_pos];
            }
            else
            {
                t = '\0';
            }
        }

        /**
         * Get the first character from the expression.
         * The character is stored into the char t.
         * If the end of the expression is reached, the function puts zero ('\0')
         * in t.
         */
        private void getFirstChar()
        {
            t_pos = 0;
            if (expr.Length > 0)
            {
                t = expr[0];
            }
            else
            {
                t = '\0';
            }
        }

        /**
         * Get next token in the current string expr.
         * Uses the JongParser data expr, e, token, t, token_type and err
         */
        private void getToken() //throws JongError
        {
            token_type = TOKENTYPE.NOTHING;
            token = "";

            // skip over whitespaces
            while (t == ' ' || t == '\t')
            {    // space or tab
                getChar();
            }

            // check for end of expression
            if (t == '\0')
            {
                // token is still empty
                token_type = TOKENTYPE.DELIMETER;
                return;
            }

            // check for minus, comma, parentheses, factorial
            if (t == '-' || t == ',' || t == '(' || t == ')' || t == '!')
            {
                token_type = TOKENTYPE.DELIMETER;
                token += t;
                getChar();
                return;
            }

            // check for operators (delimeters)
            if (isDelimeter(t))
            {
                token_type = TOKENTYPE.DELIMETER;
                while (isDelimeter(t))
                {
                    token += t;
                    getChar();
                }
                return;
            }

            // check for a number
            if (isDigitDot(t))
            {
                token_type = TOKENTYPE.NUMBER;
                while (isDigitDot(t))
                {
                    token += t;
                    getChar();
                }

                // check for scientific notation like "2.3e-4" or "1.23e50"
                if (t == 'E' || t == 'e')
                {
                    token += t;
                    getChar();

                    if (t == '+' || t == '-')
                    {
                        token += t;
                        getChar();
                    }

                    while (isDigit(t))
                    {
                        token += t;
                        getChar();
                    }
                }
                return;
            }

            // check for variables or functions
            if (isAlpha(t))
            {
                while (isAlpha(t) || isDigit(t))
                {
                    token += t;
                    getChar();
                }

                // skip whitespaces
                while (isWhiteSpace(t))
                { // space or tab
                    getChar();
                }

                // check the next non-whitespace character
                if (t == '(')
                {
                    token_type = TOKENTYPE.FUNCTION;
                }
                else
                {
                    token_type = TOKENTYPE.VARIABLE;
                }

                return;
            }

            // something unknown is found, wrong characters -> a syntax error
            token_type = TOKENTYPE.UNKNOWN;
            while (t != '\0')
            {
                token += t;
                getChar();
            }
            throw new JongError(1, token, row(), col());
        }

        /**
         * checks if the given char c is whitespace
         * whitespace when space chr(32) or tab chr(9)
         */
        private bool isWhiteSpace(char c)
        {
            return c == 32 ||
                   c == 9;  // space or tab
        }

        /**
         * checks if the given char c is a delimiter
         * minus is checked apart, can be unary minus
         */
        private bool isDelimeter(char c)
        {
            return c == '&' ||
                   c == '|' ||
                   c == '<' ||
                   c == '>' ||
                   c == '=' ||
                   c == '+' ||
                   c == '/' ||
                   c == '*' ||
                   c == '%' ||
                   c == '^' ||
                   c == '!';
        }

        /**
         * checks if the given char c is a letter or underscore
         */
        private bool isAlpha(char c)
        {
            const int greekAndCoptic_Start = 913;       //hex 0391;
            const int greekAndCoptic_End = 1023;        // 03FF;

            return ((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= greekAndCoptic_Start && c <= greekAndCoptic_End) ||
                    (c == '_') 
                    );
        }

        /**
         * checks if the given char c is a digit or dot
         */
        private bool isDigitDot(char c)
        {
            return ((c >= '0' && c <= '9') ||
                     c == '.');
        }

        /**
         * checks if the given char c is a digit
         */
        private bool isDigit(char c)
        {
            return ((c >= '0' && c <= '9'));
        }

        /**
         * checks if the given variable name is legal to use, i.e. not
         * equal to "pi", "e", etc.
         */
        private bool isLegalVariableName(String name)
        {
            String nameUpper = name.ToUpper();

            bool isLegal = !constants.ContainsKey(nameUpper);

            return isLegal;
        }

        // parse levels below are in order or precedence
        private JongFunction parse_start() //throws JongError
        {

            // get the first character in expression
            getFirstChar();

            getToken();

            // check if expression is not empty
            if (token.Length==0)
            {
                throw new JongError(4, row(), col());
            }

            JongFunction ans = parse_variable_ass();

            // check for garbage at the end of the expression
            // an expression ends with a character '\0' and token_type DELIMETER
            if (!(token.Length==0))
            {
                if (token_type == TOKENTYPE.DELIMETER)
                {
                    // user entered a not existing operator like "//"
                    throw new JongError(101, token, row(), col());
                }
                else
                {
                    throw new JongError(5, token, row(), col());
                }
            }

            return ans;
        }

        /*
         * assignment of variable, for example "a = 3.4/2"
         */
        private JongFunction parse_variable_ass() //throws JongError
        {
            if (token_type == TOKENTYPE.VARIABLE)
            {
                // copy current token (we may have to revert to it)
                char t_now = t;
                int t_pos_now = t_pos;
                TOKENTYPE token_type_now = token_type;
                String token_now = token;

                getToken();
                if (token.Equals("="))
                {
                    // assignment of a variable
                    String var_name = token_now;        //.ToUpper();

                    // check if variable name is legal
                    if (!isLegalVariableName(var_name))
                    {
                        throw new JongError(303, var_name);
                    }

                    getToken();
                    return new JongVariableAssign(variablelist,
                                              var_name,
                                              parse_conditions());
                }

                // No variable defined. go back to previous token
                t = t_now;
                t_pos = t_pos_now;
                token_type = token_type_now;
                token = token_now;
            }

            return parse_function_ass();
        }


        /*
         * assignment of variable, for example "a = 3.4/2"
         */
        private JongFunction parse_function_ass() //throws JongError
        {
            if (token_type == TOKENTYPE.FUNCTION)
            {
                // check for a function assignment like "f(a,b)=a*b"

                // copy current token (we may have to revert to it)
                char t_now = t;
                int t_pos_now = t_pos;
                TOKENTYPE token_type_now = token_type;
                String token_now = token;

                String name = token;        //.ToUpper();

                getToken();
                if (token.Equals("("))
                {
                    // possible assignment of a function.
                    // Verify this by checking if it has the form "f(a,b,c,...)="
                    bool hasCorrectParams = true;
                    localvariablelist = new List<JongLocalVariable>();
                    while (true)
                    {
                        getToken();
                        if (token_type != TOKENTYPE.VARIABLE)
                        {
                            // wrong, we would have expected a variable name.
                            // So this is no function assignment.
                            hasCorrectParams = false;
                            break;
                        }

                        // add this variable to the local variable list
                        String var_name = token;        //.ToUpper();
                        localvariablelist.Add(new JongLocalVariable(var_name));

                        getToken();
                        if (token.Equals(")"))
                        {
                            // closing parenthesis found
                            hasCorrectParams = true;
                            break;
                        }

                        if (!token.Equals(","))
                        {
                            // wrong, we would have expected a comma.
                            // So this is no function assignment.
                            hasCorrectParams = false;
                            break;
                        }
                    }

                    if (hasCorrectParams)
                    {
                        getToken();

                        if (token.Equals("="))
                        {
                            // we have a function assignment like "f(a,b)=a*b"

                            // assign a new function
                            // note: the localvariablelinks will be copied and owned by
                            // the assigned function. We do not need to delete the links
                            getToken();
                            JongFunctionAssign function =
                                new JongFunctionAssign(functionlist,
                                                    name,
                                                    localvariablelist,
                                                    parse_conditions());

                            return function;
                        }
                    }

                    // Function not assigned. Clear the created local variables
                    localvariablelist.Clear();

                    // revert to previous token
                    t = t_now;
                    t_pos = t_pos_now;
                    token_type = token_type_now;
                    token = token_now;
                }
            }

            return parse_conditions();
        }


        /*
         * conditional operators and bitshift
         */
        private JongFunction parse_conditions() //throws JongError
        {
            JongFunction ans = parse_comparison();
            while (token.Equals("&") ||
                   token.Equals("|") ||
                   token.Equals("<<") ||
                   token.Equals(">>"))
            {
                String newFncName = token; getToken(); ans = new JongFuncSpecific(newFncName, ans, parse_comparison());

                //Commented out by Ant:            if      (token.Equals("&")) {getToken(); ans = new And(ans, parse_comparison());}
                //Commented out by Ant:             else if (token.Equals("|")) {getToken(); ans = new Or(ans, parse_comparison());}
                //Commented out by Ant: else if (token.Equals("<<")) {getToken(); ans = new Bitshiftleft(ans, parse_comparison());}
                //Commented out by Ant: else if (token.Equals(">>")) {getToken(); ans = new Bitshiftright(ans, parse_comparison());}
            }

            return ans;
        }

        /*
         * comparison operators
         */
        private JongFunction parse_comparison() //throws JongError
        {
            JongFunction ans = parse_addsubtract();

            while (token.Equals("=") ||
                   token.Equals("<>") ||
                   token.Equals("<") ||
                   token.Equals(">") ||
                   token.Equals("<=") ||
                   token.Equals(">="))
            {
                String newFncName = token; getToken(); ans = new JongFuncSpecific(newFncName, ans, parse_addsubtract());

                //Commented out by Ant: if      (token.Equals("="))  {getToken(); ans = new Equal(ans, parse_addsubtract());}
                //Commented out by Ant: else if (token.Equals("<>")) {getToken(); ans = new Unequal(ans, parse_addsubtract());}
                //Commented out by Ant: else if (token.Equals("<"))  {getToken(); ans = new Smaller(ans, parse_addsubtract());}
                //Commented out by Ant: else if (token.Equals(">"))  {getToken(); ans = new Larger(ans, parse_addsubtract());}
                //Commented out by Ant: else if (token.Equals("<=")) {getToken(); ans = new Smallereq(ans, parse_addsubtract());}
                //Commented out by Ant: else if (token.Equals(">=")) {getToken(); ans = new Largereq(ans, parse_addsubtract());}
            }

            return ans;
        }

        /*
         * add or subtract
         */
        private JongFunction parse_addsubtract() //throws JongError
        {
            JongFunction ans = parse_multiplydivide();

            while (token.Equals("+") ||
                   token.Equals("-"))
            {
                String newFncName = token; getToken(); ans = new JongFuncSpecific(newFncName, ans, parse_multiplydivide());

                //Commented out by Ant: if      (token.Equals("+")) {getToken(); ans = new Add(ans, parse_multiplydivide());}
                //Commented out by Ant: else if (token.Equals("-")) {getToken(); ans = new Subtract(ans, parse_multiplydivide());}
            }

            return ans;
        }


        /*
         * multiply, divide, modulus
         */
        private JongFunction parse_multiplydivide() //throws JongError
        {
            JongFunction ans = parse_pow();

            while (token.Equals("*") ||
                   token.Equals("/") ||
                   token.Equals("%") ||
                   token.Equals("||"))
            {
                String newFncName = token; getToken(); ans = new JongFuncSpecific(newFncName, ans, parse_pow());

                //Commented out by Ant: if      (token.Equals("*")) {getToken(); ans = new Multiply(ans, parse_pow());}
                //Commented out by Ant: else if (token.Equals("/")) {getToken(); ans = new Divide(ans, parse_pow());}
                //Commented out by Ant: else if (token.Equals("%")) {getToken(); ans = new Modulus(ans, parse_pow());}
                //Commented out by Ant: else if (token.Equals("||")) {getToken(); ans = new Xor(ans, parse_pow());}
            }

            return ans;
        }


        /*
         * power
         */
        private JongFunction parse_pow() //throws JongError
        {
            JongFunction ans = parse_factorial();

            while (token.Equals("^"))
            {
                //getToken();
                String newFncName = token; getToken(); ans = new JongFuncSpecific(newFncName, ans, parse_factorial());

                //Commented out by Ant: ans = new Pow(ans, parse_factorial());
            }

            return ans;
        }

        /*
         * Factorial
         */
        private JongFunction parse_factorial() //throws JongError
        {
            JongFunction ans = parse_unaryminus();

            while (token.Equals("!"))
            {
                //getToken();
                String newFncName = token; getToken(); ans = new JongFuncSpecific(newFncName, ans);

                //Commented out by Ant: ans = new Factorial(ans);
            }

            return ans;
        }

        /*
         * Unary minus
         */
        private JongFunction parse_unaryminus() //throws JongError
        {
            if (token.Equals("-"))
            {
                String newFncName = token; getToken();
                return new JongFuncSpecific(newFncName, parse_function());

                //Commented out by Ant: return new UnaryMinus(parse_function());
            }

            return parse_function();
        }


        /*
         * functions
         */
        private JongFunction parse_function() //throws JongError
        {
            JongFunction ans;

            if (token_type == TOKENTYPE.FUNCTION)
            {
                String fn_name = token;     //.ToUpper();

                getToken();

                // parse the parentheses and parameters of the function
                List<JongFunction> values = new List<JongFunction>();
                if (token.Equals("("))
                {
                    getToken();
                    values.Add(parse_conditions());

                    // parse a list with parameters
                    while (token.Equals(","))
                    {
                        getToken();
                        values.Add(parse_conditions());
                    }

                    if (!token.Equals(")"))
                    {
                        throw new JongError(3, row(), col());
                    }
                    getToken();

                    //                // arithmetic functions
                    //                
                    //                List<String> oneArgFuncNames = new ArrayList<String>();
                    //                
                    //                oneArgFuncNames.Add("ABS");	//)      ans = new Abs();
                    //                //
                    //                oneArgFuncNames.Add("CUBE");	//)     ans = new Cube();
                    //                oneArgFuncNames.Add("EXP");	//)      ans = new Exp();
                    //                oneArgFuncNames.Add("LOG");	//)      ans = new Log();
                    //                oneArgFuncNames.Add("LOG10");	//)    ans = new Log10();
                    //                oneArgFuncNames.Add("POW");	//)      ans = new Pow();
                    //                oneArgFuncNames.Add("SIGN");	//)     ans = new Sign();
                    //                oneArgFuncNames.Add("SQRT");	//)     ans = new Sqrt();
                    //                oneArgFuncNames.Add("SQUARE");	//)   ans = new Square();
                    //                
                    //                                // probabiliy functions
                    //                oneArgFuncNames.Add("FACTORIAL");	//)ans = new Factorial();
                    //                
                    //                                // statistical functions
                    //                oneArgFuncNames.Add("AVG");	//)      ans = new Avg();
                    //                oneArgFuncNames.Add("MAX");	//)      ans = new Max();
                    //                oneArgFuncNames.Add("MEDIAN");	//)   ans = new Median();
                    //                oneArgFuncNames.Add("MIN");	//)      ans = new Min();
                    //                oneArgFuncNames.Add("PROD");	//)     ans = new Prod();
                    //                oneArgFuncNames.Add("STD");	//)      ans = new Std();
                    //                oneArgFuncNames.Add("SUM");	//)      ans = new Sum();
                    //                oneArgFuncNames.Add("VAR");	//)      ans = new Var();
                    //                
                    //                                // trigonometric functions
                    //                oneArgFuncNames.Add("SIN");	//)      ans = new Sin();
                    //                oneArgFuncNames.Add("COS");	//)      ans = new Cos();
                    //                oneArgFuncNames.Add("TAN");	//)      ans = new Tan();
                    //                oneArgFuncNames.Add("ASIN");	//)     ans = new Asin();
                    //                oneArgFuncNames.Add("ACOS");	//)     ans = new Acos();
                    //                oneArgFuncNames.Add("ATAN");	//)     ans = new Atan();
                    //                oneArgFuncNames.Add("ATAN2");	//)    ans = new Atan2();

                    bool bFound = false;
                    String sFound = "";

                    bFound = _checkModelName.CheckModelName(fn_name, "");
                    if (bFound) sFound = fn_name;

                    //                for (String knownName : oneArgFuncNames)
                    //                {
                    //                	if (fn_name.Equals(knownName))
                    //                	{
                    //                		sFound=fn_name;
                    //                     	bFound = true;
                    //                     	break;
                    //                	}
                    //                }

                    //Commented out by Ant: 
                    //                if      (fn_name.Equals("ABS"))      ans = new Abs();
                    //
                    //                else if (fn_name.Equals("CUBE"))     ans = new Cube();
                    //                else if (fn_name.Equals("EXP"))      ans = new Exp();
                    //                else if (fn_name.Equals("LOG"))      ans = new Log();
                    //                else if (fn_name.Equals("LOG10"))    ans = new Log10();
                    //                else if (fn_name.Equals("POW"))      ans = new Pow();
                    //                else if (fn_name.Equals("SIGN"))     ans = new Sign();
                    //                else if (fn_name.Equals("SQRT"))     ans = new Sqrt();
                    //                else if (fn_name.Equals("SQUARE"))   ans = new Square();
                    //
                    //                // probabiliy functions
                    //                else if (fn_name.Equals("FACTORIAL"))ans = new Factorial();
                    //
                    //                // statistical functions
                    //                else if (fn_name.Equals("AVG"))      ans = new Avg();
                    //                else if (fn_name.Equals("MAX"))      ans = new Max();
                    //                else if (fn_name.Equals("MEDIAN"))   ans = new Median();
                    //                else if (fn_name.Equals("MIN"))      ans = new Min();
                    //                else if (fn_name.Equals("PROD"))     ans = new Prod();
                    //                else if (fn_name.Equals("STD"))      ans = new Std();
                    //                else if (fn_name.Equals("SUM"))      ans = new Sum();
                    //                else if (fn_name.Equals("VAR"))      ans = new Var();
                    //
                    //                // trigonometric functions
                    //                else if (fn_name.Equals("SIN"))      ans = new Sin();
                    //                else if (fn_name.Equals("COS"))      ans = new Cos();
                    //                else if (fn_name.Equals("TAN"))      ans = new Tan();
                    //                else if (fn_name.Equals("ASIN"))     ans = new Asin();
                    //                else if (fn_name.Equals("ACOS"))     ans = new Acos();
                    //                else if (fn_name.Equals("ATAN"))     ans = new Atan();
                    //                else if (fn_name.Equals("ATAN2"))    ans = new Atan2();

                    if (bFound)
                    {
                        ans = new JongFuncSpecific(sFound);
                    }
                    else if (functionlist.ContainsKey(fn_name))
                    {
                        // it is an assigned function
                        ans = new JongFunctionGet(functionlist, fn_name);
                    }
                    else
                    {
                        // unknown function
                        throw new JongError(102, fn_name, row(), col());
                    }

                    // apply the parameter values to the found function
                    ans.set(values);

                    return ans;
                }
                else
                {
                    throw new JongError(3, row(), col());
                }
            }

            return parse_parentheses();
        }


        /*
         * parentheses
         */
        private JongFunction parse_parentheses() //throws JongError
        {
            // check if it is a parenthesized expression
            if (token.Equals("("))
            {
                getToken();
                JongFunction ans = parse_conditions(); // start again

                if (!token.Equals(")"))
                {
                    throw new JongError(3, row(), col());
                }
                getToken();
                return ans;
            }

            // if not parenthesized then the expression is a value
            return parse_number();
        }

        /*
         * parse a number
         */
        private JongFunction parse_number() //throws JongError
        {
            if (token_type == TOKENTYPE.NUMBER)
            {
                // this is a number
                JongFunction ans;
                if (token.Equals("."))
                {
                    ans = new JongValue(0.0);
                }
                else
                {
                    ans = new JongValue(double.Parse(token));
                }
                getToken();

                return ans;
            }

            return parse_constant();
        }

        /*
         * parse a constant or variable
         */
        private JongFunction parse_constant() //throws JongError
        {
            if (token_type == TOKENTYPE.VARIABLE)
            {
                // first make an uppercase copy of the variable name
                String const_name = token;      //.ToUpper();

                if (constants.ContainsKey(const_name))
                {
                    // the constant is found
                    getToken();
                    return new JongValue(constants[const_name]);
                }
            }

            return parse_variable();
        }

        /*
         * parse a constant or variable
         */
        private JongFunction parse_variable() //throws JongError
        {
            JongFunction ans;

            if (token_type == TOKENTYPE.VARIABLE)
            {
                // first make an uppercase copy of the variable name
                String var_name = token;    //.ToUpper();

                getToken();

                // check if this is a local variable
                bool found = false;
                int i = 0;
                for (i = 0; i < localvariablelist.Count(); i++)
                {
                    if (localvariablelist.ElementAt(i).getName().Equals(var_name))
                    {

                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    // create a link to this function variable
                    ans = new JongLocalVariableGet(localvariablelist.ElementAt(i));
                }
                else
                {
                    // create a normal variable object
                    ans = new JongVariableGet(variablelist, var_name);
                }
            }
            else
            {
                // unexpected end of the expression... uh oh
                ans = parse_end();
            }

            return ans;
        }

        /*
         * Evaluated when the expression is not yet ended but expected to end
         */
        private JongFunction parse_end() //throws JongError
        {
            if (token.Length==0)
            {
                // syntax error or unexpected end of expression
                throw new JongError(6, row(), col());
            }
            else
            {
                throw new JongError(7, row(), col());
            }
        }

        /**
         * Initialize the JongParser, load constants
         */
        private void init()
        {
            //Ant waters Mod:  I am remvoing these because I want them to stay as Text....
            //        constants.put("PI", 3.1415926535897932384626433832795);
            //        constants.put("E",  2.7182818284590452353602874713527);
        }

        /**
         * Shortcut for getting the current row value (one based)
         * Returns the line of the currently handled expression
         */
        private int row()
        {
            return -1;
        }

        /**
         * Shortcut for getting the current col value (one based)
         * Returns the column (position) where the last token starts
         */
        private int col()
        {
            return t_pos - token.Length + 1;
        }

        // enumerations
        private enum TOKENTYPE { NOTHING, DELIMETER, NUMBER, VARIABLE, FUNCTION, UNKNOWN };

        // data
        private String expr = "";               // holds the expression
        private int t_pos = 0;                  // points to the current position in expr
        private char t = '\0';                  // holds the current token character in expr
        private String token = "";              // holds the token
        private TOKENTYPE token_type = TOKENTYPE.NOTHING; // type of the token

        private double ans;                     // holds the result of the expression
        private String ans_str;                 // holds a string containing the result
                                                // of the expression

        private Dictionary<String, Double> constants = new Dictionary<String, Double>();                   // list with constants (like Pi and e)
        private Dictionary<String, Double> variablelist = new Dictionary<String, Double>();                // list with assigned variables (defined by the user)
        private Dictionary<String, JongFunctionAssign> functionlist = new Dictionary<String, JongFunctionAssign>();// list with assigned functions (defined by the user)
        private List<JongLocalVariable> localvariablelist = new List<JongLocalVariable>();           // list with local function variables (used during function assignment)

        public static String FuncToString(JongFunction func, String strSpaceIndent, String strLineIndent, JongErrWarn errW)
        {
            try
            {
                String retStr = "";
                if (func.name() == "Value")
                {
                    retStr += func.eval();
                }
                else if (func.name() == "VariableGet")
                {
                    retStr += ((JongVariableGet)func).getName();
                }
                else if (func.name() == "VariableAssign")
                {
                    retStr += ((JongVariableAssign)func).getName();
                    retStr += " = ";
                }
                else
                {
                    retStr += func.name();
                }

                List<JongFunction> argFuncs = func.get();

                String nextSpaceIndent = (strSpaceIndent + strSpaceIndent);

                foreach (JongFunction argFunc in argFuncs)
                {
                    retStr += "\n";
                    retStr += (strSpaceIndent + strLineIndent);

                    String subStr = FuncToString(argFunc, nextSpaceIndent, strLineIndent, errW);
                    retStr += subStr;
                }

                return retStr;

            }
            catch (JongError err)
            {
                errW.Init(ErrWStat.UNKNOWN_ERROR, err.get(), false);
                return "";
            }
        }
    }
}
