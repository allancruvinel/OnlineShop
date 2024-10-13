using Payment.Services.Interfaces;

namespace Payment;

public class PaymentWorker(ILogger<PaymentWorker> _logger,IRabbitMQService _rabbitMQService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        try
        {
            _rabbitMQService.RecebedorDeMensagem<Order.Order>();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocorreu um erro "+ ex.ToString());
        }
        await Task.CompletedTask;
    }
}
