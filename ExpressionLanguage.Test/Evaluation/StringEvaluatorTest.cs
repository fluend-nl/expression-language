using System.Collections.Generic;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class StringEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_Concatenation_Expressions()
        {
            Dictionary<string, string> tests = new()
            {
                {"'a' ~ 'b'", "ab"},
                {"'abc' ~ null", "abc"},
                {"3 ~ 4", "34"},
                {"null ~ null", ""},
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
        public void It_Can_Evaluate_A_Regular_Expression()
        {
            Dictionary<string, bool> tests = new()
            {
                {"'3' matches '[0-9]'", true},
                {"'abc' matches '.+@.+.\\..+'", false},
                {"'test@fluend.nl' matches '/.+@.+.\\..+/'", true},
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