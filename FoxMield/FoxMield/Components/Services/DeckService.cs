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

        public async Task<bool> SaveDeckChangesAsync(
            Guid deckId,
            string userId,
            List<DeckEntry> updatedCards)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "You must be logged in to edit a deck.");
            }

            if (updatedCards == null)
            {
                throw new ArgumentException(
                    "Deck card data cannot be null.");
            }

            string[] allowedSections =
            {
                "Commander",
                "Main Deck",
                "Sideboard",
                "Maybeboard",
                "Companion"
            };

            foreach (DeckEntry card in updatedCards)
            {
                if (string.IsNullOrWhiteSpace(card.Name))
                {
                    throw new ArgumentException(
                        "Every card must have a name.");
                }

                if (card.Quantity < 1)
                {
                    throw new ArgumentException(
                        "Card quantities must be at least 1.");
                }

                if (!allowedSections.Contains(card.Section))
                {
                    throw new ArgumentException(
                        $"Invalid deck section: {card.Section}");
                }

                if (card.Section == "Commander" &&
                    card.Quantity != 1)
                {
                    throw new ArgumentException(
                        "Each commander must have exactly one copy.");
                }
            }

            int commanderCount = updatedCards
                .Where(c => c.Section == "Commander")
                .Sum(c => c.Quantity);

            if (commanderCount > 2)
            {
                throw new ArgumentException(
                    "A deck cannot have more than two commanders.");
            }

            List<DeckEntry> cleanedCards = updatedCards
                .GroupBy(
                    c => (Name: c.Name.Trim().ToUpperInvariant(),
                          c.Section))
                .Select(group => new DeckEntry
                {
                    Name = group.First().Name.Trim(),
                    Quantity = group.Sum(c => c.Quantity),
                    Section = group.Key.Section
                })
                .ToList();

            var filter = Builders<Deck>.Filter.And(
                Builders<Deck>.Filter.Eq(d => d.Id, deckId),
                Builders<Deck>.Filter.Eq(d => d.UserId, userId)
            );

            var update = Builders<Deck>.Update
                .Set(d => d.Cards, cleanedCards);

            var result = await _database.Decks
                .UpdateOneAsync(filter, update);

            return result.MatchedCount == 1;
        }
    }
}