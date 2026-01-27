using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.Services
{
    /// <summary>
    /// Orchestrates the visualization of Dijkstra's algorithm step by step.
    /// Follows SRP by only handling visualization orchestration logic.
    /// </summary>
    public class VisualizationOrchestrator : IVisualizationOrchestrator
    {
        private readonly IDijkstraService _dijkstraService;
        private CancellationTokenSource? _cts;

        public VisualizationOrchestrator(IDijkstraService dijkstraService)
        {
            _dijkstraService = dijkstraService ?? throw new ArgumentNullException(nameof(dijkstraService));
        }

        /// <inheritdoc />
        public event EventHandler<NodeVisualizationEventArgs>? NodeStateChanged;

        /// <inheritdoc />
        public event EventHandler<EdgeVisualizationEventArgs>? EdgeStateChanged;

        /// <inheritdoc />
        public event EventHandler<VisualizationCompleteEventArgs>? VisualizationComplete;

        /// <inheritdoc />
        public event EventHandler<VisualizationErrorEventArgs>? VisualizationError;

        /// <inheritdoc />
        public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

        /// <inheritdoc />
        public async Task RunVisualizationAsync(GraphModel graph, Guid startId, Guid endId, int intervalMs, CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _cts.Token;

            try
            {
                foreach (var step in _dijkstraService.GetVisualizationSteps(graph, startId, endId))
                {
                    token.ThrowIfCancellationRequested();

                    var stepStopwatch = Stopwatch.StartNew();
                    
                    ProcessStep(step);
                    
                    stepStopwatch.Stop();

                    // Wait remaining time to maintain consistent interval
                    var elapsed = (int)stepStopwatch.ElapsedMilliseconds;
                    var delay = intervalMs - elapsed;
                    if (delay > 0)
                    {
                        await Task.Delay(delay, token).ConfigureAwait(true);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // User cancelled - ignore
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }

        /// <inheritdoc />
        public void Cancel()
        {
            if (_cts is { IsCancellationRequested: false })
            {
                _cts.Cancel();
            }
        }

        private void ProcessStep(AlgorithmStep step)
        {
            switch (step.StepType)
            {
                case AlgorithmStepType.Initialize:
                    ProcessInitializeStep(step);
                    break;

                case AlgorithmStepType.VisitNode:
                    ProcessVisitNodeStep(step);
                    break;

                case AlgorithmStepType.FinalizeNode:
                    ProcessFinalizeNodeStep(step);
                    break;

                case AlgorithmStepType.RelaxEdges:
                    ProcessRelaxEdgesStep(step);
                    break;

                case AlgorithmStepType.Complete:
                    ProcessCompleteStep(step);
                    break;
            }
        }

        private void ProcessInitializeStep(AlgorithmStep step)
        {
            foreach (var (nodeId, distance) in step.CurrentDistances)
            {
                NodeStateChanged?.Invoke(this, new NodeVisualizationEventArgs
                {
                    NodeId = nodeId,
                    ShowDistanceLabel = true,
                    Distance = distance
                });
            }
        }

        private void ProcessVisitNodeStep(AlgorithmStep step)
        {
            // Set current node
            NodeStateChanged?.Invoke(this, new NodeVisualizationEventArgs
            {
                NodeId = step.CurrentNodeId,
                IsCurrentNode = true
            });

            // Update distances
            foreach (var (nodeId, distance) in step.CurrentDistances)
            {
                NodeStateChanged?.Invoke(this, new NodeVisualizationEventArgs
                {
                    NodeId = nodeId,
                    Distance = distance
                });
            }

            // Update visited nodes
            foreach (var nodeId in step.FinalizedNodes)
            {
                NodeStateChanged?.Invoke(this, new NodeVisualizationEventArgs
                {
                    NodeId = nodeId,
                    IsVisited = true
                });
            }
        }

        private void ProcessFinalizeNodeStep(AlgorithmStep step)
        {
            NodeStateChanged?.Invoke(this, new NodeVisualizationEventArgs
            {
                NodeId = step.CurrentNodeId,
                IsVisited = true
            });

            foreach (var nodeId in step.FinalizedNodes)
            {
                NodeStateChanged?.Invoke(this, new NodeVisualizationEventArgs
                {
                    NodeId = nodeId,
                    IsVisited = true
                });
            }
        }

        private void ProcessRelaxEdgesStep(AlgorithmStep step)
        {
            foreach (var (edgeId, (neighborId, newDistance, isDirectionReversed)) in step.RelaxedEdges)
            {
                EdgeStateChanged?.Invoke(this, new EdgeVisualizationEventArgs
                {
                    EdgeId = edgeId,
                    IsBeingRelaxed = true,
                    RelaxationDirectionReversed = isDirectionReversed
                });

                NodeStateChanged?.Invoke(this, new NodeVisualizationEventArgs
                {
                    NodeId = neighborId,
                    Distance = newDistance,
                    TriggerDistanceAnimation = true
                });
            }
        }

        private void ProcessCompleteStep(AlgorithmStep step)
        {
            if (step.PathFound && step.FinalPath.Count > 0)
            {
                // Highlight nodes on path
                foreach (var nodeId in step.FinalPath)
                {
                    NodeStateChanged?.Invoke(this, new NodeVisualizationEventArgs
                    {
                        NodeId = nodeId,
                        IsOnShortestPath = true
                    });
                }

                VisualizationComplete?.Invoke(this, new VisualizationCompleteEventArgs
                {
                    PathFound = true,
                    Path = new(step.FinalPath),
                    TotalCost = step.TotalCost
                });
            }
            else
            {
                VisualizationError?.Invoke(this, new VisualizationErrorEventArgs
                {
                    Message = "No path found between selected nodes!"
                });

                VisualizationComplete?.Invoke(this, new VisualizationCompleteEventArgs
                {
                    PathFound = false
                });
            }
        }
    }
}
