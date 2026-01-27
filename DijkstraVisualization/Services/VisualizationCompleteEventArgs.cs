using System;
using System.Collections.Generic;

namespace DijkstraVisualization.Services
{
    /// <summary>
    /// Event arguments for visualization completion.
    /// </summary>
    public class VisualizationCompleteEventArgs : EventArgs
    {
        public bool PathFound { get; init; }
        public List<Guid> Path { get; init; } = new();
        public double TotalCost { get; init; }
    }
}
