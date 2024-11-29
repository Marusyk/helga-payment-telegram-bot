using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Helga.Function;

public class EchoFunction
{
    private readonly PrivatbankClient _pbClient;

    public EchoFunction(PrivatbankClient pbClient)
    {
        _pbClient = pbClient;
    }

    [Function("Echo")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var rate = await _pbClient.GetRate();
        return new OkObjectResult($"Welcome to Azure Functions! Rate is {rate.BuyPrice} on {rate.OriginalDate.Date}\nSo, {2877.3 * rate.BuyPrice} UAH = 2 877.3 USD * {rate.BuyPrice}");
    }
}