using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderApi.Infrastructure.Persistence;
using Shared.Events.IntegrationEvents;

namespace OrderApi.Infrastructure.Outbox;

/// <summary>
/// Outbox tablosundaki mesajları periyodik okuyup broker'a publish eder.
/// </summary>
public class OutboxPublisherHostedService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<OutboxPublisherHostedService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    public OutboxPublisherHostedService(IServiceProvider provider, ILogger<OutboxPublisherHostedService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publish cycle failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var pending = await db.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var msg in pending)
        {
            try
            {
                if (msg.MessageType == typeof(OrderPlacedEvent).FullName)
                {
                    var evt = JsonSerializer.Deserialize<OrderPlacedEvent>(msg.Payload);
                    if (evt != null)
                    {
                        await publishEndpoint.Publish(evt, cancellationToken);
                        msg.ProcessedAt = DateTime.UtcNow;
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Outbox message {Id} publish failed", msg.Id);
            }
        }
    }
}
