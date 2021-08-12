using System;
using System.Collections.Generic;
using System.Linq;
using Fluend.ExpressionLanguage.Evaluation.Functions;
using Fluend.ExpressionLanguage.Exceptions;
using Fluend.ExpressionLanguage.Parsing;
using Fluend.ExpressionLanguage.Parsing.Nodes;

namespace Fluend.ExpressionLanguage.Evaluation
{
    public class TypeResolver
    {
        private readonly ExpressiveFunctionSet _functions;

        public TypeResolver(ExpressiveFunctionSet functions)
        {
            _functions = functions;
        }

        public TypeResolver()
        {
            _functions = new();
        }
        
        public Type Resolve(Node node, IDictionary<string, object?> variables)
        {
            if (node is NameNode nameNode)
            {
                if (!variables.TryGetValue(nameNode.Name, out var variable))
                {
                    throw new Exception($"A variable with the name '{nameNode.Name}' is not defined.");
                }

                return variable?.GetType() ?? typeof(object);
            }

            if (node is ConstantNode constantNode)
            {
                return constantNode.Type switch
                {
                    ConstantType.String => typeof(string),
                    ConstantType.Number => typeof(double),
                    ConstantType.Null => typeof(void),
                    ConstantType.Bool => typeof(bool),
                    ConstantType.Name => typeof(string),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (node is HashNode)
            {
                return typeof(Dictionary<string, object>);
            }
            
            if (node is ArrayNode)
            {
                return typeof(List<object?>);
            }

            if (node is UnaryNode unaryNode)
            {
                return Resolve(unaryNode.Node, variables);
            }

            if (node is BinaryNode binaryNode)
            {
                if (Parser.BooleanOperators.Contains(binaryNode.Operator))
                {
                    return typeof(bool);
                }

                if ("~" == binaryNode.Operator)
                {
                    return typeof(string);
                }

                if (".." == binaryNode.Operator)
                {
                    return typeof(List<object>);
                }
                
                // Fallback: the left side of the equation decides the type.
                return Resolve(binaryNode.Left, variables);
            }

            if (node is FunctionNode functionNode)
            {
                if (!_functions.Has(functionNode.Name))
                {
                    throw new Exception($"A function with the name '{functionNode.Name}' is not defined.");
                }

                var overloads = _functions.Get(functionNode.Name);

                foreach (var overload in overloads)
                {
                    if (overload.IsCallableWithArguments(this, functionNode.Arguments, variables))
                    {
                        return overload.Signature.ReturnType;
                    }
                }

                throw new NoOverloadException(functionNode);
            }

            if (node is GetAttrNode getAttrNode)
            {
                if (GetAttrNodeType.ArrayCall == getAttrNode.Type)
                {
                    return Resolve(getAttrNode.Attribute, variables);
                }
                
                throw new Exception(
                    "Only the type of the expression in the array subscript can be resolved. Method calls and properties are not supported.");
            }
            
            throw new Exception($"Can not resolve the type of a '{node.GetType().Name}' node.");
        }
    }
}