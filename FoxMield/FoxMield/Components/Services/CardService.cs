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

        public void DeleteCard(int id)
        {
            Card? card = Cards.FirstOrDefault(g => g.Id == id);

            if (card != null)
            {
                Cards.Remove(card);
            }
        }

        public Card GetCard(int Id)
        {
            return Cards.FirstOrDefault(c => c.Id == Id);
        }
    }
}
