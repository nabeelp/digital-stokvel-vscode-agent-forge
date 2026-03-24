namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Message in dispute conversation thread (GG-07)
/// </summary>
public class DisputeMessage : BaseEntity
{
    public Guid DisputeId { get; set; }
    public Guid SenderId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Dispute Dispute { get; set; } = null!;
}
