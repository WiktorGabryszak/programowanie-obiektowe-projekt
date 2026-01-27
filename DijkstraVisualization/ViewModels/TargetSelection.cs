using System;

namespace DijkstraVisualization.ViewModels
{
    /// <summary>
    /// Represents a target selection request with node ID and selection kind.
    /// </summary>
    public readonly record struct TargetSelection(Guid NodeId, TargetKind Kind);
}
