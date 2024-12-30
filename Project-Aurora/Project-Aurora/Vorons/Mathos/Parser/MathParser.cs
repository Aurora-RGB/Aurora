/*
 * Copyright (C) 2012-2014 Artem Los,
 * All rights reserved.
 *
 * Please see the license file in the project folder,
 * or, goto https://mathosparser.codeplex.com/license.
 *
 * Please feel free to ask me directly at my email!
 *  artem@artemlos.net
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AuroraRgb.Vorons.Mathos.Parser;

/// <summary>
/// This is a mathematical expression parser that allows you to parser a string value,
/// perform the required calculations, and return a value in form of a decimal.
/// </summary>
internal class MathParser
{
    /// <summary>
    /// This consconstructor will add some basic operators, functions, and variables
    /// to the parser. Please note that you are able to change that using
    /// boolean flags
    /// </summary>
    /// <param name="loadPreDefinedFunctions">This will load "abs", "cos", "cosh", "arccos", "sin", "sinh", "arcsin", "tan", "tanh", "arctan", "sqrt", "rem", "round"</param>
    /// <param name="loadPreDefinedOperators">This will load "%", "*", ":", "/", "+", "-", ">", "&lt;", "="</param>
    /// <param name="loadPreDefinedVariables">This will load "pi" and "e"</param>
    public MathParser(bool loadPreDefinedFunctions = true, bool loadPreDefinedOperators = true, bool loadPreDefinedVariables = true)
    {
        if (loadPreDefinedOperators)
        {
            // by default, we will load basic arithmetic operators.
            // please note, its possible to do it either inside the constructor,
            // or outside the class. the lowest value will be executed first!
            OperatorList.Add("%"); // modulo
            OperatorList.Add("^"); // to the power of
            OperatorList.Add(":"); // division 1
            OperatorList.Add("/"); // division 2
            OperatorList.Add("*"); // multiplication
            OperatorList.Add("-"); // subtraction
            OperatorList.Add("+"); // addition

            OperatorList.Add(">"); // greater than
            OperatorList.Add("<"); // less than
            OperatorList.Add("="); // are equal


            // when an operator is executed, the parser needs to know how.
            // this is how you can add your own operators. note, the order
            // in this list does not matter.
            _operatorAction.Add("%", (numberA, numberB) => numberA % numberB);
            _operatorAction.Add("^", (numberA, numberB) => (decimal)Math.Pow((double)numberA, (double)numberB));
            _operatorAction.Add(":", (numberA, numberB) => numberA / numberB);
            _operatorAction.Add("/", (numberA, numberB) => numberA / numberB);
            _operatorAction.Add("*", (numberA, numberB) => numberA * numberB);
            _operatorAction.Add("+", (numberA, numberB) => numberA + numberB);
            _operatorAction.Add("-", (numberA, numberB) => numberA - numberB);

            _operatorAction.Add(">", (numberA, numberB) => numberA > numberB ? 1 : 0);
            _operatorAction.Add("<", (numberA, numberB) => numberA < numberB ? 1 : 0);
            _operatorAction.Add("=", (numberA, numberB) => numberA == numberB ? 1 : 0);
        }


        if (loadPreDefinedFunctions)
        {
            // these are the basic functions you might be able to use.
            // as with operators, localFunctions might be adjusted, i.e.
            // you can add or remove a function.
            // please open the "MathosTest" project, and find MathParser.cs
            // in "CustomFunction" you will see three ways of adding 
            // a new function to this variable!
            // EACH FUNCTION MAY ONLY TAKE ONE PARAMETER, AND RETURN ONE
            // VALUE. THESE VALUES SHOULD BE IN "DECIMAL FORMAT"!
            LocalFunctions.Add("abs", x => (decimal)Math.Abs((double)x[0]));

            LocalFunctions.Add("cos", x => (decimal)Math.Cos((double)x[0]));
            LocalFunctions.Add("cosh", x => (decimal)Math.Cosh((double)x[0]));
            LocalFunctions.Add("arccos", x => (decimal)Math.Acos((double)x[0]));

            LocalFunctions.Add("sin", x => (decimal)Math.Sin((double)x[0]));
            LocalFunctions.Add("sinh", x => (decimal)Math.Sinh((double)x[0]));
            LocalFunctions.Add("arcsin", x => (decimal)Math.Asin((double)x[0]));

            LocalFunctions.Add("tan", x => (decimal)Math.Tan((double)x[0]));
            LocalFunctions.Add("tanh", x => (decimal)Math.Tanh((double)x[0]));
            LocalFunctions.Add("arctan", x => (decimal)Math.Atan((double)x[0]));
            //LocalFunctions.Add("arctan2", x => (decimal)Math.Atan2((double)x[0], (double)x[1]));

            LocalFunctions.Add("sqrt", x => (decimal)Math.Sqrt((double)x[0]));
            LocalFunctions.Add("rem", x => (decimal)Math.IEEERemainder((double)x[0], (double)x[1]));
            LocalFunctions.Add("root", x => (decimal)Math.Pow((double)x[0], 1.0 / (double)x[1]));

            LocalFunctions.Add("pow", x => (decimal)Math.Pow((double)x[0], (double)x[1]));

            LocalFunctions.Add("exp", x => (decimal)Math.Exp((double)x[0]));
            //LocalFunctions.Add("log", x => (decimal)Math.Log((double)x[0]));
            //LocalFunctions.Add("log10", x => (decimal)Math.Log10((double)x[0]));
            LocalFunctions.Add("log", delegate(decimal[] input)
            {
                // input[0] is the number
                // input[1] is the base

                switch (input.Length)
                {
                    case 1:
                        return (decimal)Math.Log((double)input[0]);
                    case 2:
                        return (decimal)Math.Log((double)input[0], (double)input[1]);
                    default:
                        return 0; // false
                }
            });

            LocalFunctions.Add("round", x => (decimal)Math.Round((double)x[0]));
            LocalFunctions.Add("truncate", x => (decimal)(x[0] < 0.0m ? -Math.Floor(-(double)x[0]) : Math.Floor((double)x[0])));
            LocalFunctions.Add("floor", x => (decimal)Math.Floor((double)x[0]));
            LocalFunctions.Add("ceiling", x => (decimal)Math.Ceiling((double)x[0]));
            LocalFunctions.Add("sign", x => (decimal)Math.Sign((double)x[0]));
        }

        if (loadPreDefinedVariables)
        {
            // local variables such as pi can also be added into the parser.
            LocalVariables.Add("pi", (decimal)3.14159265358979323846264338327950288); // the simplest variable!
            LocalVariables.Add("pi2", (decimal)6.28318530717958647692528676655900576);
            LocalVariables.Add("pi05", (decimal)1.57079632679489661923132169163975144);
            LocalVariables.Add("pi025", (decimal)0.78539816339744830961566084581987572);
            LocalVariables.Add("pi0125", (decimal)0.39269908169872415480783042290993786);
            LocalVariables.Add("pitograd", (decimal)57.2957795130823208767981548141051704);
            LocalVariables.Add("piofgrad", (decimal)0.01745329251994329576923690768488612);

            LocalVariables.Add("e", (decimal)2.71828182845904523536028747135266249);
            LocalVariables.Add("phi", (decimal)1.61803398874989484820458683436563811);
            LocalVariables.Add("major", (decimal)0.61803398874989484820458683436563811);
            LocalVariables.Add("minor", (decimal)0.38196601125010515179541316563436189);
        }
    }


    /* THE LOCAL VARIABLES */
    private List<string> _operatorList = new();

    /// <summary>
    /// All operators should be inside this property.
    /// The first operator is executed first, et cetera.
    /// An operator may only be ONE character.
    /// </summary>
    public List<string> OperatorList
    {
        get { return _operatorList; }
        set { _operatorList = value; }
    }

    private Dictionary<string, Func<decimal, decimal, decimal>> _operatorAction = new();

    /// <summary>
    /// When adding a variable in the OperatorList property, you need to assign how that operator should work.
    /// </summary>
    public Dictionary<string, Func<decimal, decimal, decimal>> OperatorAction
    {
        get { return _operatorAction; }
        set { _operatorAction = value; }
    }

    private Dictionary<string, Func<decimal[], decimal>> _localFunctions = new();

    /// <summary>
    /// All functions that you want to define should be inside this property.
    /// </summary>
    public Dictionary<string, Func<decimal[], decimal>> LocalFunctions
    {
        get { return _localFunctions; }
        set { _localFunctions = value; }
    }

    private Dictionary<string, decimal> _localVariables = new();

    /// <summary>
    /// All variables that you want to define should be inside this property.
    /// </summary>
    public Dictionary<string, decimal> LocalVariables
    {
        get { return _localVariables; }
        set { _localVariables = value; }
    }

    private readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

    /// <summary>
    /// When converting the result from the Parse method or ProgrammaticallyParse method ToString(),
    /// please use this cultur info.
    /// </summary>
    public CultureInfo CultureInfo
    {
        get { return _cultureInfo; }
    }


    /* PARSER FUNCTIONS, PUBLIC */
    /// <summary>
    /// Enter the math expression in form of a string.
    /// </summary>
    /// <param name="mathExpression"></param>
    /// <returns></returns>
    public decimal Parse(string mathExpression)
    {
        return MathParserLogic(Scanner(mathExpression));
    }

    /// <summary>
    /// Enter the math expression in form of a list of tokens.
    /// </summary>
    /// <param name="mathExpression"></param>
    /// <returns></returns>
    public decimal Parse(ReadOnlyCollection<string> mathExpression)
    {
        return MathParserLogic(new List<string>(mathExpression));
    }

    /// <summary>
    /// Enter the math expression in form of a string. You might also add/edit variables using "let" keyword.
    /// For example, "let sampleVariable = 2+2".
    /// 
    /// Another way of adding/editing a variable is to type "varName := 20"
    /// 
    /// Last way of adding/editing a variable is to type "let varName be 20"
    /// </summary>
    /// <param name="mathExpression"></param>
    /// <param name="correctExpression"></param>
    /// <param name="identifyComments"></param>
    /// <returns></returns>
    public decimal ProgrammaticallyParse(string mathExpression, bool correctExpression = true, bool identifyComments = true)
    {
        if (identifyComments)
        {
            mathExpression = Regex.Replace(mathExpression, "#\\{.*?\\}#", ""); // Delete Comments #{Comment}#
            mathExpression = Regex.Replace(mathExpression, "#.*$", ""); // Delete Comments #Comment
        }

        if (correctExpression)
            mathExpression = Correction(mathExpression);
        // this refers to the Correction function which will correct stuff like artn to arctan, etc.

        string varName;
        decimal varValue;
        if (mathExpression.Contains("let"))
        {
            if (mathExpression.Contains("be"))
            {
                varName = mathExpression.Substring(mathExpression.IndexOf("let", StringComparison.Ordinal) + 3,
                    mathExpression.IndexOf("be", StringComparison.Ordinal) - mathExpression.IndexOf("let", StringComparison.Ordinal) - 3);
                mathExpression = mathExpression.Replace(varName + "be", "");
            }
            else
            {
                varName = mathExpression.Substring(mathExpression.IndexOf("let", StringComparison.Ordinal) + 3,
                    mathExpression.IndexOf('=', StringComparison.Ordinal) - mathExpression.IndexOf("let", StringComparison.Ordinal) - 3);
                mathExpression = mathExpression.Replace(varName + "=", "");
            }

            varName = varName.Replace(" ", "");
            mathExpression = mathExpression.Replace("let", "");

            varValue = Parse(mathExpression);

            if (LocalVariables.ContainsKey(varName))
                LocalVariables[varName] = varValue;
            else
                LocalVariables.Add(varName, varValue);

            return varValue;
        }

        if (!mathExpression.Contains(":=")) return Parse(mathExpression);

        //mathExpression = mathExpression.Replace(" ", ""); // remove white space
        varName = mathExpression.Substring(0, mathExpression.IndexOf(":=", StringComparison.Ordinal));
        mathExpression = mathExpression.Replace(varName + ":=", "");

        varValue = Parse(mathExpression);
        varName = varName.Replace(" ", "");

        if (LocalVariables.ContainsKey(varName))
        {
            LocalVariables[varName] = varValue;
        }
        else
        {
            LocalVariables.Add(varName, varValue);
        }

        return varValue;
    }

    /// <summary>
    /// This will convert a string expression into a list of tokens that can be later executed by Parse or ProgrammaticallyParse methods.
    /// </summary>
    /// <param name="mathExpression"></param>
    /// <returns>A ReadOnlyCollection</returns>
    public ReadOnlyCollection<string> GetTokens(string mathExpression)
    {
        return new ReadOnlyCollection<string>(Scanner(mathExpression));
    }

    /// <summary>
    /// This will correct sqrt() and arctan() written in different ways only.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private string Correction(string input)
    {
        // Word corrections

        input = Regex.Replace(input, "\\b(sqr|sqrt)\\b", "sqrt",
            RegexOptions.IgnoreCase);
        input = Regex.Replace(input, "\\b(atan2|arctan2)\\b", "arctan2",
            RegexOptions.IgnoreCase);
        //... and more

        return input;
    }


    /* UNDER THE HOOD - THE CORE OF THE PARSER */
    private List<string> Scanner(string expr)
    {
        // SCANNING THE INPUT STRING AND CONVERT IT INTO TOKENS

        var tokens = new List<string>();
        var vector = "";

        //_expr = _expr.Replace(" ", ""); // remove white space
        expr = expr.Replace("+-", "-"); // some basic arithmetical rules
        expr = expr.Replace("-+", "-");
        expr = expr.Replace("--", "+");

        /* foreach (var item in LocalVariables)
         {
             // replace the current variables with their value
             _expr = _expr.Replace(item.Key, "(" + item.Value.ToString(CULTURE_INFO) + ")");
         }*/

        for (var i = 0; i < expr.Length; i++)
        {
            var ch = expr[i];

            if (char.IsWhiteSpace(ch))
            {
            } // could also be used to remove white spaces.
            else if (Char.IsLetter(ch))
            {
                if (i != 0 && (Char.IsDigit(expr[i - 1]) || Char.IsDigit(expr[i - 1]) || expr[i - 1] == ')'))
                {
                    tokens.Add("*");
                }

                vector = vector + ch;

                while
                    ((i + 1) < expr.Length &&
                     Char.IsLetterOrDigit(
                         expr[i + 1])) // here is it is possible to choose whether you want variables that only contain letters with or without digits.
                {
                    i++;
                    vector = vector + expr[i];
                }

                tokens.Add(vector);
                vector = "";
            }
            else if (Char.IsDigit(ch))
            {
                vector = vector + ch;

                while ((i + 1) < expr.Length && (Char.IsDigit(expr[i + 1]) || expr[i + 1] == '.')) // removed || _expr[i + 1] == ','
                {
                    i++;
                    vector = vector + expr[i];
                }

                tokens.Add(vector);
                vector = "";
            }
            else if ((i + 1) < expr.Length && (ch == '-' || ch == '+') && Char.IsDigit(expr[i + 1]) &&
                     (i == 0 || OperatorList.IndexOf(expr[i - 1].ToString()) != -1 ||
                      ((i - 1) > 0 && expr[i - 1] == '('))) // removed CultureInfo.InvariantCulture from expr[i-1].ToString(...)
            {
                // if the above is true, then, the token for that negative number will be "-1", not "-","1".
                // to sum up, the above will be true if the minus sign is in front of the number, but
                // at the beginning, for example, -1+2, or, when it is inside the brakets (-1).
                // NOTE: this works for + sign as well!
                vector = vector + ch;

                while ((i + 1) < expr.Length && (Char.IsDigit(expr[i + 1]) || expr[i + 1] == '.')) // removed || _expr[i + 1] == ','
                {
                    i++;
                    vector = vector + expr[i];
                }

                tokens.Add(vector);
                vector = "";
            }
            else if (ch == '(')
            {
                if (i != 0 && (Char.IsDigit(expr[i - 1]) || Char.IsDigit(expr[i - 1]) || expr[i - 1] == ')'))
                {
                    tokens.Add("*");
                    // if we remove this line(above), we would be able to have numbers in function names. however, then we can't parser 3(2+2)
                    tokens.Add("(");
                }
                else
                    tokens.Add("(");
            }
            else
            {
                tokens.Add(ch.ToString());
            }
        }

        return tokens;
        //return MathParserLogic(_tokens);
    }

    private decimal MathParserLogic(List<string> tokens)
    {
        // CALCULATING THE EXPRESSIONS INSIDE THE BRACKETS
        // IF NEEDED, EXECUTE A FUNCTION

        // Variables replacement
        for (var i = 0; i < tokens.Count; i++)
        {
            if (LocalVariables.Keys.Contains(tokens[i]))
            {
                tokens[i] = LocalVariables[tokens[i]].ToString(CultureInfo);
            }
        }

        while (tokens.IndexOf("(") != -1)
        {
            // getting data between "(", ")"
            var open = tokens.LastIndexOf("(");
            var close = tokens.IndexOf(")", open); // in case open is -1, i.e. no "(" // , open == 0 ? 0 : open - 1

            if (open >= close)
            {
                // if there is no closing bracket, throw a new exception
                throw new ArithmeticException("No closing bracket/parenthesis! tkn: " + open.ToString(CultureInfo.InvariantCulture));
            }

            var roughExpr = new List<string>();

            for (var i = open + 1; i < close; i++)
            {
                roughExpr.Add(tokens[i]);
            }

            decimal result; // the temporary result is stored here

            var functioName = tokens[open == 0 ? 0 : open - 1];
            var args = new decimal[0];

            if (LocalFunctions.Keys.Contains(functioName))
            {
                if (roughExpr.Contains(","))
                {
                    // converting all arguments into a decimal array
                    for (var i = 0; i < roughExpr.Count; i++)
                    {
                        var firstCommaOrEndOfExpression = roughExpr.IndexOf(",", i) != -1 ? roughExpr.IndexOf(",", i) : roughExpr.Count;
                        var defaultExpr = new List<string>();

                        while (i < firstCommaOrEndOfExpression)
                        {
                            defaultExpr.Add(roughExpr[i]);
                            i++;
                        }

                        // changing the size of the array of arguments
                        Array.Resize(ref args, args.Length + 1);

                        if (defaultExpr.Count == 0)
                        {
                            args[args.Length - 1] = 0;
                        }
                        else
                        {
                            args[args.Length - 1] = BasicArithmeticalExpression(defaultExpr);
                        }
                    }

                    // finnaly, passing the arguments to the given function
                    result = decimal.Parse(LocalFunctions[functioName](args).ToString(CultureInfo), CultureInfo);
                }
                else
                {
                    // but if we only have one argument, then we pass it directly to the function
                    result = decimal.Parse(
                        LocalFunctions[functioName](new[] { BasicArithmeticalExpression(roughExpr) }).ToString(CultureInfo), CultureInfo);
                }
            }
            else
            {
                // if no function is need to execute following expression, pass it
                // to the "BasicArithmeticalExpression" method.
                result = BasicArithmeticalExpression(roughExpr);
            }

            // when all the calculations have been done
            // we replace the "opening bracket with the result"
            // and removing the rest.
            tokens[open] = result.ToString(CultureInfo);
            tokens.RemoveRange(open + 1, close - open);
            if (LocalFunctions.Keys.Contains(functioName))
            {
                // if we also executed a function, removing
                // the function name as well.
                tokens.RemoveAt(open - 1);
            }
        }

        // at this point, we should have replaced all brackets
        // with the appropriate values, so we can simply
        // calculate the expression. it's not so complex
        // any more!
        return BasicArithmeticalExpression(tokens);
    }

    private decimal BasicArithmeticalExpression(List<string> tokens)
    {
        // PERFORMING A BASIC ARITHMETICAL EXPRESSION CALCULATION
        // THIS METHOD CAN ONLY OPERATE WITH NUMBERS AND OPERATORS
        // AND WILL NOT UNDERSTAND ANYTHING BEYOND THAT.

        switch (tokens.Count)
        {
            case 1:
                return decimal.Parse(tokens[0], CultureInfo);
            case 2:
            {
                var op = tokens[0];

                if (op == "-" || op == "+")
                    return decimal.Parse((op == "+" ? "" : (tokens[1].Substring(0, 1) == "-" ? "" : "-")) + tokens[1], CultureInfo);

                return OperatorAction[op](0, decimal.Parse(tokens[1], CultureInfo));
            }
            case 0:
                return 0;
        }

        foreach (var op in OperatorList)
        {
            while (tokens.IndexOf(op) != -1)
            {
                var opPlace = tokens.IndexOf(op);

                var numberA = Convert.ToDecimal(tokens[opPlace - 1], CultureInfo);
                var numberB = Convert.ToDecimal(tokens[opPlace + 1], CultureInfo);

                var result = OperatorAction[op](numberA, numberB);

                tokens[opPlace - 1] = result.ToString(CultureInfo);
                tokens.RemoveRange(opPlace, 2);
            }
        }

        return Convert.ToDecimal(tokens[0], CultureInfo);
    }
}