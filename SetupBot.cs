using System.Globalization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Telegram.Bot;

namespace Helga.Function;

public sealed class SetupBot
{
    private readonly ITelegramBotClient _botClient;

    public SetupBot(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    private const string SetUpFunctionName = "setup-7379561817";
    public const string UpdateFunctionName = "handleupdate";

    [Function(SetUpFunctionName)]
    public async Task RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData request)
    {
        var handleUpdateFunctionUrl = request.Url.ToString().Replace(SetUpFunctionName, UpdateFunctionName, ignoreCase: true, culture: CultureInfo.InvariantCulture);
        await _botClient.SetWebhook(handleUpdateFunctionUrl);
    }
}