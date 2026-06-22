# Kamil — biblioteka kampanii GM

Dodano moduł, w którym po utworzeniu kampanii właściciel kampanii / admin kampanii może tworzyć elementy mechaniki przypisane do konkretnej kampanii:

- klasy postaci,
- przedmioty / itemy,
- umiejętności, techniki i zaklęcia.

## Nowe adresy

- `/CampaignLibrary/Index?campaignId=ID_KAMPANII`
- `/CampaignLibrary/CreateClass?campaignId=ID_KAMPANII`
- `/CampaignLibrary/CreateItem?campaignId=ID_KAMPANII`
- `/CampaignLibrary/CreateSkill?campaignId=ID_KAMPANII`

Na stronie szczegółów kampanii dodano panel „Biblioteka kampanii” z licznikami i przyciskami.

## Tworzenie postaci

Jeżeli kampania ma stworzone klasy, formularz tworzenia postaci pokazuje wybór klasy z tej kampanii. Klasa zapisuje się w `CharacterModels.CampaignClassId` i jest widoczna na karcie postaci.

## Migracja

Po podmianie projektu uruchom w Package Manager Console:

```powershell
Update-Database
```

Migracja dodana w paczce:

```txt
20260622210000_AddCampaignLibraryTemplates
```

## Pliki dodane

```txt
Models/CampaignClassModel.cs
Models/CampaignItemTemplateModel.cs
Models/CampaignSkillTemplateModel.cs
Models/Enums/CampaignItemType.cs
Models/Enums/CampaignSkillTemplateKind.cs
Controllers/CampaignLibraryController.cs
Views/CampaignLibrary/Index.cshtml
Views/CampaignLibrary/CreateClass.cshtml
Views/CampaignLibrary/CreateItem.cshtml
Views/CampaignLibrary/CreateSkill.cshtml
Migrations/20260622210000_AddCampaignLibraryTemplates.cs
Migrations/20260622210000_AddCampaignLibraryTemplates.Designer.cs
```

## Pliki zmienione

```txt
Models/CampaignModel.cs
Models/CharacterModel.cs
Data/ApplicationDbContext.cs
ViewModels/CharacterCreateInCampaignViewModel.cs
Controllers/CampaignsController.cs
Controllers/CharacterModelsController.cs
Views/Campaigns/Details.cshtml
Views/CharacterModels/CreateForCampaign.cshtml
Views/CharacterModels/Details.cshtml
```
