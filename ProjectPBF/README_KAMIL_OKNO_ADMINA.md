# Kamil — Okno admina

Dodany moduł centralnego panelu administratora.

## Nowa strona

`/AdminPanel`

## Dostęp

Panel jest dostępny tylko dla użytkownika z rolą `Administrator`.

## Co zawiera

- Akceptacja kont użytkowników.
- Lista użytkowników.
- Log aktywności.
- Kampanie.
- Kalendarz wydarzeń.
- Misje i fabuły.
- Kategorie forów, fora i wątki.
- Wszystkie postacie.
- Zatwierdzanie umiejętności.
- Ranking postaci.
- Zasady konfliktów.
- Szybkie akcje administracyjne.

## Pliki dodane/zmienione

- `ProjectPBF/Controllers/AdminPanelController.cs`
- `ProjectPBF/Views/AdminPanel/Index.cshtml`
- `ProjectPBF/Views/Shared/_Layout.cshtml`
- `ProjectPBF/Controllers/HomeController.cs`
- `ProjectPBF/Views/Home/AdminPanel.cshtml`

## Baza danych

Ten moduł nie wymaga migracji, bo nie dodaje nowych tabel.
