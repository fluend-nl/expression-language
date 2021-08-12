using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Fluend.ExpressionLanguage.Exceptions;

namespace Fluend.ExpressionLanguage.Lexing
{
    public class Lexer
    {
        private static readonly string[] WordLikeOperators =
        {
            // Note: "not in" is not present here, it is handled as a special case
            // in the word segment of the tokenizer instead.
            "or",
            "and",
            "not",
            "in",
            "matches"
        };
        public TokenStream Tokenize(string expression)
        {
            int cursor = 0;
            List<Token> tokens = new();
            Stack<(char, int)> brackets = new();
            var span = expression.AsSpan();

            while (cursor < span.Length)
            {
                switch (span[cursor])
                {
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                    case '\v':
                    case '\f':
                        cursor += 1;
                        break;
                    case '(':
                    case '[':
                    case '{':
                        brackets.Push((span[cursor], cursor));
                        tokens.Add(new Token(TokenType.Punctuation, span[cursor].ToString(), cursor + 1));
                        cursor += 1;
                        break;
                    case ')':
                    case ']':
                    case '}':
                        // Closing brackets
                        if (0 == brackets.Count)
                        {
                            throw new SyntaxErrorException(
                                $"unexpected '{span[cursor]}' at position {cursor}");
                        }

                        var (expected, _) = brackets.Pop();
                        var correct = expected switch
                        {
                            '(' => span[cursor] == ')',
                            '[' => span[cursor] == ']',
                            '{' => span[cursor] == '}',
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        if (!correct)
                        {
                            throw new SyntaxErrorException(
                                $"unclosed '{expected}' at position {cursor}");
                        }

                        tokens.Add(new Token(TokenType.Punctuation,
                            span[cursor].ToString(), cursor + 1));
                        cursor += 1;
                        break;
                    case '\'':
                    case '"':
                        int start = cursor;
                        cursor += 1;
                        while (span[cursor] != span[start])
                        {
                            cursor += 1;

                            if (cursor == span.Length)
                            {
                                throw new SyntaxErrorException(
                                    $"unexpected character '{span[start]}' at position {start}");
                            }
                        }

                        tokens.Add(new Token(TokenType.String, span[(start + 1)..cursor].ToString(), start + 1));
                        
                        // Move past the ending ' or "
                        cursor += 1;
                        break;
                    case '|':
                    case '&':
                        if (span.Length - 1 == cursor || (span[cursor + 1] != '&' && span[cursor + 1] != '|'))
                        {
                            // | and &
                            tokens.Add(new Token(TokenType.Operator, span[cursor].ToString(), cursor + 1));
                            cursor += 1;
                        }
                        else
                        {
                            // || and &&
                            tokens.Add(new Token(TokenType.Operator, span[cursor..(cursor + 2)].ToString(), cursor + 1));
                            cursor += 2;
                        }
                        break;
                    case ',':
                    case '?':
                    case ':':
                        tokens.Add(new Token(TokenType.Punctuation, span[cursor].ToString(), cursor + 1));
                        cursor += 1;
                        break;
                    case '^':
                    case '+':
                    case '-':
                    case '~':
                    case '/':
                    case '%':
                        tokens.Add(new Token(TokenType.Operator, span[cursor].ToString(), cursor + 1));
                        cursor += 1;
                        break;
                    case '*':
                        if (span.Length - 1 == cursor || span[cursor + 1] != '*')
                        {
                            // Multiply
                            tokens.Add(new Token(TokenType.Operator, "*", cursor + 1));
                            cursor += 1;
                        }
                        else
                        {
                            // To the power of
                            tokens.Add(new Token(TokenType.Operator, "**", cursor + 1));
                            cursor += 2;
                        }
                        break;
                    case '<':
                    case '>':
                        if (span.Length - 1 == cursor || span[cursor + 1] != '=')
                        {
                            // < or >
                            tokens.Add(new Token(TokenType.Operator, span[cursor].ToString(), cursor + 1));
                            cursor += 1;
                        }
                        else
                        {
                            // <= or >=
                            tokens.Add(new Token(TokenType.Operator, span[cursor..(cursor + 2)].ToString(), cursor + 1));
                            cursor += 2;
                        }
                        break;
                    case '=':
                        if (span.Length - 1 == cursor || span[cursor + 1] != '=')
                        {
                            throw new SyntaxErrorException(
                                $"unexpected character '{span[cursor]}' at position {cursor}");
                        }

                        if (span[cursor + 1] == '=')
                        {
                            if (span.Length - 1 == cursor + 1 || span[cursor + 2] != '=')
                            {
                                // ==
                                tokens.Add(new Token(TokenType.Operator, "==", cursor + 1));
                                cursor += 2;
                            }
                            else
                            {
                                // ===
                                tokens.Add(new Token(TokenType.Operator, "===", cursor + 1));
                                cursor += 3;
                            }
                        }
                        break;
                    case '!':
                        if (span.Length - 1 == cursor || span[cursor + 1] != '=')
                        {
                            // Unary !
                            tokens.Add(new Token(TokenType.Operator, "!", cursor + 1));
                            cursor += 1;
                        }
                        else
                        {
                            if (span.Length - 1 == cursor + 1 || span[cursor + 2] != '=')
                            {
                                // !=
                                tokens.Add(new Token(TokenType.Operator, "!=", cursor + 1));
                                cursor += 2;
                            }
                            else
                            {
                                // !==
                                tokens.Add(new Token(TokenType.Operator, "!==", cursor + 1));
                                cursor += 3;
                            }
                        }
                        break;
                    case '.':
                        if (span.Length - 1 == cursor || span[cursor + 1] != '.')
                        {
                            // .
                            tokens.Add(new Token(TokenType.Punctuation, ".", cursor + 1));
                            cursor += 1;
                        }
                        else
                        {
                            // ..
                            tokens.Add(new Token(TokenType.Operator, "..", cursor + 1));
                            cursor += 2;
                        }
                        break;
                    default:
                        // Words, numbers and word-like operations
                        // Words cannot start with a number.
                        int wordStart = cursor;
                        if (char.IsDigit(span[cursor]))
                        {
                            cursor += 1;
                            
                            while (span.Length > cursor)
                            {
                                if (char.IsDigit(span[cursor]))
                                {
                                    cursor += 1;
                                    continue;
                                }
                                
                                // Parse single dots as fractional separators, but make
                                // sure that we don't mistake a numeric range operator
                                // for a syntax error.
                                if (span[cursor] == '.')
                                {
                                    if (span.Length > cursor + 1 && span[cursor + 1] == '.')
                                    {
                                        break;
                                    }

                                    cursor += 1;
                                    continue;
                                }

                                if (span[cursor] == 'e' || span[cursor] == 'E')
                                {
                                    // Scientific notation
                                    cursor += 1;
                                    
                                    // + or -
                                    if (span[cursor] == '+' || span[cursor] == '-')
                                    {
                                        cursor += 1;

                                        while (span.Length > cursor && char.IsDigit(span[cursor]))
                                        {
                                            cursor += 1;
                                        }
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException(
                                            $"expected + or - at position {cursor}");
                                    }
                                }

                                break;
                            }

                            tokens.Add(new Token(TokenType.Number, span[wordStart..cursor].ToString(), wordStart + 1));
                        }
                        else if (char.IsLetter(span[cursor]))
                        {
                            cursor += 1;
                            
                            // Word
                            while (span.Length > cursor && (char.IsLetterOrDigit(span[cursor]) || span[cursor] == '_'))
                            {
                                cursor += 1;
                            }

                            string word = span[wordStart..cursor].ToString();
                            if (WordLikeOperators.Contains(word))
                            {
                                if (word == "in")
                                {
                                    // If the last token was the 'not' operator,
                                    // we will change that to 'not in'.
                                    if (tokens.Count > 0 && 
                                        tokens[^1].Type == TokenType.Operator &&
                                        tokens[^1].Value == "not")
                                    {
                                        tokens[^1] = new Token(TokenType.Operator, "not in", tokens[^1].Cursor);
                                    }
                                    else
                                    {
                                        tokens.Add(new Token(TokenType.Operator, "in", wordStart + 1));
                                    }
                                }
                                else
                                {
                                    tokens.Add(new Token(TokenType.Operator, word, wordStart + 1));
                                }
                            }
                            else
                            {
                                // Name
                                tokens.Add(new Token(TokenType.Name, word, wordStart + 1));
                            }
                        }
                        else
                        {
                            // Not lexable
                            throw new SyntaxErrorException(
                                $"unexpected character '{expression[cursor]}' at position {cursor}");
                        }
                        break;
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