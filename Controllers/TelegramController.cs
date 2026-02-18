using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;


namespace DevelopmentLaboratoryBotWebhook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        private readonly TelegramBotClient _bot;
        private readonly MessageHandler _messageHandler;

        public TelegramController(TelegramBotClient bot, MessageHandler messageHandler)
        {
            _bot = bot;
            _messageHandler = messageHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update.Message != null)
            {
                await _messageHandler.OnMessage(update.Message, update.Type);
            }

            if (update.CallbackQuery != null)
            {
                await _messageHandler.OnUpdate(update);
            }

            return Ok();
        }
    }
}
