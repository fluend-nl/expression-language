using System;
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
        public static EvaluationResult Run(string expression)
        {
            return Run(expression, 
                new ExpressiveFunctionSet(),
                new Dictionary<string, object>());
        }
        
        public static EvaluationResult Run(string expression, ExpressiveFunctionSet functions)
        {
            return Run(expression, functions, new Dictionary<string, object>());
        }
        
        public static EvaluationResult Run(string expression, ExpressiveFunctionSet functions, IDictionary<string, object> variables)
        {
            EvaluationResult result = new();
            
            try
            {
                var tokens = new Lexer().Tokenize(expression);
                var parsed = new Parser(functions.GetFunctionNames().ToList())
                    .Parse(tokens, variables.Keys.ToList());
                var evaluator = new Evaluator(functions);
                return evaluator.Evaluate(parsed, variables);
            }
            catch (Exception exception)
            {
                result.Succeeded = false;
                result.Error = exception.Message;
            }

            return result;
        }
    }
}