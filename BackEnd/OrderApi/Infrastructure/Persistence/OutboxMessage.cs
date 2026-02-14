namespace OrderApi.Infrastructure.Persistence;

/// <summary>
/// Transactional Outbox: Aynı transaction'da iş ve mesaj kaydı; arka planda broker'a gönderilir.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
