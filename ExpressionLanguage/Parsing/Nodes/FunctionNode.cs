namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public class FunctionNode : Node
    {
        public ArgumentsNode Arguments { get; }
        public string Name { get; }
        
        public FunctionNode(string name, ArgumentsNode args)
        {
            Arguments = args;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name}({Arguments})";
        }
    }
}