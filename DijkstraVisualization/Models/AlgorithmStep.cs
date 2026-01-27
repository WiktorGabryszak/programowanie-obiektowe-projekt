using System;
using System.Collections.Generic;

namespace DijkstraVisualization.Models
{
    /// <summary>
    /// Represents a single visualization step of Dijkstra's algorithm.
    /// </summary>
    public class AlgorithmStep
    {
        /// <summary>
        /// The type of this step.
        /// </summary>
        public AlgorithmStepType StepType { get; init; }

        /// <summary>
        /// The node currently being processed (for VisitNode, FinalizeNode).
        /// </summary>
        public Guid CurrentNodeId { get; init; }

        /// <summary>
        /// Edges being relaxed in this step (for RelaxEdges).
        /// Key: EdgeId, Value: (NeighborNodeId, NewDistance, IsDirectionReversed)
        /// IsDirectionReversed is true when Dijkstra is at the Target node and wave goes to Source.
        /// </summary>
        public Dictionary<Guid, (Guid NeighborId, double NewDistance, bool IsDirectionReversed)> RelaxedEdges { get; } = new();

        /// <summary>
        /// Current distances from start to each node.
        /// </summary>
        public Dictionary<Guid, double> CurrentDistances { get; } = new();

        /// <summary>
        /// Nodes that have been finalized (their shortest distance is known).
        /// </summary>
        public HashSet<Guid> FinalizedNodes { get; } = new();

        /// <summary>
        /// The final path (for Complete step).
        /// </summary>
        public List<Guid> FinalPath { get; } = new();

        /// <summary>
        /// Whether a path was found (for Complete step).
        /// </summary>
        public bool PathFound { get; init; }

        /// <summary>
        /// Total cost of the path (for Complete step).
        /// </summary>
        public double TotalCost { get; init; }
    }
}
