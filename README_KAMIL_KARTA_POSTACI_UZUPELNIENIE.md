# Kamil — uzupełnienie: karta postaci, zgłoszenia skilli, ekwipunek, rozwój

Ta wersja uzupełnia trzy wymagane punkty:

1. Formularz zgłaszania umiejętności/techniki postaci do zatwierdzenia przez MG.
2. System tworzenia kart postaci: statystyki, umiejętności, ekwipunek.
3. Mechanika rozwoju postaci: doświadczenie, rankingi, PH/Punkty Historii i PK statystyk.

## Co dodano / poprawiono

### Umiejętności i techniki
- Gracz może wejść w kartę postaci i kliknąć `Zgłoś nową`.
- Formularz ma pola: nazwa, typ, koszt, cooldown/limit, koszt PH, wymagania, efekt mechaniczny, opis fabularny.
- Można wybrać wzorzec z biblioteki kampanii MG.
- Po wysłaniu skill ma status `Oczekuje`.
- MG/Admin akceptuje lub odrzuca zgłoszenie w `/CharacterSkills/Pending`.

### Karta postaci
- Na stronie szczegółów postaci są sekcje:
  - dane główne,
  - kampanie,
  - statystyki kampanii,
  - umiejętności/techniki z komentarzem MG,
  - ekwipunek,
  - historia rozwoju.
- Przy statystykach można wydawać wolne PK.
- Ekwipunek może być dodany ręcznie albo z biblioteki kampanii.

### Rozwój postaci
- MG/właściciel kampanii może przyznać:
  - EXP,
  - PH,
  - ręczne PK statystyk.
- Poziom nadal liczy się co 100 EXP.
- Awans automatycznie dodaje PK według ustawienia kampanii `Przyrost PK na poziom`.
- Ranking działa globalnie albo z filtrem po kampanii.

## Po podmianie plików

W Visual Studio uruchom:

```powershell
Update-Database
```

Nie rób `Add-Migration`, bo migracja jest już dodana:

```txt
20260622223000_CompleteCharacterCardMechanics.cs
```

## Test po uruchomieniu

1. MG tworzy kampanię.
2. MG dodaje w bibliotece kampanii klasy, itemy i skille.
3. Gracz dołącza do kampanii.
4. Gracz tworzy postać z wybranej kampanii.
5. Na karcie postaci gracz zgłasza skill/technikę.
6. MG wchodzi w `/CharacterSkills/Pending` i zatwierdza albo odrzuca.
7. MG przyznaje EXP/PH/PK przez `Przyznaj EXP / PH / PK`.
8. Ranking jest dostępny pod `/Ranking`.
