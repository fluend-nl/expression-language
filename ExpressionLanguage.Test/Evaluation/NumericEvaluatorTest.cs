using System.Collections.Generic;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class NumericEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_A_Simple_Numeric_Constant()
        {
            var expression = "42";
            var parser = new Parser();
            var parsed = parser.Parse(new Lexer().Tokenize(expression));
            var evaluator = new Evaluator();

            var result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(42);
        }

        [Test]
        public void It_Can_Evaluate_A_Mathematical_Expression()
        {
            Dictionary<string, double> tests = new()
            {
                {"2 + 2", 4},
                {"50 - 20", 30},
                {"2 * 5", 10},
                {"2.5 * 2", 5},
                {"10 / 5", 2},
                {"0.5 / 5", 0.1},
                {"8 % 5", 3},
                {"2 ** 8", 256}
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
        public void It_Throws_An_Exception_On_Incompatible_Mathematical_Operations()
        {
            var expression = "{} + []";
            var parser = new Parser();
            var parsed = parser.Parse(new Lexer().Tokenize(expression));
            var evaluator = new Evaluator();

            var result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("Both sides of a mathematical expression have to be numeric.");
        }
    }
}