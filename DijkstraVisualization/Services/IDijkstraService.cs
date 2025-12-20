using System;
using System.Collections.Generic;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.Services
{
    public interface IDijkstraService
    {
        AlgorithmResult CalculatePath(GraphModel graph, Guid startId, Guid endId);
        IEnumerable<AlgorithmStep> GetVisualizationSteps(GraphModel graph, Guid startId, Guid endId);
    }
}
