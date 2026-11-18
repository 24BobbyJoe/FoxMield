using FoxMield.Components.Models;
using System.Net.Http.Json;
using System.Text.Json;

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
                    string imageUrl = "";

                    if (item.TryGetProperty("image_uris", out var images))
                    {
                        imageUrl = images.TryGetProperty("normal", out var image)
                            ? image.GetString() ?? ""
                            : "";
                    }
                    else if (item.TryGetProperty("card_faces", out var faces))
                    {
                        var firstFace = faces[0];

                        if (firstFace.TryGetProperty("image_uris", out var faceImages))
                        {
                            imageUrl = faceImages.TryGetProperty("normal", out var image)
                                ? image.GetString() ?? ""
                                : "";
                        }
                    }

                    string power = "";
                    string toughness = "";

                    if (item.TryGetProperty("power", out var powerValue))
                    {
                        power = powerValue.GetString() ?? "";
                    }

                    if (item.TryGetProperty("toughness", out var toughnessValue))
                    {
                        toughness = toughnessValue.GetString() ?? "";
                    }

                    string colors = string.Join(", ",
                        item.GetProperty("color_identity")
                            .EnumerateArray()
                            .Select(color => color.GetString())
                    );

                    string price = "";

                    if (item.TryGetProperty("prices", out var prices) &&
                        prices.TryGetProperty("usd", out var usd))
                    {
                        price = usd.GetString() ?? "";
                    }

                    cards.Add(new Card
                    {
                        Name = item.GetProperty("name").GetString() ?? "",
                        ManaCost = item.TryGetProperty("mana_cost", out var mana)
                            ? mana.GetString() ?? ""
                            : "",
                        TypeLine = item.GetProperty("type_line").GetString() ?? "",
                        OracleText = item.TryGetProperty("oracle_text", out var oracle)
                            ? oracle.GetString() ?? ""
                            : "",
                        Power = power,
                        Toughness = toughness,
                        ColorIdentity = colors
                    });
                }

                if (i + 75 < identifiers.Count)
                {
                    await Task.Delay(100);
                }
            }

            return cards;
        }
    }
}


