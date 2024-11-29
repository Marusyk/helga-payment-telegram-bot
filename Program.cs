using Helga.Function;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.Process)
     ?? throw new ArgumentException("Can not get token. Set token in environment setting");

var tableConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process)
     ?? throw new ArgumentException("Can not get connection string. Set token in environment setting");

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddHttpClient("tgclient")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(token, httpClient));

builder.Services.AddHttpClient<PrivatBankClient>();
builder.Services.AddSingleton(new DataStorage(tableConnectionString));

builder.Build().Run();
