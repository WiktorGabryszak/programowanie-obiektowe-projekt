using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using DijkstraVisualization.Models;
using DijkstraVisualization.Services;

namespace DijkstraVisualization.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private const int VisualizationDelayMilliseconds = 350;
        private readonly GraphModel _graph = new();
        private readonly IDijkstraService _dijkstraService;
        private readonly Dictionary<Guid, NodeViewModel> _nodeLookup = new();
        private readonly Dictionary<Guid, EdgeViewModel> _edgeLookup = new();
        private readonly RelayCommand<NodePlacement> _addNodeCommand;
        private readonly RelayCommand<EdgeCreation> _addEdgeCommand;
        private readonly RelayCommand<Guid> _removeNodeCommand;
        private readonly RelayCommand<TargetSelection> _setTargetCommand;
        private readonly AsyncRelayCommand _startAlgorithmCommand;
        private CancellationTokenSource? _visualizationCts;
        private NodeViewModel? _selectedNode;
        private NodeViewModel? _startNode;
        private NodeViewModel? _endNode;
        private bool _isVisualizing;

        public MainViewModel(IDijkstraService dijkstraService)
        {
            _dijkstraService = dijkstraService ?? throw new ArgumentNullException(nameof(dijkstraService));

            Nodes = new ObservableCollection<NodeViewModel>();
            Edges = new ObservableCollection<EdgeViewModel>();

            _addNodeCommand = new RelayCommand<NodePlacement>(AddNode);
            _addEdgeCommand = new RelayCommand<EdgeCreation>(AddEdge);
            _removeNodeCommand = new RelayCommand<Guid>(RemoveNode);
            _setTargetCommand = new RelayCommand<TargetSelection>(SetTargetNode);
            _startAlgorithmCommand = new AsyncRelayCommand(StartVisualizationAsync, CanStartVisualization);
        }

        public ObservableCollection<NodeViewModel> Nodes { get; }
        public ObservableCollection<EdgeViewModel> Edges { get; }

        public NodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode == value)
                {
                    return;
                }

                if (_selectedNode != null)
                {
                    _selectedNode.IsSelected = false;
                }

                if (SetProperty(ref _selectedNode, value))
                {
                    if (_selectedNode != null)
                    {
                        _selectedNode.IsSelected = true;
                    }
                }
            }
        }

        public NodeViewModel? StartNode
        {
            get => _startNode;
            private set
            {
                if (_startNode == value)
                {
                    return;
                }

                if (_startNode != null)
                {
                    _startNode.IsStartNode = false;
                }

                if (SetProperty(ref _startNode, value))
                {
                    if (_startNode != null)
                    {
                        _startNode.IsStartNode = true;
                    }

                    _startAlgorithmCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public NodeViewModel? EndNode
        {
            get => _endNode;
            private set
            {
                if (_endNode == value)
                {
                    return;
                }

                if (_endNode != null)
                {
                    _endNode.IsEndNode = false;
                }

                if (SetProperty(ref _endNode, value))
                {
                    if (_endNode != null)
                    {
                        _endNode.IsEndNode = true;
                    }

                    _startAlgorithmCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public bool IsVisualizing
        {
            get => _isVisualizing;
            private set
            {
                if (SetProperty(ref _isVisualizing, value))
                {
                    _startAlgorithmCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public RelayCommand<NodePlacement> AddNodeCommand => _addNodeCommand;
        public RelayCommand<EdgeCreation> AddEdgeCommand => _addEdgeCommand;
        public RelayCommand<Guid> RemoveNodeCommand => _removeNodeCommand;
        public RelayCommand<TargetSelection> SetTargetCommand => _setTargetCommand;
        public IAsyncRelayCommand StartAlgorithmCommand => _startAlgorithmCommand;

        public void Dispose()
        {
            CancelVisualization();

            foreach (var edge in _edgeLookup.Values.ToList())
            {
                edge.Dispose();
            }

            _edgeLookup.Clear();
        }

        private void AddNode(NodePlacement placement)
        {
            if (double.IsNaN(placement.X) || double.IsNaN(placement.Y))
            {
                return;
            }

            var model = new NodeModel
            {
                Id = Guid.NewGuid(),
                Name = $"Node {Nodes.Count + 1}",
                X = placement.X,
                Y = placement.Y
            };

            _graph.Nodes.Add(model);
            var viewModel = new NodeViewModel(model);
            Nodes.Add(viewModel);
            _nodeLookup[viewModel.Id] = viewModel;
        }

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
                return;
            }

            var model = new EdgeModel
            {
                Id = Guid.NewGuid(),
                SourceNodeId = creation.SourceNodeId,
                TargetNodeId = creation.TargetNodeId,
                Weight = Math.Max(0, creation.Weight),
                Name = string.IsNullOrWhiteSpace(creation.Name)
                    ? $"{source.Name}->{target.Name}"
                    : creation.Name
            };

            _graph.Edges.Add(model);
            var edgeViewModel = new EdgeViewModel(model, source, target);
            _edgeLookup[edgeViewModel.Id] = edgeViewModel;
            Edges.Add(edgeViewModel);
        }

        private void RemoveNode(Guid nodeId)
        {
            if (!_nodeLookup.TryGetValue(nodeId, out var node))
            {
                return;
            }

            if (SelectedNode == node)
            {
                SelectedNode = null;
            }

            if (StartNode == node)
            {
                StartNode = null;
            }

            if (EndNode == node)
            {
                EndNode = null;
            }

            var edgesToRemove = Edges.Where(e => e.Source == node || e.Target == node).ToList();
            foreach (var edge in edgesToRemove)
            {
                RemoveEdge(edge);
            }

            Nodes.Remove(node);
            _graph.Nodes.Remove(node.ToModel());
            _nodeLookup.Remove(nodeId);
        }

        private void RemoveEdge(EdgeViewModel edge)
        {
            edge.Dispose();
            Edges.Remove(edge);
            _edgeLookup.Remove(edge.Id);
            _graph.Edges.Remove(edge.ToModel());
        }

        private void SetTargetNode(TargetSelection selection)
        {
            if (!_nodeLookup.TryGetValue(selection.NodeId, out var node))
            {
                return;
            }

            switch (selection.Kind)
            {
                case TargetKind.Selection:
                    SelectedNode = node;
                    break;
                case TargetKind.Start:
                    StartNode = node;
                    break;
                case TargetKind.End:
                    EndNode = node;
                    break;
            }
        }

        private bool CanStartVisualization()
        {
            return StartNode != null && EndNode != null && !IsVisualizing;
        }

        private async Task StartVisualizationAsync()
        {
            if (StartNode == null || EndNode == null)
            {
                return;
            }

            CancelVisualization();
            ResetNodeStates();

            _visualizationCts = new CancellationTokenSource();
            var token = _visualizationCts.Token;
            IsVisualizing = true;

            try
            {
                foreach (var step in _dijkstraService.GetVisualizationSteps(_graph, StartNode.Id, EndNode.Id))
                {
                    token.ThrowIfCancellationRequested();
                    ApplyStep(step);
                    await Task.Delay(VisualizationDelayMilliseconds, token).ConfigureAwait(false);
                }

                var result = _dijkstraService.CalculatePath(_graph, StartNode.Id, EndNode.Id);
                ApplyPathResult(result);
            }
            catch (OperationCanceledException)
            {
                // Swallow cancellation since it is user initiated.
            }
            finally
            {
                if (_visualizationCts != null)
                {
                    _visualizationCts.Dispose();
                    _visualizationCts = null;
                }

                ClearCurrentNodeHighlight();
                IsVisualizing = false;
            }
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
            var pathNodes = result.PathFound
                ? new HashSet<Guid>(result.NodePath)
                : new HashSet<Guid>();

            foreach (var node in Nodes)
            {
                node.IsOnShortestPath = result.PathFound && pathNodes.Contains(node.Id);
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

        private void ClearCurrentNodeHighlight()
        {
            foreach (var node in Nodes)
            {
                node.IsCurrentNode = false;
            }
        }

        private void CancelVisualization()
        {
            if (_visualizationCts == null)
            {
                return;
            }

            if (!_visualizationCts.IsCancellationRequested)
            {
                _visualizationCts.Cancel();
            }
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
