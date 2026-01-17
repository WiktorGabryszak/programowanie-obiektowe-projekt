using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DijkstraVisualization.Models;
using DijkstraVisualization.Services;

namespace DijkstraVisualization.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly GraphModel _graph = new();
        private readonly IDijkstraService _dijkstraService;
        private readonly Dictionary<Guid, NodeViewModel> _nodeLookup = new();
        private readonly Dictionary<Guid, EdgeViewModel> _edgeLookup = new();
        private CancellationTokenSource? _visualizationCts;
        private NodeViewModel? _selectedNode;
        private NodeViewModel? _startNode;
        private NodeViewModel? _endNode;
        private int _nodeCounter;
        private bool _isVisualizing;
        private double _animationInterval = 0.5;
        private string? _errorMessage;
        private bool _showError;

        private const int WaveAnimationSteps = 50;
        private const int WaveStepDelayMs = 10;

        // Pulse animation constants
        private const int PulseAnimationSteps = 20;
        private const int PulseStepDelayMs = 15;
        private const double PulseMaxScale = 1.4;

        public MainViewModel(IDijkstraService dijkstraService)
        {
            _dijkstraService = dijkstraService ?? throw new ArgumentNullException(nameof(dijkstraService));
            Nodes = new ObservableCollection<NodeViewModel>();
            Edges = new ObservableCollection<EdgeViewModel>();
        }

        public ObservableCollection<NodeViewModel> Nodes { get; }
        public ObservableCollection<EdgeViewModel> Edges { get; }

        public bool IsVisualizing
        {
            get => _isVisualizing;
            private set
            {
                if (SetProperty(ref _isVisualizing, value))
                {
                    StartAlgorithmCommand.NotifyCanExecuteChanged();
                    ClearGraphCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public double AnimationInterval
        {
            get => _animationInterval;
            set => SetProperty(ref _animationInterval, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool ShowError
        {
            get => _showError;
            set => SetProperty(ref _showError, value);
        }

        public NodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode == value) return;
                if (_selectedNode != null) _selectedNode.IsSelected = false;
                if (SetProperty(ref _selectedNode, value) && _selectedNode != null)
                {
                    _selectedNode.IsSelected = true;
                }
            }
        }

        public NodeViewModel? StartNode
        {
            get => _startNode;
            private set
            {
                if (_startNode == value) return;
                if (_startNode != null) _startNode.IsStartNode = false;
                if (SetProperty(ref _startNode, value) && _startNode != null)
                {
                    _startNode.IsStartNode = true;
                }
                StartAlgorithmCommand.NotifyCanExecuteChanged();
            }
        }

        public NodeViewModel? EndNode
        {
            get => _endNode;
            private set
            {
                if (_endNode == value) return;
                if (_endNode != null) _endNode.IsEndNode = false;
                if (SetProperty(ref _endNode, value) && _endNode != null)
                {
                    _endNode.IsEndNode = true;
                }
                StartAlgorithmCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private void AddNode(NodePlacement placement)
        {
            if (double.IsNaN(placement.X) || double.IsNaN(placement.Y)) return;

            _nodeCounter++;
            var model = new NodeModel
            {
                Id = Guid.NewGuid(),
                Name = $"Node {_nodeCounter}",
                X = placement.X,
                Y = placement.Y
            };

            _graph.Nodes.Add(model);
            var viewModel = new NodeViewModel(model);
            Nodes.Add(viewModel);
            _nodeLookup[viewModel.Id] = viewModel;
        }

        [RelayCommand]
        private void AddEdge(EdgeCreation creation)
        {
            if (!_nodeLookup.TryGetValue(creation.SourceNodeId, out var source) ||
                !_nodeLookup.TryGetValue(creation.TargetNodeId, out var target) ||
                creation.SourceNodeId == creation.TargetNodeId)
            {
                return;
            }

            // Check if edge already exists in either direction (undirected graph)
            var edgeExists = _graph.Edges.Any(e => 
                (e.SourceNodeId == creation.SourceNodeId && e.TargetNodeId == creation.TargetNodeId) ||
                (e.SourceNodeId == creation.TargetNodeId && e.TargetNodeId == creation.SourceNodeId));

            if (edgeExists)
            {
                ShowErrorMessage("Edge between these nodes already exists!");
                return;
            }

            var edgeCounter = Edges.Count + 1;
            var model = new EdgeModel
            {
                Id = Guid.NewGuid(),
                SourceNodeId = creation.SourceNodeId,
                TargetNodeId = creation.TargetNodeId,
                Weight = Math.Max(0, creation.Weight),
                Name = string.IsNullOrWhiteSpace(creation.Name)
                    ? $"Edge {edgeCounter}"
                    : creation.Name
            };

            _graph.Edges.Add(model);
            var edgeViewModel = new EdgeViewModel(model, source, target);
            _edgeLookup[edgeViewModel.Id] = edgeViewModel;
            Edges.Add(edgeViewModel);
        }

        [RelayCommand]
        private void RemoveNode(Guid nodeId)
        {
            if (!_nodeLookup.TryGetValue(nodeId, out var node)) return;

            if (SelectedNode == node) SelectedNode = null;
            if (StartNode == node) StartNode = null;
            if (EndNode == node) EndNode = null;

            var edgesToRemove = Edges.Where(e => e.Source == node || e.Target == node).ToList();
            foreach (var edge in edgesToRemove)
            {
                RemoveEdgeInternal(edge);
            }

            Nodes.Remove(node);
            _graph.Nodes.Remove(node.ToModel());
            _nodeLookup.Remove(nodeId);
        }

        [RelayCommand]
        private void RemoveEdge(Guid edgeId)
        {
            if (_edgeLookup.TryGetValue(edgeId, out var edge))
            {
                RemoveEdgeInternal(edge);
            }
        }

        private void RemoveEdgeInternal(EdgeViewModel edge)
        {
            edge.Dispose();
            Edges.Remove(edge);
            _edgeLookup.Remove(edge.Id);
            var model = _graph.Edges.FirstOrDefault(e => e.Id == edge.Id);
            if (model != null)
            {
                _graph.Edges.Remove(model);
            }
        }

        [RelayCommand]
        private void SetTarget(TargetSelection selection)
        {
            if (!_nodeLookup.TryGetValue(selection.NodeId, out var node)) return;

            switch (selection.Kind)
            {
                case TargetKind.Selection:
                    SelectedNode = node;
                    break;
                case TargetKind.Start:
                    if (EndNode == node) EndNode = null;
                    StartNode = node;
                    break;
                case TargetKind.End:
                    if (StartNode == node) StartNode = null;
                    EndNode = node;
                    break;
            }
        }

        [RelayCommand(CanExecute = nameof(CanClearGraph))]
        private void ClearGraph()
        {
            CancelVisualization();

            foreach (var edge in _edgeLookup.Values.ToList())
            {
                edge.Dispose();
            }

            Edges.Clear();
            Nodes.Clear();
            _edgeLookup.Clear();
            _nodeLookup.Clear();
            _graph.Edges.Clear();
            _graph.Nodes.Clear();

            StartNode = null;
            EndNode = null;
            SelectedNode = null;
            _nodeCounter = 0;
        }

        private bool CanClearGraph() => !IsVisualizing;

        [RelayCommand(CanExecute = nameof(CanStartVisualization))]
        private async Task StartAlgorithmAsync()
        {
            if (StartNode == null || EndNode == null)
            {
                ShowErrorMessage("Please select Start and Destination nodes!");
                return;
            }

            CancelVisualization();
            ResetVisualizationStates();

            _visualizationCts = new CancellationTokenSource();
            var token = _visualizationCts.Token;
            IsVisualizing = true;

            try
            {
                var intervalMs = (int)(AnimationInterval * 1000);

                foreach (var step in _dijkstraService.GetVisualizationSteps(_graph, StartNode.Id, EndNode.Id))
                {
                    token.ThrowIfCancellationRequested();
                    
                    var stepStopwatch = Stopwatch.StartNew();
                    
                    await ApplyStepAsync(step, token);
                    
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
                _visualizationCts?.Dispose();
                _visualizationCts = null;
                ClearCurrentNodeHighlight();
                IsVisualizing = false;
            }
        }

        private bool CanStartVisualization() => StartNode != null && EndNode != null && !IsVisualizing;

        public void ShowErrorMessage(string message)
        {
            ErrorMessage = message;
            ShowError = true;
            _ = HideErrorAfterDelay();
        }

        private async Task HideErrorAfterDelay()
        {
            await Task.Delay(3000).ConfigureAwait(true);
            ShowError = false;
        }

        #region Visualization Step Application

        private async Task ApplyStepAsync(AlgorithmStep step, CancellationToken token)
        {
            switch (step.StepType)
            {
                case AlgorithmStepType.Initialize:
                    ApplyInitializeStep(step);
                    break;

                case AlgorithmStepType.VisitNode:
                    ApplyVisitNodeStep(step, token);
                    break;

                case AlgorithmStepType.FinalizeNode:
                    ApplyFinalizeNodeStep(step);
                    break;

                case AlgorithmStepType.RelaxEdges:
                    await ApplyRelaxEdgesStepAsync(step, token);
                    break;

                case AlgorithmStepType.Complete:
                    ApplyCompleteStep(step);
                    break;
            }
        }

        private void ApplyInitializeStep(AlgorithmStep step)
        {
            // Show distance labels on all nodes
            foreach (var node in Nodes)
            {
                node.ShowDistanceLabel = true;
                
                if (step.CurrentDistances.TryGetValue(node.Id, out var distance))
                {
                    node.SetDistance(distance);
                }
            }
        }

        private async Task ApplyVisitNodeStep(AlgorithmStep step, CancellationToken token)
        {
            // Find the new current node
            if (!_nodeLookup.TryGetValue(step.CurrentNodeId, out var newCurrentNode))
            {
                return;
            }

            // Find the previous current node (if any)
            var previousCurrentNode = Nodes.FirstOrDefault(n => n.IsCurrentNode);

            // If there's a previous node, animate the transition
            if (previousCurrentNode != null && previousCurrentNode != newCurrentNode)
            {
                // First, mark the previous node as visited (this will show green border when yellow moves away)
                // But don't clear IsCurrentNode yet - yellow border stays on old node during this
                previousCurrentNode.IsVisited = true;
                
                // Animate the transition (yellow border stays on old node during this)
                await AnimateBorderTransitionAsync(previousCurrentNode, newCurrentNode, token);
                
                // Now remove yellow border from previous node (revealing green border underneath)
                previousCurrentNode.IsCurrentNode = false;
            }

            // Set new current node (yellow border)
            newCurrentNode.IsCurrentNode = true;

            // Update distances
            UpdateNodeDistances(step.CurrentDistances);

            // Update visited state from finalized nodes
            foreach (var nodeId in step.FinalizedNodes)
            {
                if (_nodeLookup.TryGetValue(nodeId, out var visitedNode))
                {
                    visitedNode.IsVisited = true;
                }
            }
        }

        /// <summary>
        /// Animates a smooth transition of the yellow border from one node to another.
        /// During this animation, the old node keeps its yellow border, then reveals green underneath.
        /// </summary>
        private async Task AnimateBorderTransitionAsync(NodeViewModel fromNode, NodeViewModel toNode, CancellationToken token)
        {
            const int transitionSteps = 15;
            const int transitionDelayMs = 20;

            // Smooth transition delay - yellow border stays on old node, then jumps to new node
            for (int i = 0; i < transitionSteps; i++)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(transitionDelayMs, token).ConfigureAwait(true);
            }
        }

        private void ApplyFinalizeNodeStep(AlgorithmStep step)
        {
            // Mark node as finalized/visited (green border)
            if (_nodeLookup.TryGetValue(step.CurrentNodeId, out var node))
            {
                // Don't clear IsCurrentNode here - it's handled in ApplyVisitNodeStep
                // Just ensure it's marked as visited
                node.IsVisited = true;
            }

            // Update all visited nodes
            foreach (var nodeId in step.FinalizedNodes)
            {
                if (_nodeLookup.TryGetValue(nodeId, out var visitedNode))
                {
                    visitedNode.IsVisited = true;
                }
            }
        }

        private async Task ApplyRelaxEdgesStepAsync(AlgorithmStep step, CancellationToken token)
        {
            var edgesToAnimate = new List<EdgeViewModel>();

            // Mark ALL edges being checked as relaxing (for wave animation)
            foreach (var (edgeId, (neighborId, _, isDirectionReversed)) in step.RelaxedEdges)
            {
                if (_edgeLookup.TryGetValue(edgeId, out var edge))
                {
                    edge.IsBeingRelaxed = true;
                    edge.RelaxationProgress = 0;
                    edge.RelaxationDirectionReversed = isDirectionReversed;
                    edgesToAnimate.Add(edge);
                }
            }

            // Animate wave effect through all edges
            for (int i = 1; i <= WaveAnimationSteps; i++)
            {
                token.ThrowIfCancellationRequested();
                
                var progress = (double)i / WaveAnimationSteps;
                foreach (var edge in edgesToAnimate)
                {
                    edge.RelaxationProgress = progress;
                }
                
                await Task.Delay(WaveStepDelayMs, token).ConfigureAwait(true);
            }

            // Clear relaxation state
            foreach (var edge in edgesToAnimate)
            {
                edge.IsBeingRelaxed = false;
                edge.RelaxationProgress = 0;
                edge.RelaxationDirectionReversed = false;
            }

            // Update distances on target nodes after wave completes
            // Track which nodes got updated for pulse animation
            var updatedNodes = new List<NodeViewModel>();
            
            // Only update the neighbor nodes that were actually checked in this step
            foreach (var (edgeId, (neighborId, newDistance, _)) in step.RelaxedEdges)
            {
                if (_nodeLookup.TryGetValue(neighborId, out var node))
                {
                    var wasUpdated = node.SetDistance(newDistance);
                    if (wasUpdated)
                    {
                        node.TriggerDistanceUpdateAnimation();
                        updatedNodes.Add(node);
                    }
                }
            }

            // Animate pulse effect on updated nodes
            if (updatedNodes.Count > 0)
            {
                await AnimatePulseAsync(updatedNodes, token);
            }
        }

        /// <summary>
        /// Animates a pulse effect on the distance labels of updated nodes.
        /// </summary>
        private async Task AnimatePulseAsync(List<NodeViewModel> nodes, CancellationToken token)
        {
            // Expand phase
            for (int i = 1; i <= PulseAnimationSteps / 2; i++)
            {
                token.ThrowIfCancellationRequested();
                
                var progress = (double)i / (PulseAnimationSteps / 2);
                var scale = 1.0 + (PulseMaxScale - 1.0) * progress;
                
                foreach (var node in nodes)
                {
                    node.DistanceLabelScale = scale;
                }
                
                await Task.Delay(PulseStepDelayMs, token).ConfigureAwait(true);
            }

            // Contract phase
            for (int i = 1; i <= PulseAnimationSteps / 2; i++)
            {
                token.ThrowIfCancellationRequested();
                
                var progress = (double)i / (PulseAnimationSteps / 2);
                var scale = PulseMaxScale - (PulseMaxScale - 1.0) * progress;
                
                foreach (var node in nodes)
                {
                    node.DistanceLabelScale = scale;
                }
                
                await Task.Delay(PulseStepDelayMs, token).ConfigureAwait(true);
            }

            // Clear animation state
            foreach (var node in nodes)
            {
                node.ClearDistanceUpdateAnimation();
            }
        }

        private void ApplyCompleteStep(AlgorithmStep step)
        {
            // Clear yellow border from all nodes at the end
            foreach (var node in Nodes)
            {
                node.IsCurrentNode = false;
            }

            // Highlight shortest path
            if (step.PathFound && step.FinalPath.Count > 0)
            {
                var pathSet = new HashSet<Guid>(step.FinalPath);

                // Highlight nodes on path
                foreach (var node in Nodes)
                {
                    node.IsOnShortestPath = pathSet.Contains(node.Id);
                }

                // Highlight edges on path (check both directions since edges are undirected)
                for (int i = 0; i < step.FinalPath.Count - 1; i++)
                {
                    var fromId = step.FinalPath[i];
                    var toId = step.FinalPath[i + 1];

                    // Find edge in either direction
                    var edge = Edges.FirstOrDefault(e => 
                        (e.Source.Id == fromId && e.Target.Id == toId) ||
                        (e.Source.Id == toId && e.Target.Id == fromId));
                    
                    if (edge != null)
                    {
                        edge.IsOnShortestPath = true;
                    }
                }
            }
            else
            {
                ShowErrorMessage("No path found between selected nodes!");
            }
        }

        private void UpdateNodeDistances(Dictionary<Guid, double> distances)
        {
            foreach (var (nodeId, distance) in distances)
            {
                if (_nodeLookup.TryGetValue(nodeId, out var node))
                {
                    node.SetDistance(distance);
                }
            }
        }

        #endregion

        #region State Management

        private void ResetVisualizationStates()
        {
            foreach (var node in Nodes)
            {
                node.ResetVisualizationState();
            }

            foreach (var edge in Edges)
            {
                edge.ResetVisualizationState();
            }
        }

        private void ClearCurrentNodeHighlight()
        {
            foreach (var node in Nodes)
            {
                node.IsCurrentNode = false;
            }
        }

        private void CancelVisualization()
        {
            if (_visualizationCts is { IsCancellationRequested: false })
            {
                _visualizationCts.Cancel();
            }
        }

        #endregion

        public void Dispose()
        {
            CancelVisualization();
            foreach (var edge in _edgeLookup.Values.ToList())
            {
                edge.Dispose();
            }
            _edgeLookup.Clear();
        }
    }

    public readonly record struct NodePlacement(double X, double Y);
    public readonly record struct EdgeCreation(Guid SourceNodeId, Guid TargetNodeId, double Weight, string? Name = null);
    public readonly record struct TargetSelection(Guid NodeId, TargetKind Kind);

    public enum TargetKind
    {
        Selection,
        Start,
        End
    }
}
