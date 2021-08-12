using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fluend.ExpressionLanguage.Exceptions;

namespace Fluend.ExpressionLanguage.Lexing
{
    public class Lexer
    {
        private static readonly Regex Numbers = new(
            @"^(?:[0-9]+(?:\.[0-9]+)?([Ee][\+\-][0-9]+)?)",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(500));

        private static readonly Regex Strings = new(
            "^(?:\"([^\"\\\\]*(?:\\\\.[^\"\\\\]*)*)\"|\'([^\'\\\\]*(?:\\\\.[^\'\\\\]*)*)\')",
            RegexOptions.Compiled | RegexOptions.Singleline,
            TimeSpan.FromMilliseconds(500));

        // God help us
        private static readonly Regex Operators = new(
            @"^(?:(?<=^|[\s(])not in(?=[\s(])|\!\=\=|(?<=^|[\s(])not(?=[\s(])|(?<=^|[\s(])and(?=[\s(])|\=\=\=|\>\=|(?<=^|[\s(])or(?=[\s(])|\<\=|\*\*|\.\.|(?<=^|[\s(])in(?=[\s(])|&&|\|\||(?<=^|[\s(])matches|\=\=|\!\=|\*|~|%|\/|\>|\||\!|\^|&|\+|\<|\-)",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(500));

        // Public because the Parser also uses this regular expression.
        public static readonly Regex Names = new(
            "^(?:[a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*)",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(500));

        public TokenStream Tokenize(string expression)
        {
            int cursor = 0;
            List<Token> tokens = new();
            Stack<(char, int)> brackets = new();
            
            expression = expression
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Replace('\t', ' ')
                .Replace('\v', ' ')
                .Replace('\f', ' ');
            
            int end = expression.Length;
                
            while (cursor < end)
            {
                if (' ' == expression[cursor])
                {
                    ++cursor;
                    continue;
                }

                Match match;
                if (TryMatch(Numbers, expression, cursor, out match))
                {
                    // Numbers
                    tokens.Add(new Token(TokenType.Number,
                        match.Value, cursor + 1));
                    cursor += match.Length;
                }
                else if ("([{".Contains(expression[cursor]))
                {
                    // Opening brackets
                    brackets.Push((expression[cursor], cursor));
                    tokens.Add(new Token(TokenType.Punctuation,
                        expression[cursor].ToString(), cursor + 1));
                    cursor += 1;
                }
                else if (")]}".Contains(expression[cursor]))
                {
                    // Closing brackets
                    if (0 == brackets.Count)
                    {
                        throw new SyntaxErrorException(
                            $"unexpected '{expression[cursor]}' at position {cursor}");
                    }

                    var (expected, _) = brackets.Pop();
                    var correct = expected switch
                    {
                        '(' => expression[cursor] == ')',
                        '[' => expression[cursor] == ']',
                        '{' => expression[cursor] == '}',
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    if (!correct)
                    {
                        throw new SyntaxErrorException(
                            $"unclosed '{expected}' at position {cursor}");
                    }

                    tokens.Add(new Token(TokenType.Punctuation,
                        expression[cursor].ToString(), cursor + 1));
                    cursor += 1;
                }
                else if (TryMatch(Strings, expression, cursor, out match))
                {
                    // Strings
                    
                    // Remove surrounding quotation marks
                    var unescaped = match.Value
                        .Substring(1, match.Value.Length - 2);
                    tokens.Add(new Token(TokenType.String, unescaped, cursor + 1));
                    cursor += match.Value.Length;
                }
                else if (TryMatch(Operators, expression, cursor, out match))
                {
                    // Operators
                    tokens.Add(new Token(TokenType.Operator, match.Value, cursor + 1));
                    cursor += match.Value.Length;
                }
                else if (".,?:".Contains(expression[cursor]))
                {
                    // Punctuation
                    tokens.Add(new Token(TokenType.Punctuation, 
                        expression[cursor].ToString(), cursor + 1));
                    cursor += 1;
                }
                else if (TryMatch(Names, expression, cursor, out match))
                {
                    // Names
                    tokens.Add(new Token(TokenType.Name, match.Value, cursor + 1));
                    cursor += match.Value.Length;
                }
                else
                {
                    // Not lexable
                    throw new SyntaxErrorException(
                        $"unexpected character '{expression[cursor]}' at position {cursor}");
                }
            }
            
            tokens.Add(new Token(TokenType.Eof, null, cursor + 1));

            if (brackets.Count > 0)
            {
                var (bracket, pos) = brackets.Pop();
                throw new SyntaxErrorException(
                    $"unenclosed '{bracket}' at position {pos}");
            }

            return new TokenStream(tokens, expression);
        }

        private bool TryMatch(Regex regex, string expression, int cursor, out Match match)
        {
            // Because ^ matches the start of the string irrespective of the 
            // start position, we are forced to create a substring here.
            // Regex unfortunately does not have overloads for Span, so this
            // is pretty inefficient for now.
            match = regex.Match(expression.Substring(cursor));
            return match.Success;
        }
    }
}