namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public class ConditionalNode : Node
    {
        public Node Expr1 { get; set; }
        public Node Expr2 { get; set; }
        public Node Expr3 { get; set; }

        public ConditionalNode(Node expr1, Node expr2, Node expr3)
        {
            Expr1 = expr1;
            Expr2 = expr2;
            Expr3 = expr3;
        }

        public override string ToString()
        {
            return $"({Expr1} ? {Expr2} : {Expr3})";
        }
    }
}