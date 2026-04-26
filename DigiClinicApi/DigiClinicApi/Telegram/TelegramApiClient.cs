using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigiClinicApi.Telegram
{
    public class TelegramApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly TelegramBotOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public TelegramApiClient(HttpClient httpClient, IOptions<TelegramBotOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public bool IsConfigured =>
            _options.Enabled && !string.IsNullOrWhiteSpace(_options.BotToken);

        public async Task<IReadOnlyList<TelegramUpdate>> GetUpdates(long offset, CancellationToken cancellationToken)
        {
            if (!IsConfigured)
                return Array.Empty<TelegramUpdate>();

            var request = new
            {
                offset,
                timeout = Math.Clamp(_options.PollingTimeoutSeconds, 5, 50),
                allowed_updates = new[] { "message" }
            };

            var response = await _httpClient.PostAsJsonAsync(
                BuildMethodUrl("getUpdates"),
                request,
                _jsonOptions,
                cancellationToken);

            var payload = await response.Content.ReadFromJsonAsync<TelegramApiResponse<List<TelegramUpdate>>>(
                _jsonOptions,
                cancellationToken);

            return payload?.Ok == true && payload.Result != null
                ? payload.Result
                : Array.Empty<TelegramUpdate>();
        }

        public async Task SendMessage(long chatId, string text, CancellationToken cancellationToken)
        {
            if (!IsConfigured)
                return;

            var request = new SendMessageRequest
            {
                ChatId = chatId,
                Text = text
            };

            await _httpClient.PostAsJsonAsync(
                BuildMethodUrl("sendMessage"),
                request,
                _jsonOptions,
                cancellationToken);
        }

        private string BuildMethodUrl(string method) =>
            $"https://api.telegram.org/bot{_options.BotToken}/{method}";

        private sealed class SendMessageRequest
        {
            [JsonPropertyName("chat_id")]
            public long ChatId { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }
    }
}
