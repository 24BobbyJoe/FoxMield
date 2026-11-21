namespace FoxMield.Components.Models
{
    public class DeckAnalysis
    {
        public Guid DeckId { get; set; }

        public string DeckName { get; set; } = "";

        public List<CardAnalysis> Cards { get; set; } = new();

        public Dictionary<string, int> RoleCounts { get; set; } = new();

        public Dictionary<string, int> MechanicCounts { get; set; } = new();

        public int TotalCardsAnalyzed => Cards.Count;
    }
}