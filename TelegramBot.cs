using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Helga.Function;

public sealed class TelegramBot(ITelegramBotClient botClient, PrivatBankClient pbClient, DataStorage dataStorage, ILogger<TelegramBot> logger)
{
    private const string HelpMessage = "Пиши:\nКурс або /rate - щоб подивитися курс\nСтан або /state - дізнатися поточний стан\n/add - додати оплату";

    [Function(SetupBot.UpdateFunctionName)]
    public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        logger.LogInformation("C# trigger function processed a request.");

        long chatId = 0;
        try
        {
            var body = await request.ReadAsStringAsync()
                ?? throw new ArgumentNullException(nameof(request));

            var update = JsonSerializer.Deserialize<Update>(body, JsonBotAPI.Options);
            if (update is null)
            {
                logger.LogWarning("Unable to deserialize Update object.");
                return;
            }

            chatId = update.Message.Chat.Id;

            if (update.Message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                await botClient.SendMessage(chatId, $"👋 Привіт Іринка або Роман!\n\n{HelpMessage}");
            }
            else if (update.Message.Text.Equals("/rate", StringComparison.OrdinalIgnoreCase) || update.Message.Text.Contains("курс", StringComparison.OrdinalIgnoreCase))
            {
                await HandleRateCommand(chatId);
            }
            else if (update.Message.Text.Equals("/state", StringComparison.OrdinalIgnoreCase) || update.Message.Text.Contains("стан", StringComparison.OrdinalIgnoreCase))
            {
                await HandleStateCommand(chatId);
            }
            else if (update.Message.Text.Equals("/add", StringComparison.OrdinalIgnoreCase))
            {
                await HandleAddCommand(chatId);
            }
            else
            {
                await botClient.SendMessage(chatId, $"🙈 Невідома команда! {HelpMessage}");
            }
        }
        catch(Exception e)
        {
            logger.LogError("Exception: {Message}", e.Message);
            if (chatId != 0)
            {
                await botClient.SendMessage(chatId, $"😵 Error: {e.Message}");
            }
        }
    }

    private async Task HandleAddCommand(long chatId)
    {
        var rate = await pbClient.GetRate();
        await dataStorage.AddPayment(rate.BuyPrice);
        var message = dataStorage.GetPayments();

        await botClient.SendMessage(chatId, $"💲 Оплата по курсу {rate.BuyPrice}\n\n {message}");
    }

    private async Task HandleStateCommand(long chatId)
    {
        var message = dataStorage.GetPayments();
        await botClient.SendMessage(chatId, message);
    }

    private async Task HandleRateCommand(long chatId)
    {
        var rate = await pbClient.GetRate();
        StringBuilder sb = new($"{rate.State} Курс: {rate.BuyPrice} на {rate.Date} \n\nЗ вас {2877.3f * rate.BuyPrice} грн = 2 877.3$ * {rate.BuyPrice}");
        sb.AppendLine(Environment.NewLine);
        sb.AppendLine("📅 Курс за тиждень:");
        var rates = await pbClient.GetRates();
        foreach (var r in rates)
        {
            sb.AppendLine($"{r.Date} Курс: {r.BuyPrice}");
        }

        await botClient.SendMessage(chatId, sb.ToString());
    }
}