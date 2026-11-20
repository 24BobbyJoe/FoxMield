using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using FoxMield.Components.Models;

namespace FoxMield.Components.Services
{
    public class DeckService
    {
        private readonly object _lock = new();

        private readonly string _filePath;

        private readonly List<Deck> _decks;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public DeckService(IWebHostEnvironment environment)
        {
            _filePath = Path.Combine(
                environment.ContentRootPath,
                "App_Data",
                "decks.json"
            );

            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);

                _decks = JsonSerializer.Deserialize<List<Deck>>(json)
                    ?? new List<Deck>();
            }
            else
            {
                _decks = new List<Deck>();
            }
        }

        public List<Deck> GetAllDecks()
        {
            lock (_lock)
            {
                return _decks
                    .OrderByDescending(d => d.ImportedAt)
                    .Select(CopyDeck)
                    .ToList();
            }
        }

        public Deck? GetDeck(Guid id)
        {
            lock (_lock)
            {
                Deck? deck = _decks.FirstOrDefault(d => d.Id == id);

                return deck == null ? null : CopyDeck(deck);
            }
        }

        public Deck ImportDeck(string name, string deckText)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new FormatException(
                    "Please enter a name for your deck.");
            }

            List<DeckEntry> cards = DeckTextParser.Parse(deckText);

            Deck deck = new Deck
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                ImportedAt = DateTimeOffset.UtcNow,
                Cards = cards
            };

            lock (_lock)
            {
                _decks.Add(deck);

                try
                {
                    SaveDecks();
                }
                catch
                {
                    _decks.Remove(deck);
                    throw;
                }
            }

            return CopyDeck(deck);
        }

        private void SaveDecks()
        {
            string directory = Path.GetDirectoryName(_filePath)!;

            Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(
                _decks,
                JsonOptions
            );

            string temporaryFile = _filePath + ".tmp";

            try
            {
                File.WriteAllText(temporaryFile, json);

                File.Move(
                    temporaryFile,
                    _filePath,
                    overwrite: true
                );
            }
            finally
            {
                if (File.Exists(temporaryFile))
                {
                    File.Delete(temporaryFile);
                }
            }
        }

        private static Deck CopyDeck(Deck original)
        {
            return new Deck
            {
                Id = original.Id,
                Name = original.Name,
                ImportedAt = original.ImportedAt,

                Cards = original.Cards.Select(c => new DeckEntry
                {
                    Name = c.Name,
                    Quantity = c.Quantity,
                    Section = c.Section
                }).ToList()
            };
        }
    }
}