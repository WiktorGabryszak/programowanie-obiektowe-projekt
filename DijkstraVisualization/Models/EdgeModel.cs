using System;

namespace DijkstraVisualization.Models
{
    public class EdgeModel
    {
        public Guid Id { get; init; }
        public Guid SourceNodeId { get; set; }
        public Guid TargetNodeId { get; set; }
        public double Weight { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
