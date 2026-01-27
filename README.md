# DijkstraVisualizer

Aplikacja desktopowa do wizualizacji algorytmu Dijkstry na grafach. Stworzona przy użyciu **C#** i **Avalonia UI**.

## O projekcie

DijkstraVisualizer to interaktywne narzędzie do wizualizacji algorytmu znajdowania najkrótszej ścieżki między dwoma węzłami w grafie. Użytkownik może tworzyć własne grafy i obserwować działanie algorytmu krok po kroku.

## Funkcjonalność

- Tworzenie grafów przez dodawanie węzłów i krawędzi
- Wizualizacja algorytmu Dijkstry krok po kroku z animacjami
- Regulowana prędkość animacji
- Kolorowe oznaczenia węzłów:
  - **Zielony**: węzeł początkowy
  - **Pomarańczowy**: węzeł docelowy
  - **Czerwony**: węzły odwiedzone
  - **Jasnozielony**: aktualnie przetwarzany węzeł
  - **Złoty**: najkrótsza ścieżka
- Edycja położenia węzłów, wag krawędzi i nazw
- Automatyczne wykrywanie niespójnych grafów

## Architektura

Aplikacja opiera się na zasadach **Clean Architecture**, wzorcu **MVVM (Model-View-ViewModel)** oraz zasadach **SOLID**.

### Struktura projektu

```
DijkstraVisualization/
├── Models/                    # Warstwa domeny
│   ├── NodeModel.cs          # Model węzła
│   ├── EdgeModel.cs          # Model krawędzi
│   ├── GraphModel.cs         # Model grafu
│   ├── AlgorithmStep.cs      # Stan algorytmu
│   └── PathResult.cs         # Wynik obliczeń
│
├── Services/                  # Logika biznesowa
│   ├── IDijkstraService.cs   # Interfejs serwisu
│   └── DijkstraService.cs    # Implementacja algorytmu
│
├── ViewModels/                # Warstwa prezentacji
│   ├── MainViewModel.cs      # Logika głównego okna
│   ├── NodeViewModel.cs      # Logika prezentacji węzła
│   └── EdgeViewModel.cs      # Logika prezentacji krawędzi
│
└── Views/                     # Warstwa interfejsu (XAML)
    ├── MainWindow.axaml      # Główne okno aplikacji
    └── Edit*Dialog.axaml     # Okna dialogowe

DijkstraVisualization.Tests/  # Testy jednostkowe
└── Services/
    └── DijkstraServiceTests.cs
```

## Instrukcja użytkowania

### Tworzenie grafu

1. **Dodawanie węzłów**: Kliknij prawym przyciskiem myszy na canvas
2. **Dodawanie krawędzi**: Kliknij węzeł źródłowy, następnie węzeł docelowy
3. **Edycja**: Kliknij węzeł lub krawędź, aby otworzyć okno edycji
4. **Przesuwanie**: Przeciągnij węzły, aby zmienić ich położenie

### Uruchomienie algorytmu

1. Kliknij prawym przyciskiem na węzeł → "Set as Start"
2. Kliknij prawym przyciskiem na inny węzeł → "Set as End"
3. Ustaw prędkość animacji suwakiem (0-3 sekundy)
4. Kliknij przycisk "Start"
5. Obserwuj działanie algorytmu

## Technologie

- **C# 12.0** - język programowania
- **.NET 8.0** - framework aplikacji
- **Avalonia UI 11.3.10** - framework interfejsu użytkownika
- **CommunityToolkit.Mvvm 8.2.1** - narzędzia MVVM
- **xUnit** - framework testowy
- **FluentAssertions** - biblioteka asercji

## Zasady projektowe

Aplikacja została zaprojektowana zgodnie z:

- **SOLID** - Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **MVVM** - separacja modelu, widoku i logiki prezentacji
- **Clean Architecture** - oddzielenie warstw domenowej, biznesowej i prezentacji
- **Dependency Injection** - wstrzykiwanie zależności przez konstruktor

## Algorytm Dijkstry

Implementacja wykorzystuje kolejkę priorytetową (PriorityQueue):
- **Złożoność czasowa**: O((V + E) log V), gdzie V = wierzchołki, E = krawędzie
- **Złożoność pamięciowa**: O(V)

## Testy

Projekt zawiera testy jednostkowe obejmujące:
- Obliczanie najkrótszej ścieżki w grafie liniowym
- Obsługę niespójnych grafów
- Wybór najtańszej ścieżki spośród wielu możliwości
- Przypadki brzegowe (węzeł startowy = węzeł końcowy)

## Licencja

Projekt udostępniony na licencji MIT.
