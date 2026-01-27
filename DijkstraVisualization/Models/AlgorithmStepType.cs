namespace DijkstraVisualization.Models
{
    /// <summary>
    /// Represents the type of algorithm step for visualization.
    /// </summary>
    public enum AlgorithmStepType
    {
        /// <summary>
        /// Initial step - sets up distances (? for all, 0 for start).
        /// </summary>
        Initialize,

        /// <summary>
        /// Dijkstra enters a node (yellow border).
        /// </summary>
        VisitNode,

        /// <summary>
        /// Dijkstra is relaxing edges from current node (wave animation).
        /// </summary>
        RelaxEdges,

        /// <summary>
        /// A node's distance becomes finalized (green border).
        /// </summary>
        FinalizeNode,

        /// <summary>
        /// Algorithm completed - show final path.
        /// </summary>
        Complete
    }
}
