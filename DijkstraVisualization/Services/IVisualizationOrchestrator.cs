using System;
using System.Threading;
using System.Threading.Tasks;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.Services
{
    /// <summary>
    /// Interface for orchestrating algorithm visualization (SRP - Single Responsibility Principle).
    /// Handles the step-by-step execution and animation of algorithm steps.
    /// </summary>
    public interface IVisualizationOrchestrator
    {
        /// <summary>
        /// Event raised when a node's visualization state should be updated.
        /// </summary>
        event EventHandler<NodeVisualizationEventArgs>? NodeStateChanged;

        /// <summary>
        /// Event raised when an edge's visualization state should be updated.
        /// </summary>
        event EventHandler<EdgeVisualizationEventArgs>? EdgeStateChanged;

        /// <summary>
        /// Event raised when the visualization is complete.
        /// </summary>
        event EventHandler<VisualizationCompleteEventArgs>? VisualizationComplete;

        /// <summary>
        /// Event raised when an error occurs during visualization.
        /// </summary>
        event EventHandler<VisualizationErrorEventArgs>? VisualizationError;

        /// <summary>
        /// Gets whether a visualization is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Runs the visualization for the Dijkstra algorithm.
        /// </summary>
        /// <param name="graph">The graph to run the algorithm on.</param>
        /// <param name="startId">The start node ID.</param>
        /// <param name="endId">The end node ID.</param>
        /// <param name="intervalMs">The interval between steps in milliseconds.</param>
        /// <param name="cancellationToken">Token to cancel the visualization.</param>
        Task RunVisualizationAsync(GraphModel graph, Guid startId, Guid endId, int intervalMs, CancellationToken cancellationToken);

        /// <summary>
        /// Cancels the current visualization if running.
        /// </summary>
        void Cancel();
    }
}
