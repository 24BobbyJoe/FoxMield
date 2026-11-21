
using System.Text.Json;
using System.Net.Http.Json;
using FoxMield.Components.Models;

namespace FoxMield.Components.Services
{
    public class ScryfallService
    {
        private readonly HttpClient _http;

        public ScryfallService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Card>> GetCardsByNames(List<string> names)
        {
            var cards = new List<Card>();

            var identifiers = names
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => new { name = name.Trim() })
                .ToList();

            if (identifiers.Count == 0)
            {
                return cards;
            }

            for (int i = 0; i < identifiers.Count; i += 75)
            {
                var batch = identifiers.Skip(i).Take(75).ToList();

                var response = await _http.PostAsJsonAsync(
                    "cards/collection",
                    new { identifiers = batch }
                );

                response.EnsureSuccessStatusCode();

                using var document = JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync()
                );

                var data = document.RootElement.GetProperty("data");

                foreach (var item in data.EnumerateArray())
                {
                    var card = new Card
                    {
                        Id = item.GetProperty("id").GetGuid(),

                        Name = item.GetProperty("name").GetString() ?? "",

                        ManaCost = GetString(item, "mana_cost"),

                        Cmc = item.GetProperty("cmc").GetDouble(),

                        TypeLine = GetString(item, "type_line"),


                        Color = item.TryGetProperty("colors", out var colors)
                            && colors.ValueKind == JsonValueKind.Array
                                ? string.Join("",
                                    colors.EnumerateArray()
                                        .Select(color => color.GetString()))
                                : "",


                        ColorIdentity = string.Join("",
                            item.GetProperty("color_identity")
                                .EnumerateArray()
                                .Select(color => color.GetString())
                        ),

                        Keywords = item.GetProperty("keywords")
                            .EnumerateArray()
                            .Select(keyword => keyword.GetString() ?? "")
                            .ToArray(),

                        Power = GetNullableString(item, "power"),

                        Toughness = GetNullableString(item, "toughness"),

                        Loyalty = GetNullableString(item, "loyalty"),

                        Rarity = GetString(item, "rarity"),

                        OracleText = GetNullableString(item, "oracle_text"),

                        ImageUris = GetImageUris(item),

                        Legality = item.GetProperty("legalities")
                            .GetProperty("commander")
                            .GetString() == "legal"
                    };

                    cards.Add(card);
                }

                if (i + 75 < identifiers.Count)
                {
                    await Task.Delay(100);
                }
            }

            return cards;
        }

        private static string GetString(JsonElement item, string property)
        {
            return item.TryGetProperty(property, out var value)
                && value.ValueKind == JsonValueKind.String
                    ? value.GetString() ?? ""
                    : "";
        }

        private static string? GetNullableString(JsonElement item, string property)
        {
            return item.TryGetProperty(property, out var value)
                && value.ValueKind == JsonValueKind.String
                    ? value.GetString()
                    : null;
        }

        private static Dictionary<string, Uri> GetImageUris(JsonElement item)
        {
            var images = new Dictionary<string, Uri>();

            if (item.TryGetProperty("image_uris", out var imageUris))
            {
                foreach (var image in imageUris.EnumerateObject())
                {
                    if (image.Value.ValueKind == JsonValueKind.String &&
                        Uri.TryCreate(image.Value.GetString(), UriKind.Absolute, out var uri))
                    {
                        images[image.Name] = uri;
                    }
                }
            }
            else if (item.TryGetProperty("card_faces", out var faces) &&
                     faces.GetArrayLength() > 0)
            {
                var firstFace = faces[0];

                if (firstFace.TryGetProperty("image_uris", out var faceImages))
                {
                    foreach (var image in faceImages.EnumerateObject())
                    {
                        if (image.Value.ValueKind == JsonValueKind.String &&
                            Uri.TryCreate(image.Value.GetString(), UriKind.Absolute, out var uri))
                        {
                            images[image.Name] = uri;
                        }
                    }
                }
            }

            return images;
        }
    }
}
