using System;
using System.Collections.Generic;

namespace Fluend.ExpressionLanguage.Evaluation.Functions
{
    public class ExpressiveFunctionParameter
    {
        public string Name { get; }

        public Type Type { get; }

        public int Position { get; }

        public static bool operator ==(ExpressiveFunctionParameter? p1, ExpressiveFunctionParameter? p2)
        {
            if (p1 is null)
            {
                return p2 is null;
            }
            
            return p1.Equals(p2);
        }

        public static bool operator !=(ExpressiveFunctionParameter p1, ExpressiveFunctionParameter p2)
        {
            return !(p1 == p2);
        }

        public bool Equals(ExpressiveFunctionParameter? other)
        {
            if (null == other)
            {
                return false;
            }
            
            return Name == other.Name
                   && Type == other.Type
                   && Position == other.Position;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type, Position);
        }

        public ExpressiveFunctionParameter(string name, Type type, int position)
        {
            Name = name;
            Type = type;
            Position = position;
        }
    }
}