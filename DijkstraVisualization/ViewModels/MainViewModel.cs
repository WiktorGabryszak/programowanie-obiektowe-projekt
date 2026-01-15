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
            private set => SetProperty(ref _isVisualizing, value);
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

            if (_graph.Edges.Any(e => e.SourceNodeId == creation.SourceNodeId && e.TargetNodeId == creation.TargetNodeId))
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

            ResetNodeStates();
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
            ResetNodeStates();
            ResetEdgeStates();

            _visualizationCts = new CancellationTokenSource();
            var token = _visualizationCts.Token;
            IsVisualizing = true;

            try
            {
                var intervalMs = (int)(AnimationInterval * 1000);
                var stopwatch = new Stopwatch();

                foreach (var step in _dijkstraService.GetVisualizationSteps(_graph, StartNode.Id, EndNode.Id))
                {
                    token.ThrowIfCancellationRequested();
                    stopwatch.Restart();

                    ApplyStep(step);

                    stopwatch.Stop();
                    var elapsed = (int)stopwatch.ElapsedMilliseconds;
                    var delay = intervalMs - elapsed;
                    if (delay > 0)
                    {
                        await Task.Delay(delay, token).ConfigureAwait(true);
                    }
                }

                var result = _dijkstraService.CalculatePath(_graph, StartNode.Id, EndNode.Id);
                ApplyPathResult(result);

                if (!result.PathFound)
                {
                    ShowErrorMessage("No path found between selected nodes!");
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

        private void ApplyStep(AlgorithmStep step)
        {
            var visited = new HashSet<Guid>(step.VisitedNodes);
            foreach (var node in Nodes)
            {
                node.IsVisited = visited.Contains(node.Id);
                node.IsCurrentNode = node.Id == step.CurrentNodeId;
            }
        }

        private void ApplyPathResult(PathResult result)
        {
            var pathNodes = result.PathFound ? new HashSet<Guid>(result.NodePath) : new HashSet<Guid>();

            foreach (var node in Nodes)
            {
                node.IsOnShortestPath = result.PathFound && pathNodes.Contains(node.Id);
            }

            // Highlight edges on the shortest path
            if (result.PathFound && result.NodePath.Count > 1)
            {
                for (int i = 0; i < result.NodePath.Count - 1; i++)
                {
                    var fromId = result.NodePath[i];
                    var toId = result.NodePath[i + 1];
                    var edge = Edges.FirstOrDefault(e => e.Source.Id == fromId && e.Target.Id == toId);
                    if (edge != null)
                    {
                        edge.IsOnShortestPath = true;
                    }
                }
            }
        }

        private void ResetNodeStates()
        {
            foreach (var node in Nodes)
            {
                node.IsVisited = false;
                node.IsCurrentNode = false;
                node.IsOnShortestPath = false;
            }
        }

        private void ResetEdgeStates()
        {
            foreach (var edge in Edges)
            {
                edge.IsOnShortestPath = false;
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
