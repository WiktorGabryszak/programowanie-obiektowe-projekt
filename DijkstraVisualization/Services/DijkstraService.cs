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
                    // Determine neighbor - the other end of the edge
                    var neighbor = GetNeighbor(edge, current);
                    
                    if (!distances.ContainsKey(neighbor) || visited.Contains(neighbor))
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

            if (double.IsInfinity(distances[endId]) || !visited.Contains(endId))
            {
                result.TotalCost = distances[endId];
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

            // Step: Initialize - show all distances (? except start = 0)
            yield return CreateInitializeStep(distances);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                // Skip if already visited
                if (visited.Contains(current))
                {
                    continue;
                }

                // KROK 1: Visit node - Dijkstra enters this node (green border)
                yield return CreateVisitNodeStep(current, distances, visited);

                // KROK 2: Check if we reached the destination - STOP condition
                if (current == endId)
                {
                    // Mark as visited before completing
                    visited.Add(current);
                    yield return CreateFinalizeNodeStep(current, distances, visited);
                    break;
                }

                // Get edges from current node for relaxation
                var edgesToCheck = new List<(Guid EdgeId, Guid NeighborId, bool IsDirectionReversed)>();
                var edgesToRelax = new Dictionary<Guid, (Guid NeighborId, double NewDistance, bool IsDirectionReversed)>();

                if (adjacency.TryGetValue(current, out var edges))
                {
                    // KROK 3: Relaxation - check ALL unvisited neighbors
                    foreach (var edge in edges)
                    {
                        // Determine neighbor - the other end of the edge
                        var neighbor = GetNeighbor(edge, current);
                        
                        // Determine if wave direction is reversed (current is at Target, going to Source)
                        var isDirectionReversed = edge.TargetNodeId == current;
                        
                        // Skip if neighbor doesn't exist or is already visited
                        if (!distances.ContainsKey(neighbor) || visited.Contains(neighbor))
                        {
                            continue;
                        }

                        // Add to edges to check (for wave animation to ALL unvisited neighbors)
                        edgesToCheck.Add((edge.Id, neighbor, isDirectionReversed));

                        ValidateWeight(edge.Weight);
                        var tentativeDistance = distances[current] + edge.Weight;
                        
                        // Only update if new distance is better
                        if (tentativeDistance < distances[neighbor])
                        {
                            edgesToRelax[edge.Id] = (neighbor, tentativeDistance, isDirectionReversed);
                            
                            // Update state
                            distances[neighbor] = tentativeDistance;
                            previous[neighbor] = current;
                            queue.Enqueue(neighbor, tentativeDistance);
                        }
                    }
                }

                // Show relaxation step - wave to ALL unvisited neighbors
                if (edgesToCheck.Count > 0)
                {
                    yield return CreateRelaxEdgesStep(current, edgesToCheck, edgesToRelax, distances, visited);
                }

                // KROK 4: Mark as visited (finalize) - change to green color
                visited.Add(current);
                yield return CreateFinalizeNodeStep(current, distances, visited);
            }

            // Step: Complete - show final path
            var pathFound = visited.Contains(endId) && !double.IsInfinity(distances[endId]);
            var path = new List<Guid>();
            
            if (pathFound)
            {
                BuildPath(endId, startId, previous, path);
            }

            yield return CreateCompleteStep(distances, visited, path, pathFound, 
                pathFound ? distances[endId] : double.PositiveInfinity);
        }

        #region Step Creators

        private static AlgorithmStep CreateInitializeStep(Dictionary<Guid, double> distances)
        {
            var step = new AlgorithmStep
            {
                StepType = AlgorithmStepType.Initialize
            };

            foreach (var pair in distances)
            {
                step.CurrentDistances[pair.Key] = pair.Value;
            }

            return step;
        }

        private static AlgorithmStep CreateVisitNodeStep(Guid nodeId, Dictionary<Guid, double> distances, HashSet<Guid> visited)
        {
            var step = new AlgorithmStep
            {
                StepType = AlgorithmStepType.VisitNode,
                CurrentNodeId = nodeId
            };

            foreach (var pair in distances)
            {
                step.CurrentDistances[pair.Key] = pair.Value;
            }

            foreach (var id in visited)
            {
                step.FinalizedNodes.Add(id);
            }

            return step;
        }

        private static AlgorithmStep CreateFinalizeNodeStep(Guid nodeId, Dictionary<Guid, double> distances, HashSet<Guid> visited)
        {
            var step = new AlgorithmStep
            {
                StepType = AlgorithmStepType.FinalizeNode,
                CurrentNodeId = nodeId
            };

            foreach (var pair in distances)
            {
                step.CurrentDistances[pair.Key] = pair.Value;
            }

            foreach (var id in visited)
            {
                step.FinalizedNodes.Add(id);
            }

            return step;
        }

        private static AlgorithmStep CreateRelaxEdgesStep(
            Guid currentNodeId,
            List<(Guid EdgeId, Guid NeighborId, bool IsDirectionReversed)> allEdgesToCheck,
            Dictionary<Guid, (Guid NeighborId, double NewDistance, bool IsDirectionReversed)> edgesWithUpdates,
            Dictionary<Guid, double> distances,
            HashSet<Guid> visited)
        {
            var step = new AlgorithmStep
            {
                StepType = AlgorithmStepType.RelaxEdges,
                CurrentNodeId = currentNodeId
            };

            // Add ALL edges being checked (for wave animation)
            foreach (var (edgeId, neighborId, isDirectionReversed) in allEdgesToCheck)
            {
                // If this edge has an update, include the new distance
                // Otherwise, include current distance (no change)
                if (edgesWithUpdates.TryGetValue(edgeId, out var update))
                {
                    step.RelaxedEdges[edgeId] = update;
                }
                else
                {
                    step.RelaxedEdges[edgeId] = (neighborId, distances[neighborId], isDirectionReversed);
                }
            }

            foreach (var pair in distances)
            {
                step.CurrentDistances[pair.Key] = pair.Value;
            }

            foreach (var id in visited)
            {
                step.FinalizedNodes.Add(id);
            }

            return step;
        }

        private static AlgorithmStep CreateCompleteStep(
            Dictionary<Guid, double> distances,
            HashSet<Guid> visited,
            List<Guid> path,
            bool pathFound,
            double totalCost)
        {
            var step = new AlgorithmStep
            {
                StepType = AlgorithmStepType.Complete,
                PathFound = pathFound,
                TotalCost = totalCost
            };

            foreach (var pair in distances)
            {
                step.CurrentDistances[pair.Key] = pair.Value;
            }

            foreach (var id in visited)
            {
                step.FinalizedNodes.Add(id);
            }

            foreach (var nodeId in path)
            {
                step.FinalPath.Add(nodeId);
            }

            return step;
        }

        #endregion

        #region Helper Methods

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
            distances[startId] = 0;
            return distances;
        }

        private static Dictionary<Guid, List<EdgeModel>> BuildAdjacency(GraphModel graph)
        {
            var adjacency = new Dictionary<Guid, List<EdgeModel>>();
            foreach (var edge in graph.Edges)
            {
                // Add edge for Source -> Target direction
                if (!adjacency.TryGetValue(edge.SourceNodeId, out var sourceList))
                {
                    sourceList = new List<EdgeModel>();
                    adjacency[edge.SourceNodeId] = sourceList;
                }
                sourceList.Add(edge);

                // Add edge for Target -> Source direction (undirected graph)
                if (!adjacency.TryGetValue(edge.TargetNodeId, out var targetList))
                {
                    targetList = new List<EdgeModel>();
                    adjacency[edge.TargetNodeId] = targetList;
                }
                targetList.Add(edge);
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

        // Determine neighbor - the other end of the edge
        private static Guid GetNeighbor(EdgeModel edge, Guid current)
        {
            return edge.SourceNodeId == current ? edge.TargetNodeId : edge.SourceNodeId;
        }

        #endregion
    }
}
