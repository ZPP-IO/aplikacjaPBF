namespace ProjectPBF.Models.Enums
{
    // Typ powiadomienia generowanego dla uzytkownika.
    public enum NotificationType
    {
        NewReplyInThread = 0,   // ktos odpowiedzial w watku, w ktorym uzytkownik juz pisal
        NewPrivateMessage = 1   // otrzymano nowa wiadomosc prywatna
    }
}