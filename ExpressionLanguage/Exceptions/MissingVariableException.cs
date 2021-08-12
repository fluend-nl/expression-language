using System;

namespace Fluend.ExpressionLanguage.Exceptions
{
    public class MissingVariableException : Exception
    {
        public MissingVariableException(string variableName, int cursor, string expression)
            : base($"The variable '{variableName}' does not exist. Position: {cursor}, {expression}")
        {
        }
        
        public MissingVariableException(string variableName, int cursor, string expression, Exception inner)
            : base($"The variable '{variableName}' does not exist. Position: {cursor}, {expression}", inner)
        {
        }
    }
}