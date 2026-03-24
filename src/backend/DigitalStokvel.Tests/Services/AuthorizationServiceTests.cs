using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Services.Authorization;
using DigitalStokvel.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DigitalStokvel.Tests.Services;

/// <summary>
/// Unit tests for AuthorizationService
/// Focus: RBAC permission checks, group membership validation
/// Coverage Target: ≥70%
/// </summary>
public class AuthorizationServiceTests
{
    private readonly MockRepositoryFixture _mockFixture;
    private readonly Mock<ILogger<AuthorizationService>> _mockLogger;
    private readonly AuthorizationService _authService;

    public AuthorizationServiceTests()
    {
        _mockFixture = new MockRepositoryFixture();
        _mockLogger = new Mock<ILogger<AuthorizationService>>();

        _authService = new AuthorizationService(
            _mockFixture.MemberRepository.Object,
            _mockFixture.GroupRepository.Object,
            _mockFixture.PayoutRepository.Object,
            _mockLogger.Object
        );
    }

    #region CheckGroupMembershipAsync Tests

    [Fact]
    public async Task CheckGroupMembershipAsync_ForMember_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var member = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { member };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        var result = await _authService.CheckGroupMembershipAsync(userId, group.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckGroupMembershipAsync_ForNonMember_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var differentMember = TestDataBuilder.BuildMember(role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { differentMember };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Member?)null);

        // Act
        var result = await _authService.CheckGroupMembershipAsync(userId, group.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CheckPayoutApprovalPermissionAsync Tests

    [Fact]
    public async Task CheckPayoutApprovalPermissionAsync_ForTreasurer_ReturnsTrue_PE04()
    {
        // Arrange (PE-04: Only Treasurer can approve)
        var userId = Guid.NewGuid().ToString();
        var treasurer = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Treasurer);
        var group = TestDataBuilder.BuildGroup();
        var payout = TestDataBuilder.BuildPayout(groupId: group.Id);
        group.Members = new List<Member> { treasurer };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.PayoutRepository.Setup(r => r.GetByIdAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(treasurer);

        // Act
        var result = await _authService.CheckPayoutApprovalPermissionAsync(userId, payout.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPayoutApprovalPermissionAsync_ForChairperson_ReturnsFalse_PE04()
    {
        // Arrange (PE-04: Chairperson cannot approve payouts!)
        var userId = Guid.NewGuid().ToString();
        var chairperson = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Chairperson);
        var group = TestDataBuilder.BuildGroup();
        var payout = TestDataBuilder.BuildPayout(groupId: group.Id);
        group.Members = new List<Member> { chairperson };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.PayoutRepository.Setup(r => r.GetByIdAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chairperson);

        // Act
        var result = await _authService.CheckPayoutApprovalPermissionAsync(userId, payout.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPayoutApprovalPermissionAsync_ForMember_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var member = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup();
        var payout = TestDataBuilder.BuildPayout(groupId: group.Id);
        group.Members = new List<Member> { member };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.PayoutRepository.Setup(r => r.GetByIdAsync(payout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payout);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        var result = await _authService.CheckPayoutApprovalPermissionAsync(userId, payout.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CheckPayoutInitiationPermissionAsync Tests

    [Fact]
    public async Task CheckPayoutInitiationPermissionAsync_ForChairperson_ReturnsTrue_PE02()
    {
        // Arrange (PE-02: Only Chairperson can initiate)
        var userId = Guid.NewGuid().ToString();
        var chairperson = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Chairperson);
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { chairperson };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chairperson);

        // Act
        var result = await _authService.CheckPayoutInitiationPermissionAsync(userId, group.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckPayoutInitiationPermissionAsync_ForTreasurer_ReturnsFalse_PE02()
    {
        // Arrange (PE-02: Treasurer cannot initiate payouts)
        var userId = Guid.NewGuid().ToString();
        var treasurer = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Treasurer);
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { treasurer };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(treasurer);

        // Act
        var result = await _authService.CheckPayoutInitiationPermissionAsync(userId, group.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckPayoutInitiationPermissionAsync_ForMember_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var member = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { member };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        var result = await _authService.CheckPayoutInitiationPermissionAsync(userId, group.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CheckGroupEditPermissionAsync Tests

    [Fact]
    public async Task CheckGroupEditPermissionAsync_ForChairperson_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var chairperson = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Chairperson);
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { chairperson };

        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chairperson);

        // Act
        var result = await _authService.CheckGroupEditPermissionAsync(userId, group.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckGroupEditPermissionAsync_ForTreasurer_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var treasurer = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Treasurer);
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { treasurer };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(treasurer);

        // Act
        var result = await _authService.CheckGroupEditPermissionAsync(userId, group.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckGroupEditPermissionAsync_ForMember_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var member = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { member };

        _mockFixture.SetupGroupExists(group);
        _mockFixture.MemberRepository.Setup(r => r.GetByGroupIdAndUserIdAsync(group.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        var result = await _authService.CheckGroupEditPermissionAsync(userId, group.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
