namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public class UnaryNode : Node
    {
        public string Operator { get; set; }
        public Node Node { get; set; }

        public UnaryNode(string @operator, Node node)
        {
            Operator = @operator;
            Node = node;
        }

        public override string ToString()
        {
            return $"{Operator} ({Node})";
        }
    }
}