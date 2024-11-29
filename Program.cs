using Helga.Function;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.Process)
     ?? throw new ArgumentException("Can not get token. Set token in environment setting");

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddHttpClient("tgclient")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(token, httpClient));

builder.Services.AddHttpClient<PrivatbankClient>();

builder.Build().Run();
