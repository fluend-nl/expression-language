using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fluend.ExpressionLanguage.Exceptions;
using Fluend.ExpressionLanguage.Lexing;
using Fluend.ExpressionLanguage.Parsing.Nodes;

namespace Fluend.ExpressionLanguage.Parsing
{
    public enum Associativity
    {
        Left,
        Right
    }

    public sealed class BinaryOperator
    {
        public int Precedence { get; set; }
        public Associativity Associativity { get; set; }
    }

    public sealed class UnaryOperator
    {
        public int Precedence { get; set; }
    }

    public class Parser
    {
        /// <summary>
        /// A list of all operators that return a boolean when evaluated.
        /// Note that bit-operations (^, &, |, etc.) are not included.
        /// </summary>
        public static readonly string[] BooleanOperators = 
        {
            "or",
            "||",
            "and",
            "&&",
            "==",
            "===",
            "!=",
            "!==",
            "<",
            ">",
            ">=",
            "<=",
            "in",
            "not in",
            "matches"
        };

        private static readonly Dictionary<string, UnaryOperator> UnaryOperators = new()
        {
            {"not", new UnaryOperator {Precedence = 50}},
            {"!", new UnaryOperator {Precedence = 50}},
            {"-", new UnaryOperator {Precedence = 500}},
            {"+", new UnaryOperator {Precedence = 500}}
        };

