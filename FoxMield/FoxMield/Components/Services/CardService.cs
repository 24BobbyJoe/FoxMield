
using FoxMield.Components.Models;

namespace FoxMield.Components.Services
{
    public class CardService
    {
        private readonly ScryfallService _scryfall;

        public List<Card> Cards { get; } = new();

        public CardService(ScryfallService scryfall)
        {
            _scryfall = scryfall;
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public void DeleteCard(string name)
        {
            Card? card = Cards.FirstOrDefault(c => c.Name == name);

            if (card != null)
            {
                Cards.Remove(card);
            }
        }

        public Card? GetCard(string name)
        {
            return Cards.FirstOrDefault(c => c.Name == name);
        }

        public async Task ImportDeck(List<string> names)
        {
            List<Card> foundCards = await _scryfall.GetCardsByNames(names);

            Cards.AddRange(foundCards);
        }
    }
}
