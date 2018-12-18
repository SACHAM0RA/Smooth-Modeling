using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Excel = Microsoft.Office.Interop.Excel;

namespace SatFuncGenerator
{
    class Program
    {
        static char[] SeperatingSymbols = { '(', ')', '<', '>', '=', '~', '&', '|' };
        static char[] ops = { '<', '>', '=', '~', '&', '|' };


        static void Main(string[] args)
        {
    
            Console.WriteLine("");
            Console.WriteLine("   GUIDE========================================================================================");
            Console.WriteLine("   |     - A variable name must be a sequence of letters followed by a sequence of numbers.    |");
            Console.WriteLine("   |     - Supported arithmetic operators are -, +, *, /, ^                                    |");
            Console.WriteLine("   |     - Supported real functions are: sin, cos, tan, cot, ln, log, sqrt                     |");
            Console.WriteLine("   |     - Supported arithmetic conditions are >, <, =                                         |");
            Console.WriteLine("   |     - Supported boolean operators: & (AND), | (OR), ~ (NEGATION)                          |");
            Console.WriteLine("   |     - Some examples of valid variable names are x,xx2, and x234                           |");
            Console.WriteLine("   |     - Sample constraint: (x^2>sqrt(x+y))&(x<0.5)                                          |");
            Console.WriteLine("   |     - To exit the program type exit.                                                      |");
            Console.WriteLine("   =============================================================================================");

            string constraint = string.Empty;
            while (!constraint.Equals("exit"))
            {
                Console.Write("\n Constraint  :  ");
                constraint = Console.ReadLine();
                Console.Write(" Sat Function:  ");

                GenerateAndPrintFunctionForSingleExpression(constraint, true);
            }
        }

        private static void GenerateFuncsForExpressionsInSpreadSheet(string FileAdress, bool justFuncInOutput)
        {
            DateTime startTime = DateTime.Now;
            List<string> infixes = LoadExpressionList(FileAdress);
            double loadSeconds = (DateTime.Now - startTime).TotalSeconds;

            startTime = DateTime.Now;

            List<string> funcs = new List<string>(infixes.Count);
            for (int i = 0; i < infixes.Count; i++)
            {
                funcs.Add(GenerateFuncFromExpression(infixes[i]));
            }
            double generationSeconds = (DateTime.Now - startTime).TotalSeconds;


            if (!justFuncInOutput)
            {
                Console.WriteLine("- " + infixes.Count + " Expressions loaded in " + loadSeconds.ToString() + " seconds");
                Console.WriteLine("- " + funcs.Count + " functions generated in " + generationSeconds.ToString() + " seconds");
                Console.WriteLine("\n============================================= \n");

                List<Tuple<string, string>> prints = new List<Tuple<string, string>>();
                for (int i = 0; i < funcs.Count(); i++)
                {
                    Console.WriteLine(infixes[i]);
                    Console.WriteLine(funcs[i] + "\n");
                }
            }
            else
            {
                for (int i = 0; i < funcs.Count(); i++)
                    Console.WriteLine(funcs[i]);
            }


            StreamWriter writer = new StreamWriter(FileAdress.Replace(".txt", "_PE.txt"));
            for (int i = 0; i < funcs.Count(); i++)
                writer.WriteLine(funcs[i]);

            writer.Close();
        }

        public static List<string> LoadExpressionList(string xlsxPath)
        {
            List<string> expressions = new List<string>();

            string line;

            StreamReader file = new StreamReader(xlsxPath);

            while ((line = file.ReadLine()) != null)
            {
                expressions.Add(line);
            }

            file.Close();
            return expressions;
        }

        public static string GenerateFuncFromExpression(string infix)
        {
            infix = infix.Replace(" ", "");

            infix = infix.Replace(')', '@');
            infix = infix.Replace('(', ')');
            infix = infix.Replace('@', '(');

            List<Token> infixTokens = Tokenize(infix);

            List<Token> prefix = Infix2Prefix(infixTokens);
            ParseTree tree = new ParseTree(prefix);

            return GenerateSmoothExpressionFromParseTree(tree);
        }

