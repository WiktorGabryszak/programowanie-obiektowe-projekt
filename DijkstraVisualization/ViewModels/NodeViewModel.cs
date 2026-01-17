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

        public NodeViewModel(NodeModel model)
        {
            _model = model;
            name = model.Name;
            x = model.X;
            y = model.Y;
            customColor = DefaultNodeColor;
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

        [ObservableProperty]
        private bool isVisited;

        [ObservableProperty]
        private bool isOnShortestPath;

        [ObservableProperty]
        private bool isCurrentNode;

        [ObservableProperty]
        private Color customColor;

        public IBrush DisplayBrush
        {
            get
            {
                if (IsCurrentNode) return new SolidColorBrush(Colors.LimeGreen);
                if (IsOnShortestPath) return new SolidColorBrush(Colors.Gold);
                if (IsVisited) return new SolidColorBrush(Colors.Crimson);
                if (IsStartNode) return new SolidColorBrush(Colors.ForestGreen);
                if (IsEndNode) return new SolidColorBrush(Colors.OrangeRed);
                return new SolidColorBrush(CustomColor);
            }
        }

        public IBrush BorderBrush
        {
            get
            {
                if (IsStartNode) return new SolidColorBrush(Colors.LimeGreen);
                if (IsEndNode) return new SolidColorBrush(Colors.Red);
                if (IsSelected) return new SolidColorBrush(Colors.White);
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public double BorderThickness => (IsStartNode || IsEndNode || IsSelected) ? 3 : 0;

        partial void OnIsCurrentNodeChanged(bool value) => NotifyVisualChanges();
        partial void OnIsOnShortestPathChanged(bool value) => NotifyVisualChanges();
        partial void OnIsVisitedChanged(bool value) => NotifyVisualChanges();
        partial void OnIsStartNodeChanged(bool value) => NotifyVisualChanges();
        partial void OnIsEndNodeChanged(bool value) => NotifyVisualChanges();
        partial void OnIsSelectedChanged(bool value) => NotifyVisualChanges();
        partial void OnCustomColorChanged(Color value) => NotifyVisualChanges();

        private void NotifyVisualChanges()
        {
            OnPropertyChanged(nameof(DisplayBrush));
            OnPropertyChanged(nameof(BorderBrush));
            OnPropertyChanged(nameof(BorderThickness));
        }

        public NodeModel ToModel() => _model;
    }
}
