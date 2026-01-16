using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using DijkstraVisualization.ViewModels;

namespace DijkstraVisualization.Views
{
    public partial class MainWindow : Window
    {
        private Canvas? _graphCanvas;
        private Canvas? _edgesCanvas;
        private Canvas? _nodesCanvas;
        private Line? _drawingLine;

        private bool _isDrawingEdge;
        private NodeViewModel? _edgeSourceNode;

        // New: Shift+drag edge creation
        private bool _isShiftDraggingEdge;

        private bool _isDraggingNode;
        private NodeViewModel? _draggedNode;
        private Point _dragOffset;

        private const double NodeRadius = 25;
        private const double NodeSize = 50;
        private const double NodeContainerSize = 60; // NodeSize + 10 (padding)
        private const double NodeCenterOffset = 30;  // Half of NodeContainerSize
        private const double DistanceLabelSize = 30;
        private const double DistanceLabelOffset = -35; // Above the node

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private MainViewModel? ViewModel => DataContext as MainViewModel;

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _graphCanvas = this.FindControl<Canvas>("GraphCanvas");
            _edgesCanvas = this.FindControl<Canvas>("EdgesCanvas");
            _nodesCanvas = this.FindControl<Canvas>("NodesCanvas");
            _drawingLine = this.FindControl<Line>("DrawingLine");

            if (_graphCanvas != null)
            {
                _graphCanvas.PointerPressed += OnCanvasPointerPressed;
                _graphCanvas.PointerMoved += OnCanvasPointerMoved;
                _graphCanvas.PointerReleased += OnCanvasPointerReleased;
            }

