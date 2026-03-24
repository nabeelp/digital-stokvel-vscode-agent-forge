namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Monthly interest capitalization transaction (GW-03)
/// Tracks daily interest calculations compounded monthly
/// </summary>
public class InterestTransaction : BaseEntity
{
    public Guid GroupId { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal AverageBalance { get; set; }
    public decimal InterestRate { get; set; } // Annual rate as decimal (e.g., 0.045 = 4.5%)
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int DaysInPeriod { get; set; }
    
    // Navigation properties
    public Group Group { get; set; } = null!;
}
