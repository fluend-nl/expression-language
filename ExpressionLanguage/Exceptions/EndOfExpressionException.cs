using System;

namespace Fluend.ExpressionLanguage.Exceptions
{
    public class EndOfExpressionException : Exception
    {
        public EndOfExpressionException(int cursor, string expression)
            : base($"Unexpected end of expression at position {cursor} in expression '{expression}'")
        {
        }
        
        public EndOfExpressionException(int cursor, string expression, Exception inner)
            : base($"Unexpected end of expression at position {cursor} in expression '{expression}'", inner)
        {
        }
    }
}