            if (ViewModel != null)
            {
                ViewModel.Nodes.CollectionChanged += OnNodesCollectionChanged;
                ViewModel.Edges.CollectionChanged += OnEdgesCollectionChanged;
                
                foreach (var node in ViewModel.Nodes)
                {
                    SubscribeToNode(node);
                }
                foreach (var edge in ViewModel.Edges)
                {
                    SubscribeToEdge(edge);
                }
            }

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Nodes.CollectionChanged -= OnNodesCollectionChanged;
                ViewModel.Nodes.CollectionChanged += OnNodesCollectionChanged;
                ViewModel.Edges.CollectionChanged -= OnEdgesCollectionChanged;
                ViewModel.Edges.CollectionChanged += OnEdgesCollectionChanged;
                RedrawAll();
            }
        }

        #region Nodes Rendering

        private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (NodeViewModel node in e.OldItems)
                {
                    UnsubscribeFromNode(node);
                }
            }

            if (e.NewItems != null)
            {
                foreach (NodeViewModel node in e.NewItems)
                {
                    SubscribeToNode(node);
                }
            }

            RedrawAll();
        }

        private void SubscribeToNode(NodeViewModel node)
        {
            node.PropertyChanged += OnNodePropertyChanged;
        }

        private void UnsubscribeFromNode(NodeViewModel node)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
        }

        private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Redraw when node visual properties change
            if (e.PropertyName is nameof(NodeViewModel.X) or nameof(NodeViewModel.Y) or
                nameof(NodeViewModel.Name) or nameof(NodeViewModel.DisplayBrush) or
                nameof(NodeViewModel.BorderBrush) or nameof(NodeViewModel.BorderThickness) or
                nameof(NodeViewModel.IsVisited) or nameof(NodeViewModel.IsCurrentNode) or
                nameof(NodeViewModel.IsOnShortestPath) or nameof(NodeViewModel.IsStartNode) or
                nameof(NodeViewModel.IsEndNode) or nameof(NodeViewModel.ShowDistanceLabel) or
                nameof(NodeViewModel.DisplayedDistance) or nameof(NodeViewModel.DistanceLabelBrush))
            {
                RedrawAll();
            }
        }

        private void RedrawAllNodes()
        {
            if (_nodesCanvas == null || ViewModel == null) return;

            _nodesCanvas.Children.Clear();

            foreach (var node in ViewModel.Nodes)
            {
                DrawNode(node);
            }
        }

        private void DrawNode(NodeViewModel node)
        {
            if (_nodesCanvas == null) return;

            // Create a container grid for the node
            var container = new Grid
            {
                Width = NodeSize + 10,
                Height = NodeSize + 10
            };

            // Node circle
            var ellipse = new Ellipse
            {
                Width = NodeSize,
                Height = NodeSize,
                Fill = node.DisplayBrush,
                Stroke = node.BorderBrush,
                StrokeThickness = node.BorderThickness,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            container.Children.Add(ellipse);

            // Node name label
            var label = new TextBlock
            {
                Text = node.Name,
                Foreground = Brushes.White,
                FontSize = 10,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                MaxWidth = NodeSize - 5,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            container.Children.Add(label);

            // Position the container
            Canvas.SetLeft(container, node.X);
            Canvas.SetTop(container, node.Y);
            
            _nodesCanvas.Children.Add(container);

            // Draw distance label if visualization is active
            if (node.ShowDistanceLabel)
            {
                DrawDistanceLabel(node);
            }
        }

        private void DrawDistanceLabel(NodeViewModel node)
        {
            if (_nodesCanvas == null) return;

            // Create distance label container
            // Border color based on visited state (green when visited/finalized)
            var labelBorder = new Border
            {
                Width = DistanceLabelSize,
                Height = DistanceLabelSize,
                Background = node.DistanceLabelBrush,
                BorderBrush = node.IsVisited 
                    ? new SolidColorBrush(Colors.LimeGreen) 
                    : new SolidColorBrush(Colors.DarkRed),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(3),
                Child = new TextBlock
                {
                    Text = node.DisplayedDistance,
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeight.Bold,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            };

            // Position above the node (centered)
            var labelX = node.X + (NodeContainerSize - DistanceLabelSize) / 2;
            var labelY = node.Y + DistanceLabelOffset;

            Canvas.SetLeft(labelBorder, labelX);
            Canvas.SetTop(labelBorder, labelY);
            
            _nodesCanvas.Children.Add(labelBorder);
        }

        #endregion

        #region Edges Rendering

        private void OnEdgesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (EdgeViewModel edge in e.OldItems)
                {
                    UnsubscribeFromEdge(edge);
                }
            }

            if (e.NewItems != null)
            {
                foreach (EdgeViewModel edge in e.NewItems)
                {
                    SubscribeToEdge(edge);
                }
            }

            RedrawAllEdges();
        }

        private void SubscribeToEdge(EdgeViewModel edge)
        {
            edge.PropertyChanged += OnEdgePropertyChanged;
        }

        private void UnsubscribeFromEdge(EdgeViewModel edge)
        {
            edge.PropertyChanged -= OnEdgePropertyChanged;
        }

        private void OnEdgePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(EdgeViewModel.StartX) or nameof(EdgeViewModel.StartY) or 
                nameof(EdgeViewModel.EndX) or nameof(EdgeViewModel.EndY) or
                nameof(EdgeViewModel.DisplayBrush) or nameof(EdgeViewModel.StrokeThickness) or
                nameof(EdgeViewModel.IsOnShortestPath) or nameof(EdgeViewModel.Weight) or
                nameof(EdgeViewModel.IsBeingRelaxed) or nameof(EdgeViewModel.RelaxationProgress))
            {
                RedrawAllEdges();
            }
        }

        private void RedrawAll()
        {
            RedrawAllEdges();
            RedrawAllNodes();
        }

        private void RedrawAllEdges()
        {
            if (_edgesCanvas == null || ViewModel == null) return;

            _edgesCanvas.Children.Clear();

            foreach (var edge in ViewModel.Edges)
            {
                DrawEdge(edge);
            }
        }

        private void DrawEdge(EdgeViewModel edge)
        {
            if (_edgesCanvas == null) return;

            // Calculate center points of nodes (using NodeCenterOffset for correct center position)
            var startX = edge.StartX + NodeCenterOffset;
            var startY = edge.StartY + NodeCenterOffset;
            var endX = edge.EndX + NodeCenterOffset;
            var endY = edge.EndY + NodeCenterOffset;

            // Calculate direction
            var dx = endX - startX;
            var dy = endY - startY;
            var length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 1) return;

            // Normalize
            var nx = dx / length;
            var ny = dy / length;

            // Shorten line to not overlap with node circles (using NodeRadius for the circle size)
            var shortenedStartX = startX + nx * NodeRadius;
            var shortenedStartY = startY + ny * NodeRadius;
            var shortenedEndX = endX - nx * NodeRadius;
            var shortenedEndY = endY - ny * NodeRadius;

            // Draw the main line
            var line = new Line
            {
                StartPoint = new Point(shortenedStartX, shortenedStartY),
                EndPoint = new Point(shortenedEndX, shortenedEndY),
                Stroke = edge.DisplayBrush,
                StrokeThickness = edge.StrokeThickness,
                StrokeLineCap = PenLineCap.Round
            };
            _edgesCanvas.Children.Add(line);

            // Draw wave animation overlay if being relaxed
            if (edge.IsBeingRelaxed && edge.RelaxationProgress > 0)
            {
                DrawRelaxationWave(edge, shortenedStartX, shortenedStartY, shortenedEndX, shortenedEndY);
            }

            // Draw weight label at midpoint
            var midX = (shortenedStartX + shortenedEndX) / 2;
            var midY = (shortenedStartY + shortenedEndY) / 2;

            var labelBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 2),
                Child = new TextBlock
                {
                    Text = edge.Weight.ToString("F0"),
                    Foreground = Brushes.White,
                    FontSize = 10
                }
            };
            
            Canvas.SetLeft(labelBorder, midX - 15);
            Canvas.SetTop(labelBorder, midY - 10);
            _edgesCanvas.Children.Add(labelBorder);
        }

        private void DrawRelaxationWave(EdgeViewModel edge, double startX, double startY, double endX, double endY)
        {
            if (_edgesCanvas == null) return;

            var progress = edge.RelaxationProgress;

            // If direction is reversed, swap start and end points
            double waveStartX, waveStartY, waveEndX, waveEndY;
            if (edge.RelaxationDirectionReversed)
            {
                // Wave goes from Target (end) to Source (start)
                waveStartX = endX;
                waveStartY = endY;
                waveEndX = endX + (startX - endX) * progress;
                waveEndY = endY + (startY - endY) * progress;
            }
            else
            {
                // Wave goes from Source (start) to Target (end)
                waveStartX = startX;
                waveStartY = startY;
                waveEndX = startX + (endX - startX) * progress;
                waveEndY = startY + (endY - startY) * progress;
            }

            // Draw the animated wave line
            var waveLine = new Line
            {
                StartPoint = new Point(waveStartX, waveStartY),
                EndPoint = new Point(waveEndX, waveEndY),
                Stroke = new SolidColorBrush(Colors.Yellow),
                StrokeThickness = edge.StrokeThickness + 2,
                StrokeLineCap = PenLineCap.Round
            };
            _edgesCanvas.Children.Add(waveLine);

            // Draw a glowing dot at the wave front
            var glowEllipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Yellow)
            };
            Canvas.SetLeft(glowEllipse, waveEndX - 5);
            Canvas.SetTop(glowEllipse, waveEndY - 5);
            _edgesCanvas.Children.Add(glowEllipse);
        }

        #endregion

        #region Input Handling

        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (ViewModel == null || _graphCanvas == null) return;
            if (ViewModel.IsVisualizing) return;

            var point = e.GetPosition(_graphCanvas);
            var canvasPoint = TransformToCanvas(point);
            var properties = e.GetCurrentPoint(_graphCanvas).Properties;
            var keyModifiers = e.KeyModifiers;

            var clickedNode = FindNodeAt(canvasPoint);
            var clickedEdge = clickedNode == null ? FindEdgeAt(canvasPoint) : null;

            if (properties.IsRightButtonPressed)
            {
                if (_isDrawingEdge || _isShiftDraggingEdge)
                {
                    CancelEdgeDrawing();
                    return;
                }

                ShowContextMenu(canvasPoint, clickedNode, clickedEdge);
                e.Handled = true;
            }
            else if (properties.IsLeftButtonPressed)
            {
                // Check if we're completing an edge drawing (from context menu mode)
                if (_isDrawingEdge)
                {
                    if (clickedNode != null && clickedNode != _edgeSourceNode)
                    {
                        CompleteEdgeDrawing(clickedNode);
                    }
                    else
                    {
                        CancelEdgeDrawing();
                    }
                    e.Handled = true;
                    return;
                }

                // Ctrl + LPM: Add node on empty space, or remove node if clicked on a node
                if (keyModifiers.HasFlag(KeyModifiers.Control))
                {
                    if (clickedNode != null)
                    {
                        // Ctrl + LPM on node = remove node
                        ViewModel.RemoveNodeCommand.Execute(clickedNode.Id);
                    }
                    else
                    {
                        // Ctrl + LPM on empty space = add node
                        ViewModel.AddNodeCommand.Execute(new NodePlacement(canvasPoint.X - NodeCenterOffset, canvasPoint.Y - NodeCenterOffset));
                    }
                    e.Handled = true;
                    return;
                }

                // Shift + LPM on a node starts edge drawing via drag
                if (keyModifiers.HasFlag(KeyModifiers.Shift) && clickedNode != null)
                {
                    _isShiftDraggingEdge = true;
                    _edgeSourceNode = clickedNode;
                    
                    if (_drawingLine != null)
                    {
                        _drawingLine.IsVisible = false;
                    }
                    
                    e.Handled = true;
                    return;
                }

                // Normal node dragging (without Shift or Ctrl)
                if (clickedNode != null)
                {
                    _isDraggingNode = true;
                    _draggedNode = clickedNode;
                    _dragOffset = new Point(canvasPoint.X - clickedNode.X, canvasPoint.Y - clickedNode.Y);
                    e.Handled = true;
                }
                // Clicking on empty canvas does nothing special
            }
        }


        private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_graphCanvas == null) return;

            var point = e.GetPosition(_graphCanvas);
            var canvasPoint = TransformToCanvas(point);

            // Handle both context menu edge drawing and shift+drag edge drawing
            if ((_isDrawingEdge || _isShiftDraggingEdge) && _drawingLine != null && _edgeSourceNode != null)
            {
                // Calculate center of source node
                var centerX = _edgeSourceNode.X + NodeCenterOffset;
                var centerY = _edgeSourceNode.Y + NodeCenterOffset;
                
                // Calculate direction to cursor
                var dx = canvasPoint.X - centerX;
                var dy = canvasPoint.Y - centerY;
                var length = Math.Sqrt(dx * dx + dy * dy);
                
                if (length > NodeRadius)
                {
                    // Normalize and offset start point to edge of node circle
                    var nx = dx / length;
                    var ny = dy / length;
                    var startX = centerX + nx * NodeRadius;
                    var startY = centerY + ny * NodeRadius;
                    
                    _drawingLine.StartPoint = new Point(startX, startY);
                    _drawingLine.EndPoint = new Point(canvasPoint.X, canvasPoint.Y);
                    _drawingLine.IsVisible = true;
                }
                else
                {
                    // Cursor is inside the node - hide the line
                    _drawingLine.IsVisible = false;
                }
            }
            else if (_isDraggingNode && _draggedNode != null)
            {
                _draggedNode.X = canvasPoint.X - _dragOffset.X;
                _draggedNode.Y = canvasPoint.Y - _dragOffset.Y;
            }
        }

        private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_graphCanvas == null) return;

            var point = e.GetPosition(_graphCanvas);
            var canvasPoint = TransformToCanvas(point);

            // Complete shift+drag edge creation
            if (_isShiftDraggingEdge && _edgeSourceNode != null)
            {
                var targetNode = FindNodeAt(canvasPoint);
                
                if (targetNode != null && targetNode != _edgeSourceNode)
                {
                    CompleteEdgeDrawing(targetNode);
                }
                else
                {
                    CancelEdgeDrawing();
                }
                
                e.Handled = true;
                return;
            }

            // Normal node drag completion
            if (_isDraggingNode && _draggedNode != null)
            {
                // Update weights of all edges connected to the dragged node
                UpdateConnectedEdgeWeights(_draggedNode);
            }

            _isDraggingNode = false;
            _draggedNode = null;
        }

        /// <summary>
        /// Updates the weights of all edges connected to the specified node based on their current length.
        /// Skips edges that have IsWeightLocked set to true.
        /// </summary>
        private void UpdateConnectedEdgeWeights(NodeViewModel node)
        {
            if (ViewModel == null) return;

            foreach (var edge in ViewModel.Edges)
            {
                if (edge.Source == node || edge.Target == node)
                {
                    edge.UpdateWeightFromLength();
                }
            }
        }

        #endregion

        #region Hit Testing

        private Point TransformToCanvas(Point screenPoint)
        {
            // No transformation needed - direct canvas coordinates
            return screenPoint;
        }

        private NodeViewModel? FindNodeAt(Point canvasPoint)
        {
            if (ViewModel == null) return null;

            foreach (var node in ViewModel.Nodes.Reverse())
            {
                var centerX = node.X + NodeCenterOffset;
                var centerY = node.Y + NodeCenterOffset;
                var dx = canvasPoint.X - centerX;
                var dy = canvasPoint.Y - centerY;
                var distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance <= NodeRadius)
                {
                    return node;
                }
            }
            return null;
        }

        private EdgeViewModel? FindEdgeAt(Point canvasPoint)
        {
            if (ViewModel == null) return null;

            const double tolerance = 10;

            foreach (var edge in ViewModel.Edges.Reverse())
            {
                var startX = edge.StartX + NodeCenterOffset;
                var startY = edge.StartY + NodeCenterOffset;
                var endX = edge.EndX + NodeCenterOffset;
                var endY = edge.EndY + NodeCenterOffset;

                var distance = PointToLineDistance(
                    canvasPoint.X, canvasPoint.Y,
                    startX, startY,
                    endX, endY);

                if (distance <= tolerance)
                {
                    return edge;
                }
            }
            return null;
        }

        private static double PointToLineDistance(double px, double py, double x1, double y1, double x2, double y2)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var lengthSquared = dx * dx + dy * dy;

            if (lengthSquared == 0)
                return Math.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1));

            var t = Math.Max(0, Math.Min(1, ((px - x1) * dx + (py - y1) * dy) / lengthSquared));
            var projX = x1 + t * dx;
            var projY = y1 + t * dy;

            return Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
        }

        #endregion

        #region Context Menu

        private void ShowContextMenu(Point canvasPoint, NodeViewModel? node, EdgeViewModel? edge)
        {
            var menu = new ContextMenu();

            if (node != null)
            {
                var editItem = new MenuItem { Header = "Edit node" };
                editItem.Click += async (s, e) =>
                {
                    var dialog = new EditNodeDialog(node);
                    await dialog.ShowDialog(this);
                    RedrawAllNodes();
                };
                menu.Items.Add(editItem);

                var removeItem = new MenuItem { Header = "Remove node" };
                removeItem.Click += (s, e) => ViewModel?.RemoveNodeCommand.Execute(node.Id);
                menu.Items.Add(removeItem);

                menu.Items.Add(new Separator());

                var addEdgeItem = new MenuItem { Header = "Add edge" };
                addEdgeItem.Click += (s, e) => StartEdgeDrawing(node);
                menu.Items.Add(addEdgeItem);

                menu.Items.Add(new Separator());

                var startItem = new MenuItem { Header = "Set as start" };
                startItem.Click += (s, e) => ViewModel?.SetTargetCommand.Execute(new TargetSelection(node.Id, TargetKind.Start));
                menu.Items.Add(startItem);

                var endItem = new MenuItem { Header = "Set as destination" };
                endItem.Click += (s, e) => ViewModel?.SetTargetCommand.Execute(new TargetSelection(node.Id, TargetKind.End));
                menu.Items.Add(endItem);
            }
            else if (edge != null)
            {
                var editItem = new MenuItem { Header = "Edit edge" };
                editItem.Click += async (s, e) =>
                {
                    var dialog = new EditEdgeDialog(edge);
                    await dialog.ShowDialog(this);
                    RedrawAllEdges();
                };
                menu.Items.Add(editItem);

                var removeItem = new MenuItem { Header = "Remove edge" };
                removeItem.Click += (s, e) => ViewModel?.RemoveEdgeCommand.Execute(edge.Id);
                menu.Items.Add(removeItem);
            }
            else
            {
                var addNodeItem = new MenuItem { Header = "Add node" };
                addNodeItem.Click += (s, e) => ViewModel?.AddNodeCommand.Execute(new NodePlacement(canvasPoint.X, canvasPoint.Y));
                menu.Items.Add(addNodeItem);
            }

            menu.Open(this);
        }

        #endregion

        #region Edge Drawing Mode

        private void StartEdgeDrawing(NodeViewModel sourceNode)
        {
            _isDrawingEdge = true;
            _edgeSourceNode = sourceNode;

            if (_drawingLine != null)
            {
                // Line will be positioned correctly in OnCanvasPointerMoved
                // Initially hidden until cursor moves outside the source node
                _drawingLine.IsVisible = false;
            }
        }

        private void CompleteEdgeDrawing(NodeViewModel targetNode)
        {
            if (ViewModel == null || _edgeSourceNode == null) return;

            var dx = targetNode.X - _edgeSourceNode.X;
            var dy = targetNode.Y - _edgeSourceNode.Y;
            var weight = Math.Round(Math.Sqrt(dx * dx + dy * dy));

            ViewModel.AddEdgeCommand.Execute(new EdgeCreation(
                _edgeSourceNode.Id,
                targetNode.Id,
                weight));

            CancelEdgeDrawing();
        }

        private void CancelEdgeDrawing()
        {
            _isDrawingEdge = false;
            _isShiftDraggingEdge = false;
            _edgeSourceNode = null;

            if (_drawingLine != null)
            {
                _drawingLine.IsVisible = false;
            }
        }

        #endregion
    }
}
