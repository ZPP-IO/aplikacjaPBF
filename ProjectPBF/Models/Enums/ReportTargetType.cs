namespace ProjectPBF.Models.Enums
{
    // Typ obiektu, ktory jest przedmiotem zgloszenia do administracji.
    public enum ReportTargetType
    {
        ForumPost = 0,      // pojedynczy post na forum
        ForumThread = 1,    // caly watek (np. tytul / zalozenie watku)
        PrivateMessage = 2  // wiadomosc prywatna
    }
}
