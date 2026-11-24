using System.Text;
using System.Text.Json;
using FoxMield.Components.Models;

namespace FoxMield.Components.Services
{
    public enum DeckExportFormat
    {
        PlainText,
        Csv,
        Json
    }

    public static class DeckExportFormatter
    {
        public static string Generate(
            Deck deck,
            DeckExportFormat format)
        {
            ArgumentNullException.ThrowIfNull(deck);

            return format switch
            {
                DeckExportFormat.PlainText => BuildPlainText(deck),
                DeckExportFormat.Csv => BuildCsv(deck),
                DeckExportFormat.Json => BuildJson(deck),
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
        }

        public static string GetExtension(DeckExportFormat format)
        {
            return format switch
            {
                DeckExportFormat.PlainText => ".txt",
                DeckExportFormat.Csv => ".csv",
                DeckExportFormat.Json => ".json",
                _ => ".txt"
            };
        }

        public static string GetMediaType(DeckExportFormat format)
        {
            return format switch
            {
                DeckExportFormat.PlainText => "text/plain",
                DeckExportFormat.Csv => "text/csv",
                DeckExportFormat.Json => "application/json",
                _ => "text/plain"
            };
        }

        public static string GetFileName(
            Deck deck,
            DeckExportFormat format)
        {
            string safeName = new string(
                deck.Name.Select(c =>
                    char.IsLetterOrDigit(c) ||
                    c == '-' ||
                    c == '_'
                        ? c
                        : '-').ToArray()
            ).Trim('-');

            if (string.IsNullOrWhiteSpace(safeName))
            {
                safeName = "deck";
            }

            return safeName + GetExtension(format);
        }

        private static List<DeckEntry> GetSortedCards(Deck deck)
        {
            return deck.Cards
                .Where(c =>
                    !string.IsNullOrWhiteSpace(c.Name) &&
                    c.Quantity > 0)
                .OrderBy(c => GetSectionOrder(c.Section))
                .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int GetSectionOrder(string section)
        {
            return section switch
            {
                "Commander" => 0,
                "Main Deck" => 1,
                "Sideboard" => 2,
                "Maybeboard" => 3,
                "Companion" => 4,
                _ => 5
            };
        }

        private static string BuildPlainText(Deck deck)
        {
            StringBuilder builder = new();

            foreach (var group in GetSortedCards(deck)
                .GroupBy(c => c.Section))
            {
                string heading = group.Key == "Main Deck"
                    ? "Deck"
                    : group.Key;

                builder.AppendLine(heading);

                foreach (DeckEntry card in group)
                {
                    builder.AppendLine(
                        $"{card.Quantity} {card.Name}");
                }

                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private static string BuildCsv(Deck deck)
        {
            StringBuilder builder = new();

            builder.AppendLine("Section,Quantity,Name");

            foreach (DeckEntry card in GetSortedCards(deck))
            {
                builder.AppendLine(
                    $"{EscapeCsv(card.Section)}," +
                    $"{card.Quantity}," +
                    $"{EscapeCsv(card.Name)}");
            }

            return builder.ToString().TrimEnd();
        }

        private static string EscapeCsv(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string BuildJson(Deck deck)
        {
            var data = new
            {
                deck.Name,
                deck.ImportedAt,
                Cards = GetSortedCards(deck)
                    .Select(c => new
                    {
                        c.Name,
                        c.Quantity,
                        c.Section
                    })
                    .ToList()
            };

            return JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }
    }
}