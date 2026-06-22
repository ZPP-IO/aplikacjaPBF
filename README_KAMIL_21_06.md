# Moduł Kamil do wersji aktualnej 21.06

Ta paczka jest przygotowana pod projekt `aplikacjaPBF-aktualne-21.06-`.
Najważniejsza zmiana: tworzenie postaci jest dopięte do kampanii, czyli MG tworzy kampanię z własnym schematem statystyk, a gracz tworzy postać według tego schematu.

## 1. Gdzie wkleić pliki

Rozpakuj zawartość paczki do folderu:

```txt
aplikacjaPBF-aktualne-21.06-/ProjectPBF/
```

## 2. Pliki do DODANIA

```txt
Models/Enums/SkillKind.cs
Models/Enums/SkillStatus.cs
Models/CharacterSkillModel.cs
Models/InventoryItemModel.cs
Models/CharacterDevelopmentLogModel.cs
ViewModels/CharacterStatisticInputViewModel.cs
ViewModels/CharacterCreateInCampaignViewModel.cs
Controllers/CharacterSkillsController.cs
Controllers/CharacterProgressController.cs
Controllers/InventoryItemsController.cs
Controllers/RankingController.cs
Views/CharacterModels/CreateForCampaign.cshtml
Views/CharacterSkills/Create.cshtml
Views/CharacterSkills/Index.cshtml
Views/CharacterSkills/Pending.cshtml
Views/CharacterProgress/Award.cshtml
Views/InventoryItems/Create.cshtml
Views/Ranking/Index.cshtml
```

## 3. Pliki do NADPISANIA

```txt
Models/CharacterModel.cs
Data/ApplicationDbContext.cs
Controllers/CharacterModelsController.cs
Views/CharacterModels/Details.cshtml
Views/CharacterModels/Index.cshtml
Views/Campaigns/Details.cshtml
Views/Shared/_Layout.cshtml
```

## 4. Co dodaje moduł

- tworzenie postaci bezpośrednio z kampanii,
- dynamiczne statystyki postaci według schematu kampanii,
- kontrolę puli punktów ustawionej przez MG,
- EXP, PH i poziom postaci,
- historię rozwoju postaci,
- zgłaszanie umiejętności/technik do zatwierdzenia przez MG,
- panel zatwierdzania umiejętności,
- ekwipunek postaci,
- ranking postaci.

## 5. Migracja bazy danych

Po wklejeniu plików uruchom w Package Manager Console:

```powershell
Add-Migration AddCharacterProgressSkillsInventory
Update-Database
```

Albo w terminalu w folderze `ProjectPBF`:

```bash
dotnet ef migrations add AddCharacterProgressSkillsInventory
dotnet ef database update
```

## 6. Test po dodaniu

Sprawdź te adresy:

```txt
/Campaigns/Browse
/Campaigns/Details/1
/CharacterModels/Index
/Ranking
/CharacterSkills/Index
/CharacterSkills/Pending
```

Najważniejszy test:

1. MG tworzy kampanię i wpisuje statystyki, np. `Siła, Zręczność, Inteligencja, Mana`.
2. Wejdź w szczegóły kampanii.
3. Kliknij `Stwórz postać`.
4. Formularz postaci pokaże statystyki z tej konkretnej kampanii.
5. Po zapisie postać pojawi się w kampanii i na karcie postaci.
