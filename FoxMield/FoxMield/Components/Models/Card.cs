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
    }
}