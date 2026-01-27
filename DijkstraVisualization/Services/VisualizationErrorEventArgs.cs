using System;

namespace DijkstraVisualization.Services
{
    /// <summary>
    /// Event arguments for visualization errors.
    /// </summary>
    public class VisualizationErrorEventArgs : EventArgs
    {
        public string Message { get; init; } = string.Empty;
    }
}