        private static string GenerateAndPrintFunctionForSingleExpression(string infix, bool justFuncInOutput)
        {
            DateTime startTime = DateTime.Now;

            if (!justFuncInOutput)
            {
                Console.WriteLine("- Expression : " + infix);
            }
            infix = infix.Replace(" ", "");

            infix = infix.Replace(')', '@');
            infix = infix.Replace('(', ')');
            infix = infix.Replace('@', '(');

            List<Token> infixTokens = Tokenize(infix);

            List<Token> prefix = Infix2Prefix(infixTokens);
            ParseTree tree = new ParseTree(prefix);

            string smoothExpression;
            smoothExpression = GenerateSmoothExpressionFromParseTree(tree);


            if (justFuncInOutput)
            {
                Console.WriteLine(smoothExpression);
            }
            else
            {
                Console.WriteLine("- Prefix     : " + GetTokenListAsString(prefix));
                PrintTree(tree);
                Console.WriteLine("\n- Smooth Func : " + smoothExpression);
                double seconds = (DateTime.Now - startTime).TotalSeconds;
                Console.WriteLine("\n- Generation Time :" + seconds.ToString() + " seconds");
            }
            return infix;
        }

        private static string GenerateSmoothExpressionFromParseTree(ParseTree tree)
        {
            return GenerateSubExpression(tree.Root);
        }

        private static string GenerateSubExpression(ParseTreeNode root)
        {
            if (root.Token.TokenType == ETokenType.MathExpression)
            {
                return root.Token.tokenData;
            }
            else if (root.Token.TokenType == ETokenType.NumericRelation)
            {
                switch (root.Token.tokenData)
                {
                    case ("<"):
                        {
                            return "sigmoid(" + root.RightChild.Token.tokenData + "-(" + root.LeftChild.Token.tokenData + "))";
                        }
                    case (">"):
                        {
                            return "sigmoid(" + root.LeftChild.Token.tokenData + "-(" + root.RightChild.Token.tokenData + "))";
                        }
                    case ("="):
                        {
                            return "gaussian(" + root.LeftChild.Token.tokenData + "-(" + root.RightChild.Token.tokenData + "))";
                        }
                }
            }
            else if (root.Token.TokenType == ETokenType.BooleanRelation)
            {
                switch (root.Token.tokenData)
                {
                    case ("~"):
                        {
                            return "(1-" + GenerateSubExpression(root.LeftChild) + ")";
                        }
                    case ("&"):
                        {
                            return "(" + GenerateSubExpression(root.LeftChild) + "*" + GenerateSubExpression(root.RightChild) + ")";
                        }
                    case ("|"):
                        {
                            return "(" + GenerateSubExpression(root.LeftChild) + "+" + GenerateSubExpression(root.RightChild) + "-(" + GenerateSubExpression(root.LeftChild) + "*" + GenerateSubExpression(root.RightChild) + "))";
                        }
                }
            }

            return "";
        }

        private static string GetTokenListAsString(List<Token> tokenList)
        {
            string ret = string.Empty;
            for (int i = 0; i < tokenList.Count(); i++)
            {
                ret += tokenList[i].tokenData + " ";
            }

            return ret;
        }

        static List<Token> Infix2Prefix(List<Token> infix)
        {
            var stack = new Stack<Token>();
            var postfix = new Stack<Token>();

            infix.Reverse();

            Token st;
            for (int i = 0; i < infix.Count(); i++)
            {
                if (infix[i].TokenType == ETokenType.MathExpression)
                {
                    postfix.Push(infix[i]);
                }
                else
                {
                    if (infix[i].TokenType == ETokenType.LeftParentize)
                    {
                        stack.Push(infix[i]);
                    }
                    else if (infix[i].TokenType == ETokenType.RightParentize)
                    {
                        st = stack.Pop();
                        while (!(st.TokenType == ETokenType.LeftParentize))
                        {
                            postfix.Push(st);
                            st = stack.Pop();
                        }
                    }
                    else
                    {
                        while (stack.Count > 0)
                        {
                            st = stack.Pop();
                            if (IsPriorOrEquallyPrior(st, infix[i]) && st.TokenType != ETokenType.LeftParentize && st.TokenType != ETokenType.RightParentize)
                            {
                                postfix.Push(st);
                            }
                            else
                            {
                                stack.Push(st);
                                break;
                            }
                        }
                        stack.Push(infix[i]);
                    }
                }
            }

            while (stack.Count > 0)
            {
                postfix.Push(stack.Pop());
            }

            return postfix.ToList();
        }

        private static bool IsPriorOrEquallyPrior(Token a, Token b)
        {
            if (a.tokenData == "|" && b.tokenData == "&")
                return false;

            if ((a.tokenData == "&" || a.tokenData == "|") && b.tokenData == "~")
                return false;

            return true;
        }

