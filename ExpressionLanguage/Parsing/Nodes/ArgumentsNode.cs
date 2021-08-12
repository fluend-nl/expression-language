using System.Collections.Generic;
using System.Text;

namespace Fluend.ExpressionLanguage.Parsing.Nodes
{
    public class ArgumentsNode : ArrayNode
    {
        public ArgumentsNode()
        {
        }
        
        public ArgumentsNode(IList<Node> nodes) : base(nodes)
        {
        }

        public override string ToString()
        {
            // Nodes are Value,Value
            var result = new StringBuilder();

            for (int i = 0; i < Nodes.Count; i++)
            {
                result.Append(Nodes[i]);

                if (i + 1 < Nodes.Count)
                {
                    result.Append(", ");
                }
            }

            return result.ToString();
        }
    }
}