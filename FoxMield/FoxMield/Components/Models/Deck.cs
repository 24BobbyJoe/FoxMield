namespace FoxMield.Components.Models
{
    public class Deck
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";

        public DateTimeOffset ImportedAt { get; set; } =
            DateTimeOffset.UtcNow;

        public List<DeckEntry> Cards { get; set; } = new();

        public int CommanderCount =>
            Cards.Where(c => c.Section == "Commander")
                 .Sum(c => c.Quantity);

        public int MainDeckCount =>
            Cards.Where(c => c.Section == "Main Deck")
                 .Sum(c => c.Quantity);

        public int TotalCards =>
            CommanderCount + MainDeckCount;

        public int UniqueCards => Cards.Count;

        public bool HasCorrectCardCount =>
            TotalCards == 100;
    }

    public class DeckEntry
    {
        public string Name { get; set; } = "";

        public int Quantity { get; set; } = 1;

        public string Section { get; set; } = "Main Deck";
    }
}