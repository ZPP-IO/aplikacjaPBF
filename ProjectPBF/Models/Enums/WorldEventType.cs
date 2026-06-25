namespace ProjectPBF.Models.Enums
{
    // Typ wydarzenia swiatowego/historycznego w ramach kampanii.
    public enum WorldEventType
    {
        War = 0,            // wojna / konflikt zbrojny
        SpecialMission = 1, // misja specjalna ogloszona przez MG
        Disaster = 2,       // kleska, kataklizm
        PoliticalChange = 3,// zmiana wladzy, edykt, traktat
        Other = 4
    }
}