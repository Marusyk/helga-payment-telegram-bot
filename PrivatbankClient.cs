using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace Helga.Function;

public sealed class PrivatBankClient(HttpClient httpClient)
{
    public async Task<(string Date, decimal BuyPrice, string State)> GetRate()
    {
        var response = await httpClient.GetFromJsonAsync<IEnumerable<Rate>>("https://privatbank.ua/rates/get-archive?period=day&from_currency=UAH&to_currency=USD")
            ?? throw new Exception("Can not get response from Privatbank API");

        const string dateFormat = "yyyy-MM-dd HH:mm:ss.ffffff";
        DateTime dateTime = DateTime.Now;

        var rateForToday = response.FirstOrDefault(x => x.OriginalDate.Date == dateTime.Date.ToString(dateFormat))
            ?? throw new Exception($"Can not get rate for {dateTime.Date}");

        var rateForYesterday = response.FirstOrDefault(x => x.OriginalDate.Date == dateTime.AddDays(-1).Date.ToString(dateFormat));

        string trend = rateForToday.BuyPrice > rateForYesterday?.BuyPrice ? "⬆️" : rateForToday.BuyPrice < rateForYesterday?.BuyPrice ? "⬇️" : "➡️";
        return (rateForToday.OriginalDate.Date[..^7], rateForToday.BuyPrice, trend);
    }

    public async Task<IEnumerable<(string Date, decimal BuyPrice)>> GetRates()
    {
        var response = await httpClient.GetFromJsonAsync<IEnumerable<Rate>>("https://privatbank.ua/rates/get-archive?period=week&from_currency=UAH&to_currency=USD")
            ?? throw new Exception("Can not get response from Privatbank API");

        return response.Select(x => (x.OriginalDate.Date[..^7], x.BuyPrice));
    }
}

public class Rate
{
    [JsonPropertyName("original_date")]
    public OriginalDate OriginalDate { get; set; }

    [JsonPropertyName("buyPrice")]
    public decimal BuyPrice { get; set; }
}
public class OriginalDate
{
    [JsonPropertyName("date")]
    public string Date { get; set; }
}