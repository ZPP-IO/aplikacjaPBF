namespace ProjectPBF.Models.Enums
{
    // Status rozpatrzenia zgloszenia przez administracje.
    public enum ReportStatus
    {
        New = 0,        // nowe, jeszcze nie rozpatrzone
        Reviewed = 1,   // rozpatrzone, podjeto dzialanie
        Dismissed = 2   // rozpatrzone, odrzucone jako nieuzasadnione
    }
}
