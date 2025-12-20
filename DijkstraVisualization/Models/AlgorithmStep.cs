using System;
using System.Collections.Generic;

namespace DijkstraVisualization.Models
{
    public class AlgorithmStep
    {
        public Guid CurrentNodeId { get; set; }
        public List<Guid> VisitedNodes { get; } = new();
        public Dictionary<Guid, double> CurrentDistances { get; } = new();
    }
}
