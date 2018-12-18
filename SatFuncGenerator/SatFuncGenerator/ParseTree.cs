using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatFuncGenerator
{
    public class ParseTree
    {
        List<Token> prefixExpression;
        private ParseTreeNode root;

        public ParseTree(List<Token> PrefixExpression)
        {
            prefixExpression = PrefixExpression;
            root = GenerateTreeFromExpression(prefixExpression);
        }

        static ParseTreeNode GenerateTreeFromExpression(List<Token> Expression)
        {

            ParseTreeNode _root = new ParseTreeNode(Expression[0]);
            ParseTreeNode currentParent = _root;

            ParseTreeNode temp;

            for(int i=1; i<Expression.Count(); i++)
            {
                if (Expression[i].TokenType == ETokenType.MathExpression)
                {
                    temp = new ParseTreeNode(Expression[i]);
                    currentParent.SetChildFromLeftToRight(temp);
                    if (currentParent.AreBothChildrenLeaf() && i != Expression.Count()-1)
                        currentParent = FindNonefullParentOf(currentParent);
                }
                else
                {

                    temp = new ParseTreeNode(Expression[i]);
                    currentParent.SetChildFromLeftToRight(temp);
                    currentParent = temp;
                }
            }

            return _root;
        }

        private static ParseTreeNode FindNonefullParentOf(ParseTreeNode node)
        {

            while (node.RightChild != null)
                node = node.Parent;

            return node;
        }

        public ParseTreeNode Root
        {
            get { return root; }
        }
    }
}
