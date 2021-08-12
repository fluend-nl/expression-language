using System;
using Fluend.ExpressionLanguage.Parsing.Nodes;

namespace Fluend.ExpressionLanguage.Exceptions
{
    public class NoOverloadException : Exception
    {
        public NoOverloadException(FunctionNode node)
            : base($"No suitable overload was found for '{node}'")
        {
        }
        
        public NoOverloadException(FunctionNode node, Exception inner)
            : base($"No suitable overload was found for '{node}'", inner)
        {
        }
    }
}