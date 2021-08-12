using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public class ArrayNode : Node
    {
        protected int Index = -1;

        public IList<Node> Nodes { get; set; }

        public ArrayNode()
        {
            Nodes = new List<Node>();
        }
        
        public ArrayNode(IList<Node> nodes)
        {
            Nodes = nodes;
        }

        public void AddElement(Node value)
        {
            AddElement(new ConstantNode(++Index, ConstantType.Number), value);
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

            builder.Append('[');
            
            for (int i = 1; i < Nodes.Count; i += 2)
            {
                // We're only appending the values to the output.
                builder.Append(Nodes[i]);

                if (i + 1 < Nodes.Count)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(']');

            return builder.ToString();
        }
    }
}