using System;

namespace DijkstraVisualization.Services
{
    /// <summary>
    /// Event arguments for node visualization state changes.
    /// </summary>
    public class NodeVisualizationEventArgs : EventArgs
    {
        public Guid NodeId { get; init; }
        public bool IsCurrentNode { get; init; }
        public bool IsVisited { get; init; }
        public bool IsOnShortestPath { get; init; }
        public double Distance { get; init; } = double.PositiveInfinity;
        public bool ShowDistanceLabel { get; init; }
        public bool TriggerDistanceAnimation { get; init; }
    }
}
