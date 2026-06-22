# Poprawka: AvatarUrl + licznik punktów statystyk

## Co poprawiono

1. Błąd `String or binary data would be truncated` przy tworzeniu postaci.
   - Przyczyną było wklejenie avatara jako `data:image/jpeg;base64,...` zamiast zwykłego linku.
   - Formularz i kontroler blokują teraz base64 i pokazują komunikat.
   - Dodano migrację, która ustawia `CharacterModels.AvatarUrl` jako `nvarchar(max)`.

2. Formularz statystyk postaci.
   - Pokazuje `Rozdano X/Y PK`.
   - Pokazuje `Pozostało X PK`.
   - Ma przyciski `+` i `-` przy każdej statystyce.
   - Jeżeli zostały 2 punkty, a gracz wpisze 20, pole samo ustawi 2.
   - Przed zapisem kontroler jeszcze raz sprawdza, czy nie przekroczono puli.

## Po podmianie folderu wykonaj

W Package Manager Console:

```powershell
Update-Database
```

Jeżeli baza nadal ma za krótką kolumnę AvatarUrl, możesz awaryjnie wykonać SQL:

```sql
ALTER TABLE [dbo].[CharacterModels] ALTER COLUMN [AvatarUrl] nvarchar(max) NULL;
```

## Zmienione pliki

- `ProjectPBF/Controllers/CharacterModelsController.cs`
- `ProjectPBF/ViewModels/CharacterCreateInCampaignViewModel.cs`
- `ProjectPBF/Views/CharacterModels/CreateForCampaign.cshtml`
- `ProjectPBF/Migrations/20260622193000_ExpandCharacterAvatarUrl.cs`
