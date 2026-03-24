using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for Dispute-specific operations
/// </summary>
public interface IDisputeRepository : IRepository<Dispute>
{
    Task<IEnumerable<Dispute>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dispute>> GetByStatusAsync(DisputeStatus status, CancellationToken cancellationToken = default);
    Task<Dispute?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dispute>> GetDisputesPendingEscalationAsync(CancellationToken cancellationToken = default);
    Task<DisputeMessage> AddMessageAsync(DisputeMessage message, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dispute>> GetOpenDisputesAsync(CancellationToken cancellationToken = default);
}
