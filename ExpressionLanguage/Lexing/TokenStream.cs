using System.Collections.Generic;
using Fluend.ExpressionLanguage.Exceptions;

namespace Fluend.ExpressionLanguage.Lexing
{
    public class TokenStream
    {
        public Token Current { get; set; }
        public string Expression { get; }

        private int _position = 0;
        private IList<Token> _tokens;
        
        public IList<Token> Tokens => _tokens;

        public TokenStream(IList<Token> tokens, string expression)
        {
            // TODO: add test with zero tokens.
            Current = tokens[0];
            Expression = expression;

            _tokens = tokens;
        }

        /// <summary>
        /// Sets the pointer to the next token.
        /// </summary>
        public void Next()
        {
            ++_position;

            if (_position > _tokens.Count)
            {
                throw new EndOfExpressionException(Current.Cursor, Expression);
            }

            Current = _tokens[_position];
        }

        public void Expect(TokenType type, string? value = null, string? message = null)
        {
            var token = Current;

            bool test = value switch
            {
                null => token.Test(type),
                _ => token.Test(type, value)
            };

            if (!test)
            {
                string expected = $"{type} expected" + (null != value ? $" with value {value}" : "");
                string errorMessage = message != null ? message + ". " : "";
                throw new SyntaxErrorException(
                    $"{errorMessage} unexpected token {token.Type} of value {token.Value}, {expected}");
            }

            Next();
        }

        public bool IsEof()
        {
            return Current.Type == TokenType.Eof;
        }
    }
}