using System;

namespace DijkstraVisualization.ViewModels
{
    /// <summary>
    /// Represents an edge creation request with source/target node IDs and weight.
    /// </summary>
    public readonly record struct EdgeCreation(Guid SourceNodeId, Guid TargetNodeId, double Weight, string? Name = null);
}
