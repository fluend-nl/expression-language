using System.Collections.Generic;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class ComparisonEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_An_Comparison_Expression()
        {
            Dictionary<string, bool> tests = new()
            {
                {"2 == 2", true},
                {"'abc' == 'abc'", true},
                {"'4' == 4", true},
                {"'4' === 4", false},
                {"'8' != 4", true},
                {"'8' !== 7", true},
                {"2 < 3", true},
                {"3 <= 3", true},
                {"3 > 3", false},
                {"3 >= 3", true},
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
        public void It_Can_Evaluate_An_Equality_Expression()
        {
            Dictionary<string, bool> tests = new()
            {
                {"true && true", true},
                {"true && false", false},
                {"true and false", false},
                {"true or true", true},
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
    }
}