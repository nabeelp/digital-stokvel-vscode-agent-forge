using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Infrastructure.Integrations;

/// <summary>
/// Mock Core Banking Client for MVP (per PRD Section 7.3)
/// </summary>
public class CoreBankingClient : ICoreBankingClient
{
    private readonly ILogger<CoreBankingClient> _logger;

    public CoreBankingClient(ILogger<CoreBankingClient> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateGroupSavingsAccountAsync(string groupName, string chairpersonUserId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating group savings account for {GroupName}", groupName);
        await Task.Delay(100, cancellationToken); // Simulate API call
        var accountNumber = $"GSA{Random.Shared.Next(100000, 999999)}";
        _logger.LogInformation("Group savings account created: {AccountNumber}", accountNumber);
        return accountNumber;
    }

    public async Task<bool> ExecutePaymentAsync(string fromAccount, string toAccount, decimal amount, string reference, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing payment: {Amount} from {FromAccount} to {ToAccount}", amount, fromAccount, toAccount);
        await Task.Delay(50, cancellationToken); // Simulate API call
        _logger.LogInformation("Payment executed successfully");
        return true; // Mock success
    }

    public async Task<bool> ExecuteEFTAsync(string toAccount, decimal amount, string reference, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing EFT: {Amount} to {ToAccount}", amount, toAccount);
        await Task.Delay(50, cancellationToken); // Simulate API call
        _logger.LogInformation("EFT executed successfully");
        return true; // Mock success
    }

    public async Task<decimal> GetAccountBalanceAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting account balance for {AccountNumber}", accountNumber);
        await Task.Delay(50, cancellationToken); // Simulate API call
        return 5000m; // Mock balance
    }

    public async Task<bool> SetupDebitOrderAsync(string accountNumber, decimal amount, int dayOfMonth, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting up debit order: {Amount} on day {DayOfMonth}", amount, dayOfMonth);
        await Task.Delay(50, cancellationToken); // Simulate API call
        return true; // Mock success
    }

    public async Task<bool> CancelDebitOrderAsync(string accountNumber, string debitOrderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling debit order: {DebitOrderId}", debitOrderId);
        await Task.Delay(50, cancellationToken); // Simulate API call
        return true; // Mock success
    }
}
