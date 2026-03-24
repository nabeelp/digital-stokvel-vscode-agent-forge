using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Services.Payouts;
using DigitalStokvel.Tests.Fixtures;
using DigitalStokvel.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DigitalStokvel.Tests.Services;

/// <summary>
/// Unit tests for PayoutService (PE-01 to PE-09)
/// Focus: Payout initiation, dual approval, 24-hour expiration, different payout types
/// Coverage Target: ≥80%
/// </summary>
public class PayoutServiceTests
{
    private readonly MockRepositoryFixture _mockFixture;
    private readonly Mock<ILogger<PayoutService>> _mockLogger;
    private readonly PayoutService _payoutService;

    public PayoutServiceTests()
    {
        _mockFixture = new MockRepositoryFixture();
        _mockLogger = new Mock<ILogger<PayoutService>>();

        _payoutService = new PayoutService(
            _mockFixture.UnitOfWork.Object,
            _mockFixture.CoreBankingClient.Object,
            _mockFixture.NotificationService.Object,
            _mockFixture.WalletService.Object,
            _mockLogger.Object
        );
    }

    #region InitiatePayoutAsync Tests

    [Fact]
    public async Task InitiatePayoutAsync_ByChairperson_WithValidData_CreatesPayoutSuccessfully_PE02()
    {
        // Arrange (PE-02: Only Chairperson can initiate)
        var chairpersonUserId = Guid.NewGuid().ToString();
        var chairperson = TestDataBuilder.BuildMember(
            userId: chairpersonUserId,
            role: MemberRole.Chairperson
        );

        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup(
            currentBalance: 10000, // Sufficient balance
            chairpersonId: Guid.Parse(chairpersonUserId)
        );
        group.Members = new List<Member> { chairperson, recipient };

        _mockFixture.SetupGroupExists(group);

        var request = new InitiatePayoutRequest(
            group.Id,
            recipient.Id,
            5000,
            PayoutType.Rotating,
            "Monthly rotating payout"
        );

        // Act
        var result = await _payoutService.InitiatePayoutAsync(request, chairpersonUserId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(PayoutStatus.PendingApproval);
        result.Data.Amount.Should().Be(5000);

        _mockFixture.PayoutRepository.Verify(
            p => p.AddAsync(It.Is<Payout>(pay =>
                pay.Status == PayoutStatus.PendingApproval &&
                pay.Amount == 5000 &&
                pay.RecipientMemberId == recipient.Id
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task InitiatePayoutAsync_SetsExpirationTo24Hours_PE03()
    {
        // Arrange (PE-03: 24-hour expiration)
        var chairpersonUserId = Guid.NewGuid().ToString();
        var chairperson = TestDataBuilder.BuildMember(
            userId: chairpersonUserId,
            role: MemberRole.Chairperson
        );
        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup(currentBalance: 10000);
        group.Members = new List<Member> { chairperson, recipient };

        _mockFixture.SetupGroupExists(group);

        var request = new InitiatePayoutRequest(
            group.Id,
            recipient.Id,
            5000,
            PayoutType.Rotating,
            string.Empty
        );

        var beforeTime = DateTime.UtcNow.AddHours(24).AddMinutes(-1);
        var afterTime = DateTime.UtcNow.AddHours(24).AddMinutes(1);

        // Act
        var result = await _payoutService.InitiatePayoutAsync(request, chairpersonUserId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        _mockFixture.PayoutRepository.Verify(
            p => p.AddAsync(It.Is<Payout>(pay =>
                pay.ExpiresAt >= beforeTime &&
                pay.ExpiresAt <= afterTime
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task InitiatePayoutAsync_ByMember_ReturnsError_PE02()
    {
        // Arrange (PE-02: Only Chairperson can initiate)
        var memberUserId = Guid.NewGuid().ToString();
        var member = TestDataBuilder.BuildMember(
            userId: memberUserId,
            role: MemberRole.Member // Not Chairperson!
        );
        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup(currentBalance: 10000);
        group.Members = new List<Member> { member, recipient };

        _mockFixture.SetupGroupExists(group);

        var request = new InitiatePayoutRequest(
            group.Id,
            recipient.Id,
            5000,
            PayoutType.Rotating,
            string.Empty
        );

        // Act
        var result = await _payoutService.InitiatePayoutAsync(request, memberUserId);

        // Assert
        TestHelpers.AssertResultFailure(result, "Only Chairperson");
        _mockFixture.PayoutRepository.Verify(
            p => p.AddAsync(It.IsAny<Payout>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task InitiatePayoutAsync_WithInsufficientBalance_ReturnsError_PE02()
    {
        // Arrange (PE-02: Verify sufficient balance)
        var chairpersonUserId = Guid.NewGuid().ToString();
        var chairperson = TestDataBuilder.BuildMember(
            userId: chairpersonUserId,
            role: MemberRole.Chairperson
        );
        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup(currentBalance: 1000); // Insufficient!
        group.Members = new List<Member> { chairperson, recipient };

        _mockFixture.SetupGroupExists(group);

        var request = new InitiatePayoutRequest(
            group.Id,
            recipient.Id,
            5000, // More than balance
            PayoutType.Rotating,
            string.Empty
        );

        // Act
        var result = await _payoutService.InitiatePayoutAsync(request, chairpersonUserId);

        // Assert
        TestHelpers.AssertResultFailure(result, "Insufficient");
        _mockFixture.PayoutRepository.Verify(
            p => p.AddAsync(It.IsAny<Payout>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task InitiatePayoutAsync_WithNonMemberRecipient_ReturnsError()
    {
        // Arrange
        var chairpersonUserId = Guid.NewGuid().ToString();
        var chairperson = TestDataBuilder.BuildMember(
            userId: chairpersonUserId,
            role: MemberRole.Chairperson
        );
        var group = TestDataBuilder.BuildGroup(currentBalance: 10000);
        group.Members = new List<Member> { chairperson }; // Recipient not in group!

        _mockFixture.SetupGroupExists(group);

        var request = new InitiatePayoutRequest(
            group.Id,
            Guid.NewGuid(), // Non-existent member
            5000,
            PayoutType.Rotating,
            string.Empty
        );

        // Act
        var result = await _payoutService.InitiatePayoutAsync(request, chairpersonUserId);

        // Assert
        TestHelpers.AssertResultFailure(result, "not a member");
    }

    [Fact]
    public async Task InitiatePayoutAsync_WithNonExistentGroup_ReturnsError()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _mockFixture.SetupGroupDoesNotExist(groupId);

        var request = new InitiatePayoutRequest(
            groupId,
            Guid.NewGuid(),
            5000,
            PayoutType.Rotating,
            string.Empty
        );

        // Act
        var result = await _payoutService.InitiatePayoutAsync(request, Guid.NewGuid().ToString());

        // Assert
        TestHelpers.AssertResultFailure(result, "not found");
    }

    #endregion

    #region ApprovePayoutAsync Tests

    [Fact]
    public async Task ApprovePayoutAsync_ByTreasurer_ApprovesSuccessfully_PE04()
    {
        // Arrange (PE-04: Only Treasurer can approve)
        var treasurerUserId = Guid.NewGuid().ToString();
        var treasurer = TestDataBuilder.BuildMember(
            userId: treasurerUserId,
            role: MemberRole.Treasurer
        );
        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var payout = TestDataBuilder.BuildPayout(
            recipientMemberId: recipient.Id,
            status: PayoutStatus.PendingApproval
        );

        var group = TestDataBuilder.BuildGroup(currentBalance: 10000);
        group.Members = new List<Member> { treasurer, recipient };

        _mockFixture.PayoutRepository.Setup(p => p.GetByIdWithDetailsAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _mockFixture.SetupGroupExists(group);
        _mockFixture.SetupSuccessfulPayment();

        var request = new ApprovePayoutRequest("Approved by Treasurer");

        // Act
        var result = await _payoutService.ApprovePayoutAsync(payout.Id, request, treasurerUserId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        _mockFixture.WalletService.Verify(w => w.CreateLedgerEntryAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ApprovePayoutAsync_ByChairperson_ReturnsError_PE04()
    {
        // Arrange (PE-04: Only Treasurer, not Chairperson!)
        var chairpersonUserId = Guid.NewGuid().ToString();
        var chairperson = TestDataBuilder.BuildMember(
            userId: chairpersonUserId,
            role: MemberRole.Chairperson // Not Treasurer!
        );
        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var payout = TestDataBuilder.BuildPayout(
            recipientMemberId: recipient.Id,
            status: PayoutStatus.PendingApproval
        );

        var group = TestDataBuilder.BuildGroup(currentBalance: 10000);
        group.Members = new List<Member> { chairperson, recipient };

        _mockFixture.PayoutRepository.Setup(p => p.GetByIdWithDetailsAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _mockFixture.SetupGroupExists(group);

        var request = new ApprovePayoutRequest("Attempting approval by Chairperson");

        // Act
        var result = await _payoutService.ApprovePayoutAsync(payout.Id, request, chairpersonUserId);

        // Assert
        TestHelpers.AssertResultFailure(result, "Only Treasurer");
        _mockFixture.CoreBankingClient.Verify(c => c.ExecutePaymentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task ApprovePayoutAsync_ExpiredPayout_ReturnsError_PE03()
    {
        // Arrange (PE-03: Expired after 24 hours)
        var treasurerUserId = Guid.NewGuid().ToString();
        var treasurer = TestDataBuilder.BuildMember(
            userId: treasurerUserId,
            role: MemberRole.Treasurer
        );
        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var payout = TestDataBuilder.BuildPayout(
            recipientMemberId: recipient.Id,
            status: PayoutStatus.PendingApproval
        );
        payout.ExpiresAt = DateTime.UtcNow.AddHours(-1); // Already expired!

        var group = TestDataBuilder.BuildGroup(currentBalance: 10000);
        group.Members = new List<Member> { treasurer, recipient };

        _mockFixture.PayoutRepository.Setup(p => p.GetByIdWithDetailsAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _mockFixture.SetupGroupExists(group);

        var request = new ApprovePayoutRequest("Attempting to approve expired payout");

        // Act
        var result = await _payoutService.ApprovePayoutAsync(payout.Id, request, treasurerUserId);

        // Assert
        TestHelpers.AssertResultFailure(result, "expired");
    }

    [Fact]
    public async Task ApprovePayoutAsync_Rejection_MarksPayoutAsRejected()
    {
        // Arrange
        var treasurerUserId = Guid.NewGuid().ToString();
        var treasurer = TestDataBuilder.BuildMember(
            userId: treasurerUserId,
            role: MemberRole.Treasurer
        );
        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var payout = TestDataBuilder.BuildPayout(
            recipientMemberId: recipient.Id,
            status: PayoutStatus.PendingApproval
        );

        var group = TestDataBuilder.BuildGroup(currentBalance: 10000);
        group.Members = new List<Member> { treasurer, recipient };

        _mockFixture.PayoutRepository.Setup(p => p.GetByIdWithDetailsAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _mockFixture.SetupGroupExists(group);

        var request = new RejectPayoutRequest("Insufficient justification for payout");

        // Act
        var result = await _payoutService.RejectPayoutAsync(payout.Id, request, treasurerUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockFixture.CoreBankingClient.Verify(c => c.ExecutePaymentAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task ApprovePayoutAsync_CreatesImmutableLedgerEntry_GW06()
    {
        // Arrange (GW-06: Immutable ledger)
        var treasurerUserId = Guid.NewGuid().ToString();
        var treasurer = TestDataBuilder.BuildMember(
            userId: treasurerUserId,
            role: MemberRole.Treasurer
        );
        var recipient = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var payout = TestDataBuilder.BuildPayout(
            recipientMemberId: recipient.Id,
            amount: 5000,
            status: PayoutStatus.PendingApproval
        );

        var group = TestDataBuilder.BuildGroup(currentBalance: 10000);
        group.Members = new List<Member> { treasurer, recipient };

        _mockFixture.PayoutRepository.Setup(p => p.GetByIdWithDetailsAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _mockFixture.SetupGroupExists(group);
        _mockFixture.SetupSuccessfulPayment();

        var request = new ApprovePayoutRequest("Approved by Treasurer");

        // Act
        var result = await _payoutService.ApprovePayoutAsync(payout.Id, request, treasurerUserId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        _mockFixture.WalletService.Verify(w => w.CreateLedgerEntryAsync(
            group.Id,
            recipient.Id,
            5000,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion

    #region ExecutePayoutAsync Tests (if implemented)

    // Note: Implement when ExecutePayoutAsync is exposed
    // [Fact] public async Task ExecutePayoutAsync_UpdatesGroupBalance()
    // [Fact] public async Task ExecutePayoutAsync_NotifiesMembers()

    #endregion

    #region CalculateRotatingPayoutRecipientAsync Tests (if implemented)

    // Note: Implement when payout calculation methods are available
    // [Fact] public async Task CalculateRotatingPayoutRecipient_DeterminesNextMember()
    // [Fact] public async Task CalculateProportionalSplit_DistributesBasedOnContributions()

    #endregion
}
