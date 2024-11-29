using System.Text.Json;
using System.Text.Json.Serialization;

namespace Helga.Function;

public sealed class PrivatbankClient
{
    private readonly HttpClient _httpClient;

    public PrivatbankClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Rate> GetRate()
    {
        var response = await _httpClient.GetStringAsync("https://privatbank.ua/rates/get-archive?period=day&from_currency=UAH&to_currency=USD");
        var rates = JsonSerializer.Deserialize<List<Rate>>(response);
        return rates?.FirstOrDefault();
    }
}

public class Rate
{
    [JsonPropertyName("original_date")]
    public OriginalDate OriginalDate { get; set; }

    [JsonPropertyName("sellPrice")]
    public double SellPrice { get; set; }

    [JsonPropertyName("buyPrice")]
    public double BuyPrice { get; set; }
}
public class OriginalDate
{
    [JsonPropertyName("date")]
    public string Date { get; set; }
}