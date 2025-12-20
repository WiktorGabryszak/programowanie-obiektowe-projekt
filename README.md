# Algorytm Dijkstra - Dokumentacja Projektu

## Opis Projektu

Aplikacja implementująca algorytmy wyszukiwania najkrótszej ścieżki na mapie/grafie: **A\* (A-Star)** lub **Dijkstra**. Projekt demonstruje zaawansowane koncepty programowania obiektowego w Javie.

## Funkcje Aplikacji

### 1. Moduł Reprezentacji Mapy/Grafu

**Klasy:** `Node` (Węzeł), `Tile` (Kafelka), `Grid` (Mapa/Graf)

- 1.1 Tworzenie Struktury:** System umożliwia utworzenie siatki/mapy (dwuwymiarowa tablica obiektów `Tile` lub zbiór połączonych obiektów `Node`)
- 1.2 Definicja Startu/Końca:** System pozwala na wyznaczenie i oznaczenie początkowego węzła (Start) i docelowego węzła (Cel)
- 1.3 Definicja Przeszkód:** System umożliwia oznaczanie niektórych kafelków/węzłów jako przeszkody (nieprzejezdne/niedostępne)
- 1.4 Koszt Krawędzi:** System przypisuje stały koszt (np. 1) do przejścia między sąsiadującymi, dostępnymi węzłami (ruch w pionie/poziomie)

### 2. Moduł Implementacji Algorytmu

**Główna Klasa:** `PathfindingAlgorithm`, `AStarAlgorithm`, `DijkstraAlgorithm`

- 2.1 Inicjalizacja Algorytmu:** System inicjuje algorytm, ustawiając koszt startowy (g) na 0 dla węzła początkowego
- 2.2 Lista Otwarta (Open List):** System utrzymuje listę/kolejkę węzłów do odwiedzenia (Open List), zaczynając od węzła startowego
- 2.3 Lista Zamknięta (Closed List):** System utrzymuje listę węzłów już odwiedzonych (Closed List), aby ich nie przetwarzać ponownie
- 2.4 Obliczanie Kosztu (Dla A\*):** System oblicza łączny koszt (**f = g + h**) dla każdego węzła, gdzie:
  - g = koszt rzeczywisty od Startu
  - h = koszt heurystyczny (Manhattan lub Euklidesa) do Celu
- 2.5 Przetwarzanie Sąsiadów:** System iteracyjnie przetwarza sąsiadów wybranego węzła, aktualizując ich koszty i przypisując im rodzica
- 2.6 Warunek Zatrzymania:** Algorytm zatrzymuje się po osiągnięciu węzła docelowego lub gdy lista otwarta jest pusta

### 3. Moduł Prezentacji Wyników

**Klasy:** `PathfindingAlgorithm`, `MapVisualizer`

- 3.1 Odtwarzanie Ścieżki:** Po znalezieniu celu, system odtwarza najkrótszą ścieżkę cofając się od węzła docelowego do węzła startowego
- 3.2 Wyświetlanie Ścieżki:** System wyświetla:
  - Mapę z oznaczeniem Startu (S), Celu (G) i Przeszkód (#)
  - Znalezioną najkrótszą ścieżkę (np. za pomocą symbolu `*`)
- 3.3 Informacja o Koszcie:** System wyświetla całkowity koszt znalezionej ścieżki
- 3.4 Brak Ścieżki:** System informuje użytkownika, jeśli ścieżka do celu nie została znaleziona

### 4. Interfejs Użytkownika (Konsola)

**Główna Klasa:** `Main`

- 4.1 Uruchomienie:** System umożliwia uruchomienie symulacji z predefiniowaną (lub wczytywaną) mapą
- 4.2 Wyświetlanie Stanu:** W trakcie działania, system opcjonalnie wypisuje w konsoli kolejność odwiedzanych węzłów

## Technologia

- Język: C#

## Licencja

Projekt edukacyjny - do użytku dydaktycznego

## Autor

Algorytm Dijkstra - C# OOP 2025
