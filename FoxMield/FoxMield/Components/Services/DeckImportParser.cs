using System.Text;
using System.Text.Json;
using FoxMield.Components.Models;

namespace FoxMield.Components.Services
{
    public class DeckImportResult
    {
        public string? DeckName { get; set; }

        public string Format { get; set; } = "";

        public List<DeckEntry> Cards { get; set; } = new();
    }

    public static class DeckImportParser
    {
        public static DeckImportResult Parse(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new FormatException(
                    "Please paste a deck or upload a file.");
            }

            string content = input.Trim().TrimStart('\uFEFF');

            if (content.StartsWith("{") || content.StartsWith("["))
            {
                return ParseJson(content);
            }

            if (IsCsv(content))
            {
                return ParseCsv(content);
            }

            return new DeckImportResult
            {
                Format = "Plain Text",
                Cards = DeckTextParser.Parse(content)
            };
        }

        private static bool IsCsv(string content)
        {
            string header = content
                .Split('\n', 2)[0]
                .Trim();

            header = new string(header
                .Where(c => c != '"' && !char.IsWhiteSpace(c))
                .ToArray());

            return header.Equals(
                "Section,Quantity,Name",
                StringComparison.OrdinalIgnoreCase);
        }

        private static DeckImportResult ParseJson(string content)
        {
            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(content);

                JsonElement root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException(
                        "The JSON must contain a deck object.");
                }

                if (!root.TryGetProperty("Cards", out JsonElement cards) ||
                    cards.ValueKind != JsonValueKind.Array)
                {
                    throw new FormatException(
                        "The JSON deck must contain a Cards array.");
                }

                string? deckName = null;

                if (root.TryGetProperty("Name", out JsonElement name) &&
                    name.ValueKind == JsonValueKind.String)
                {
                    deckName = name.GetString();
                }

                List<DeckEntry> entries = new();

                foreach (JsonElement item in cards.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object ||
                        !item.TryGetProperty("Name", out JsonElement cardName) ||
                        cardName.ValueKind != JsonValueKind.String ||
                        !item.TryGetProperty("Quantity", out JsonElement quantity) ||
                        quantity.ValueKind != JsonValueKind.Number ||
                        !quantity.TryGetInt32(out int amount) ||
                        !item.TryGetProperty("Section", out JsonElement section) ||
                        section.ValueKind != JsonValueKind.String)
                    {
                        throw new FormatException(
                            "A JSON card entry is missing valid fields.");
                    }

                    entries.Add(new DeckEntry
                    {
                        Name = cardName.GetString() ?? "",
                        Quantity = amount,
                        Section = NormalizeSection(
                            section.GetString() ?? "")
                    });
                }

                return new DeckImportResult
                {
                    DeckName = deckName,
                    Format = "JSON",
                    Cards = ValidateAndCombine(entries)
                };
            }
            catch (JsonException)
            {
                throw new FormatException(
                    "The JSON file contains invalid JSON.");
            }
        }

        private static DeckImportResult ParseCsv(string content)
        {
            List<string[]> rows = ReadCsvRows(content);

            if (rows.Count < 2)
            {
                throw new FormatException(
                    "The CSV must contain a header and at least one card.");
            }

            List<DeckEntry> entries = new();

            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];

                if (row.Length != 3)
                {
                    throw new FormatException(
                        $"Invalid CSV row {i + 1}.");
                }

                if (!int.TryParse(row[1], out int quantity))
                {
                    throw new FormatException(
                        $"Invalid quantity on CSV row {i + 1}.");
                }

                entries.Add(new DeckEntry
                {
                    Section = NormalizeSection(row[0]),
                    Quantity = quantity,
                    Name = row[2].Trim()
                });
            }

            return new DeckImportResult
            {
                Format = "CSV",
                Cards = ValidateAndCombine(entries)
            };
        }

        private static List<string[]> ReadCsvRows(string content)
        {
            List<string[]> rows = new();
            List<string> columns = new();
            StringBuilder field = new();

            bool inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                char character = content[i];

                if (character == '"')
                {
                    if (inQuotes &&
                        i + 1 < content.Length &&
                        content[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (character == ',' && !inQuotes)
                {
                    columns.Add(field.ToString());
                    field.Clear();
                }
                else if ((character == '\n' || character == '\r') &&
                         !inQuotes)
                {
                    if (character == '\r' &&
                        i + 1 < content.Length &&
                        content[i + 1] == '\n')
                    {
                        i++;
                    }

                    columns.Add(field.ToString());
                    field.Clear();

                    if (columns.Any(c => !string.IsNullOrWhiteSpace(c)))
                    {
                        rows.Add(columns.ToArray());
                    }

                    columns.Clear();
                }
                else
                {
                    field.Append(character);
                }
            }

            if (inQuotes)
            {
                throw new FormatException(
                    "The CSV contains an unclosed quotation mark.");
            }

            columns.Add(field.ToString());

            if (columns.Any(c => !string.IsNullOrWhiteSpace(c)))
            {
                rows.Add(columns.ToArray());
            }

            return rows;
        }

        private static string NormalizeSection(string section)
        {
            return section.Trim().ToLowerInvariant() switch
            {
                "commander" or "commanders" => "Commander",
                "deck" or "main deck" or "main" or "mainboard"
                    => "Main Deck",
                "sideboard" => "Sideboard",
                "maybeboard" or "considering" => "Maybeboard",
                "companion" => "Companion",
                _ => throw new FormatException(
                    $"Unknown deck section: {section}")
            };
        }

        private static List<DeckEntry> ValidateAndCombine(
            List<DeckEntry> entries)
        {
            List<DeckEntry> result = new();

            foreach (DeckEntry entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    throw new FormatException(
                        "A card entry has no name.");
                }

                if (entry.Quantity <= 0)
                {
                    throw new FormatException(
                        $"Invalid quantity for {entry.Name}.");
                }

                DeckEntry? existing = result.FirstOrDefault(c =>
                    c.Name.Equals(
                        entry.Name.Trim(),
                        StringComparison.OrdinalIgnoreCase) &&
                    c.Section == entry.Section);

                if (existing != null)
                {
                    existing.Quantity = checked(
                        existing.Quantity + entry.Quantity);
                }
                else
                {
                    result.Add(new DeckEntry
                    {
                        Name = entry.Name.Trim(),
                        Quantity = entry.Quantity,
                        Section = entry.Section
                    });
                }
            }

            if (result.Count == 0)
            {
                throw new FormatException(
                    "No cards were found in the imported deck.");
            }

            return result;
        }
    }
}