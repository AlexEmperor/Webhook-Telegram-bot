using DevelopmentLaboratoryBotWebhook;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
builder.Services.AddSingleton(new TelegramBotClient(token!));
builder.Services.AddSingleton<MessageHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
*/
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// ====== Настройка webhook ======
var bot = app.Services.GetRequiredService<TelegramBotClient>();
var url = Environment.GetEnvironmentVariable("WEBHOOK_URL"); // типа https://yourdomain.com/api/telegram
await bot.SetWebhook(url);
var info = await bot.GetWebhookInfo();
Console.WriteLine($"Webhook URL: {info.Url}");
Console.WriteLine($"Last Error: {info.LastErrorMessage}");

app.Run();
