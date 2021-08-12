using System;

namespace Fluend.ExpressionLanguage.Exceptions
{
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(string message)
            : base("Syntax error: " + message)
        {
        }

        public SyntaxErrorException(string message, Exception inner)
            : base("Syntax error: " + message, inner)
        {
        }
    }
}