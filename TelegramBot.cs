using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Helga.Function;

public sealed class TelegramBot
{
    private readonly ILogger<TelegramBot> _logger;
    private readonly PrivatbankClient _pbClient;
    private readonly ITelegramBotClient _botClient;

    public TelegramBot(PrivatbankClient pbClient, ITelegramBotClient botClient, ILogger<TelegramBot> logger)
    {
        _pbClient = pbClient;
        _botClient = botClient;
        _logger = logger;
    }

    [Function(SetupBot.UpdateFunctionName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        _logger.LogInformation("C# trigger function processed a request.");

        try
        {
            var body = await request.ReadAsStringAsync()
                ?? throw new ArgumentNullException(nameof(request));

            var update = JsonSerializer.Deserialize<Update>(body, JsonBotAPI.Options);
            if (update is null)
            {
                _logger.LogWarning("Unable to deserialize Update object.");
                return;
            }

            var rate = await _pbClient.GetRate();
            await _botClient.SendMessage(chatId: update.Message.Chat.Id, text: $"Курс: {rate.BuyPrice} на {rate.OriginalDate.Date} \nЗ вас {2877.3 * rate.BuyPrice}грн = 2 877.3💲 * {rate.BuyPrice}");
        }
        catch(Exception e)
        {
            _logger.LogError("Exception: {Message}", e.Message);
        }
    }
}