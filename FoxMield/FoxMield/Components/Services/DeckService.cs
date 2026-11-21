
using FoxMield.App_Data;
using FoxMield.Components.Models;
using MongoDB.Driver;

namespace FoxMield.Components.Services
{
    public class DeckService
    {
        private readonly MongoDbContext _database;

        public DeckService(MongoDbContext database)
        {
            _database = database;
        }

        public async Task<List<Deck>> GetAllDecksAsync(string userId)
        {
            return await _database.Decks
                .Find(d => d.UserId == userId)
                .SortByDescending(d => d.ImportedAt)
                .ToListAsync();
        }

        public async Task<Deck?> GetDeckAsync(Guid id, string userId)
        {
            return await _database.Decks
                .Find(d => d.Id == id && d.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<Deck> ImportDeckAsync(
            string name,
            string deckText,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new FormatException(
                    "Please enter a name for your deck.");
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "You must be logged in to import a deck.");
            }

            List<DeckEntry> cards = DeckTextParser.Parse(deckText);

            Deck deck = new Deck
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name.Trim(),
                ImportedAt = DateTimeOffset.UtcNow,
                Cards = cards
            };

            await _database.Decks.InsertOneAsync(deck);

            return deck;
        }
    }
}
