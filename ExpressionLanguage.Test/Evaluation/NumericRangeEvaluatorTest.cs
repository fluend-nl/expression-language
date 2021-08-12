using System.Collections.Generic;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class NumericRangeEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_A_Numeric_Range_Expression()
        {
            var parser = new Parser();
            var evaluator = new Evaluator();

            var expression = "18..68";
            var parsed = parser.Parse(new Lexer().Tokenize(expression));
            var result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().BeOfType<List<object?>>();
            ((List<object?>) result.Value!).Count.Should().Be(68 - 18);
        }

        [Test]
        public void It_Can_Evaluate_A_Negative_Numeric_Range_Expression()
        {
            var parser = new Parser();
            var evaluator = new Evaluator();

            var expression = "-10..0";
            var parsed = parser.Parse(new Lexer().Tokenize(expression));
            var result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(true);
            result.Value.Should().BeOfType<List<object?>>();
            ((List<object?>) result.Value!).Count.Should().Be(10);
        }
        
        [Test]
        public void It_Throws_An_Exception_On_Incompatible_Numeric_Range_Expression()
        {
            var parser = new Parser();
            var evaluator = new Evaluator();
            
            var expression = "'abc'..4";
            var parsed = parser.Parse(new Lexer().Tokenize(expression));

            var result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("The left hand of a numeric range expression ('..') should be a number");
            
            expression = "0..'abc'";
            parsed = parser.Parse(new Lexer().Tokenize(expression));
            result = evaluator.Evaluate(parsed);
            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("The right hand of a numeric range expression ('..') should be a number");
        }
    }
}