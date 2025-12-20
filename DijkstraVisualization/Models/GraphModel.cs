using System.Collections.Generic;

namespace DijkstraVisualization.Models
{
    public class GraphModel
    {
        public List<NodeModel> Nodes { get; } = new();
        public List<EdgeModel> Edges { get; } = new();
    }
}
