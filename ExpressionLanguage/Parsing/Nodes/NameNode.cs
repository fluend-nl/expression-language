namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public class NameNode : Node
    {
        public string Name { get; set; }
        
        public NameNode(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}