using System;
using System.Collections.ObjectModel;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.Services
{
    /// <summary>
    /// Interface for managing graph operations (SRP - Single Responsibility Principle).
    /// Handles creation, modification, and deletion of nodes and edges.
    /// </summary>
    public interface IGraphManager
    {
        /// <summary>
        /// The underlying graph model.
        /// </summary>
        GraphModel Graph { get; }

        /// <summary>
        /// Adds a new node to the graph at the specified position.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>The ID of the created node.</returns>
        Guid AddNode(string name, double x, double y);

        /// <summary>
        /// Removes a node and all its connected edges from the graph.
        /// </summary>
        /// <param name="nodeId">The ID of the node to remove.</param>
        /// <returns>True if the node was found and removed.</returns>
        bool RemoveNode(Guid nodeId);

        /// <summary>
        /// Gets a node by its ID.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>The node model, or null if not found.</returns>
        NodeModel? GetNode(Guid nodeId);

        /// <summary>
        /// Adds a new edge between two nodes.
        /// </summary>
        /// <param name="sourceId">The source node ID.</param>
        /// <param name="targetId">The target node ID.</param>
        /// <param name="weight">The edge weight.</param>
        /// <param name="name">Optional edge name.</param>
        /// <returns>The ID of the created edge, or null if creation failed.</returns>
        Guid? AddEdge(Guid sourceId, Guid targetId, double weight, string? name = null);

        /// <summary>
        /// Removes an edge from the graph.
        /// </summary>
        /// <param name="edgeId">The ID of the edge to remove.</param>
        /// <returns>True if the edge was found and removed.</returns>
        bool RemoveEdge(Guid edgeId);

        /// <summary>
        /// Checks if an edge exists between two nodes (in either direction).
        /// </summary>
        /// <param name="sourceId">First node ID.</param>
        /// <param name="targetId">Second node ID.</param>
        /// <returns>True if an edge exists between the nodes.</returns>
        bool EdgeExists(Guid sourceId, Guid targetId);

        /// <summary>
        /// Gets all edges connected to a specific node.
        /// </summary>
        /// <param name="nodeId">The node ID.</param>
        /// <returns>Collection of edges connected to the node.</returns>
        ReadOnlyCollection<EdgeModel> GetConnectedEdges(Guid nodeId);

        /// <summary>
        /// Clears all nodes and edges from the graph.
        /// </summary>
        void Clear();
    }
}
