using System.Text.Json.Serialization;

namespace FoxMield.Components.Models
{
    public class Card
    {
        public string Name { get; set; } = "Ass Whuppin'";
        public Guid Id { get; set; } = Guid.Parse("fbbf33d6-e00d-4d60-9c7f-c56c3b1f84c0");
        public string ManaCost { get; set; } = "{1}{W}{B}";
        public double Cmc { get; set; } = 3;
        public string TypeLine { get; set; } = "Sorcery";
        public string Color { get; set; } = "WB"; //Racism
        public string ColorIdentity { get; set; } = "WB";
        public string[] Keywords { get; set; } = [];
        public string? Power { get; set; } = null;
        public string? Toughness { get; set; } = null;
        public string? Loyalty { get; set; } = null;
        public string Rarity { get; set; } = "";
        public bool Legality { get; set; } = false;
        

        //This part is entirely display purposes
        public string DisplayColorIdentity
        {
            get
            {
                return string.Concat(ColorIdentity) switch
                {
                    // Colorless
                    "" => "Colorless",

                    // Mono-color
                    "W" => "White",
                    "U" => "Blue",
                    "B" => "Black",
                    "R" => "Red",
                    "G" => "Green",

                    // Two-color (Guilds)
                    "WU" => "Azorius",
                    "WB" => "Orzhov",
                    "WR" => "Boros",
                    "WG" => "Selesnya",
                    "UB" => "Dimir",
                    "UR" => "Izzet",
                    "UG" => "Simic",
                    "BR" => "Rakdos",
                    "BG" => "Golgari",
                    "RG" => "Gruul",

                    // Three-color (Shards)
                    "WUG" => "Bant",
                    "WUB" => "Esper",
                    "UBR" => "Grixis",
                    "BRG" => "Jund",
                    "WRG" => "Naya",

                    // Three-color (Wedges)
                    "WBG" => "Abzan",
                    "WUR" => "Jeskai",
                    "UBG" => "Sultai",
                    "WBR" => "Mardu",
                    "URG" => "Temur",

                    // Four-color
                    "WUBR" => "Yore-Tiller",
                    "UBRG" => "Glint-Eye",
                    "WBRG" => "Dune-Brood",
                    "WURG" => "Ink-Treader",
                    "WUBG" => "Witch-Maw",

                    // Five-color
                    "WUBRG" => "Five-Color",

                    _ => "Unknown"
                };
            }
        }
    }
}
