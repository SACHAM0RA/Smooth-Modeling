using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatFuncGenerator
{
    public enum ETokenType
    {
        LeftParentize,
        RightParentize,
        MathExpression,
        BooleanRelation,
        NumericRelation,
        Empty
    }

    public class Token
    {
        public string tokenData;
        public ETokenType TokenType;

        public Token(string data , ETokenType type)
        {
            tokenData = data;
            TokenType = type;
        }
    }
}
