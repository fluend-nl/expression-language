using System.Collections.Generic;
using Fluend.ExpressionLanguage.Exceptions;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Parsing
{
    public class ParserTest
    {
        private Lexer _lexer;

        [SetUp]
        public void Setup()
        {
            _lexer = new();
        }

        [Test]
        public void It_Throws_An_Exception_When_A_Variable_Is_Missing()
        {
            var parser = new Parser();

            Assert.Throws<MissingVariableException>(() =>
            {
                parser.Parse(_lexer.Tokenize("foo"), new List<string>());
            }, "The variable 'foo' does not exist. Position: 1, foo");
        }

        [Test]
        public void It_Throws_An_Exception_When_A_Function_Is_Missing()
        {
            var parser = new Parser();

            Assert.Throws<MissingFunctionException>(() =>
            {
                parser.Parse(_lexer.Tokenize("foo(1, 2)"));
            }, "The function 'foo' does not exist. Position 1, foo(1, 2)");
        }

        [Test]
        public void It_Can_Parse_A_Complex_Expression()
        {
            string expression = "(1 + 1) && x not in [a, b, foo()]";
            var parser = new Parser(new List<string> {"foo"});
            var result = parser.Parse(_lexer.Tokenize(expression),
                new List<string> {"x", "a", "b"});

            // Note that emitting back to the expression language added ( and ) around
            // x not in [a, b, foo()].
            Assert.AreEqual("(1 + 1) && (x not in [a, b, foo()])", result.ToString());
        }
        
        [Test]
        public void It_Can_Parse_A_Function_Expression()
        {
            string expression = "foo(1, bar(2, 3 + 3))";
            var parser = new Parser(new List<string> {"foo", "bar"});
            var result = parser.Parse(_lexer.Tokenize(expression));

            Assert.AreEqual("foo(1, bar(2, 3 + 3))", result.ToString());
        }

        [Test]
        public void It_Can_Parse_An_Array_Expression()
        {
            string expression = "[1, 2, 3]";
            var parser = new Parser();
            var result = parser.Parse(_lexer.Tokenize(expression));

            Assert.AreEqual("[1, 2, 3]", result.ToString());
        }

        [Test]
        public void It_Can_Parse_A_Hash_Expression()
        {
            string expression = "{ a: 1 }";
            var parser = new Parser(new List<string> {"foo"});
            var result = parser.Parse(_lexer.Tokenize(expression));
            Assert.AreEqual("{a: 1}", result.ToString());

            expression = "{ 'x': foo(), 'y': bar }";
            result = parser.Parse(_lexer.Tokenize(expression), new List<string> {"bar"});
            Assert.AreEqual("{'x': foo(), 'y': bar}", result.ToString());
        }
    }
}