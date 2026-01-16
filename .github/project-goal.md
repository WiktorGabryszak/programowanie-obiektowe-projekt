# Rola
Jesteœ ekspertem w technologii .NET (C#) oraz frameworku **AvaloniaUI**. Twoim zadaniem jest dokoñczenie implementacji aplikacji wizualizuj¹cej algorytm Dijkstry. Bazuj na istniej¹cym kodzie (Models, ViewModels), rozszerzaj¹c go o brakuj¹c¹ logikê UI, interakcje oraz implementacjê algorytmu.

# Cel Projektu
Stworzenie interaktywnej aplikacji desktopowej, która pozwala u¿ytkownikowi rysowaæ grafy (wêz³y i krawêdzie skierowane), a nastêpnie wizualizowaæ dzia³anie algorytmu Dijkstry z konfigurowaln¹ prêdkoœci¹ animacji.

# Stack Technologiczny
* **Framework:** AvaloniaUI
* **Jêzyk:** C#
* **Wzorzec:** MVVM (Model-View-ViewModel)
    * Dozwolone jest u¿ycie "Code-Behind" (`.axaml.cs`) dla skomplikowanych interakcji na Canvasie (przeci¹ganie, rysowanie linii), aby zachowaæ prostotê kodu.
    * Logika biznesowa musi pozostaæ w ViewModelach.
* **Modele:** Istniej¹ce modele (`NodeModel`, `NodeViewModel` itd.) mog¹ byæ modyfikowane i rozszerzane.

# Wymagania Funkcjonalne

## 1. Obszar Roboczy (Canvas)
* Aplikacja startuje z pustym, czarnym t³em (Canvas).
* **Nawigacja:**
    * **Przesuwanie (Pan):** Klikniêcie i przytrzymanie LPM na tle przesuwa widok.
    * **Przybli¿anie (Zoom):** Scroll myszy (ŒPM) obs³uguje zoom in/out.

## 2. Zarz¹dzanie Wêz³ami (Nodes)
Interakcje oparte na menu kontekstowym (PPM):
* **Dodawanie:** PPM na puste miejsce -> "Add node".
    * Tworzy szare ko³o z nazw¹ "Node N" (N = licznik).
* **Edycja:** PPM na wêze³ -> "Edit node".
    * Otwiera okno dialogowe do zmiany nazwy i koloru wêz³a.
* **Usuwanie:** PPM na wêze³ -> "Remove node".
    * Usuwa wêze³ i wszystkie przyleg³e krawêdzie.
* **Punkty Start/Koniec:** PPM na wêze³ -> "Set as start" / "Set as destination".
    * Wyró¿nia wêze³ w widoku (np. specjalny kolor/obramowanie). Start i Koniec musz¹ siê ró¿niæ wizualnie.

## 3. Zarz¹dzanie Krawêdziami (Edges) - Graf Skierowany
* **Dodawanie:**
    1.  PPM na wêze³ Ÿród³owy -> "Add edge".
    2.  Tryb rysowania: Bia³a linia pod¹¿a za kursorem myszy od wêz³a Ÿród³owego.
    3.  Klikniêcie LPM na wêze³ docelowy: Tworzy krawêdŸ skierowan¹.
        * Domyœlna waga: Odleg³oœæ euklidesowa miêdzy wêz³ami.
        * Nazwa: "Edge N".
    4.  Walidacja: Próba po³¹czenia wêz³ów ju¿ po³¹czonych wyœwietla tymczasowy komunikat b³êdu (np. dymek w rogu ekranu znikaj¹cy po kilku sekundach).
    5.  Anulowanie: Klikniêcie LPM w t³o przerywa dodawanie.
* **Edycja:** PPM na krawêdŸ -> "Edit edge" (zmiana wagi, nazwy, koloru).
* **Usuwanie:** PPM na krawêdŸ -> "Remove edge".

## 4. Pasek Narzêdzi i Sterowanie
Panel (np. na dole ekranu) zawieraj¹cy:
* **Suwak prêdkoœci (Slider):**
    * Zakres: 0.0s - 3.0s (skok co 0.1s).
    * Okreœla minimalny czas trwania jednego kroku algorytmu.
* **Przycisk Start:** Uruchamia wizualizacjê (blokuje edycjê grafu na czas trwania).
* **Przycisk Wyczyœæ:** Usuwa ca³y graf.

## 5. Algorytm Dijkstry i Animacja
Zaimplementuj logikê w `DijkstraService` w sposób asynchroniczny, aby nie blokowaæ UI.

**Logika Kroku i Timera:**
1.  Przed obliczeniem kolejnego przeskoku (wyborem nastêpnego wêz³a), uruchom stoper.
2.  Wykonaj obliczenia kroku algorytmu.
3.  Zatrzymaj stoper i oblicz czas trwania operacji (`elapsed`).
4.  Oblicz opóŸnienie: `delay = user_interval - elapsed`.
5.  Je¿eli `delay > 0`, wstrzymaj w¹tek (`await Task.Delay`) na wyliczony czas. Dziêki temu zachowasz sta³e tempo animacji niezale¿nie od szybkoœci procesora.

**Kolorowanie Wêz³ów (Feedback wizualny):**
* **Aktualny wêze³ (przetwarzany):** Kolor Zielony.
* **Wêze³ odwiedzony:** Kolor Czerwony.
* **Najkrótsza œcie¿ka (wynik):** Wyró¿niony kolor (np. Niebieski lub Z³oty).

# Wytyczne Implementacyjne
* Skorzystaj z dostêpnych modeli (`NodeViewModel` posiada flagi `IsVisited`, `IsCurrentNode` - wykorzystaj je w bindingach do kolorów).
* Mo¿esz zainstalowaæ potrzebne pakiety NuGet (np. do okien dialogowych `DialogHost.Avalonia` lub powiadomieñ `Avalonia.Notification`), jeœli usprawni to pracê.
* Zadbaj o obs³ugê b³êdów: uruchomienie algorytmu bez punktu Start/Koniec powinno wyœwietliæ komunikat b³êdu (taki sam jak przy duplikacji krawêdzi).
