using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class HashEvaluatorTest
    {
        [Test]
        public void It_Can_Get_The_Value_From_A_HashTable()
        {
            var expression = "{a: 42}['a']";
            
            var parsed = new Parser()
                .Parse(new Lexer().Tokenize(expression));
            var evaluator = new Evaluator();
            var result = evaluator.Evaluate(parsed);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(42);
        }
        
        [Test]
        public void It_Can_Get_The_Value_From_A_HashTable_With_A_Numeric_Index()
        {
            var expression = "{10: 42}[10]";
            
            var parsed = new Parser()
                .Parse(new Lexer().Tokenize(expression));
            var evaluator = new Evaluator();
            var result = evaluator.Evaluate(parsed);

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(42);
        }
    }
}