# Aplikacja Quiz - Dokumentacja Projektu

## Opis Projektu

Aplikacja Quiz to interaktywna gra edukacyjna napisana w Javie z wykorzystaniem **programowania obiektowego**. Umożliwia graczom rozwiązywanie quizów na różnych poziomach trudności i kategoriach, z możliwością śledzenia wyników w globalnym rankingu.

## Funkcje Aplikacji

### 1. Zarządzanie Sesją i Graczem

(Główne zadania gracza)

- **RF1.1 Identyfikacja Gracza**: System musi przyjąć nick/nazwę gracza.

  - Gracz może się zarejestrować lub zalogować podając swój nick
  - Nick jest unikalny (case-insensitive)
  - Dane gracza są przechowywane i wczytywane przy każdym uruchomieniu

- **RF1.2 Ustawienia Quizu**: Umożliwienie wyboru poziomu trudności (Łatwy/Średni/Trudny) i kategorii pytań.

  - Gracz wybiera kategorię spośród dostępnych
  - Gracz wybiera poziom trudności
  - System filtruje pytania zgodnie z wyborem

- **RF1.3 Rozpoczęcie/Zakończenie**: Umożliwienie rozpoczęcia quizu i jego zakończenia po określonej liczbie pytań.
  - Quiz zawiera 10 pytań
  - Gracz może przejść przez wszystkie pytania
  - Po ostatnim pytaniu gra się kończy automatycznie

### 2. Funkcjonalność Quizu (Pytania)

(Logika gry)

- **RF2.1 Wyświetlanie Pytania**: System musi wylosować i wyświetlić pytanie zgodne z wybranymi parametrami.

  - Pytania są losowo wybierane z bazy
  - Pytanie jest wyświetlane z podpowiedzią o kategorii i trudności
  - Wyświetlany jest postęp w grze (np. "Pytanie 3/10")

- **RF2.2 Udzielenie Odpowiedzi**: System musi przyjąć i zweryfikować odpowiedź gracza.

  - System wyświetla 4 możliwe odpowiedzi
  - Gracz wpisuje numer odpowiedzi
  - System natychmiast weryfikuje poprawność

- **RF2.3 Punktacja**: System musi naliczać punkty za poprawne odpowiedzi w bieżącej sesji.
  - Punkty zależą od poziomu trudności (Łatwy=10 pkt, Średni=20 pkt, Trudny=30 pkt)
  - Bieżący wynik jest wyświetlany po każdym pytaniu
  - Wynik jest zapisywany po zakończeniu gry

### 3. Zarządzanie Danymi (Admin/Trwałość)

(Utrzymanie systemu)

- **RF3.1 Zarządzanie Pytaniami**: Musi istnieć funkcja (np. w konsoli) do dodawania, edytowania i usuwania pytań wraz z ich poziomami trudności i kategoriami.

  - Panel administratora chroniony hasłem (hasło: admin123)
  - Możliwość dodawania nowych pytań
  - Możliwość przeglądania bazy pytań
  - Możliwość usuwania pytań
  - Każde pytanie zawiera: ID, tekst, odpowiedzi, poprawną odpowiedź, kategorię, trudność

- **RF3.2 Trwałość Danych**: System musi zapisywać bazę pytań i ranking graczy do pliku oraz wczytywać je przy starcie.
  - Pytania są zapisywane w pliku `data/questions.json`
  - Graczy i ich wyniki w pliku `data/players.json`
  - Dane są automatycznie wczytywane przy starcie aplikacji
  - Format JSON zapewnia czytelność i łatwe zarządzanie

### 4. Ranking

(Wyniki graczy)

- **RF4.1 Zapis Wyniku**: System musi zapisać wynik gracza (nick + punkty) po każdej zakończonej grze.

  - Po każdej grze wynik gracza jest automatycznie aktualizowany
  - Śledzony jest: suma punktów, liczba gier, najlepszy wynik, średnia

- **RF4.2 Wyświetlanie Rankingu**: System musi sortować i wyświetlać globalny ranking graczy.
  - Ranking sortowany malejąco po liczbie punktów
  - W przypadku równych punktów, sortuje się po liczbie rozegranych gier
  - Wyświetlane dane: pozycja, nick, punkty, liczba gier, średnia


## Technologia

- **Język**: Java 14+

## Format danych

### questions.json

```json
[
	{
		"id": "q1",
		"text": "Jaka jest stolica Polski?",
		"answers": ["Warszawa", "Kraków", "Wrocław", "Poznań"],
		"correctAnswerIndex": 0,
		"category": "Geografia",
		"difficulty": "EASY"
	}
]
```

### players.json

```json
[
	{
		"nickname": "Player1",
		"totalScore": 150,
		"gamesPlayed": 3,
		"bestScore": 80
	}
]
```

## Rozszerzenia w przyszłości

- [ ] Interfejs graficzny (JavaFX/Swing)
- [ ] Baza danych (MySQL/PostgreSQL)
- [ ] Multiplayer (sieć)
- [ ] Statystyki per kategoria
- [ ] Tryb czasowy
- [ ] Wskazówki w grze
- [ ] API REST
- [ ] Mobilna wersja

## Licencja

Projekt edukacyjny - do użytku dydaktycznego

## Autor

Projekt Quiz - Java OOP 2025
