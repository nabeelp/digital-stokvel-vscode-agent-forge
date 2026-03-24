namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Method used for contribution payment
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Payment via mobile app
    /// </summary>
    App,
    
    /// <summary>
    /// Payment via USSD (*120*STOKVEL#)
    /// </summary>
    USSD,
    
    /// <summary>
    /// Automatic monthly debit order
    /// </summary>
    DebitOrder
}
