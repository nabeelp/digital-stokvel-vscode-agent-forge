using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using Moq;

namespace DigitalStokvel.Tests.Fixtures;

/// <summary>
/// Provides pre-configured Mock objects for all repository interfaces
/// </summary>
public class MockRepositoryFixture
{
    public Mock<IUserRepository> UserRepository { get; }
    public Mock<IGroupRepository> GroupRepository { get; }
    public Mock<IMemberRepository> MemberRepository { get; }
    public Mock<IContributionRepository> ContributionRepository { get; }
    public Mock<IPayoutRepository> PayoutRepository { get; }
    public Mock<IRefreshTokenRepository> RefreshTokenRepository { get; }
    public Mock<IAuditLogRepository> AuditLogRepository { get; }
    public Mock<ILedgerRepository> LedgerRepository { get; }
    public Mock<IVoteRepository> VoteRepository { get; }
    public Mock<IDisputeRepository> DisputeRepository { get; }
    public Mock<IUnitOfWork> UnitOfWork { get; }
    public Mock<INotificationService> NotificationService { get; }
    public Mock<IFraudDetectionService> FraudDetectionService { get; }
    public Mock<ICoreBankingClient> CoreBankingClient { get; }
    public Mock<IWalletService> WalletService { get; }

    public MockRepositoryFixture()
    {
        UserRepository = new Mock<IUserRepository>();
        GroupRepository = new Mock<IGroupRepository>();
        MemberRepository = new Mock<IMemberRepository>();
        ContributionRepository = new Mock<IContributionRepository>();
        PayoutRepository = new Mock<IPayoutRepository>();
        RefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        AuditLogRepository = new Mock<IAuditLogRepository>();
        LedgerRepository = new Mock<ILedgerRepository>();
        VoteRepository = new Mock<IVoteRepository>();
        DisputeRepository = new Mock<IDisputeRepository>();
        UnitOfWork = new Mock<IUnitOfWork>();
        NotificationService = new Mock<INotificationService>();
        FraudDetectionService = new Mock<IFraudDetectionService>();
        CoreBankingClient = new Mock<ICoreBankingClient>();
        WalletService = new Mock<IWalletService>();

        // Set up UnitOfWork to return mock repositories
        UnitOfWork.Setup(u => u.Groups).Returns(GroupRepository.Object);
        UnitOfWork.Setup(u => u.Members).Returns(MemberRepository.Object);
        UnitOfWork.Setup(u => u.Contributions).Returns(ContributionRepository.Object);
        UnitOfWork.Setup(u => u.Payouts).Returns(PayoutRepository.Object);
        UnitOfWork.Setup(u => u.Ledger).Returns(LedgerRepository.Object);
        UnitOfWork.Setup(u => u.Votes).Returns(VoteRepository.Object);
        UnitOfWork.Setup(u => u.Disputes).Returns(DisputeRepository.Object);
        UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Set up common repository operations that return created entities
        RefreshTokenRepository.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken token) => token);
        AuditLogRepository.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync((AuditLog log) => log);
        LedgerRepository.Setup(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Sets up UserRepository to return a user that exists
    /// </summary>
    public void SetupUserExists(User user)
    {
        UserRepository.Setup(r => r.GetByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        UserRepository.Setup(r => r.GetByPhoneNumberAsync(user.PhoneNumber)).ReturnsAsync(user);
        UserRepository.Setup(r => r.PhoneNumberExistsAsync(user.PhoneNumber)).ReturnsAsync(true);
        UserRepository.Setup(r => r.IdNumberExistsAsync(user.IdNumber)).ReturnsAsync(true);
        UserRepository.Setup(r => r.IncrementFailedLoginAttemptsAsync(user.Id.ToString())).Returns(Task.CompletedTask);
        UserRepository.Setup(r => r.ResetFailedLoginAttemptsAsync(user.Id.ToString())).Returns(Task.CompletedTask);
        UserRepository.Setup(r => r.LockAccountAsync(user.Id.ToString(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        UserRepository.Setup(r => r.UpdateLastLoginAsync(user.Id.ToString(), It.IsAny<string>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Sets up UserRepository for a non-existent user
    /// </summary>
    public void SetupUserDoesNotExist(string phoneNumber)
    {
        UserRepository.Setup(r => r.GetByPhoneNumberAsync(phoneNumber)).ReturnsAsync((User?)null);
        UserRepository.Setup(r => r.PhoneNumberExistsAsync(phoneNumber)).ReturnsAsync(false);
        UserRepository.Setup(r => r.IdNumberExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        UserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);
        UserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);
    }

    /// <summary>
    /// Sets up GroupRepository to return a group that exists
    /// </summary>
    public void SetupGroupExists(Group group)
    {
        GroupRepository.Setup(r => r.GetByIdAsync(group.Id, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        GroupRepository.Setup(r => r.GetByIdWithMembersAsync(group.Id, It.IsAny<CancellationToken>())).ReturnsAsync(group);
    }

    /// <summary>
    /// Sets up GroupRepository for a non-existent group
    /// </summary>
    public void SetupGroupDoesNotExist(Guid groupId)
    {
        GroupRepository.Setup(r => r.GetByIdAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync((Group?)null);
        GroupRepository.Setup(r => r.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync((Group?)null);
    }

    /// <summary>
    /// Sets up MemberRepository to return members for a group
    /// </summary>
    public void SetupMembersForGroup(Guid groupId, List<Member> members)
    {
        MemberRepository.Setup(r => r.GetByGroupIdAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync(members);

        // Setup GetByGroupIdAndUserIdAsync for each member
        foreach (var member in members)
        {
            MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(groupId, member.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(member);
        }
    }

    /// <summary>
    /// Sets up MemberRepository for a member that exists
    /// </summary>
    public void SetupMemberExists(Member member)
    {
        MemberRepository.Setup(r => r.GetByIdAsync(member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(member.GroupId, member.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
    }

    /// <summary>
    /// Sets up RefreshTokenRepository to return a valid refresh token
    /// </summary>
    public void SetupValidRefreshToken(RefreshToken token)
    {
        RefreshTokenRepository.Setup(r => r.GetByTokenAsync(token.Token)).ReturnsAsync(token);
    }

    /// <summary>
    /// Sets up CoreBankingClient for successful payments
    /// </summary>
    public void SetupSuccessfulPayment()
    {
        CoreBankingClient.Setup(c => c.ExecutePaymentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(true);

        CoreBankingClient.Setup(c => c.ExecuteEFTAsync(
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(true);
    }

    /// <summary>
    /// Sets up CoreBankingClient for failed payments
    /// </summary>
    public void SetupFailedPayment()
    {
        CoreBankingClient.Setup(c => c.ExecutePaymentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(false);

        CoreBankingClient.Setup(c => c.ExecuteEFTAsync(
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(false);
    }

    /// <summary>
    /// Sets up FraudDetectionService with low risk score
    /// </summary>
    public void SetupLowFraudRisk()
    {
        FraudDetectionService.Setup(f => f.DetectSuspiciousLoginAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync(new Core.Interfaces.FraudCheckResult
        {
            IsSuspicious = false,
            RiskScore = 10,
            RiskFactors = new List<string>()
        });
    }

    /// <summary>
    /// Sets up FraudDetectionService with high risk score
    /// </summary>
    public void SetupHighFraudRisk(int riskScore = 80)
    {
        FraudDetectionService.Setup(f => f.DetectSuspiciousLoginAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync(new Core.Interfaces.FraudCheckResult
        {
            IsSuspicious = true,
            RiskScore = riskScore,
            RiskFactors = new List<string> { "New device", "Different IP" }
        });
    }

    /// <summary>
    /// Resets all mocks to initial state
    /// </summary>
    public void Reset()
    {
        UserRepository.Reset();
        GroupRepository.Reset();
        MemberRepository.Reset();
        ContributionRepository.Reset();
        PayoutRepository.Reset();
        RefreshTokenRepository.Reset();
        AuditLogRepository.Reset();
        LedgerRepository.Reset();
        VoteRepository.Reset();
        DisputeRepository.Reset();
        UnitOfWork.Reset();
        NotificationService.Reset();
        FraudDetectionService.Reset();
        CoreBankingClient.Reset();
        WalletService.Reset();
    }
}
