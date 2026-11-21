
using FoxMield.Components.Models;

namespace FoxMield.Components.Services
{
    public class CardAnalysisService
    {
        public CardAnalysis AnalyzeCard(Card card)
        {
            CardAnalysis analysis = new CardAnalysis
            {
                CardId = card.Id,
                CardName = card.Name
            };

            string oracle = card.OracleText?.ToLowerInvariant() ?? "";
            string type = card.TypeLine.ToLowerInvariant();

            // Ramp
            if (oracle.Contains("add") && oracle.Contains("mana"))
            {
                analysis.Roles.Add("Ramp");
                analysis.Reasons.Add("Produces mana.");
            }

            // Card draw
            if (oracle.Contains("draw"))
            {
                analysis.Roles.Add("Card Draw");
                analysis.Reasons.Add("Contains a card draw effect.");
            }

            // Tutors
            if (oracle.Contains("search your library"))
            {
                analysis.Roles.Add("Tutor");
                analysis.Reasons.Add("Searches the library for a card.");
            }


            // Removal
            if (oracle.Contains("destroy") ||
                oracle.Contains("exile") ||
                oracle.Contains("deals") && oracle.Contains("damage"))
            {
                analysis.Roles.Add("Removal");
                analysis.Reasons.Add("Contains an effect that can remove or damage something.");
            }


            // Board wipes
            if (oracle.Contains("destroy all") ||
                oracle.Contains("exile all"))
            {
                analysis.Roles.Add("Board Wipe");
                analysis.Reasons.Add("Can remove multiple permanents.");
            }

            // Reanimation
            if (oracle.Contains("return") &&
                oracle.Contains("graveyard") &&
                (oracle.Contains("battlefield") || oracle.Contains("hand")))
            {
                analysis.Roles.Add("Reanimation");
                analysis.Reasons.Add("Moves cards from a graveyard to another zone.");
            }

            // Keywords and mechanics
            foreach (string keyword in card.Keywords)
            {
                if (keyword.Equals("Cascade", StringComparison.OrdinalIgnoreCase))
                {
                    analysis.Mechanics.Add("Cascade");
                }

                if (keyword.Equals("Storm", StringComparison.OrdinalIgnoreCase))
                {
                    analysis.Mechanics.Add("Storm");
                }

                if (keyword.Equals("Discover", StringComparison.OrdinalIgnoreCase))
                {
                    analysis.Mechanics.Add("Discover");
                }
            }

            // Creature identification
            if (type.Contains("creature"))
            {
                analysis.Roles.Add("Creature");
            }

            // Land identification
            if (type.Contains("land"))
            {
                analysis.Roles.Add("Land");
            }

            return analysis;
        }

        public List<CardAnalysis> AnalyzeDeck(List<Card> cards)
        {
            List<CardAnalysis> analyses = new();

            foreach (Card card in cards)
            {
                CardAnalysis analysis = AnalyzeCard(card);
                analyses.Add(analysis);
            }

            return analyses;
        }

        public DeckAnalysis CreateDeckAnalysis(
            Guid deckId,
            string deckName,
            List<Card> cards)
        {
            List<CardAnalysis> analyses = AnalyzeDeck(cards);

            FoxMield.Components.Models.DeckAnalysis result = new FoxMield.Components.Models.DeckAnalysis
            {
                DeckId = deckId,
                DeckName = deckName,
                Cards = analyses
            };

            foreach (CardAnalysis card in analyses)
            {
                foreach (string role in card.Roles)
                {
                    if (!result.RoleCounts.ContainsKey(role))
                    {
                        result.RoleCounts[role] = 0;
                    }

                    result.RoleCounts[role]++;
                }

                foreach (string mechanic in card.Mechanics)
                {
                    if (!result.MechanicCounts.ContainsKey(mechanic))
                    {
                        result.MechanicCounts[mechanic] = 0;
                    }

                    result.MechanicCounts[mechanic]++;
                }
            }

            return result;
        }

    }
}
    
