## EF Core Migrations

This solution uses:

* **Startup project:** `OrbWeaver.Host`
* **Data / migrations project:** `OrbWeaver.Data`

### Create a migration for a specific DbContext

```bash
dotnet ef migrations add <MigratioName> \
         --context OrbWeaverDbContext \
         --project ./src/OrbWeaver.Data/OrbWeaver.Data.csproj \
         --startup-project ./src/OrbWeaver.Host/OrbWeaver.Host.csproj
```

### Apply migrations to the database

```bash
dotnet ef database update --project ./src/OrbWeaver.Data/OrbWeaver.Data.csproj --connection "<CONNECTION_STRING>"
```

### Prerequisites

Ensure EF Core CLI tools are installed:

```bash
dotnet tool restore
```

## Telegram Bot

The application includes a Telegram bot listener that responds to commands.

### Setup

1. Configure your Telegram bot token in `appsettings.json`:
```json
{
  "Telegram": {
    "BotToken": "your-bot-token-here",
    "ChatId": "your-chat-id-here"
  }
}
```

2. To get your chat ID, start the bot and send `/start` command. The bot will respond with your chat ID.

### Features

- **`/start` command**: Returns your chat ID that can be used in the configuration for receiving notifications.


