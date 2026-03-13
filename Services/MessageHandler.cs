using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevelopmentLaboratoryBotWebhook
{
    public class MessageHandler
    {
        // ======= Словари для хранения состояния пользователей (для формы и калькулятора) =======
        private Dictionary<long, string> userStates = [];
        private Dictionary<long, (string Name, string Email, string TaskDescription)> formData = [];
        private readonly Dictionary<string, Func<CallbackQuery, Task>> callbackHandlers;
        private TelegramBotClient bot;
        private HashSet<long> locationSent = [];

        public MessageHandler(TelegramBotClient client)
        {
            bot = client;

            callbackHandlers = new()
            {
                ["devices"] = HandleProjects,
                ["services"] = HandleServices,
                ["contacts"] = HandleContacts,
                ["news"] = HandleNews,
                ["main_menu"] = HandleMainMenu,
                ["write_to_human"] = HandleWriteToHuman,
                ["online_form"] = HandleOnlineForm
            };
        }

        // =================== Обработка сообщений ===================
        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.Text == null)
            {
                return;
            }

            var chatId = msg.Chat.Id;
            // Любое другое сообщение — сбрасываем флаг геолокации
            locationSent.Remove(chatId);
            // ======== Онлайн-заявка ========
            if (userStates.ContainsKey(chatId) && userStates[chatId].StartsWith("form_"))
            {
                switch (userStates[chatId])
                {
                    case "form_name":
                        formData[chatId] = (msg.Text, "", "");
                        userStates[chatId] = "form_email";
                        await bot.SendMessage(chatId, "Введите ваш E-mail:");
                        break;
                    case "form_email":
                        var oldForm = formData[chatId];

                        formData[chatId] = (oldForm.Name, msg.Text, "");
                        userStates[chatId] = "form_task";
                        await bot.SendMessage(chatId, "Коротко опишите задачу:");
                        break;
                    case "form_task":
                        {
                            var data = formData[chatId];

                            // Обновляем данные формы
                            formData[chatId] = (data.Name, data.Email, msg.Text);

                            // ===== Telegram данные пользователя =====
                            var telegramId = msg.From!.Id;
                            var username = msg.From.Username;
                            var firstName = msg.From.FirstName ?? "";
                            var lastName = msg.From.LastName ?? "";

                            var usernameText = username != null ? "@" + username : "не указан";
                            var profileLink = username != null
                                ? $"https://t.me/{username}"
                                : $"tg://user?id={telegramId}";

                            var adminChatId = -5123579887; // твой ID

                            await bot.SendMessage(
                                adminChatId,
                                $"📩 <b>Новая заявка</b>\n\n" +
                                $"👤 Имя: {data.Name}\n" +
                                $"📝 Описание заявки: {msg.Text}\n\n" +
                                $"📞 Контакты:\n" +
                                $"📧 Email: {data.Email}\n" +
                                $"📱 Telegram:\n" +
                                $"Username: {usernameText}\n" +
                                $"ID: {telegramId}\n" +
                                $"👤 Профиль: {profileLink}\n\n" +
                                $"👤 Клиент: {firstName} {lastName}",
                                parseMode: ParseMode.Html
                            );

                            await bot.SendMessage(chatId, "✅ Заявка отправлена! Спасибо!");

                            await bot.SendMessage(
                                chatId,
                                "Вернуться в меню:",
                                replyMarkup: ButtonHandler.ReturnKeyboard()
                            );

                            userStates.Remove(chatId);
                            formData.Remove(chatId);

                            break;
                        }

                }
                return;
            }
            // ======== Стартовое сообщение ========
            if (msg.Text.StartsWith("/start"))
            {
                await bot.SendMessage(
                    chatId,
                    "👋 Привет!\nНа связи лаборатория разработки систем беспроводной связи 📡\n\nЧем могу помочь?",
                    replyMarkup: ButtonHandler.MainMenuKeyboard()
                );
            }
        }

        // =================== Callback ===================

        public async Task OnUpdate(Update update)
        {
            if (update.CallbackQuery == null)
            {
                return;
            }

            var query = update.CallbackQuery;

            await bot.AnswerCallbackQuery(query.Id);

            if (callbackHandlers.TryGetValue(query.Data!, out var handler))
            {
                await handler(query);
            }
        }

        // =================== Методы ===================

        private async Task HandleMainMenu(CallbackQuery query)
        {
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Чем могу помочь?",
                replyMarkup: ButtonHandler.MainMenuKeyboard()
            );
        }

        private async Task HandleProjects(CallbackQuery query)
        {
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text:
                "🧪 Наши устройства:\n\n" +

                "1️⃣ АК «SDR»\n" +
                "Устройство сканирования частотного спектра с возможностью указания на карте источников сигналов.\n\n" +

                "2️⃣ Аппаратный модуль VVizor\n" +
                "Устройство фиксирования видео передатчиков.\n\n" +

                "3️⃣ МЭМС\n" +
                "Устройство позиционирования.\n\n" +

                "4️⃣ Плата ретранслятора\n" +
                "Система автоматизированного тестирования выпускаемых изделий.\n\n" +

                "5️⃣ Радиосканер\n" +
                "Двухканальное устройство сканирования частотного спектра.\n\n" +

                "6️⃣ Bluetooth-маяки\n\n" +
                "7️⃣ Радиомаяки",

                replyMarkup: ButtonHandler.ReturnKeyboard()
            );
        }

        private async Task HandleServices(CallbackQuery query)
        {
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text:
                "⚙️ Услуги:\n\n" +

                "• Разработка ПО под заказ\n\n" +
                "• Создание прототипов устройств\n\n" +
                "• Интеграция оборудования\n\n" +
                "• Консультационные услуги\n\n" +
                "• Поставка готовых изделий\n\n" +
                "• Гарантийное обслуживание\n\n" +
                "• Сопровождение проектов",

                replyMarkup: ButtonHandler.ReturnKeyboard()
            );
        }

        private async Task HandleNews(CallbackQuery query)
        {
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text:
                "📰 Новости лаборатории:\n\n" +

                "1️⃣ Проведена проверка изделия AK SDR в полевых условиях на полигоне в г. Калуга.\n\n" +

                "2️⃣ Завершена отладка устройства VVizor.\n\n" +

                "3️⃣ Выпущена новая партия модулей МЭМС.",

                replyMarkup: ButtonHandler.ReturnKeyboard()
            );
        }

        private async Task HandleContacts(CallbackQuery query)
        {
            var chatId = query.Message!.Chat.Id;

            await bot.EditMessageText(
                chatId: chatId,
                messageId: query.Message.MessageId,
                text:
                "📞 Контакты:\n\n" +

                "Телефон: +7 (977) 488-90-30\n" +
                "E-mail: electriks0comp26@gmail.com\n" +
                "Telegram: @YgorGrupStar\n\n" +

                "📍 Адрес:\n" +
                "125183, г. Москва\n" +
                "Проспект Черепановых, д. 54\n\n" +

                "Контактное лицо:\n" +
                "Тарасов Игорь Анатольевич\n\n" +

                "Время работы:\n" +
                "Пн-Пт: 9:00 – 18:00",

                replyMarkup: ButtonHandler.ReturnKeyboard()
            );

            // отправка карты

            if (!locationSent.Contains(chatId))
            {
                await bot.SendLocation(chatId, 55.843475, 37.537694);
                locationSent.Add(chatId);
            }
        }

        private async Task HandleWriteToHuman(CallbackQuery query)
        {
            await bot.EditMessageText(
                chatId: query.Message!.Chat.Id,
                messageId: query.Message.MessageId,
                text: "💬 Переход к специалисту:\nhttps://t.me/YgorGrupStar",
                replyMarkup: ButtonHandler.ReturnKeyboard()
            );
        }

        private async Task HandleOnlineForm(CallbackQuery query)
        {
            userStates[query.Message!.Chat.Id] = "form_name";

            await bot.EditMessageText(
                chatId: query.Message.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Введите ваше имя:",
                replyMarkup: ButtonHandler.ReturnKeyboard()
            );
        }

    }
}
