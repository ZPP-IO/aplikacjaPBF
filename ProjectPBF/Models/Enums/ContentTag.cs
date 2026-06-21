using System;

namespace ProjectPBF.Models.Enums
{
    // Oznaczenia treści wątków i postów. Flags - można łączyć (np. Spoiler | Important).
    [Flags]
    public enum ContentTag
    {
        None = 0,
        Spoiler = 1,
        Important = 2,
        OOC = 4,        // Out of character / poza fabułą
        GMOnly = 8       // informacja przeznaczona głównie dla MG
    }
}