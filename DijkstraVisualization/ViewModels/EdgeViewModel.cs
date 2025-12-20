using System;
using System.ComponentModel;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.ViewModels
{
    public class EdgeViewModel : ViewModelBase, IDisposable
    {
        private readonly EdgeModel _model;
        private readonly NodeViewModel _source;
        private readonly NodeViewModel _target;

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
            OnPropertyChanged(nameof(Angle));
            OnPropertyChanged(nameof(Length));
        }
    }
}
