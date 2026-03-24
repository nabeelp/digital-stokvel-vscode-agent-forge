namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Schedule for group payouts
/// </summary>
public enum PayoutSchedule
{
    /// <summary>
    /// Members receive payouts in rotating order
    /// </summary>
    Rotating,
    
    /// <summary>
    /// All members receive proportional payout at year-end
    /// </summary>
    YearEnd
}
