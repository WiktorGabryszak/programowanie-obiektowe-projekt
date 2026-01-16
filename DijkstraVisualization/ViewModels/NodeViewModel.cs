using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.ViewModels
{
    public partial class NodeViewModel : ViewModelBase
    {
        private readonly NodeModel _model;
        private static readonly Color DefaultNodeColor = Color.FromRgb(128, 128, 128);
        private const string InfinitySymbol = "?";

        public NodeViewModel(NodeModel model)
        {
            _model = model;
            name = model.Name;
            x = model.X;
            y = model.Y;
            customColor = DefaultNodeColor;
            displayedDistance = InfinitySymbol;
        }

        public Guid Id => _model.Id;

        [ObservableProperty]
        private string name = string.Empty;

        partial void OnNameChanged(string value)
        {
            _model.Name = value;
        }

        [ObservableProperty]
        private double x;

        partial void OnXChanged(double value)
        {
            _model.X = value;
        }

        [ObservableProperty]
        private double y;

        partial void OnYChanged(double value)
        {
            _model.Y = value;
        }

        [ObservableProperty]
        private bool isSelected;

        [ObservableProperty]
        private bool isStartNode;

        [ObservableProperty]
        private bool isEndNode;

        /// <summary>
        /// Node has been fully processed (visited) - shown with green fill.
        /// </summary>
        [ObservableProperty]
        private bool isVisited;

        [ObservableProperty]
        private bool isOnShortestPath;

        /// <summary>
        /// Dijkstra is currently at this node (processing it) - shown with green border.
        /// </summary>
        [ObservableProperty]
        private bool isCurrentNode;

        [ObservableProperty]
        private Color customColor;

        // === Properties for visualization ===

        /// <summary>
        /// Whether to show the distance label above the node.
        /// </summary>
        [ObservableProperty]
        private bool showDistanceLabel;

        /// <summary>
        /// The displayed distance value (? or actual number).
        /// </summary>
        [ObservableProperty]
        private string displayedDistance = InfinitySymbol;

        /// <summary>
        /// Whether the distance was just updated (triggers pulse animation).
        /// </summary>
        [ObservableProperty]
        private bool isDistanceUpdated;

        /// <summary>
        /// Scale factor for the distance label (for pulse animation).
        /// </summary>
        [ObservableProperty]
        private double distanceLabelScale = 1.0;

        /// <summary>
        /// The actual numeric distance value (for internal use).
        /// </summary>
        private double _numericDistance = double.PositiveInfinity;

        /// <summary>
        /// Sets the distance value and updates the display string.
        /// Returns true if the distance was actually improved (decreased).
        /// </summary>
        public bool SetDistance(double distance)
        {
            var oldDistance = _numericDistance;
            _numericDistance = distance;
            DisplayedDistance = double.IsPositiveInfinity(distance) 
                ? InfinitySymbol 
                : distance.ToString("F0");
            
            // Return true if distance was improved (decreased from a finite value or from infinity)
            var wasImproved = distance < oldDistance;
            return wasImproved;
        }

        /// <summary>
        /// Triggers the distance update animation.
        /// </summary>
        public void TriggerDistanceUpdateAnimation()
        {
            IsDistanceUpdated = true;
        }

        /// <summary>
        /// Clears the distance update animation state.
        /// </summary>
        public void ClearDistanceUpdateAnimation()
        {
            IsDistanceUpdated = false;
            DistanceLabelScale = 1.0;
        }

        /// <summary>
        /// Resets all visualization-related properties.
        /// </summary>
        public void ResetVisualizationState()
        {
            IsVisited = false;
            IsCurrentNode = false;
            IsOnShortestPath = false;
            ShowDistanceLabel = false;
            DisplayedDistance = InfinitySymbol;
            IsDistanceUpdated = false;
            DistanceLabelScale = 1.0;
            _numericDistance = double.PositiveInfinity;
        }

        /// <summary>
        /// Gets the display color based on the node's current state.
        /// Priority: OnShortestPath (Gold) > Visited (Green) > Start > End > Custom
        /// </summary>
        public IBrush DisplayBrush
        {
            get
            {
                if (IsOnShortestPath) return new SolidColorBrush(Colors.Gold);
                if (IsVisited) return new SolidColorBrush(Colors.ForestGreen);
                if (IsStartNode) return new SolidColorBrush(Colors.DodgerBlue);
                if (IsEndNode) return new SolidColorBrush(Colors.OrangeRed);
                return new SolidColorBrush(CustomColor);
            }
        }

        /// <summary>
        /// Border brush - green when current (being processed), otherwise based on selection state.
        /// </summary>
        public IBrush BorderBrush
        {
            get
            {
                if (IsCurrentNode) return new SolidColorBrush(Colors.LimeGreen);
                if (IsStartNode && !IsVisited) return new SolidColorBrush(Colors.DodgerBlue);
                if (IsEndNode && !IsVisited) return new SolidColorBrush(Colors.OrangeRed);
                if (IsSelected) return new SolidColorBrush(Colors.White);
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        /// <summary>
        /// Border thickness for the node circle.
        /// </summary>
        public double BorderThickness => (IsCurrentNode || IsStartNode || IsEndNode || IsSelected) ? 3 : 0;

        /// <summary>
        /// Background brush for the distance label.
        /// Shows the current known distance color.
        /// </summary>
        public IBrush DistanceLabelBrush
        {
            get
            {
                // If visited (finalized), show green
                if (IsVisited) return new SolidColorBrush(Colors.ForestGreen);
                // If has a known distance (not infinity), show orange
                if (!double.IsPositiveInfinity(_numericDistance)) return new SolidColorBrush(Colors.DarkOrange);
                // Unknown distance - red
                return new SolidColorBrush(Colors.Crimson);
            }
        }

        /// <summary>
        /// Glow effect color when distance is updated.
        /// </summary>
        public IBrush DistanceUpdateGlowBrush => new SolidColorBrush(Colors.Yellow);

        partial void OnIsCurrentNodeChanged(bool value) => NotifyVisualChanges();
        partial void OnIsOnShortestPathChanged(bool value) => NotifyVisualChanges();
        partial void OnIsVisitedChanged(bool value) => NotifyVisualChanges();
        partial void OnIsStartNodeChanged(bool value) => NotifyVisualChanges();
        partial void OnIsEndNodeChanged(bool value) => NotifyVisualChanges();
        partial void OnIsSelectedChanged(bool value) => NotifyVisualChanges();
        partial void OnCustomColorChanged(Color value) => NotifyVisualChanges();
        partial void OnShowDistanceLabelChanged(bool value) => NotifyVisualChanges();
        partial void OnIsDistanceUpdatedChanged(bool value) => NotifyVisualChanges();
        partial void OnDistanceLabelScaleChanged(double value) => NotifyVisualChanges();
        partial void OnDisplayedDistanceChanged(string value) 
        {
            OnPropertyChanged(nameof(DisplayedDistance));
            OnPropertyChanged(nameof(DistanceLabelBrush));
        }

        private void NotifyVisualChanges()
        {
            OnPropertyChanged(nameof(DisplayBrush));
            OnPropertyChanged(nameof(BorderBrush));
            OnPropertyChanged(nameof(BorderThickness));
            OnPropertyChanged(nameof(DistanceLabelBrush));
        }

        public NodeModel ToModel() => _model;
    }
}
