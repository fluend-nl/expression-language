using System;
using System.Collections.Generic;
using System.Linq;
using Fluend.ExpressionLanguage.Parsing.Nodes;

namespace Fluend.ExpressionLanguage.Evaluation.Functions
{
    public abstract class ExpressiveFunction
    {
        public string Name { get; }

        // This value is set from the constructor in the 
        // inheriting class.
        public Signature Signature { get; protected set; } = null!;

        public ExpressiveFunction(string name)
        {
            Name = name;
        }

        public abstract object? Invoke(object?[] parameters);

        public bool IsCallableWithArguments(TypeResolver typeResolver, ArgumentsNode arguments, IDictionary<string, object?> variables)
        {
            Type[] argumentTypes = arguments.Nodes
                .Select(n => typeResolver.Resolve(n, variables))
                .ToArray();

            return IsCallableWithArguments(argumentTypes);
        }

        public bool IsCallableWithArguments(Type[] argumentTypes)
        {
            // First: check if the argument counts match.
            if (argumentTypes.Length != Signature.Parameters.Count)
            {
                return false;
            }

            // Then check whether all types match.
            return Signature.Parameters
                .Select(p => p.Type)
                .SequenceEqual(argumentTypes);
        }

        public static bool operator ==(ExpressiveFunction? f1, ExpressiveFunction? f2)
        {
            if (f1 is null)
            {
                return f2 is null;
            }
            
            return f1.Equals(f2);
        }

        public static bool operator !=(ExpressiveFunction? f1, ExpressiveFunction? f2)
        {
            return !(f1 == f2);
        }

        public bool Equals(ExpressiveFunction? other)
        {
            if (null == other)
            {
                return false;
            }
            
            return Name == other.Name && Signature == other.Signature;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Signature);
        }
    }

    public sealed class ExpressiveFunction<TFunc> : ExpressiveFunction
        where TFunc : Delegate
    {
        private TFunc _callable;

        public ExpressiveFunction(string name, TFunc callable)
            : base(name)
        {
            _callable = callable;

            ResolveSignature();
        }
        
        private void ResolveSignature()
        {
            var genericArguments = typeof(TFunc).GenericTypeArguments;
            var parameters = _callable.Method.GetParameters();

            List<ExpressiveFunctionParameter> expressiveParameters = new();
            
            foreach (var parameter in parameters)
            {
                if (null == parameter.Name)
                {
                    throw new Exception(
                        $"Name of parameter at index {parameter.Position} of function '{Name}' is null. Please give it a name.");
                }

                var argument = new ExpressiveFunctionParameter(
                    parameter.Name,
                    parameter.ParameterType,
                    parameter.Position);

                expressiveParameters.Add(argument);
            }

            if (typeof(TFunc).Name.Contains("Action"))
            {
                Signature = new Signature(expressiveParameters, typeof(void));
            }
            else
            {
                // Func<...> is assumed
                Signature = new Signature(expressiveParameters, genericArguments[^1]);
            }
        }

        public override object? Invoke(object?[] parameters)
        {
            return _callable.DynamicInvoke(parameters);
        }
    }
}