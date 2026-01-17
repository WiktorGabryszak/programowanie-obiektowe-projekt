# DijkstraVisualizer

A desktop application for visualizing Dijkstra's shortest path algorithm on user-defined graphs, built with **C#** and **Avalonia UI**.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Avalonia](https://img.shields.io/badge/Avalonia-11.3.10-8B44AC?style=flat)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Overview

DijkstraVisualizer is an interactive graph visualization tool that demonstrates Dijkstra's algorithm for finding the shortest path between two nodes. The application allows users to create custom graphs by adding nodes and weighted edges, then visualizes the algorithm's step-by-step execution with real-time animations.

### Key Features

- **Interactive Graph Creation**: Add, edit, and remove nodes and edges with an intuitive canvas interface
- **Real-Time Visualization**: Watch Dijkstra's algorithm execute step-by-step with animated transitions
- **Customizable Animation Speed**: Adjust visualization speed from instant to slow motion
- **Visual Feedback**: Color-coded nodes and edges show algorithm state:
  - **Forest Green**: Start node
  - **Orange Red**: End node  
  - **Crimson**: Visited nodes
  - **Lime Green**: Currently processing node
  - **Gold**: Shortest path found
- **Graph Editing**: Modify node positions, edge weights, and names dynamically
- **Path Validation**: Automatic detection of disconnected graphs and unreachable nodes

## 🏗️ Architecture

The application follows **Clean Architecture** principles with strict adherence to **MVVM (Model-View-ViewModel)** pattern and **SOLID** principles.

### Project Structure

```
DijkstraVisualization/
├── Models/                    # Domain Layer (POCOs)
│   ├── NodeModel.cs          # Node entity
│   ├── EdgeModel.cs          # Edge entity
│   ├── GraphModel.cs         # Graph aggregate
│   ├── AlgorithmStep.cs      # Algorithm state snapshot
│   ├── PathResult.cs         # Path calculation result
│   └── AlgorithmResult.cs    # Extended result with execution time
│
├── Services/                  # Business Logic Layer
│   ├── IDijkstraService.cs   # Service interface
│   └── DijkstraService.cs    # Dijkstra algorithm implementation
│
├── ViewModels/                # Presentation Layer
│   ├── ViewModelBase.cs      # Base ViewModel
│   ├── MainViewModel.cs      # Main window logic & commands
│   ├── NodeViewModel.cs      # Node presentation logic
│   ├── EdgeViewModel.cs      # Edge presentation logic & geometry
│   └── Converters.cs         # Value converters for UI binding
│
└── Views/                     # UI Layer (XAML)
    ├── MainWindow.axaml      # Main application window
    ├── EditNodeDialog.axaml  # Node editing dialog
    └── EditEdgeDialog.axaml  # Edge editing dialog

DijkstraVisualization.Tests/  # Unit Tests
└── Services/
    └── DijkstraServiceTests.cs
```

### Architectural Layers

#### 1. Domain Layer (Models)
Pure POCOs with no dependencies on UI or infrastructure:
- **NodeModel**: `Guid Id`, `string Name`, `double X`, `double Y`
- **EdgeModel**: `Guid Id`, `Guid SourceNodeId`, `Guid TargetNodeId`, `double Weight`, `string Name`
- **GraphModel**: Aggregates `List<NodeModel>` and `List<EdgeModel>`
- **AlgorithmStep**: Algorithm state for visualization
- **PathResult**: Path calculation result with metrics

#### 2. Service Layer
Implements business logic with dependency injection:
- **IDijkstraService**: 
  - `AlgorithmResult CalculatePath(GraphModel, Guid startId, Guid endId)`
  - `IEnumerable<AlgorithmStep> GetVisualizationSteps(GraphModel, Guid startId, Guid endId)` (uses `yield return`)

#### 3. Presentation Layer (ViewModels)
Implements `INotifyPropertyChanged` using **CommunityToolkit.Mvvm**:
- **MainViewModel**: Graph state management, commands, async visualization orchestration
- **NodeViewModel**: Node presentation, visual state (selected, visited, on path)
- **EdgeViewModel**: Edge presentation, dynamic geometry calculations (position, angle, length)

#### 4. View Layer
XAML-only views with minimal code-behind for platform-specific operations

## 🚀 Getting Started

### Prerequisites

