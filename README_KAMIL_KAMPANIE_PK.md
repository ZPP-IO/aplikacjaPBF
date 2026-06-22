# Poprawka Kamil: wybór kampanii + PK na poziom

Dodane/zmienione:

1. Przy `Moje postacie -> Stwórz nową postać` najpierw wybiera się kampanię.
2. Jeśli gracz nie jest w kampanii, widzi przycisk `Dołącz i twórz postać`.
3. Formularz postaci pobiera statystyki z wybranej kampanii.
4. Kampania ma nową opcję `Przyrost PK na poziom`.
5. Postać ma `Wolne PK statystyk`.
6. Przy awansie poziomu system automatycznie dodaje wolne PK według ustawienia kampanii.
7. MG / właściciel kampanii może ręcznie dodać albo odjąć PK w panelu `Przyznaj EXP / PH / PK`.
8. Na karcie postaci można wydawać wolne PK przyciskiem `+1 PK` przy statystyce.

Po podmianie plików uruchom:

```powershell
Update-Database
```

Jeśli migracja nie zaskoczy, awaryjnie wykonaj SQL:

```sql
ALTER TABLE [dbo].[CampaignModels] ADD [StatisticPointsPerLevel] int NOT NULL DEFAULT 5;
ALTER TABLE [dbo].[CharacterModels] ADD [AvailableStatisticPoints] int NOT NULL DEFAULT 0;
ALTER TABLE [dbo].[CharacterDevelopmentLogs] ADD [StatisticPointsChange] int NOT NULL DEFAULT 0;
```
