using System.Collections.Generic;
using Fluend.ExpressionLanguage.Evaluation;
using Fluend.ExpressionLanguage.Evaluation.Objects;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Evaluation
{
    public class SomeObject : ExpressiveObject
    {
        [Expressive]
        public int SomeValue { get; } = 42;

        public double SomeHiddenValue { get; } = 1.0;
            
        [Expressive]
        public string GetName()
        {
            return "SomeName";
        }

        public string NonPublishedFunction()
        {
            return "...";
        }
    }
    
    public class ObjectEvaluatorTest
    {
        [Test]
        public void It_Can_Evaluate_An_Object_Property_Expression()
        {
            var expression = "object.SomeValue";
            var someObject = new SomeObject();

            var parsed = new Parser()
                .Parse(new Lexer().Tokenize(expression), new List<string> {"object"});
            var evaluator = new Evaluator();
            var result = evaluator.Evaluate(parsed, new Dictionary<string, object>
            {
                {"object", someObject}
            });

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be(42);
        }
        
        [Test]
        public void It_Cannot_Access_Non_Expressive_Properties()
        {
            var expression = "object.SomeHiddenValue";
            var someObject = new SomeObject();

            var parsed = new Parser()
                .Parse(new Lexer().Tokenize(expression), new List<string> {"object"});
            var evaluator = new Evaluator();
            var result = evaluator.Evaluate(parsed, new Dictionary<string, object>
            {
                {"object", someObject}
            });

            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("No property with the name 'SomeHiddenValue' exists on the object.");
        }
        
        [Test]
        public void It_Can_Evaluate_An_Object_Method_Call_Expression()
        {
            var expression = "object.GetName()";
            var someObject = new SomeObject();

            var parsed = new Parser()
                .Parse(new Lexer().Tokenize(expression), new List<string> {"object"});
            var evaluator = new Evaluator();
            var result = evaluator.Evaluate(parsed, new Dictionary<string, object>
            {
                {"object", someObject}
            });

            result.Succeeded.Should().Be(true);
            result.Value.Should().Be("SomeName");
        }
        
        [Test]
        public void It_Cannot_Access_Non_Expressive_Methods()
        {
            var expression = "object.NonPublishedFunction()";
            var someObject = new SomeObject();

            var parsed = new Parser()
                .Parse(new Lexer().Tokenize(expression), new List<string> {"object"});
            var evaluator = new Evaluator();
            var result = evaluator.Evaluate(parsed, new Dictionary<string, object>
            {
                {"object", someObject}
            });

            result.Succeeded.Should().Be(false);
            result.Error.Should().Be("No method with the name 'NonPublishedFunction' exists on the object.");
        }
    }
}