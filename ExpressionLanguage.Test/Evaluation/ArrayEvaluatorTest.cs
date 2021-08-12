using System;
using System.Collections.Generic;
using System.Linq;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class ArrayEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_An_In_Array_Expression()
        {
            Dictionary<string, bool> tests = new()
            {
                {"1 in [1, 2, 3]", true},
                {"1 not in [1, 2, 3]", false},
                {"'x' in ['v', 'w']", false},
                {"null in [1, 2, 3]", false},
                {"null not in [1, 2, 3]", true},
                {"3 in []", false},
                {"3 not in []", true},
            };

            var parser = new Parser();
            var lexer = new Lexer();
            var evaluator = new Evaluator();

            foreach (var (expression, expectedResult) in tests)
            {
                var parsed = parser.Parse(lexer.Tokenize(expression));
                var result = evaluator.Evaluate(parsed);
                result.Succeeded.Should().Be(true);
                result.Value.Should().Be(expectedResult, expression);
            }
        }

        [Test]
        public void It_Can_Evaluate_An_Array_Expression()
        {
            var expression = "foo([2,4,6][1 + 1])";
            var functions = new ExpressiveFunctionSet();
            var someCalculation =
                new ExpressiveFunction<Func<double, double>>("foo", val => val / 2);
            functions.Add(someCalculation);

            var parsed = new Parser(functions.GetFunctionNames().ToList())
                .Parse(new Lexer().Tokenize(expression));
            var evaluator = new Evaluator(functions);
            var result = evaluator.Evaluate(parsed);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(3);
        }
    }
}