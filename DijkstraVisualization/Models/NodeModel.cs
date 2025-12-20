using System;

namespace DijkstraVisualization.Models
{
    public class NodeModel
    {
        public Guid Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
    }
}
