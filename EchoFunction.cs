using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Helga.Function;

public class EchoFunction(PrivatBankClient pbClient)
{
    [Function("Echo")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request)
    {
        var rate = await pbClient.GetRate();
        return new OkObjectResult($"Welcome to Azure Functions! Rate is {rate.BuyPrice:F2} on {rate.Date}\nSo, {2877.3m * rate.BuyPrice} UAH = 2 877.3 USD * {rate.BuyPrice:F2}");
    }
}