- **.NET 8.0 SDK** or newer
- **IDE**: Visual Studio 2022, JetBrains Rider, or VS Code
- **OS**: Windows, macOS, or Linux (Avalonia is cross-platform)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd programowanie-obiektowe-projekt
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run --project DijkstraVisualization
   ```

### Running Tests

```bash
dotnet test
```

The project includes unit tests for the Dijkstra service implementation using **xUnit** and **FluentAssertions**.

## 🎮 Usage Guide

### Creating a Graph

1. **Add Nodes**: Right-click on the canvas to add a new node at the cursor position
2. **Add Edges**: 
   - Click on a source node
   - Click on a target node
   - Edge dialog will appear to set weight and name
3. **Edit Elements**: Click on nodes or edges to open edit dialogs
4. **Move Nodes**: Drag nodes to reposition them (edges update automatically)

### Running the Algorithm

1. **Set Start Node**: Right-click a node → "Set as Start"
2. **Set End Node**: Right-click a node → "Set as End"
3. **Adjust Speed**: Use the animation speed slider (0-3 seconds per step)
4. **Start**: Click the "▶ Start" button
5. **Watch**: Observe the algorithm's step-by-step execution
6. **Result**: Final path highlights in gold if found

### Controls

- **▶ Start**: Execute Dijkstra's algorithm visualization
- **🗑️ Clear**: Reset the entire graph
- **Speed Slider**: Adjust animation interval (0-3 seconds)
- **Right-Click Context Menu**: Node operations (set start/end, delete)

## 🛠️ Technology Stack

| Technology | Purpose | Version |
|------------|---------|---------|
| **C#** | Programming language | 12.0 |
| **.NET** | Application framework | 8.0 |
| **Avalonia UI** | Cross-platform UI framework | 11.3.10 |
| **CommunityToolkit.Mvvm** | MVVM helpers | 8.2.1 |
| **xUnit** | Unit testing framework | Latest |
| **FluentAssertions** | Assertion library | Latest |

### Key Dependencies

```xml
<PackageReference Include="Avalonia" Version="11.3.10" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.10" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.10" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.1" />
```

## 📐 Design Principles

### SOLID Principles

- **Single Responsibility**: Each class has one reason to change
  - `DijkstraService` only handles algorithm logic
  - `NodeViewModel` only handles node presentation
  - `MainViewModel` orchestrates graph operations

- **Open/Closed**: Extensible through interfaces
  - `IDijkstraService` allows alternative implementations
  - ViewModels can be extended without modifying base logic

- **Liskov Substitution**: Consistent interface contracts
  - `AlgorithmResult` extends `PathResult` without breaking expectations

- **Interface Segregation**: Focused interfaces
  - `IDijkstraService` provides only necessary methods

- **Dependency Inversion**: Depend on abstractions
  - `MainViewModel` depends on `IDijkstraService` interface
  - Constructor injection for testability

### MVVM Pattern

- **Models**: Pure domain objects, no UI dependencies
- **Views**: XAML-only, declarative UI definitions
- **ViewModels**: Presentation logic, state management, commands
- **Binding**: Data flows via `INotifyPropertyChanged`

### Big Design Up Front (BDUF)

The architecture was designed completely before implementation:
1. Domain model definition
2. Service interface contracts
3. ViewModel structure and responsibilities
4. View composition and data templates

## 🧪 Testing

Unit tests cover the core algorithm implementation:

```csharp
[Fact]
public void CalculatePath_WithLinearGraph_ReturnsShortestPath()
{
    // Arrange: A -> B -> C
    var result = service.CalculatePath(graph, nodeA.Id, nodeC.Id);
    
    // Assert
    result.PathFound.Should().BeTrue();
    result.TotalCost.Should().Be(5);
    result.NodePath.Should().Equal(nodeA.Id, nodeB.Id, nodeC.Id);
}
```

Test coverage includes:
- ✅ Linear path calculation
- ✅ Disconnected graph handling
- ✅ Preferring cheaper paths over shorter ones
- ✅ Start equals end edge case
- ✅ Non-null result guarantees

## 🎨 Visual Design

The application uses a dark theme with high-contrast colors:

- **Background**: Deep blue (#0f0f23, #1a1a2e)
- **Canvas**: Dark navy (#1a1a2e)
- **Toolbar**: Charcoal (#16213e)
- **Start Button**: Green (#4CAF50)
- **Clear Button**: Red (#E53935)

Node color states are carefully chosen for accessibility and clarity during algorithm visualization.

## 📊 Algorithm Complexity

**Dijkstra's Algorithm** (with Priority Queue):
- **Time Complexity**: O((V + E) log V) where V = vertices, E = edges
- **Space Complexity**: O(V) for distance and previous node tracking

The implementation uses .NET's built-in `PriorityQueue<T, TPriority>` for efficient min-heap operations.

## 🔮 Future Enhancements

Potential improvements:
- [ ] Graph save/load functionality (JSON serialization)
- [ ] Undo/Redo operations
- [ ] Additional algorithms (A*, Bellman-Ford, Floyd-Warshall)
- [ ] Directed vs undirected graph toggle
- [ ] Graph auto-layout algorithms
- [ ] Performance metrics dashboard
- [ ] Export visualization as video/GIF

## 📝 License

This project is licensed under the MIT License.

## 👥 Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Follow SOLID and MVVM principles
4. Add unit tests for new features
5. Submit a pull request

## 📚 References

- [Dijkstra's Algorithm](https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm)
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [MVVM Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/maui/mvvm)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

## 🙋 Support

For issues, questions, or suggestions, please open an issue on the repository.

---

**Built with ❤️ using C# and Avalonia UI**
