using System.Collections.Generic;
using System.Linq;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;

namespace Fluend.ExpressionLanguage
{
    public static class Expression
    {
        public static TokenStream Tokenize(string expression)
        {
            return new Lexer().Tokenize(expression);
        }

        public static Node Parse(TokenStream stream, IList<string> functions, IList<string> names)
        {
            return new Parser(functions).Parse(stream, names);
        }
        
        public static Node Lint(TokenStream stream, IList<string> functions, IList<string> names)
        {
            return new Parser(functions).Lint(stream, names);
        }

        public static EvaluationResult Evaluate(Node parsed, ExpressiveFunctionSet functions, IDictionary<string, object> variables)
        {
            var evaluator = new Evaluator(functions);
            return evaluator.Evaluate(parsed, variables);
        }
      
        public static EvaluationResult Run(string expression)
        {
            var tokens = Tokenize(expression);
            var parsed = Parse(tokens, 
                new List<string>(),
                new List<string>());

            return Evaluate(parsed, new ExpressiveFunctionSet(),
                new Dictionary<string, object>());
        }
        
        public static EvaluationResult Run(string expression, ExpressiveFunctionSet functions)
        {
            var tokens = Tokenize(expression);
            var parsed = Parse(tokens, 
                functions.GetFunctionNames().ToList(),
                new List<string>());

            return Evaluate(parsed, functions, new Dictionary<string, object>());
        }
        
        public static EvaluationResult Run(string expression, ExpressiveFunctionSet functions, IDictionary<string, object> variables)
        {
            var tokens = Tokenize(expression);
            var parsed = Parse(tokens, 
                functions.GetFunctionNames().ToList(),
                variables.Keys.ToList());

            return Evaluate(parsed, functions, variables);
        }

        public static string ToLanguage(Node node)
        {
            return node.ToString();
        }
    }
}