using System;

namespace Fluend.ExpressionLanguage.Exceptions
{
    public class MissingFunctionException : Exception
    {
        public MissingFunctionException(string functionName, int cursor, string expression)
            : base($"The function '{functionName}' does not exist. Position {cursor}, {expression}")
        {
        }
        
        public MissingFunctionException(string functionName, int cursor, string expression, Exception inner)
            : base($"The function '{functionName}' does not exist. Position {cursor}, {expression}", inner)
        {
        }
    }
}