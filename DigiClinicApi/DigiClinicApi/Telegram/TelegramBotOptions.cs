namespace DigiClinicApi.Telegram
{
    public class TelegramBotOptions
    {
        public bool Enabled { get; set; }
        public string BotToken { get; set; } = string.Empty;
        public int PollingTimeoutSeconds { get; set; } = 25;
    }
}
