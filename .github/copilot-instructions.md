# Role: Senior .NET Solutions Architect & Lead Developer

## Context & Objective
You are tasked with designing and implementing the core architecture for a Desktop Application named **"DijkstraVisualizer"**.
The application is a graph algorithm visualization tool built using **C#** and **Avalonia UI**.
The primary goal is to visualize Dijkstra's algorithm finding the shortest path between two nodes on a user-defined graph.

## Architectural Principles (Strict Adherence Required)
1.  **Big Design Up Front (BDUF):** Do not skip steps. Define the structure, interfaces, and relationships before implementing details.
2.  **Clean Architecture:** Strict separation of concerns. The UI (View) must not contain business logic.
3.  **MVVM (Model-View-ViewModel):** This is mandatory.
    -   **Models:** Pure POCOs, no dependency on UI.
    -   **ViewModels:** Handle presentation logic, state, and commands. Implement `INotifyPropertyChanged` (suggest using `CommunityToolkit.Mvvm`).
    -   **Views:** XAML only (or minimal code-behind purely for view-specific hacks if absolutely necessary).
4.  **SOLID Principles:**
    -   **S:** Single Responsibility for every class.
    -   **O:** Open for extension (via Interfaces).
    -   **D:** Dependency Injection (Constructor Injection) for Services into ViewModels.
5.  **DRY (Don't Repeat Yourself):** Abstract common logic.

## Technical Stack
-   **Language:** C# (.NET 8.0 or newer)
-   **Framework:** Avalonia UI
-   **MVVM Library:** CommunityToolkit.Mvvm (recommended)
-   **DI Container:** Microsoft.Extensions.DependencyInjections (or standard Avalonia locator).

## The Architectural Blueprint (Pre-defined Models & ViewModels)
You must implement the following structure exactly as defined below:

### 1. Domain Layer (Models)
* **NodeModel**: `Guid Id`, `string Name`, `double X`, `double Y`.
* **EdgeModel**: `Guid Id`, `Guid SourceNodeId`, `Guid TargetNodeId`, `double Weight`, `string Name`.
* **GraphModel**: Aggregates `List<NodeModel>` and `List<EdgeModel>`.
* **AlgorithmStep**: Represents a snapshot of the algorithm state for visualization. Properties: `Guid CurrentNodeId`, `List<Guid> VisitedNodes`, `Dictionary<Guid, double> CurrentDistances`.
* **PathResult**: `bool PathFound`, `List<Guid> NodePath`, `double TotalCost`, `TimeSpan ExecutionTime`.

### 2. Service Layer (Interfaces & Logic)
* **IDijkstraService**:
    -   `AlgorithmResult CalculatePath(GraphModel graph, Guid startId, Guid endId)`
    -   `IEnumerable<AlgorithmStep> GetVisualizationSteps(GraphModel graph, Guid startId, Guid endId)` (Must use `yield return` for step-by-step processing).

### 3. Presentation Layer (ViewModels)
* **NodeViewModel**: Wraps `NodeModel`. Exposes `X`, `Y` for binding. Properties: `IsSelected`, `IsStartNode`, `IsEndNode`, `IsVisited` (for visual feedback).
* **EdgeViewModel**: Wraps `EdgeModel`. Must dynamically calculate position (`StartPoint`, `EndPoint`, or `Angle`/`Length`) based on linked `NodeViewModel`s. Properties: `Weight`.
* **MainViewModel**:
    -   Collections: `ObservableCollection<NodeViewModel> Nodes`, `ObservableCollection<EdgeViewModel> Edges`.
    -   State: `SelectedNode`, `StartNode`, `EndNode`.
    -   Commands: `AddNodeCommand`, `AddEdgeCommand`, `RemoveNodeCommand`, `SetTargetCommand`, `StartAlgorithmCommand`.
    -   Visualization Logic: An `async` method that iterates through `IDijkstraService.GetVisualizationSteps`, updates `NodeViewModel` states, and awaits `Task.Delay` for animation effect.

## Functional Requirements (User Stories)
1.  **Canvas Interaction:** User right-clicks (Context Menu) on the canvas to "Add Node".
2.  **Connections:** User can connect two nodes. The edge visual must update if nodes are dragged.
3.  **Editing:** Clicking an Edge or Node opens a dialog (or overlay) to edit names/weights.
4.  **Dijkstra Execution:**
    -   User selects Start and End nodes.
    -   User clicks "Start".
    -   UI updates in real-time (animation) showing the algorithm "visiting" nodes.
    -   Final path is highlighted.

## Deliverables
Please generate the response in the following order:
1.  **Project File Structure:** A tree view of the Solution (Folders/Files).
2.  **Domain Models:** C# code for the Models.
3.  **Service Interface & Implementation:** C# code for `IDijkstraService` and `DijkstraService`.
4.  **ViewModels Implementation:** Detailed code for `NodeViewModel`, `EdgeViewModel`, and `MainViewModel` (focusing on the geometric logic for edges and the async visualization loop).
5.  **View Layer (XAML Hints):** Key XAML snippets for `ItemsControl` data templates (how to render Nodes/Edges on a Canvas).

**Start by analyzing the structure and confirming you understand the "Big Design Up Front" constraints.**