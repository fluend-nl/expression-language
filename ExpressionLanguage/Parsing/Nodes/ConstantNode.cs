using System;

namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public enum ConstantType
    {
        String,
        Number,
        Null,
        Bool,
        Name
    }
    
    public class ConstantNode : Node
    {
        public object? Value { get; set; }
        public ConstantType Type { get; set; }

        public ConstantNode(bool value)
        {
            Value = value;
            Type = ConstantType.Bool;
        }

        public ConstantNode(string value)
        {
            Value = value;
            Type = ConstantType.String;
        }
        
        public ConstantNode(double value)
        {
            Value = value;
            Type = ConstantType.Number;
        }
        
        public ConstantNode(object? value, ConstantType type)
        {
            Value = value;
            Type = type;
        }

        public override string ToString()
        {
            // Note: the null-coalesce with string.Empty is only
            // to make the compiler happy. Value is semantically only
            // null when the Type is ConstantType is Null.
            return Type switch
            {
                ConstantType.String => $"'{Value}'",
                ConstantType.Number or ConstantType.Name => Value!.ToString(),
                ConstantType.Null => "null",
                ConstantType.Bool => (bool) Value! ? "true" : "false",
                _ => throw new ArgumentOutOfRangeException()
            } ?? string.Empty;
        }
    }
}