        private static readonly Dictionary<string, BinaryOperator> BinaryOperators = new()
        {
            {"or", new BinaryOperator {Precedence = 50, Associativity = Associativity.Left}},
            {"||", new BinaryOperator {Precedence = 10, Associativity = Associativity.Left}},
            {"and", new BinaryOperator {Precedence = 15, Associativity = Associativity.Left}},
            {"&&", new BinaryOperator {Precedence = 15, Associativity = Associativity.Left}},
            {"|", new BinaryOperator {Precedence = 16, Associativity = Associativity.Left}},
            {"^", new BinaryOperator {Precedence = 17, Associativity = Associativity.Left}},
            {"&", new BinaryOperator {Precedence = 18, Associativity = Associativity.Left}},
            {"==", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"===", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"!=", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"!==", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"<", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {">", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {">=", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"<=", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"not in", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"in", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"matches", new BinaryOperator {Precedence = 20, Associativity = Associativity.Left}},
            {"..", new BinaryOperator {Precedence = 25, Associativity = Associativity.Left}},
            {"+", new BinaryOperator {Precedence = 30, Associativity = Associativity.Left}},
            {"-", new BinaryOperator {Precedence = 30, Associativity = Associativity.Left}},
            {"~", new BinaryOperator {Precedence = 40, Associativity = Associativity.Left}},
            {"*", new BinaryOperator {Precedence = 60, Associativity = Associativity.Left}},
            {"/", new BinaryOperator {Precedence = 60, Associativity = Associativity.Left}},
            {"%", new BinaryOperator {Precedence = 60, Associativity = Associativity.Left}},
            {"**", new BinaryOperator {Precedence = 200, Associativity = Associativity.Right}},
        };

        private bool _shouldLint;

        // _names is assigned in both Parse(...) and Lint(...).
        private IList<string> _names = null;

        // _stream is assigned in both Parse(...) and Lint(...).
        private TokenStream _stream = null!;
        private IList<string> _functions;

        public Parser()
        {
            _functions = new List<string>();
        }

        public Parser(IList<string> functions)
        {
            _functions = functions;
        }

        public Node Parse(TokenStream stream, IList<string>? names = null)
        {
            _shouldLint = false;
            return DoParse(stream, names ?? new List<string>());
        }

        public Node Lint(TokenStream stream, IList<string>? names = null)
        {
            _shouldLint = true;
            return DoParse(stream, names ?? new List<string>());
        }

        private Node DoParse(TokenStream stream, IList<string> names)
        {
            _stream = stream;
            _names = names;

            var node = ParseExpression();
            if (!_stream.IsEof())
            {
                var token = _stream.Current;
                throw new SyntaxErrorException(
                    $"unexpected token '{token.Type}' of value '{token.Value}'. {token.Cursor}, {_stream.Expression}");
            }

            return node;
        }

        private Node ParseExpression(int precedence = 0)
        {
            var expr = GetPrimary();
            var token = _stream.Current;

            while (token.Test(TokenType.Operator) &&
                   BinaryOperators.ContainsKey(token.Value) &&
                   BinaryOperators[token.Value].Precedence >= precedence)
            {
                var op = BinaryOperators[token.Value];
                _stream.Next();

                var expr1 = this.ParseExpression(
                    Associativity.Left == op.Associativity
                        ? op.Precedence + 1
                        : op.Precedence);

                expr = new BinaryNode(token.Value, expr, expr1);
                token = _stream.Current;
            }

            if (0 == precedence)
            {
                return ParseConditionalExpression(expr);
            }

            return expr;
        }

        private Node GetPrimary()
        {
            var token = _stream.Current;

            if (token.Test(TokenType.Operator) && UnaryOperators.TryGetValue(token.Value, out var op))
            {
                _stream.Next();
                var expr = ParseExpression(op.Precedence);

                return ParsePostfixExpression(new UnaryNode(token.Value, expr));
            }

            if (token.Test(TokenType.Punctuation, "("))
            {
                _stream.Next();

                // TODO: double check we don't need to pass precedence here.
                var expr = ParseExpression();
                _stream.Expect(TokenType.Punctuation, ")",
                    "An opened parenthesis is not properly closed");

                return ParsePostfixExpression(expr);
            }

            return ParsePrimaryExpression();
        }

        private Node ParseConditionalExpression(Node expr)
        {
            while (_stream.Current.Test(TokenType.Punctuation, "?"))
            {
                Node expr2;
                Node expr3;

                _stream.Next();

                if (!_stream.Current.Test(TokenType.Punctuation, ":"))
                {
                    expr2 = ParseExpression();

                    if (_stream.Current.Test(TokenType.Punctuation, ":"))
                    {
                        _stream.Next();
                        expr3 = ParseExpression();
                    }
                    else
                    {
                        expr3 = new ConstantNode(null, ConstantType.Null);
                    }
                }
                else
                {
                    _stream.Next();
                    expr2 = expr;
                    expr3 = ParseExpression();
                }

                expr = new ConditionalNode(expr, expr2, expr3);
            }

            return expr;
        }

        private Node ParsePrimaryExpression()
        {
            var token = _stream.Current;
            Node node;

            switch (token.Type)
            {
                case TokenType.Name:
                    _stream.Next();
                    switch (token.Value)
                    {
                        case "true":
                        case "TRUE":
                            return new ConstantNode(true);
                        case "false":
                        case "FALSE":
                            return new ConstantNode(false);
                        case "null":
                        case "NULL":
                            return new ConstantNode(null, ConstantType.Null);

                        default:
                            if ("(" == _stream.Current.Value)
                            {
                                if (!_functions.Contains(token.Value))
                                {
                                    // Function not defined.
                                    throw new MissingFunctionException(token.Value, token.Cursor,
                                        _stream.Expression);
                                }

                                node = new FunctionNode(token.Value, ParseArguments());
                            }
                            else
                            {
                                if (!_shouldLint || _names.Count > 0)
                                {
                                    if (!_names.Contains(token.Value))
                                    {
                                        throw new MissingVariableException(token.Value, token.Cursor,
                                            _stream.Expression);
                                    }
                                }

                                node = new NameNode(token.Value);
                            }

                            break;
                    }

                    break;

                case TokenType.Number:
                    _stream.Next();
                    return new ConstantNode(double.Parse(token.Value), ConstantType.Number);

                case TokenType.String:
                    _stream.Next();
                    return new ConstantNode(token.Value, ConstantType.String);

                default:
                    if (token.Test(TokenType.Punctuation, "["))
                    {
                        node = ParseArrayExpression();
                    }
                    else if (token.Test(TokenType.Punctuation, "{"))
                    {
                        node = ParseHashExpression();
                    }
                    else
                    {
                        throw new SyntaxErrorException(
                            $"unexpected token '{token.Type}' of value '{token.Value}'. {token.Cursor}, {_stream.Expression}");
                    }

                    break;
            }

            return ParsePostfixExpression(node);
        }

        private Node ParseArrayExpression()
        {
            _stream.Expect(TokenType.Punctuation, "[",
                "An array element was expected");

            var node = new ArrayNode();
            var first = true;

            while (!_stream.Current.Test(TokenType.Punctuation, "]"))
            {
                if (!first)
                {
                    _stream.Expect(TokenType.Punctuation, ",",
                        "An array element must be followed by a comma");

                    // TODO: Trailing ,?
                    if (_stream.Current.Test(TokenType.Punctuation, "]"))
                    {
                        break;
                    }
                }

                first = false;
                node.AddElement(ParseExpression());
            }

            _stream.Expect(TokenType.Punctuation, "]",
                "An opened array is not properly closed");

            return node;
        }

        private Node ParseHashExpression()
        {
            _stream.Expect(TokenType.Punctuation, "{",
                "A hash element was expected");

            var node = new HashNode();
            var first = true;

            while (!_stream.Current.Test(TokenType.Punctuation, "}"))
            {
                if (!first)
                {
                    _stream.Expect(TokenType.Punctuation, ",",
                        "A hash value must be followed by a comma");

                    // TODO: Trailing ,?
                    if (_stream.Current.Test(TokenType.Punctuation, "}"))
                    {
                        break;
                    }
                }

                first = false;

                Node key;

                // A hash can be:
                // * a number -- 12
                // * a string -- 'a'
                // * a name, which is equivalent to a string -- a
                // * an expression, which must be enclosed in parenthesis -- (1 + 2)
                if (_stream.Current.Test(TokenType.String))
                {
                    key = new ConstantNode(_stream.Current.Value, ConstantType.String);
                    _stream.Next();
                }
                else if (_stream.Current.Test(TokenType.Name))
                {
                    key = new ConstantNode(_stream.Current.Value, ConstantType.Name);
                    _stream.Next();
                }
                else if (_stream.Current.Test(TokenType.Number))
                {
                    key = new ConstantNode(double.Parse(_stream.Current.Value), ConstantType.Number);
                    _stream.Next();
                }
                else if (_stream.Current.Test(TokenType.Punctuation, "("))
                {
                    key = ParseExpression();
                }
                else
                {
                    string expected = $"unexpected token '{_stream.Current.Type}' of value '{_stream.Current.Value}'";
                    throw new SyntaxErrorException(
                        $"a hash key must be a quoted string, a number, a name or an expression enclosed in parenthesis ({expected}). {_stream.Current.Cursor}, {_stream.Expression}");
                }

                _stream.Expect(TokenType.Punctuation, ":",
                    "A hash key must be followed by a colon (:)");
                Node value = ParseExpression();

                node.AddElement(key, value);
            }

            _stream.Expect(TokenType.Punctuation, "}",
                "An opened hash is not properly closed");

            return node;
        }

        private Node ParsePostfixExpression(Node node)
        {
            var token = _stream.Current;

            while (TokenType.Punctuation == token.Type)
            {
                if ("." == token.Value)
                {
                    _stream.Next();
                    token = _stream.Current;
                    _stream.Next();

                    var arg = new ConstantNode(token.Value, ConstantType.Name);
                    var args = new ArgumentsNode();
                    GetAttrNodeType type;

                    if (_stream.Current.Test(TokenType.Punctuation, "("))
                    {
                        type = GetAttrNodeType.MethodCall;

                        foreach (var n in ParseArguments().Nodes)
                        {
                            args.AddElement(n);
                        }
                    }
                    else
                    {
                        type = GetAttrNodeType.PropertyCall;
                    }

                    node = new GetAttrNode(node, arg, args, type);
                }
                else if ("[" == token.Value)
                {
                    _stream.Next();
                    var arg = ParseExpression();
                    _stream.Expect(TokenType.Punctuation, "]");

                    node = new GetAttrNode(node, arg, new ArgumentsNode(), GetAttrNodeType.ArrayCall);
                }
                else
                {
                    break;
                }

                token = _stream.Current;
            }

            return node;
        }

        private ArgumentsNode ParseArguments()
        {
            List<Node> arguments = new();
            _stream.Expect(TokenType.Punctuation, "(",
                "A list of arguments must begin with an opening parenthesis");

            while (!_stream.Current.Test(TokenType.Punctuation, ")"))
            {
                if (arguments.Count > 0)
                {
                    _stream.Expect(TokenType.Punctuation, ",",
                        "Arguments must be separated by a comma");
                }

                arguments.Add(ParseExpression());
            }

            _stream.Expect(TokenType.Punctuation, ")",
                "A list of arguments must be closed by a parenthesis");

            return new ArgumentsNode(arguments);
        }
    }
}