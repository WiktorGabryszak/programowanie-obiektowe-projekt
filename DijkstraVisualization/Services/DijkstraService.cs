using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.Services
{
    public class DijkstraService : IDijkstraService
    {
        public AlgorithmResult CalculatePath(GraphModel graph, Guid startId, Guid endId)
        {
            var result = new AlgorithmResult();

            if (!TryValidateGraph(graph, startId, endId))
            {
                return result;
            }

            var stopwatch = Stopwatch.StartNew();
            var adjacency = BuildAdjacency(graph);
            var distances = InitializeDistances(graph, startId);
            var previous = new Dictionary<Guid, Guid>();
            var visited = new HashSet<Guid>();
            var queue = CreateQueue(startId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current))
                {
                    continue;
                }

                if (current == endId)
                {
                    break;
                }

                if (!adjacency.TryGetValue(current, out var edges))
                {
                    continue;
                }

                foreach (var edge in edges)
                {
                    var neighbor = edge.TargetNodeId;
                    if (!distances.ContainsKey(neighbor))
                    {
                        continue;
                    }

                    ValidateWeight(edge.Weight);
                    var tentativeDistance = distances[current] + edge.Weight;
                    if (tentativeDistance >= distances[neighbor])
                    {
                        continue;
                    }

                    distances[neighbor] = tentativeDistance;
                    previous[neighbor] = current;
                    queue.Enqueue(neighbor, tentativeDistance);
                }
            }

            stopwatch.Stop();

            if (double.IsInfinity(distances[endId]))
            {
                result.ExecutionTime = stopwatch.Elapsed;
                return result;
            }

            result.PathFound = true;
            result.TotalCost = distances[endId];
            result.ExecutionTime = stopwatch.Elapsed;
            BuildPath(endId, startId, previous, result.NodePath);
            return result;
        }

        public IEnumerable<AlgorithmStep> GetVisualizationSteps(GraphModel graph, Guid startId, Guid endId)
        {
            if (!TryValidateGraph(graph, startId, endId))
            {
                yield break;
            }

            var adjacency = BuildAdjacency(graph);
            var distances = InitializeDistances(graph, startId);
            var visited = new HashSet<Guid>();
            var previous = new Dictionary<Guid, Guid>();
            var queue = CreateQueue(startId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current))
                {
                    continue;
                }

                yield return CreateStep(current, visited, distances);

                if (current == endId)
                {
                    break;
                }

                if (!adjacency.TryGetValue(current, out var edges))
                {
                    continue;
                }

                foreach (var edge in edges)
                {
                    var neighbor = edge.TargetNodeId;
                    if (!distances.ContainsKey(neighbor))
                    {
                        continue;
                    }

                    ValidateWeight(edge.Weight);
                    var tentativeDistance = distances[current] + edge.Weight;
                    if (tentativeDistance >= distances[neighbor])
                    {
                        continue;
                    }

                    distances[neighbor] = tentativeDistance;
                    previous[neighbor] = current;
                    queue.Enqueue(neighbor, tentativeDistance);
                    yield return CreateStep(current, visited, distances);
                }
            }
        }

        private static void ValidateWeight(double weight)
        {
            if (weight < 0)
            {
                throw new InvalidOperationException("Dijkstra's algorithm requires non-negative edge weights.");
            }
        }

        private static PriorityQueue<Guid, double> CreateQueue(Guid startId)
        {
            var queue = new PriorityQueue<Guid, double>();
            queue.Enqueue(startId, 0);
            return queue;
        }

        private static Dictionary<Guid, double> InitializeDistances(GraphModel graph, Guid startId)
        {
            var distances = graph.Nodes.ToDictionary(node => node.Id, _ => double.PositiveInfinity);
            if (distances.ContainsKey(startId))
            {
                distances[startId] = 0;
            }

            return distances;
        }

        private static Dictionary<Guid, List<EdgeModel>> BuildAdjacency(GraphModel graph)
        {
            var adjacency = new Dictionary<Guid, List<EdgeModel>>();
            foreach (var edge in graph.Edges)
            {
                if (!adjacency.TryGetValue(edge.SourceNodeId, out var list))
                {
                    list = new List<EdgeModel>();
                    adjacency[edge.SourceNodeId] = list;
                }

                list.Add(edge);
            }

            return adjacency;
        }

        private static bool TryValidateGraph(GraphModel graph, Guid startId, Guid endId)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            var hasStart = graph.Nodes.Any(n => n.Id == startId);
            var hasEnd = graph.Nodes.Any(n => n.Id == endId);
            return hasStart && hasEnd;
        }

        private static void BuildPath(Guid endId, Guid startId, Dictionary<Guid, Guid> previous, List<Guid> path)
        {
            var stack = new Stack<Guid>();
            var current = endId;

            while (true)
            {
                stack.Push(current);
                if (current == startId)
                {
                    break;
                }

                if (!previous.TryGetValue(current, out current))
                {
                    stack.Clear();
                    break;
                }
            }

            while (stack.Count > 0)
            {
                path.Add(stack.Pop());
            }
        }

        private static AlgorithmStep CreateStep(Guid currentNodeId, HashSet<Guid> visited, Dictionary<Guid, double> distances)
        {
            var step = new AlgorithmStep
            {
                CurrentNodeId = currentNodeId
            };

            foreach (var nodeId in visited)
            {
                step.VisitedNodes.Add(nodeId);
            }

            foreach (var pair in distances)
            {
                step.CurrentDistances[pair.Key] = pair.Value;
            }

            return step;
        }
    }
}
