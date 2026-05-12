System PBF - gotowy projekt

Błąd: Invalid object name 'CampaignMembers' oznacza, że baza nie ma tabel z nowego modelu.
W tej paczce usunąłem stare migracje, bo były pomieszane.

Po otwarciu projektu:
1. Tools -> NuGet Package Manager -> Package Manager Console
2. Wpisz:
   Add-Migration InitSystemPbf
   Update-Database

Jeśli masz starą bazę i nie chcesz jej usuwać, zmień nazwę bazy w appsettings.json w ConnectionStrings:DefaultConnection.
Np. Database=aspnet-SystemPBF-GamerStyle

Konto admin:
admin@local.test
Admin123!

Konto test:
test@test.test
Test123!

Zmiany:
- nazwa: System PBF
- usunięty Rynek z menu
- kampanie / sesje
- StartingPoints, np. 20 punktów do rozdania
- statystyki zależne od kampanii
- dodatkowy opis postaci w kampanii
- front fantasy/gaming
