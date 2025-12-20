using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DijkstraVisualization.Models;

namespace DijkstraVisualization.ViewModels
{
    public partial class NodeViewModel : ViewModelBase
    {
        private readonly NodeModel _model;

        public NodeViewModel(NodeModel model)
        {
            _model = model;
            name = model.Name;
            x = model.X;
            y = model.Y;
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

        public NodeModel ToModel() => _model;
    }
}
