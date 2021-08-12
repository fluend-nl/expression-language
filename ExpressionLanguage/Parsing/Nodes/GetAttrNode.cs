using System;

namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public enum GetAttrNodeType
    {
        PropertyCall,
        MethodCall,
        ArrayCall
    }

    public class GetAttrNode : Node
    {
        public Node Base { get; set; }
        public Node Attribute { get; set; }
        public ArrayNode Arguments { get; set; }
        public GetAttrNodeType Type { get; set; }

        public GetAttrNode(Node @base, Node attribute, ArrayNode arguments, GetAttrNodeType type)
        {
            Base = @base;
            Attribute = attribute;
            Arguments = arguments;
            Type = type;
        }

        public override string ToString()
        {
            return Type switch
            {
                GetAttrNodeType.PropertyCall => $"{Base}.{Attribute}",
                GetAttrNodeType.ArrayCall => $"{Base}[{Attribute}]",
                GetAttrNodeType.MethodCall => $"{Base}.{Attribute}({Arguments})",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}