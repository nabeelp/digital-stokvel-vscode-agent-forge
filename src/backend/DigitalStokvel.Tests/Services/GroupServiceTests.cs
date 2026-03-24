using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Services.Groups;
using DigitalStokvel.Tests.Fixtures;
using DigitalStokvel.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DigitalStokvel.Tests.Services;

/// <summary>
/// Unit tests for GroupService (GM-01 to GM-09)
/// Focus: Group creation, contribution amount validation, member management
/// Coverage Target: ≥70%
/// </summary>
public class GroupServiceTests
{
    private readonly MockRepositoryFixture _mockFixture;
    private readonly Mock<ILogger<GroupService>> _mockLogger;
    private readonly GroupService _groupService;

    public GroupServiceTests()
    {
        _mockFixture = new MockRepositoryFixture();
        _mockLogger = new Mock<ILogger<GroupService>>();

        _groupService = new GroupService(
            _mockFixture.UnitOfWork.Object,
            _mockFixture.CoreBankingClient.Object,
            _mockFixture.NotificationService.Object,
            _mockLogger.Object
        );
    }

    #region CreateGroupAsync Tests

    [Fact]
    public async Task CreateGroupAsync_WithValidData_CreatesGroup_GM01()
    {
        // Arrange (GM-01: Create group)
        var userId = Guid.NewGuid().ToString();
        var request = new CreateGroupRequest(
            "Ubuntu Savings Circle",
            "Monthly savings for community development",
            GroupType.RotatingPayout,
            500,
            ContributionFrequency.Monthly,
            PayoutSchedule.Rotating
        );

        _mockFixture.CoreBankingClient.Setup(c => c.CreateGroupSavingsAccountAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync("6201234567890");

        // Act
        var result = await _groupService.CreateGroupAsync(request, userId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Ubuntu Savings Circle");
        result.Data.ContributionAmount.Should().Be(500);
        result.Data.MemberCount.Should().Be(1); // Chairperson created

        _mockFixture.GroupRepository.Verify(
            g => g.AddAsync(It.Is<Group>(grp =>
                grp.Name == "Ubuntu Savings Circle" &&
                grp.ContributionAmount == 500 &&
                grp.Status == GroupStatus.Active
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateGroupAsync_AssignsChairpersonRoleToCreator_GM01()
    {
        // Arrange (GM-01: Creator becomes Chairperson)
        var userId = Guid.NewGuid().ToString();
        var request = new CreateGroupRequest(
            "Kopanelo Group Savings",
            "Community savings",
            GroupType.SavingsPot,
            1000,
            ContributionFrequency.Monthly,
            PayoutSchedule.YearEnd
        );

        _mockFixture.CoreBankingClient.Setup(c => c.CreateGroupSavingsAccountAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync("6201234567890");

        // Act
        var result = await _groupService.CreateGroupAsync(request, userId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        _mockFixture.MemberRepository.Verify(
            m => m.AddAsync(It.Is<Member>(mem =>
                mem.UserId == userId &&
                mem.Role == MemberRole.Chairperson &&
                mem.Status == MemberStatus.Active
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData(30)] // Below R50 minimum
    [InlineData(49.99)]
    [InlineData(10001)] // Above R10,000 maximum
    [InlineData(15000)]
    public async Task CreateGroupAsync_WithInvalidContributionAmount_ReturnsError_GM02(decimal invalidAmount)
    {
        // Arrange (GM-02: R50-R10,000 contribution range)
        var userId = Guid.NewGuid().ToString();
        var request = new CreateGroupRequest(
            "Test Group",
            "Test",
            GroupType.RotatingPayout,
            invalidAmount,
            ContributionFrequency.Monthly,
            PayoutSchedule.Rotating
        );

        // Act
        var result = await _groupService.CreateGroupAsync(request, userId);

        // Assert
        TestHelpers.AssertResultFailure(result, "between R50 and R10,000");
        _mockFixture.GroupRepository.Verify(
            g => g.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData(50)] // Minimum valid
    [InlineData(500)] // Typical
    [InlineData(10000)] // Maximum valid
    public async Task CreateGroupAsync_WithValidContributionAmount_CreatesGroup_GM02(decimal validAmount)
    {
        // Arrange (GM-02: Valid range)
        var userId = Guid.NewGuid().ToString();
        var request = new CreateGroupRequest(
            "Valid Amount Group",
            "Test",
            GroupType.RotatingPayout,
            validAmount,
            ContributionFrequency.Monthly,
            PayoutSchedule.Rotating
        );

        _mockFixture.CoreBankingClient.Setup(c => c.CreateGroupSavingsAccountAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync("6201234567890");

        // Act
        var result = await _groupService.CreateGroupAsync(request, userId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        result.Data!.ContributionAmount.Should().Be(validAmount);
    }

    [Fact]
    public async Task CreateGroupAsync_CreatesBankAccountNumber_GM06()
    {
        // Arrange (GM-06: Bank account creation)
        var userId = Guid.NewGuid().ToString();
        var expectedAccountNumber = "6201234567890";
        var request = new CreateGroupRequest(
            "Banking Group",
            "Test",
            GroupType.RotatingPayout,
            500,
            ContributionFrequency.Monthly,
            PayoutSchedule.Rotating
        );

        _mockFixture.CoreBankingClient.Setup(c => c.CreateGroupSavingsAccountAsync(
            request.Name,
            userId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(expectedAccountNumber);

        // Act
        var result = await _groupService.CreateGroupAsync(request, userId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        _mockFixture.CoreBankingClient.Verify(c => c.CreateGroupSavingsAccountAsync(
            request.Name,
            userId,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _mockFixture.GroupRepository.Verify(
            g => g.AddAsync(It.Is<Group>(grp =>
                grp.BankAccountNumber == expectedAccountNumber
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region GetGroupDetailsAsync Tests

    [Fact]
    public async Task GetGroupDetailsAsync_ForMember_ReturnsDetails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var member = TestDataBuilder.BuildMember(userId: userId, role: MemberRole.Member);
        var group = TestDataBuilder.BuildGroup(currentBalance: 5000);
        group.Members = new List<Member> { member };

        _mockFixture.SetupGroupExists(group);

        // Act
        var result = await _groupService.GetGroupDetailsAsync(group.Id, userId);

        // Assert
        TestHelpers.AssertResultSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(group.Name);
        result.Data.CurrentBalance.Should().Be(5000);
    }

    [Fact]
    public async Task GetGroupDetailsAsync_ForNonMember_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var member = TestDataBuilder.BuildMember(role: MemberRole.Member); // Different user!
        var group = TestDataBuilder.BuildGroup();
        group.Members = new List<Member> { member };

        _mockFixture.SetupGroupExists(group);

        // Act
        var result = await _groupService.GetGroupDetailsAsync(group.Id, userId);

        // Assert
        TestHelpers.AssertResultFailure(result, "not a member");
    }

    [Fact]
    public async Task GetGroupDetailsAsync_ForNonExistentGroup_ReturnsError()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        _mockFixture.SetupGroupDoesNotExist(groupId);

        // Act
        var result = await _groupService.GetGroupDetailsAsync(groupId, userId);

        // Assert
        TestHelpers.AssertResultFailure(result, "not found");
    }

    #endregion

    #region AddMemberAsync / UpdateGroupAsync / ArchiveGroupAsync Tests

    // Note: Implement when these methods are implemented in GroupService
    // [Fact] public async Task AddMemberAsync_WithValidInvitation_AddsMember_GM03()
    // [Fact] public async Task UpdateGroupAsync_RequiresTreasurerApproval_GM08()
    // [Fact] public async Task ArchiveGroupAsync_SetsGroupToArchived_GM09()
    // [Fact] public async Task RemoveMemberAsync_RequiresDualApproval()

    #endregion
}
