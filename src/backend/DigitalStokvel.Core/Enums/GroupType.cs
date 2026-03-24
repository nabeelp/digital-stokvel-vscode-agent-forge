namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Types of stokvel groups
/// </summary>
public enum GroupType
{
    /// <summary>
    /// Members receive rotating payouts in turn
    /// </summary>
    RotatingPayout,
    
    /// <summary>
    /// All contributions accumulate for year-end disbursement
    /// </summary>
    SavingsPot,
    
    /// <summary>
    /// Group invests pooled funds (Phase 2 feature)
    /// </summary>
    InvestmentClub
}
