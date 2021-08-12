using Fluend.ExpressionLanguage.Exceptions;
using Fluend.ExpressionLanguage.Lexing;
using FluentAssertions;
using NUnit.Framework;

namespace Fluend.ExpressionLanguage.Test.Lexing
{
    public class LexerTest
    {
        [Test]
        public void It_Throws_An_Exception_For_Misplaced_Apostrophes()
        {
            string expression = "service(faulty.expression.example').dummyMethod()";

            Assert.Throws<SyntaxErrorException>(() =>
            {
                new Lexer().Tokenize(expression);
            }, "Syntax error: unexpected character ''' at position 33");
        }

        [Test]
        public void It_Throws_An_Exception_For_An_Unclosed_Brace()
        {
            string expression = "service(faulty.expression.example.dummyMethod()";
            
            Assert.Throws<SyntaxErrorException>(() =>
            {
                new Lexer().Tokenize(expression);
            }, "Syntax error: unenclosed '(' at position 7");
        }
        
        [Test]
        public void It_Tokenizes_A_Hashtable()
        {
            string expression = "{ 'x': 4 }";
            var stream = new Lexer().Tokenize(expression);
            stream.Tokens.Count.Should().Be(6);
            stream.Tokens[0].Value.Should().Be("{");
            stream.Tokens[1].Value.Should().Be("x");
            stream.Tokens[2].Value.Should().Be(":");
            stream.Tokens[3].Value.Should().Be("4");
            stream.Tokens[4].Value.Should().Be("}");
            stream.Tokens[5].Type.Should().Be(TokenType.Eof);
        }

        [Test]
        public void It_Ignores_Whitespace_Control_Characters()
        {
            string expression = "1 + \n\v\f\r\n 1";
            var stream = new Lexer().Tokenize(expression);
            stream.Tokens.Count.Should().Be(4);
            stream.Tokens[0].Value.Should().Be("1");
            stream.Tokens[1].Value.Should().Be("+");
            stream.Tokens[2].Value.Should().Be("1");
            stream.Tokens[3].Type.Should().Be(TokenType.Eof);
        }
    }
}