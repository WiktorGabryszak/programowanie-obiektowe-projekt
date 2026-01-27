using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.Services
{
    /// <summary>
    /// Manages graph operations including node and edge CRUD operations.
    /// Follows SRP by only handling graph structure management.
    /// </summary>
    public class GraphManager : IGraphManager
    {
        private readonly GraphModel _graph = new();
        private int _nodeCounter;
        private int _edgeCounter;

        /// <inheritdoc />
        public GraphModel Graph => _graph;

        /// <inheritdoc />
        public Guid AddNode(string name, double x, double y)
        {
            var model = new NodeModel
            {
                Id = Guid.NewGuid(),
                Name = name,
                X = x,
                Y = y
            };

            _graph.Nodes.Add(model);
            _nodeCounter++;
            return model.Id;
        }

        /// <inheritdoc />
        public bool RemoveNode(Guid nodeId)
        {
            var node = _graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null) return false;

            // Remove all connected edges first
            var connectedEdges = _graph.Edges
                .Where(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId)
                .ToList();

            foreach (var edge in connectedEdges)
            {
                _graph.Edges.Remove(edge);
            }

            return _graph.Nodes.Remove(node);
        }

        /// <inheritdoc />
        public NodeModel? GetNode(Guid nodeId)
        {
            return _graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
        }

        /// <inheritdoc />
        public Guid? AddEdge(Guid sourceId, Guid targetId, double weight, string? name = null)
        {
            // Validate nodes exist
            var sourceExists = _graph.Nodes.Any(n => n.Id == sourceId);
            var targetExists = _graph.Nodes.Any(n => n.Id == targetId);

            if (!sourceExists || !targetExists || sourceId == targetId)
            {
                return null;
            }

            // Check for existing edge
            if (EdgeExists(sourceId, targetId))
            {
                return null;
            }

            _edgeCounter++;
            var model = new EdgeModel
            {
                Id = Guid.NewGuid(),
                SourceNodeId = sourceId,
                TargetNodeId = targetId,
                Weight = Math.Max(0, weight),
                Name = string.IsNullOrWhiteSpace(name) ? $"Edge {_edgeCounter}" : name
            };

            _graph.Edges.Add(model);
            return model.Id;
        }

        /// <inheritdoc />
        public bool RemoveEdge(Guid edgeId)
        {
            var edge = _graph.Edges.FirstOrDefault(e => e.Id == edgeId);
            return edge != null && _graph.Edges.Remove(edge);
        }

        /// <inheritdoc />
        public bool EdgeExists(Guid sourceId, Guid targetId)
        {
            return _graph.Edges.Any(e =>
                (e.SourceNodeId == sourceId && e.TargetNodeId == targetId) ||
                (e.SourceNodeId == targetId && e.TargetNodeId == sourceId));
        }

        /// <inheritdoc />
        public ReadOnlyCollection<EdgeModel> GetConnectedEdges(Guid nodeId)
        {
            var edges = _graph.Edges
                .Where(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId)
                .ToList();
            return edges.AsReadOnly();
        }

        /// <inheritdoc />
        public void Clear()
        {
            _graph.Nodes.Clear();
            _graph.Edges.Clear();
            _nodeCounter = 0;
            _edgeCounter = 0;
        }

        /// <summary>
        /// Gets the next node name based on the counter.
        /// </summary>
        public string GetNextNodeName() => $"Node {_nodeCounter + 1}";

        /// <summary>
        /// Gets the next edge name based on the counter.
        /// </summary>
        public string GetNextEdgeName() => $"Edge {_edgeCounter + 1}";
    }
}
