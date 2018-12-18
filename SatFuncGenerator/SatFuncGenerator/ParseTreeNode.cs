using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatFuncGenerator
{
    public class ParseTreeNode
    {
        private ParseTreeNode parent = null;
        private ParseTreeNode leftChild = null;
        private ParseTreeNode rightChild = null;

        Token token;

        public static ParseTreeNode DummyNode = new ParseTreeNode(new Token("" , ETokenType.Empty));

        public ParseTreeNode(Token Token)
        {
            token = Token;
        }

        public bool SetChildFromLeftToRight(ParseTreeNode child)
        {
            if (IsOfHeritage(child))
                return false;

            if (leftChild == null)
            {
                leftChild = child;
                leftChild.Parent = this;
                if(token.tokenData == "~")
                    rightChild = DummyNode;
                return true;
            }
            else if (rightChild == null)
            {
                rightChild = child;
                rightChild.Parent = this;
                return true;
            }
            else return false;
        }

        private bool IsOfHeritage(ParseTreeNode node)
        {
            ParseTreeNode p = parent;
            while(p != null)
            {
                if (p == node)
                    return true;
                p = p.Parent;
            }
            return false;
        }

        public bool IsLeaf()
        {
            return token.TokenType == ETokenType.MathExpression;
        }

        public bool AreBothChildrenLeaf()
        {
            if (rightChild == null || leftChild == null)
                return false;

            return leftChild.IsLeaf() && rightChild.IsLeaf();
        }

        public ParseTreeNode Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public ParseTreeNode LeftChild
        {
            get { return leftChild; }
        }

        public ParseTreeNode RightChild
        {
            get { return rightChild; }
        }

        public Token Token
        {
            get { return token; }
        }

    }
}
