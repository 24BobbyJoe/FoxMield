using System.Text.RegularExpressions;
using FoxMield.Components.Models;

namespace FoxMield.Components.Services
{
    public static class DeckTextParser
    {
        private static readonly Regex CardLine = new(
            @"^(?<quantity>\d+)\s*x?\s+(?<name>.+)$",
            RegexOptions.IgnoreCase);

        private static readonly Regex SetInformation = new(
            @"^(?<name>.+?)\s+\([A-Za-z0-9]{2,12}\)\s+\S+$");

        private static readonly Dictionary<string, string> Sections = new(
            StringComparer.OrdinalIgnoreCase)
        {
            ["Commander"] = "Commander",
            ["Commanders"] = "Commander",
            ["Deck"] = "Main Deck",
            ["Main"] = "Main Deck",
            ["Mainboard"] = "Main Deck",
            ["Main Deck"] = "Main Deck",
            ["Sideboard"] = "Sideboard",
            ["Maybeboard"] = "Maybeboard",
            ["Considering"] = "Maybeboard",
            ["Companion"] = "Companion"
        };

        public static List<DeckEntry> Parse(string? deckText)
        {
            if (string.IsNullOrWhiteSpace(deckText))
            {
                throw new FormatException(
                    "Please paste a deck list before importing.");
            }

            List<DeckEntry> cards = new();

            string currentSection = "Main Deck";

            string[] lines = deckText.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string header = line
                    .TrimStart('/', '#', ' ')
                    .TrimEnd(':')
                    .Trim();

                header = Regex.Replace(
                    header,
                    @"\s*\(\d+\)$",
                    "");

                if (Sections.TryGetValue(
                    header, out string? section))
                {
                    currentSection = section;
                    continue;
                }

                if (line.StartsWith("//") ||
                    line.StartsWith("#"))
                {
                    continue;
                }

                if (line.StartsWith(
                    "SB:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "Sideboard";
                    line = line[3..].Trim();
                }

                int quantity = 1;
                string cardName = line;

                Match match = CardLine.Match(line);

                if (match.Success)
                {
                    quantity = int.Parse(
                        match.Groups["quantity"].Value);

                    cardName = match.Groups["name"].Value.Trim();
                }

                if (quantity <= 0)
                {
                    throw new FormatException(
                        $"Invalid quantity on line {i + 1}.");
                }

                cardName = Regex.Replace(
                    cardName,
                    @"\s+(?:#\S+\s*)+$",
                    "").Trim();

                cardName = Regex.Replace(
                    cardName,
                    @"\s+\*(F|E)\*\s*$",
                    "",
                    RegexOptions.IgnoreCase).Trim();

                Match setMatch = SetInformation.Match(cardName);

                if (setMatch.Success)
                {
                    cardName = setMatch.Groups["name"].Value.Trim();
                }

                if (string.IsNullOrWhiteSpace(cardName))
                {
                    throw new FormatException(
                        $"Missing card name on line {i + 1}.");
                }

                DeckEntry? existing = cards.FirstOrDefault(c =>
                    c.Name.Equals(
                        cardName,
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    c.Section.Equals(
                        currentSection,
                        StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Quantity += quantity;
                }
                else
                {
                    cards.Add(new DeckEntry
                    {
                        Name = cardName,
                        Quantity = quantity,
                        Section = currentSection
                    });
                }
            }

            if (cards.Count == 0)
            {
                throw new FormatException(
                    "No cards were found in the deck list.");
            }

            return cards;
        }
    }
}