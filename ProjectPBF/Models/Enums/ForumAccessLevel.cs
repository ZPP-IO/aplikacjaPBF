using System;

namespace ProjectPBF.Models.Enums
{
    public enum ForumAccessLevel
    {
        Public = 0,       // wszyscy mog¹ czytaæ
        PlayersOnly = 1,  // tylko zalogowani gracze
        GMsOnly = 2,      // tylko Mistrzowie Gry i Administratorzy
        Hidden = 3        // ukryty (np. tylko dla wybranych)
    }

    [Flags]
    public enum ThreadStatus
    {
        Open = 0,
        Closed = 1,
        Archived = 2,
        Pinned = 4
    }
}