        static List<Token> Tokenize(string expression)
        {
            List<Token> tokens = new List<Token>();

            Stack<int> ps = new Stack<int>();
            Stack<int> opers = new Stack<int>();
            for (int i = 0; i < expression.Count(); i++)
            {
                if (expression[i] == ')')
                    ps.Push(i);
                else if (expression[i] == '(')
                {
                    int start = ps.Pop();

                    if (opers.Count() == 0 || !(opers.First() < i && opers.First() > start))
                    {

                        if (opers.Count() != 0)
                            opers.Pop();

                        expression = expression.Remove(start, 1);
                        expression = expression.Insert(start, "{");

                        expression = expression.Remove(i, 1);
                        expression = expression.Insert(i, "}");
                    }
                }
                else if (ops.Contains(expression[i]))
                    opers.Push(i);

            }

            for (int i = 0; i < expression.Count(); i++)
            {
                switch (expression[i])
                {
                    case ' ':
                        {
                            i++;
                            break;
                        }
                    case '(':
                        {
                            tokens.Add(new Token("(", ETokenType.LeftParentize));
                            break;
                        }
                    case ')':
                        {
                            tokens.Add(new Token(")", ETokenType.RightParentize));
                            break;
                        }
                    case '&':
                        {
                            tokens.Add(new Token("&", ETokenType.BooleanRelation));
                            break;
                        }
                    case '|':
                        {
                            tokens.Add(new Token("|", ETokenType.BooleanRelation));
                            break;
                        }
                    case '~':
                        {
                            tokens.Add(new Token("~", ETokenType.BooleanRelation));
                            break;
                        }
                    case '<':
                        {
                            if (expression[i + 1] == '=')
                            {
                                tokens.Add(new Token("<=", ETokenType.NumericRelation));
                                i++;
                            }
                            else
                                tokens.Add(new Token("<", ETokenType.NumericRelation));
                            break;
                        }
                    case '>':
                        {
                            if (expression[i + 1] == '=')
                            {
                                tokens.Add(new Token(">=", ETokenType.NumericRelation));
                                i++;
                            }
                            else
                                tokens.Add(new Token(">", ETokenType.NumericRelation));
                            break;
                        }
                    case '=':
                        {
                            tokens.Add(new Token("=", ETokenType.NumericRelation));
                            break;
                        }
                    default:
                        {
                            int length = 0;
                            string data = string.Empty;
                            bool predefined = false;
                            while (i + length < expression.Count() && !SeperatingSymbols.Contains(expression[i + length]))
                            {
                                length++;
                                data = expression.Substring(i, length);

                                data = data.Replace('{', '(');
                                data = data.Replace('}', ')');
                            }


                            tokens.Add(new Token(data, ETokenType.MathExpression));
                            i = i + length - 1;
                            break;
                        }
                }
            }

            return tokens;
        }

        public static void PrintTree(ParseTree tree)
        {
            Console.WriteLine("- Pars Tree  :");

            ParseTreeNode node = tree.Root;
            List<ParseTreeNode> firstStack = new List<ParseTreeNode>();
            firstStack.Add(node);

            List<List<ParseTreeNode>> childListStack = new List<List<ParseTreeNode>>();
            childListStack.Add(firstStack);

            while (childListStack.Count > 0)
            {
                List<ParseTreeNode> childStack = childListStack[childListStack.Count - 1];

                if (childStack.Count == 0)
                {
                    childListStack.RemoveAt(childListStack.Count - 1);
                }
                else
                {
                    node = childStack[0];
                    childStack.RemoveAt(0);

                    string indent = "             ";
                    for (int i = 0; i < childListStack.Count - 1; i++)
                    {
                        indent += (childListStack[i].Count > 0) ? "|  " : "   ";
                    }

                    Console.WriteLine(indent + "+-[" + node.Token.tokenData + "]");

                    if (node.LeftChild != null && node.LeftChild != ParseTreeNode.DummyNode)
                    {
                        List<ParseTreeNode> children = new List<ParseTreeNode>();
                        children.Add(node.LeftChild);
                        if (node.RightChild != null && node.RightChild != ParseTreeNode.DummyNode)
                            children.Add(node.RightChild);
                        childListStack.Add(children);
                    }
                }
            }
        }
    }
}
