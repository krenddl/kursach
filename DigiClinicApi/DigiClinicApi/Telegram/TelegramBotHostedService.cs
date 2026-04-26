namespace DigiClinicApi.Telegram
{
    public class TelegramBotHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TelegramBotHostedService> _logger;

        public TelegramBotHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<TelegramBotHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            long offset = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var client = scope.ServiceProvider.GetRequiredService<TelegramApiClient>();

                    if (!client.IsConfigured)
                        return;

                    var botService = scope.ServiceProvider.GetRequiredService<TelegramBotService>();
                    var updates = await client.GetUpdates(offset, stoppingToken);

                    foreach (var update in updates)
                    {
                        offset = update.UpdateId + 1;
                        await botService.HandleUpdate(update, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Telegram polling failed");
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }
            }
        }
    }
}
