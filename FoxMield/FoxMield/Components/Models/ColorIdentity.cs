namespace FoxMield.Components.Models
{
    public enum ColorIdentity
    {
        Colorless,

        // Mono-color
        White,
        Blue,
        Black,
        Red,
        Green,

        // Two-color
        Azorius,    // WU
        Orzhov,     // WB
        Boros,      // WR
        Selesnya,   // WG
        Dimir,      // UB
        Izzet,      // UR
        Simic,      // UG
        Rakdos,     // BR
        Golgari,    // BG
        Gruul,      // RG

        // Three-color
        Bant,       // WUG
        Esper,      // WUB
        Grixis,     // UBR
        Jund,       // BRG
        Naya,       // WRG

        Abzan,      // WBG
        Jeskai,     // WUR
        Sultai,     // UBG
        Mardu,      // WBR
        Temur,      // URG

        // Four-color
        YoreTiller,    // WUBR
        GlintEye,      // UBRG
        DuneBrood,     // WBRG
        InkTreader,    // WURG
        WitchMaw,      // WUBG

        // Five-color
        FiveColor      // WUBRG
    }
}
