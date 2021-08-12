using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public class HashNode : Node
    {
        public IList<Node> Nodes { get; set; }

        public HashNode()
        {
            Nodes = new List<Node>();
        }
        
        public HashNode(IList<Node> nodes)
        {
            Nodes = nodes;
        }

        public void AddElement(Node key, Node value)
        {
            Nodes.Add(key);
            Nodes.Add(value);
        }

        public override string ToString()
        {
            // Nodes are Key,Value,Key,Value
            var builder = new StringBuilder();

            Debug.Assert(Nodes.Count % 2 == 0);

            builder.Append('{');
            
            for (int i = 0; i < Nodes.Count; i += 2)
            {
                builder.Append(Nodes[i])
                    .Append(": ")
                    .Append(Nodes[i + 1]);

                if (i + 2 < Nodes.Count)
                {
                    builder.Append(", ");
                } 
            }

            builder.Append('}');
            
            return builder.ToString();
        }
    }
}