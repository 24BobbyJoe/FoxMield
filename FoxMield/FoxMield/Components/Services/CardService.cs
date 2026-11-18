using FoxMield.Components.Models;

namespace FoxMield.Components.Services
{
    public class CardService
    {
        public readonly List<Card> Cards;

        public CardService()
        {
            Cards = new List<Card>();
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public void DeleteCard(string name)
        {
            Card? card = Cards.FirstOrDefault(g => g.Name == name);

            if (card != null)
            {
                Cards.Remove(card);
            }
        }

        public Card GetCard(string name)
        {
            return Cards.FirstOrDefault(c => c.Name == name);
        }
    }
}
