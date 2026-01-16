using System;
using System.ComponentModel;
using Avalonia.Media;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.ViewModels
{
    public class EdgeViewModel : ViewModelBase, IDisposable
    {
        private readonly EdgeModel _model;
        private readonly NodeViewModel _source;
        private readonly NodeViewModel _target;
        private static readonly Color DefaultEdgeColor = Colors.White;
        private Color _customColor = DefaultEdgeColor;
        private bool _isOnShortestPath;
        private bool _isWeightLocked;
        private bool _isBeingRelaxed;
        private double _relaxationProgress;
        private bool _relaxationDirectionReversed;

        public EdgeViewModel(EdgeModel model, NodeViewModel source, NodeViewModel target)
        {
            _model = model;
            _source = source;
            _target = target;

            _source.PropertyChanged += HandleNodeChanged;
            _target.PropertyChanged += HandleNodeChanged;
        }

        public Guid Id => _model.Id;
        public NodeViewModel Source => _source;
        public NodeViewModel Target => _target;

        public double StartX => _source.X;
        public double StartY => _source.Y;
        public double EndX => _target.X;
        public double EndY => _target.Y;

        /// <summary>
        /// Gets the midpoint X for label placement.
        /// </summary>
        public double MidX => (StartX + EndX) / 2;

        /// <summary>
        /// Gets the midpoint Y for label placement.
        /// </summary>
        public double MidY => (StartY + EndY) / 2;

        public double Length
        {
            get
            {
                var dx = EndX - StartX;
                var dy = EndY - StartY;
                return Math.Sqrt((dx * dx) + (dy * dy));
            }
        }

        public double Angle
        {
            get
            {
                var dx = EndX - StartX;
                var dy = EndY - StartY;
                return Math.Atan2(dy, dx) * 180 / Math.PI;
            }
        }

        public double Weight
        {
            get => _model.Weight;
            set
            {
                if (Math.Abs(_model.Weight - value) > double.Epsilon)
                {
                    _model.Weight = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _model.Name;
            set
            {
                if (!string.Equals(_model.Name, value, StringComparison.Ordinal))
                {
                    _model.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color CustomColor
        {
            get => _customColor;
            set
            {
                if (_customColor != value)
                {
                    _customColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayBrush));
                }
            }
        }

        public bool IsOnShortestPath
        {
            get => _isOnShortestPath;
            set
            {
                if (_isOnShortestPath != value)
                {
                    _isOnShortestPath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayBrush));
                    OnPropertyChanged(nameof(StrokeThickness));
                }
            }
        }

        /// <summary>
        /// When true, the weight of this edge will not be automatically updated when nodes are moved.
        /// </summary>
        public bool IsWeightLocked
        {
            get => _isWeightLocked;
            set
            {
                if (_isWeightLocked != value)
                {
                    _isWeightLocked = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether this edge is currently being relaxed (wave animation).
        /// </summary>
        public bool IsBeingRelaxed
        {
            get => _isBeingRelaxed;
            set
            {
                if (_isBeingRelaxed != value)
                {
                    _isBeingRelaxed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayBrush));
                    OnPropertyChanged(nameof(StrokeThickness));
                }
            }
        }

        /// <summary>
        /// Progress of relaxation animation (0.0 to 1.0).
        /// Used to animate the "wave" effect along the edge.
        /// </summary>
        public double RelaxationProgress
        {
            get => _relaxationProgress;
            set
            {
                if (Math.Abs(_relaxationProgress - value) > 0.001)
                {
                    _relaxationProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// When true, the wave animation goes from Target to Source instead of Source to Target.
        /// This is needed when Dijkstra is at the Target node and checking the Source neighbor.
        /// </summary>
        public bool RelaxationDirectionReversed
        {
            get => _relaxationDirectionReversed;
            set
            {
                if (_relaxationDirectionReversed != value)
                {
                    _relaxationDirectionReversed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Resets all visualization-related properties.
        /// </summary>
        public void ResetVisualizationState()
        {
            IsOnShortestPath = false;
            IsBeingRelaxed = false;
            RelaxationProgress = 0;
            RelaxationDirectionReversed = false;
        }

        public IBrush DisplayBrush
        {
            get
            {
                if (IsOnShortestPath) return new SolidColorBrush(Colors.Gold);
                // Removed cyan color for IsBeingRelaxed - keep normal color during animation
                return new SolidColorBrush(CustomColor);
            }
        }

        public double StrokeThickness
        {
            get
            {
                if (IsOnShortestPath) return 4;
                // Removed thicker line for IsBeingRelaxed - keep normal thickness during animation
                return 2;
            }
        }

        /// <summary>
        /// Updates the weight to match the current Euclidean length of the edge (rounded to nearest integer).
        /// Does nothing if IsWeightLocked is true.
        /// </summary>
        public void UpdateWeightFromLength()
        {
            if (!IsWeightLocked)
            {
                Weight = Math.Round(Length);
            }
        }

        public EdgeModel ToModel() => _model;

        public void Dispose()
        {
            _source.PropertyChanged -= HandleNodeChanged;
            _target.PropertyChanged -= HandleNodeChanged;
        }

        private void HandleNodeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(NodeViewModel.X) or nameof(NodeViewModel.Y))
            {
                RaiseGeometryChanges();
            }
        }

        private void RaiseGeometryChanges()
        {
            OnPropertyChanged(nameof(StartX));
            OnPropertyChanged(nameof(StartY));
            OnPropertyChanged(nameof(EndX));
            OnPropertyChanged(nameof(EndY));
            OnPropertyChanged(nameof(MidX));
            OnPropertyChanged(nameof(MidY));
            OnPropertyChanged(nameof(Angle));
            OnPropertyChanged(nameof(Length));
        }
    }
}
