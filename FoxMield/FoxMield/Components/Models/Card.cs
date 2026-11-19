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
        public string Rarity { get; set; } = "rare";
        public bool Legality { get; set; } = false;
        public string? OracleText { get; set; } = "Destroy target silver-bordered permanent in any game you can see from your seat.\r\n";
        public Dictionary<string, Uri> ImageUris { get; set; } = new()
        {
            ["small"] = new Uri("https://cards.scryfall.io/small/front/0/9/09ceffd6-2fe3-4d8b-93f2-291b31f83668.jpg"),
            ["normal"] = new Uri("https://cards.scryfall.io/normal/front/0/9/09ceffd6-2fe3-4d8b-93f2-291b31f83668.jpg"),
            ["large"] = new Uri("https://cards.scryfall.io/large/front/0/9/09ceffd6-2fe3-4d8b-93f2-291b31f83668.jpg"),
            ["png"] = new Uri("https://cards.scryfall.io/png/front/0/9/09ceffd6-2fe3-4d8b-93f2-291b31f83668.png"),
            ["art_crop"] = new Uri("https://cards.scryfall.io/art_crop/front/0/9/09ceffd6-2fe3-4d8b-93f2-291b31f83668.jpg"),
            ["border_crop"] = new Uri("https://cards.scryfall.io/border_crop/front/0/9/09ceffd6-2fe3-4d8b-93f2-291b31f83668.jpg")
        };
        

        //This part is entirely display purposes
        public string DisplayColorIdentity
        {
            get
            {
                string orderedColors = new string(
                    "WUBRG".Where(color => ColorIdentity.Contains(color)).ToArray()
                );

                return orderedColors switch
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