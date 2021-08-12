using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using Fluend.ExpressionLanguage.Evaluation.Objects;
using Fluend.ExpressionLanguage.Exceptions;
using Fluend.ExpressionLanguage.Parsing;
using Fluend.ExpressionLanguage.Parsing.Nodes;

namespace Fluend.ExpressionLanguage.Evaluation
{
    public class EvaluationResult
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public object? Value { get; set; }
    }

    public class Evaluator
    {
        private readonly ExpressiveFunctionSet _functions;
        private double _tolerance = 1E-9;
        private TimeSpan _regexTimeout = TimeSpan.FromMilliseconds(250);

        public Evaluator()
        {
            _functions = new();
        }

        /// <summary>
        /// Set the tolerance that is used when comparing doubles.
        /// </summary>
        /// <param name="tolerance"></param>
        public void SetTolerance(double tolerance)
        {
            _tolerance = tolerance;
        }

        /// <summary>
        /// Set the timeout that is used when evaluating regular
        /// expression. When processing user input it is important
        /// to specify this timeout, as the user could specify
        /// a pattern that could cause a DoS otherwise.
        ///
        /// The default value is 250 milliseconds.
        /// </summary>
        /// <param name="timeout"></param>
        public void SetRegexTimeout(TimeSpan timeout)
        {
            _regexTimeout = timeout;
        }

        public Evaluator(ExpressiveFunctionSet functions)
        {
            _functions = functions;
        }

        public EvaluationResult Evaluate(Node node, IDictionary<string, object>? variables = null)
        {
            EvaluationResult result = new();
            try
            {
                result.Succeeded = true;
                result.Value = EvalNode(node, variables ?? new Dictionary<string, object>());
            }
            catch (Exception exception)
            {
                result.Succeeded = false;
                result.Error = exception.Message;
            }

            return result;
        }

        public object? EvalNode(Node node, IDictionary<string, object> variables)
        {
            if (node is ConstantNode constantNode)
            {
                return constantNode.Value;
            }

            if (node is ArgumentsNode argumentsNode)
            {
                return argumentsNode.Nodes
                    .Select(argumentNode => (object?) EvalNode(argumentNode, variables))
                    .ToList();
            }

            if (node is ConditionalNode conditionalNode)
            {
                var condition = EvalNode(conditionalNode.Expr1, variables);

                if (condition is null or false or 0)
                {
                    return EvalNode(conditionalNode.Expr3, variables);
                }

                return EvalNode(conditionalNode.Expr2, variables);
            }
            
            if (node is NameNode nameNode)
            {
                if (!variables.TryGetValue(nameNode.Name, out var variable))
                {
                    throw new Exception($"A variable with the name '{nameNode.Name}' is not defined.");
                }
                
                // If the variable is numeric, we want to make sure that it is a double
                // because that is what it expects internally.
                if (IsNumber(variable))
                {
                    return Convert.ToDouble(variable);
                }

                return variable;
            }

            if (node is GetAttrNode getAttrNode)
            {
                return EvaluateGetAttrNode(getAttrNode, variables);
            }

            if (node is UnaryNode unaryNode)
            {
                return EvaluateUnaryNode(unaryNode, variables);
            }

            if (node is BinaryNode binaryNode)
            {
                return EvaluateBinaryNode(binaryNode, variables);
            }

            if (node is ArrayNode arrayNode)
            {
                List<object?> result = new();

                // Nodes are Key,Value,Key,Value
                // where the key is the index in the array (0, 1, 2, ...)
                for (int i = 1; i < arrayNode.Nodes.Count; i += 2)
                {
                    result.Add(EvalNode(arrayNode.Nodes[i], variables));
                }

                return result;
            }

            if (node is HashNode hashNode)
            {
                Dictionary<string, object> result = new();

                // Nodes are Key,Value,Key,Value
                for (int i = 0; i < hashNode.Nodes.Count; i += 2)
                {
                    var key = EvalNode(hashNode.Nodes[i], variables)!.ToString();
                    var value = EvalNode(hashNode.Nodes[i + 1], variables);

                    result.Add(key, value);
                }

                return result;
            }

            if (node is FunctionNode functionNode)
            {
                var overloads = _functions.Get(functionNode.Name);
                var typeResolver = new TypeResolver(_functions);
                var argumentTypes = functionNode.Arguments.Nodes
                    .Select(n => typeResolver.Resolve(n, variables))
                    .ToArray();

                foreach (var overload in overloads)
                {
                    if (overload.IsCallableWithArguments(argumentTypes))
                    {
                        var parameters = (List<object?>) EvalNode(functionNode.Arguments, variables)!;
                        return overload.Invoke(parameters.ToArray());
                    }
                }

                throw new NoOverloadException(functionNode);
            }

            return null;
        }

        private object? EvaluateGetAttrNode(GetAttrNode node, IDictionary<string, object> variables)
        {
            var baseValue = EvalNode(node.Base, variables);
            if (GetAttrNodeType.ArrayCall == node.Type)
            {
                var idx = EvalNode(node.Attribute, variables);

                if (baseValue is List<object?> list)
                {
                    if (idx is not double)
                    {
                        throw new Exception("Array indices can only be numeric.");
                    }

                    // Directly casting idx to an integer causes
                    // an exception here because idx is boxed.
                    int index = Convert.ToInt32(idx);

                    if (list.Count - 1 < index)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    return list[index];
                }

                if (baseValue is Dictionary<string, object?> hashTable)
                {
                    if (idx is not double && idx is not string)
                    {
                        throw new Exception("Hashtable indices can only be numeric or string.");
                    }

                    string index = idx.ToString()!;

                    return hashTable[index];
                }

                throw new Exception(
                    "The array subscript operator can only be used to retrieve the value from an array or hashtable.");
            }

            if (GetAttrNodeType.MethodCall == node.Type)
            {
                if (baseValue is not ExpressiveObject expressiveObject)
                {
                    throw new Exception("The object being accessed has to be an expressive object.");
                }

                var attributeName = EvalNode(node.Attribute, variables);
                if (attributeName is not string functionName)
                {
                    throw new Exception("The name of a function that is called on an object has to be a string.");
                }

                var arguments = (List<object?>) EvalNode(node.Arguments, variables)!;
                return expressiveObject.InvokeMethod(functionName, arguments.ToArray());
            }

            if (GetAttrNodeType.PropertyCall == node.Type)
            {
                if (baseValue is not ExpressiveObject expressiveObject)
                {
                    throw new Exception("The object being accessed has to be an expressive object.");
                }

                var attributeName = EvalNode(node.Attribute, variables);
                if (attributeName is not string propertyName)
                {
                    throw new Exception(
                        "The name of a property that is being accessed on an object has to be a string.");
                }

                return expressiveObject.GetPropertyValue(propertyName);
            }

            throw new ArgumentOutOfRangeException();
        }

        private object? EvaluateUnaryNode(UnaryNode unaryNode, IDictionary<string, object> variables)
        {
            var value = EvalNode(unaryNode.Node, variables);
            if ("not" == unaryNode.Operator || "!" == unaryNode.Operator)
            {
                if (null == value)
                {
                    return true;
                }

                if (value is double d)
                {
                    return d == 0;
                }

                if (value is bool b)
                {
                    return !b;
                }

                throw new Exception("Negation expects a numeric or boolean value");
            }

            if ("-" == unaryNode.Operator)
            {
                if (value is double d)
                {
                    return -d;
                }

                throw new Exception("Unary subtraction (negative sign) expects a numeric value.");
            }

            if ("+" == unaryNode.Operator)
            {
                // Note: unary plus is basically useless. Normally it promotes
                // a byte/short to integer. It also forces the operand to be numeric.
                // Besides that, the operand is returned as-is. So to implement it,
                // we just have to check whether the operand is numeric.
                if (value is double d)
                {
                    return d;
                }

                throw new Exception("Unary addition (plus sign) expects a numeric value.");
            }

            throw new ArgumentOutOfRangeException();
        }

        private object? EvaluateBinaryNode(BinaryNode node, IDictionary<string, object> variables)
        {
            object? lhs = EvalNode(node.Left, variables);
            object? rhs = EvalNode(node.Right, variables);

            return node.Operator switch
            {
                "||" or "or" => (bool) (lhs ?? false) || (bool) (rhs ?? false),
                "&&" or "and" => (bool) (lhs ?? false) && (bool) (rhs ?? false),
                "|" => EvaluateBitExpression("|", lhs, rhs),
                "^" => EvaluateBitExpression("^", lhs, rhs),
                "&" => EvaluateBitExpression("&", lhs, rhs),
                "==" => EvaluateEqualityExpression("==", (IComparable?) lhs, (IComparable?) rhs),
                "===" => EvaluateEqualityExpression("===", (IComparable?) lhs, (IComparable?) rhs),
                "!=" => EvaluateEqualityExpression("!=", (IComparable?) lhs, (IComparable?) rhs),
                "!==" => EvaluateEqualityExpression("!==", (IComparable?) lhs, (IComparable?) rhs),
                "<" => EvaluateEqualityExpression("<", (IComparable?) lhs, (IComparable?) rhs),
                ">" => EvaluateEqualityExpression(">", (IComparable?) lhs, (IComparable?) rhs),
                ">=" => EvaluateEqualityExpression(">=", (IComparable?) lhs, (IComparable?) rhs),
                "<=" => EvaluateEqualityExpression("<=", (IComparable?) lhs, (IComparable?) rhs),
                "not in" => !EvaluateInArrayExpression(lhs, rhs),
                "in" => EvaluateInArrayExpression(lhs, rhs),
                "matches" => EvaluateMatchesExpression(lhs, rhs),
                ".." => EvaluateNumericRangeExpression(lhs, rhs),
                "+" => EvaluateMathExpression("+", lhs, rhs),
                "-" => EvaluateMathExpression("-", lhs, rhs),
                "~" => EvaluateStringConcatenationExpression(lhs, rhs),
                "*" => EvaluateMathExpression("*", lhs, rhs),
                "/" => EvaluateMathExpression("/", lhs, rhs),
                "%" => EvaluateMathExpression("%", lhs, rhs),
                "**" => EvaluateMathExpression("**", lhs, rhs),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private double EvaluateMathExpression(string op, object? lhs, object? rhs)
        {
            lhs ??= (double) 0;
            rhs ??= (double) 0;

            if (lhs is not double lhsd || rhs is not double rhsd)
            {
                throw new Exception("Both sides of a mathematical expression have to be numeric.");
            }

            return op switch
            {
                "+" => lhsd + rhsd,
                "-" => lhsd - rhsd,
                "*" => lhsd * rhsd,
                "/" => lhsd / rhsd,
                "%" => lhsd % rhsd,
                "**" => Math.Pow(lhsd, rhsd),
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        private string EvaluateStringConcatenationExpression(object? lhs, object? rhs)
        {
            if (lhs == null && rhs == null)
            {
                return "";
            }

            return (null == lhs ? "" : lhs.ToString())
                   + (null == rhs ? "" : rhs.ToString());
        }

        private List<object?> EvaluateNumericRangeExpression(object? lhs, object? rhs)
        {
            if (lhs is not double lhsd)
            {
                throw new Exception("The left hand of a numeric range expression ('..') should be a number");
            }

            if (rhs is not double rhsd)
            {
                throw new Exception("The right hand of a numeric range expression ('..') should be a number");
            }

            int lhsi = (int) lhsd;
            int rhsi = (int) rhsd;

            if (rhsi - lhsi < 1)
            {
                throw new Exception(
                    "The right hand of a numeric range expression ('..') should be greater than the left hand.");
            }

            List<object?> result = new(rhsi - lhsi);

            for (int i = lhsi; i < rhsi; i++)
            {
                result.Add(i);
            }

            return result;
        }

        private bool EvaluateMatchesExpression(object? lhs, object? rhs)
        {
            if (lhs is not string subject)
            {
                return false;
            }

            if (rhs is not string pattern)
            {
                throw new Exception("The right hand of a matches expression should contain a pattern.");
            }

            if (pattern[0] == '/')
            {
                pattern = pattern.Substring(1);
            }

            if (pattern[^1] == '/')
            {
                pattern = pattern.Substring(0, pattern.Length - 1);
            }

            Regex regex = new(pattern, RegexOptions.None, _regexTimeout);

            return regex.IsMatch(subject);
        }

        private bool EvaluateInArrayExpression(object? lhs, object? rhs)
        {
            if (rhs is not IList<object?> list)
            {
                throw new Exception($"The right hand of an 'in' or 'not in' expression has to be an array.");
            }

            return list.Contains(lhs);
        }

        private object? EvaluateEqualityExpression(string op, IComparable? lhs, IComparable? rhs)
        {
            if (op.Length == 3)
            {
                // Strict equality
                if (null == lhs)
                {
                    return rhs == null;
                }

                if (null == rhs)
                {
                    return false;
                }

                if (lhs.GetType() != rhs.GetType())
                {
                    // If the op contains a negation ("!=" or "!==")
                    // they are not equal and this should return true.
                    return op.Contains('!');
                }
            }

            if (lhs is int)
            {
                lhs = (double) lhs;
            }

            if (rhs is int)
            {
                rhs = (double) rhs;
            }

            if (lhs is double lhsd && rhs is double rhsd)
            {
                return op switch
                {
                    "==" or "===" => Math.Abs(lhsd - rhsd) < _tolerance,
                    "!=" or "!==" => Math.Abs(lhsd - rhsd) > _tolerance,
                    "<" => lhsd < rhsd,
                    ">" => lhsd > rhsd,
                    "<=" => lhsd <= rhsd,
                    ">=" => lhsd >= rhsd,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }

            return op switch
            {
                "==" => Equals(lhs?.ToString(), rhs?.ToString()),
                "!=" => !Equals(lhs?.ToString(), rhs?.ToString()),
                "===" => Equals(lhs, rhs),
                "!==" => !Equals(lhs, rhs),
                "<" => lhs != null && rhs != null && lhs.CompareTo(rhs) < 0,
                ">" => lhs != null && rhs != null && lhs.CompareTo(rhs) > 0,
                "<=" => lhs != null && rhs != null && lhs.CompareTo(rhs) <= 0,
                ">=" => lhs != null && rhs != null && lhs.CompareTo(rhs) >= 0,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        private object? EvaluateBitExpression(string op, object? lhs, object? rhs)
        {
            if (lhs == null || rhs == null)
            {
                throw new Exception($"Cannot pass null to a bitwise operator ('{op}')");
            }

            return op switch
            {
                "|" => (int) lhs | (int) rhs,
                "^" => (int) lhs ^ (int) rhs,
                "&" => (int) lhs & (int) rhs,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }
        
        private static bool IsNumber(object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
    }
}