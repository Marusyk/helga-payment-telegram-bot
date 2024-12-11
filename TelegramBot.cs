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
    private const string HelpMessage = "Пиши:\nКурс або /rate - щоб подивитися курс\nСтан або /state - дізнатися поточний стан\n/adda - додати оплату по квартирі\n/addp - додати оплату по паркомісцю";

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
            else if (update.Message.Text.Equals("/adda", StringComparison.OrdinalIgnoreCase))
            {
                await HandleAddApartmentCommand(chatId);
            }
            else if (update.Message.Text.Equals("/addp", StringComparison.OrdinalIgnoreCase))
            {
                await HandleAddParkingCommand(chatId);
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

    private async Task HandleAddApartmentCommand(long chatId)
    {
        await botClient.SendMessage(chatId, $"Вибач, це поки не працює");
        // var rate = await pbClient.GetRate();
        // await dataStorage.AddPayment(rate.BuyPrice);
        // var message = dataStorage.GetPayments();

        // await botClient.SendMessage(chatId, $"💲 Оплата по курсу {rate.BuyPrice}\n\n {message}");
    }

    private async Task HandleAddParkingCommand(long chatId)
    {
        await botClient.SendMessage(chatId, $"Вибач, це поки не працює");
        // var rate = await pbClient.GetRate();
        // await dataStorage.AddPayment(rate.BuyPrice);
        // var message = dataStorage.GetPayments();

        // await botClient.SendMessage(chatId, $"💲 Оплата по курсу {rate.BuyPrice}\n\n {message}");
    }

    private async Task HandleStateCommand(long chatId)
    {
        var message = dataStorage.GetPayments();
        await botClient.SendMessage(chatId, message);
    }

    private async Task HandleRateCommand(long chatId)
    {
        var apartmentMonthly = 2210.40m;
        var parkingMonthly = 666.90m;
        var total = apartmentMonthly + parkingMonthly; // 2 877.3$

        var rate = await pbClient.GetRate();
        StringBuilder sb = new($"{rate.State} Курс: {rate.BuyPrice:F2} на {rate.Date} \n\nЗ вас {total}$ * {rate.BuyPrice:F2} = {total * rate.BuyPrice:F2} грн");
        sb.AppendLine(Environment.NewLine);
        sb.AppendLine($"Квартира: {apartmentMonthly}$ * {rate.BuyPrice:F2} = {apartmentMonthly * rate.BuyPrice:F2}");
        sb.AppendLine($"Паркомісце: {parkingMonthly}$ * {rate.BuyPrice:F2} = {parkingMonthly * rate.BuyPrice:F2}");
        sb.AppendLine(Environment.NewLine);
        sb.AppendLine("📅 Курс за тиждень:");
        var rates = await pbClient.GetRates();
        foreach (var r in rates)
        {
            sb.AppendLine($"{r.Date} Курс: {r.BuyPrice:F2}");
        }

        await botClient.SendMessage(chatId, sb.ToString());
    }
}