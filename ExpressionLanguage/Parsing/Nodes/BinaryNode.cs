namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public class BinaryNode : Node
    {
        public string Operator { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }
        
        public BinaryNode(string @operator, Node left, Node right)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            if (Operator.Contains("&&") || Operator.Contains("||"))
            {
                return $"({Left}) {Operator} ({Right})";
            }

            return $"{Left} {Operator} {Right}";
        }
    }
}