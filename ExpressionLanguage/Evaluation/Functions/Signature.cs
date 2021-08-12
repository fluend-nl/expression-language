using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluend.ExpressionLanguage.Evaluation.Functions
{
    /// <summary>
    /// The signature represents the incoming and outgoing types of a function.
    /// When comparing signatures, the return type is not compared. Only the
    /// parameters. This is because the return type does not participate in
    /// overload resolution.
    /// </summary>
    public class Signature
    {
        public List<ExpressiveFunctionParameter> Parameters { get; }
        public Type ReturnType { get; }

        public Signature(List<ExpressiveFunctionParameter> parameters, Type returnType)
        {
            Parameters = parameters;
            ReturnType = returnType;
        }

        public static bool operator ==(Signature? s1, Signature? s2)
        {
            if (s1 is null)
            {
                return s2 is null;
            }
            
            return s1.Equals(s2);
        }

        public static bool operator !=(Signature? s1, Signature? s2)
        {
            return !(s1 == s2);
        }

        public bool Equals(Signature? other)
        {
            // Note that the return type does not factor into signature
            // equality.
            return null != other 
                   && Parameters.SequenceEqual(other.Parameters);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType()
                   && Equals((Signature) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ReturnType, Parameters);
        }
    }
}