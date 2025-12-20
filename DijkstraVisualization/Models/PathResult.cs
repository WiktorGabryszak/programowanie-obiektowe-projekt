using System;
using System.Collections.Generic;

namespace DijkstraVisualization.Models
{
    public class PathResult
    {
        public bool PathFound { get; set; }
        public List<Guid> NodePath { get; } = new();
        public double TotalCost { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }
}
