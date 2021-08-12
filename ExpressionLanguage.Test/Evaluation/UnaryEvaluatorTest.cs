using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class UnaryEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_Unary_Expression()
        {
            var parser = new Parser();
            var evaluator = new Evaluator();

            var expression = "!null";
            var parsed = parser.Parse(new Lexer().Tokenize(expression));

            var result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(true);

            expression = "!true";
            parsed = parser.Parse(new Lexer().Tokenize(expression));
            result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(false);

            expression = "-10";
            parsed = parser.Parse(new Lexer().Tokenize(expression));
            result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(-10);

            expression = "+10";
            parsed = parser.Parse(new Lexer().Tokenize(expression));
            result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(10);
        }

        [Test]
        public void It_Throws_An_Exception_On_Illegal_Unary_Expression()
        {
            var parser = new Parser();
            var evaluator = new Evaluator();

            var expression = "![1,2]";
            var parsed = parser.Parse(new Lexer().Tokenize(expression));

            var result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("Negation expects a numeric or boolean value");

            expression = "-'abc'";
            parsed = parser.Parse(new Lexer().Tokenize(expression));
            result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("Unary subtraction (negative sign) expects a numeric value.");

            expression = "+null";
            parsed = parser.Parse(new Lexer().Tokenize(expression));
            result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("Unary addition (plus sign) expects a numeric value.");
        }
    }
}