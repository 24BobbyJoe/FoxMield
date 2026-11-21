namespace FoxMield.Components.Models
{
    public class CardAnalysis
    {
        public Guid CardId { get; set; }

        public string CardName { get; set; } = "";

        public List<string> Roles { get; set; } = new();

        public List<string> Mechanics { get; set; } = new();

        public bool IsGameChanger { get; set; }

        public bool IsWinCondition { get; set; }

        public bool IsComboPiece { get; set; }

        public List<string> Reasons { get; set; } = new();
    }
}
