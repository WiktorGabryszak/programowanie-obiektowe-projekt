using System;

namespace DijkstraVisualization.Services
{
    /// <summary>
    /// Event arguments for edge visualization state changes.
    /// </summary>
    public class EdgeVisualizationEventArgs : EventArgs
    {
        public Guid EdgeId { get; init; }
        public bool IsOnShortestPath { get; init; }
        public bool IsBeingRelaxed { get; init; }
        public double RelaxationProgress { get; init; }
        public bool RelaxationDirectionReversed { get; init; }
    }
}
