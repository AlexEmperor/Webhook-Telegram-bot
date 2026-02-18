# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем csproj и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем весь код и билдим релиз
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Используем облегчённый образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Прописываем переменные окружения для бота
# Их можно переопределять через docker run или docker-compose
ENV BOT_TOKEN=YOUR_BOT_TOKEN
ENV WEBHOOK_URL=https://yourdomain.com/api/telegram
ENV ASPNETCORE_URLS=http://+:80;https://+:443

# Экспонируем HTTP и HTTPS порты
EXPOSE 80
EXPOSE 443

# Запуск приложения
ENTRYPOINT ["dotnet", "DevelopmentLaboratoryBotWebhook.dll"]
