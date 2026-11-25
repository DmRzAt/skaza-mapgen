# Proceduralny generator map 2D (seed) z REST API i podglądem

## Przedmiot

Testowanie i Jakość Oprogramowania II, Akademia Tarnowska

## Autor

Dmytro Zatserkivnyi ([DmRzAt](https://github.com/DmRzAt))

## Temat projektu

Proceduralny generator map 2D (seed) z REST API i podglądem

## Opis

Aplikacja generuje mapę doliny 2D na podstawie ziarna (seed). To samo ziarno i ten sam rozmiar zawsze dają identyczną mapę. Mapę można obejrzeć w przeglądarce, pobrać jako PNG albo odczytać jako JSON przez REST API.

Generator jest częścią gry „Zielarz: Dolina Zarazy”, rozwijanej w ramach pracy inżynierskiej. W kodzie i nazwie repozytorium zostaje robocza nazwa gry „Skaza” (np. przestrzeń nazw `Skaza.MapGen`). Biblioteka `MapGen.Core` nie zależy od żadnego silnika, dlatego ten sam kod działa w tym API i jako lokalny pakiet w projekcie Unity.

Mapa powstaje w sześciu krokach:

1. **Ziarno i PCG32.** Własny generator liczb pseudolosowych daje ten sam ciąg liczb na każdym komputerze.
2. **Strefy.** Odległość od Serca Zarazy (środek mapy) z dodatkiem szumu dzieli mapę na trzy pierścienie: czyste ziemie, pogranicze i głęboką zarazę.
3. **Biomy.** Dwie warstwy szumu wartości (po 3 oktawy) wybierają łąkę, las albo bagno.
4. **Skały.** Automat komórkowy: 30% losowych skał i 3 przejścia wygładzania, a brzeg mapy jest zawsze skałą.
5. **Spójność.** Przeszukiwanie wszerz (BFS) od Serca zamienia w skałę miejsca, do których nie da się dojść.
6. **Kapliczki.** Trzy kapliczki, po jednej w każdej strefie i w innym kierunku od Serca, co najmniej 4 pola od niego i 8 pól od siebie.

### Struktura repozytorium

| Katalog | Zawartość |
|---|---|
| [`src/MapGen.Core`](src/MapGen.Core) | Generator map (.NET Standard 2.1, także pakiet Unity `com.skaza.mapgen`) |
| [`src/MapGen.Api`](src/MapGen.Api) | REST API (ASP.NET Core Minimal API) i podgląd w przeglądarce (`wwwroot`) |
| [`tests/MapGen.UnitTests`](tests/MapGen.UnitTests) | Testy jednostkowe generatora |
| [`tests/MapGen.IntegrationTests`](tests/MapGen.IntegrationTests) | Testy integracyjne REST API |

## Uruchomienie

Wymagania: [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
git clone https://github.com/DmRzAt/skaza-mapgen.git
cd skaza-mapgen
dotnet run --project src/MapGen.Api
```

Po uruchomieniu:

- podgląd w przeglądarce: `http://localhost:5186`
- przykładowe zapytanie do API: `http://localhost:5186/api/maps/42`
- dokument OpenAPI: `http://localhost:5186/openapi/v1.json`

## Testy

Wszystkie testy są napisane w NUnit w stylu AAA (Arrange, Act, Assert). Uruchomienie wszystkich testów:

```bash
dotnet test
```

### Testy jednostkowe

Katalog: [`tests/MapGen.UnitTests`](tests/MapGen.UnitTests). 36 metod testowych, 76 uruchomień (część metod ma kilka przypadków `[TestCase]`).

| Plik | Co sprawdza |
|---|---|
| [`SimRandomTests.cs`](tests/MapGen.UnitTests/SimRandomTests.cs) | Generator PCG32: wartości referencyjne, powtarzalność, zakresy liczb |
| [`WorldGridTests.cs`](tests/MapGen.UnitTests/WorldGridTests.cs) | Siatka mapy: granice, odczyt i zapis pól, pola przechodnie |
| [`GridPositionTests.cs`](tests/MapGen.UnitTests/GridPositionTests.cs) | Pozycja na siatce: równość i hash |
| [`GeneratorPartsTests.cs`](tests/MapGen.UnitTests/GeneratorPartsTests.cs) | Części generatora: zakres rozmiaru mapy, szum, strefy, spójność |
| [`WorldGeneratorTests.cs`](tests/MapGen.UnitTests/WorldGeneratorTests.cs) | Cała mapa: powtarzalność, rozmiar, Serce, osiągalność, kapliczki, brzeg ze skał |
| [`MapStatisticsTests.cs`](tests/MapGen.UnitTests/MapStatisticsTests.cs) | Statystyki mapy: liczby pól, pola przechodnie |

### Testy integracyjne

Katalog: [`tests/MapGen.IntegrationTests`](tests/MapGen.IntegrationTests). 17 metod testowych, 23 uruchomienia. Testy uruchamiają całe API w pamięci (`WebApplicationFactory`) i wysyłają prawdziwe zapytania HTTP.

| Plik | Co sprawdza |
|---|---|
| [`MapEndpointTests.cs`](tests/MapGen.IntegrationTests/MapEndpointTests.cs) | `GET /api/maps/{seed}`: rozmiar, wartości domyślne, powtarzalność, Serce i kapliczki, największe ziarno, błędy 400 |
| [`MapImageEndpointTests.cs`](tests/MapGen.IntegrationTests/MapImageEndpointTests.cs) | `GET /api/maps/{seed}/image`: poprawny PNG i jego wymiary, skala domyślna, błędy 400 |
| [`MapStatisticsEndpointTests.cs`](tests/MapGen.IntegrationTests/MapStatisticsEndpointTests.cs) | `GET /api/maps/{seed}/stats`: sumy pól, zgodność z mapą, błąd 400 |
| [`DocumentationAndPreviewTests.cs`](tests/MapGen.IntegrationTests/DocumentationAndPreviewTests.cs) | Dokument OpenAPI i pliki podglądu w przeglądarce |

## Dokumentacja API

Pełna specyfikacja jest generowana automatycznie w formacie OpenAPI pod adresem `/openapi/v1.json`.

| Metoda | Ścieżka | Parametry zapytania | Odpowiedź |
|---|---|---|---|
| `GET` | `/api/maps/{seed}` | `width`, `height` (32–256, domyślnie 96) | `200` JSON z mapą |
| `GET` | `/api/maps/{seed}/image` | `width`, `height` (32–256, domyślnie 96), `scale` (1–16 pikseli na pole, domyślnie 4) | `200` obraz `image/png` |
| `GET` | `/api/maps/{seed}/stats` | `width`, `height` (32–256, domyślnie 96) | `200` JSON ze statystykami |

`seed` to liczba całkowita od 0 do 18446744073709551615.

### Przykład: mapa

`GET /api/maps/42?width=32&height=32` (wiersze `tiles` i `zones` skrócone):

```json
{
  "seed": 42,
  "width": 32,
  "height": 32,
  "heart": { "x": 16, "y": 16 },
  "shrines": [
    { "x": 30, "y": 18, "zone": "Clean" },
    { "x": 8, "y": 19, "zone": "Borderland" },
    { "x": 18, "y": 11, "zone": "DeepCorruption" }
  ],
  "tiles": ["rrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr", "rrrrrrrrrrrrrrrrrrmmmmrrrrrrrrrr", "..."],
  "zones": ["cccccccccccccccccccccccccccccccc", "..."],
  "tileLegend": { "m": "Meadow", "f": "Forest", "s": "Swamp", "r": "Rock" },
  "zoneLegend": { "c": "Clean", "b": "Borderland", "d": "DeepCorruption" }
}
```

Każdy wiersz `tiles` i `zones` to jeden rząd mapy, a każdy znak to jedno pole według legendy.

### Przykład: statystyki

`GET /api/maps/42/stats?width=32&height=32`:

```json
{
  "seed": 42,
  "width": 32,
  "height": 32,
  "totalTiles": 1024,
  "walkableTiles": 664,
  "walkableShare": 0.6484,
  "tileCounts": { "Meadow": 539, "Forest": 45, "Swamp": 80, "Rock": 360 },
  "zoneCounts": { "Clean": 678, "Borderland": 260, "DeepCorruption": 86 },
  "shrineCount": 3
}
```

### Błędy

Niepoprawne parametry zwracają `400` w formacie `application/problem+json`, np. `GET /api/maps/42?width=300`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "width": ["width must be between 32 and 256, got 300."] }
}
```

Ziarno, które nie jest liczbą całkowitą nieujemną (np. `abc` albo `-1`), też zwraca `400`.

## Przypadki testowe

Warunek wspólny dla wszystkich przypadków: aplikacja jest uruchomiona poleceniem `dotnet run --project src/MapGen.Api` i działa pod adresem `http://localhost:5186`.

| ID | Tytuł | Warunki początkowe | Kroki | Oczekiwany rezultat |
|---|---|---|---|---|
| TC-01 | Wyświetlenie mapy po otwarciu strony | Strona nie jest otwarta | 1. Otwórz `http://localhost:5186` | Pola formularza mają wartości: ziarno 42, szerokość 96, wysokość 96. Mapa wyświetla się z animacją od środka. Statystyki są wypełnione, w tym „Kapliczki: 3”. Adres strony zmienia się na `/?seed=42&width=96&height=96` |
| TC-02 | Generowanie mapy dla podanego ziarna i rozmiaru | Strona jest otwarta | 1. Wpisz ziarno `12345`<br>2. Wpisz szerokość `128` i wysokość `64`<br>3. Kliknij „Generuj mapę” | Wyświetla się mapa dwa razy szersza niż wysoka. Statystyki się zmieniają. Adres strony zmienia się na `/?seed=12345&width=128&height=64` |
| TC-03 | To samo ziarno daje tę samą mapę | Wykonano TC-02 | 1. Zapisz wartości statystyk<br>2. Kliknij ponownie „Generuj mapę” bez zmiany pól | Mapa i wszystkie wartości statystyk są identyczne jak przed kliknięciem |
| TC-04 | Losowe ziarno | Strona jest otwarta | 1. Kliknij „Losowe ziarno” | W polu „Ziarno” pojawia się nowa liczba. Wyświetla się nowa mapa, a adres strony zawiera nowe ziarno |
| TC-05 | Walidacja niepoprawnego ziarna | Strona jest otwarta i wyświetla mapę | 1. Wpisz ziarno `abc`<br>2. Kliknij „Generuj mapę” | Pod polem pojawia się komunikat „Ziarno to liczba całkowita od 0 do 18446744073709551615.” Pole jest oznaczone jako błędne. Mapa i adres strony się nie zmieniają |
| TC-06 | Walidacja rozmiaru mapy | Strona jest otwarta | 1. Wpisz poprawne ziarno `42`<br>2. Wpisz szerokość `20` i wysokość `300`<br>3. Kliknij „Generuj mapę” | Pod polami pojawiają się komunikaty „Szerokość musi być liczbą od 32 do 256.” i „Wysokość musi być liczbą od 32 do 256.” Pod polem ziarna nie ma błędu. Mapa się nie zmienia |
| TC-07 | Podgląd pola pod kursorem | Wyświetlona jest mapa dla ziarna 42 i rozmiaru 96 × 96 | 1. Najedź kursorem na różowy punkt w środku mapy<br>2. Zjedź kursorem z mapy | Po kroku 1 pod mapą widać „(48, 48) Serce Zarazy, głęboka zaraza”. Po kroku 2 widać „Najedź na mapę, aby zobaczyć pole.” |
| TC-08 | Pobranie mapy jako PNG | Wyświetlona jest mapa dla ziarna 42 i rozmiaru 96 × 96 | 1. Kliknij „Obraz PNG” | Pobiera się plik `zielarz-42.png`. Obraz ma rozmiar 768 × 768 pikseli (8 pikseli na pole) i przedstawia tę samą mapę |
| TC-09 | Otwarcie danych JSON | Wyświetlona jest mapa dla ziarna 42 i rozmiaru 96 × 96 | 1. Kliknij „Dane JSON” | W nowej karcie otwiera się JSON z polami `seed: 42`, `width: 96`, `height: 96`, `heart: {x: 48, y: 48}`, 96 wierszami `tiles` i 3 kapliczkami |
| TC-10 | Otwarcie linku z parametrami | Strona nie jest otwarta | 1. Otwórz `http://localhost:5186/?seed=2024&width=64&height=64` | Formularz ma wartości ziarno 2024, szerokość 64 i wysokość 64. Wyświetla się mapa 64 × 64 dla tego ziarna |
| TC-11 | Komunikat przy niedziałającym serwerze | Strona jest otwarta i wyświetla mapę | 1. Zatrzymaj aplikację (Ctrl+C w terminalu)<br>2. Wpisz ziarno `555`<br>3. Kliknij „Generuj mapę” | Pojawia się komunikat „Serwer nie odpowiada. Sprawdź, czy aplikacja działa, i spróbuj ponownie.” Przycisk „Generuj mapę” znowu jest aktywny |
| TC-12 | Walidacja parametrów w REST API | Brak | 1. Otwórz w przeglądarce `http://localhost:5186/api/maps/42?width=300` | Odpowiedź ma status 400 i typ `application/problem+json`. W polu `errors.width` jest komunikat „width must be between 32 and 256, got 300.” |
| TC-13 | Dokumentacja OpenAPI | Brak | 1. Otwórz `http://localhost:5186/openapi/v1.json` | Dokument zawiera w `paths` trzy ścieżki: `/api/maps/{seed}`, `/api/maps/{seed}/image` i `/api/maps/{seed}/stats` |

### Wykonanie

| Data | Środowisko | Wynik |
|---|---|---|
| 21.11.2026 | Windows 11, .NET 10, przeglądarka Chromium | TC-01 – TC-13: zaliczone |

## Technologie

- C# i .NET 10
- ASP.NET Core Minimal API
- Microsoft.AspNetCore.OpenApi 10.0.13 (dokument OpenAPI)
- HTML, CSS i JavaScript (canvas) w podglądzie
- NUnit 4.3.2
- Microsoft.AspNetCore.Mvc.Testing 10.0.13 (testy integracyjne)
- .NET Standard 2.1 i Unity 6.3 (biblioteka generatora jako pakiet Unity)
