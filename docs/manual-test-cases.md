# Manualne przypadki testowe

Przypadki testowe dla podglądu w przeglądarce i REST API generatora map.

**Warunek wspólny dla wszystkich przypadków:** aplikacja jest uruchomiona poleceniem `dotnet run --project src/MapGen.Api` i działa pod adresem `http://localhost:5186`.

| ID | Tytuł | Warunki początkowe | Kroki | Oczekiwany rezultat |
|---|---|---|---|---|
| TC-01 | Wyświetlenie mapy po otwarciu strony | Strona nie jest otwarta | 1. Otwórz `http://localhost:5186` | Pola formularza mają wartości: ziarno 42, szerokość 96, wysokość 96. Mapa wyświetla się z animacją od środka. Statystyki są wypełnione, w tym „Kapliczki: 3”. Adres strony zmienia się na `/?seed=42&width=96&height=96` |
| TC-02 | Generowanie mapy dla podanego ziarna i rozmiaru | Strona jest otwarta | 1. Wpisz ziarno `12345`<br>2. Wpisz szerokość `128` i wysokość `64`<br>3. Kliknij „Generuj mapę” | Wyświetla się mapa dwa razy szersza niż wysoka. Statystyki się zmieniają. Adres strony zmienia się na `/?seed=12345&width=128&height=64` |
| TC-03 | To samo ziarno daje tę samą mapę | Wykonano TC-02 | 1. Zapisz wartości statystyk<br>2. Kliknij ponownie „Generuj mapę” bez zmiany pól | Mapa i wszystkie wartości statystyk są identyczne jak przed kliknięciem |
| TC-04 | Losowe ziarno | Strona jest otwarta | 1. Kliknij „Losowe ziarno” | W polu „Ziarno” pojawia się nowa liczba. Wyświetla się nowa mapa, a adres strony zawiera nowe ziarno |
| TC-05 | Walidacja niepoprawnego ziarna | Strona jest otwarta i wyświetla mapę | 1. Wpisz ziarno `abc`<br>2. Kliknij „Generuj mapę” | Pod polem pojawia się komunikat „Ziarno to liczba całkowita od 0 do 18446744073709551615.” Pole jest oznaczone jako błędne. Mapa i adres strony się nie zmieniają |
| TC-06 | Walidacja rozmiaru mapy | Strona jest otwarta | 1. Wpisz poprawne ziarno `42`<br>2. Wpisz szerokość `20` i wysokość `300`<br>3. Kliknij „Generuj mapę” | Pod polami pojawiają się komunikaty „Szerokość musi być liczbą od 32 do 256.” i „Wysokość musi być liczbą od 32 do 256.” Pod polem ziarna nie ma błędu. Mapa się nie zmienia |
| TC-07 | Podgląd pola pod kursorem | Wyświetlona jest mapa dla ziarna 42 i rozmiaru 96 × 96 | 1. Najedź kursorem na różowy punkt w środku mapy<br>2. Zjedź kursorem z mapy | Po kroku 1 pod mapą widać „(48, 48) Serce Skazy, głęboka skaza”. Po kroku 2 widać „Najedź na mapę, aby zobaczyć pole.” |
| TC-08 | Pobranie mapy jako PNG | Wyświetlona jest mapa dla ziarna 42 i rozmiaru 96 × 96 | 1. Kliknij „Obraz PNG” | Pobiera się plik `skaza-42.png`. Obraz ma rozmiar 768 × 768 pikseli (8 pikseli na pole) i przedstawia tę samą mapę |
| TC-09 | Otwarcie danych JSON | Wyświetlona jest mapa dla ziarna 42 i rozmiaru 96 × 96 | 1. Kliknij „Dane JSON” | W nowej karcie otwiera się JSON z polami `seed: 42`, `width: 96`, `height: 96`, `heart: {x: 48, y: 48}`, 96 wierszami `tiles` i 3 kapliczkami |
| TC-10 | Otwarcie linku z parametrami | Strona nie jest otwarta | 1. Otwórz `http://localhost:5186/?seed=2024&width=64&height=64` | Formularz ma wartości ziarno 2024, szerokość 64 i wysokość 64. Wyświetla się mapa 64 × 64 dla tego ziarna |
| TC-11 | Komunikat przy niedziałającym serwerze | Strona jest otwarta i wyświetla mapę | 1. Zatrzymaj aplikację (Ctrl+C w terminalu)<br>2. Wpisz ziarno `555`<br>3. Kliknij „Generuj mapę” | Pojawia się komunikat „Serwer nie odpowiada. Sprawdź, czy aplikacja działa, i spróbuj ponownie.” Przycisk „Generuj mapę” znowu jest aktywny |
| TC-12 | Walidacja parametrów w REST API | Brak | 1. Otwórz w przeglądarce `http://localhost:5186/api/maps/42?width=300` | Odpowiedź ma status 400 i typ `application/problem+json`. W polu `errors.width` jest komunikat „width must be between 32 and 256, got 300.” |
| TC-13 | Dokumentacja OpenAPI | Brak | 1. Otwórz `http://localhost:5186/openapi/v1.json` | Dokument zawiera w `paths` trzy ścieżki: `/api/maps/{seed}`, `/api/maps/{seed}/image` i `/api/maps/{seed}/stats` |

## Wykonanie

| Data | Środowisko | Wynik |
|---|---|---|
| 21.11.2026 | Windows 11, .NET 10, przeglądarka Chromium | TC-01 – TC-13: zaliczone |
