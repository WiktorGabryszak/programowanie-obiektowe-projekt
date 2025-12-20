using System;
using DijkstraVisualization.Models;
using DijkstraVisualization.Services;
using FluentAssertions;
using Xunit;

namespace DijkstraVisualization.Tests.Services;

public class DijkstraServiceTests
{
    [Fact]
    public void CalculatePath_WithLinearGraph_ReturnsShortestPath()
    {
        // Arrange
        var nodeA = CreateNode("A");
        var nodeB = CreateNode("B");
        var nodeC = CreateNode("C");
        var graph = new GraphModel();
        graph.Nodes.AddRange(new[] { nodeA, nodeB, nodeC });
        graph.Edges.AddRange(new[]
        {
            CreateEdge(nodeA, nodeB, 2),
            CreateEdge(nodeB, nodeC, 3)
        });
        var service = new DijkstraService();

        // Act
        var result = service.CalculatePath(graph, nodeA.Id, nodeC.Id);

        // Assert
        result.PathFound.Should().BeTrue();
        result.TotalCost.Should().Be(5);
        result.NodePath.Should().Equal(nodeA.Id, nodeB.Id, nodeC.Id);
    }

    [Fact]
    public void CalculatePath_WithDisconnectedGraph_ReturnsNoPath()
    {
        // Arrange
        var nodeA = CreateNode("A");
        var nodeB = CreateNode("B");
        var graph = new GraphModel();
        graph.Nodes.AddRange(new[] { nodeA, nodeB });
        var service = new DijkstraService();

        // Act
        var result = service.CalculatePath(graph, nodeA.Id, nodeB.Id);

        // Assert
        result.PathFound.Should().BeFalse();
        result.TotalCost.Should().Be(double.PositiveInfinity);
        result.NodePath.Should().BeEmpty();
    }

    [Fact]
    public void CalculatePath_PrefersCheaperPathOverShorterOne()
    {
        // Arrange
        var nodeA = CreateNode("A");
        var nodeB = CreateNode("B");
        var nodeC = CreateNode("C");
        var graph = new GraphModel();
        graph.Nodes.AddRange(new[] { nodeA, nodeB, nodeC });
        graph.Edges.AddRange(new[]
        {
            CreateEdge(nodeA, nodeC, 10),
            CreateEdge(nodeA, nodeB, 1),
            CreateEdge(nodeB, nodeC, 1)
        });
        var service = new DijkstraService();

        // Act
        var result = service.CalculatePath(graph, nodeA.Id, nodeC.Id);

        // Assert
        result.PathFound.Should().BeTrue();
        result.TotalCost.Should().Be(2);
        result.NodePath.Should().Equal(nodeA.Id, nodeB.Id, nodeC.Id);
    }

    [Fact]
    public void CalculatePath_WhenStartEqualsEnd_ReturnsZeroCost()
    {
        // Arrange
        var node = CreateNode("Solo");
        var graph = new GraphModel();
        graph.Nodes.Add(node);
        var service = new DijkstraService();

        // Act
        var result = service.CalculatePath(graph, node.Id, node.Id);

        // Assert
        result.PathFound.Should().BeTrue();
        result.TotalCost.Should().Be(0);
        result.NodePath.Should().Equal(node.Id);
    }

    [Fact]
    public void CalculatePath_ReturnsNonNullResult()
    {
        // Arrange
        var service = new DijkstraService();

        // Act
        var result = service.CalculatePath(new GraphModel(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
    }

    private static NodeModel CreateNode(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name
    };

    private static EdgeModel CreateEdge(NodeModel source, NodeModel target, double weight) => new()
    {
        Id = Guid.NewGuid(),
        SourceNodeId = source.Id,
        TargetNodeId = target.Id,
        Weight = weight,
        Name = $"{source.Name}->{target.Name}"
    };
}
