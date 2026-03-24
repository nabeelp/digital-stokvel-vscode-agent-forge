using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Tests.Fixtures;

/// <summary>
/// Builder pattern for creating test entities with realistic South African data
/// </summary>
public class TestDataBuilder
{
    private static readonly string[] SouthAfricanNames = new[]
    {
        "Thabo Mbeki", "Nomsa Dlamini", "Sipho Khumalo", "Zanele Nkosi", "Mandla Mthembu",
        "Lerato Mokoena", "Bongani Ndlovu", "Nandi Zulu", "Sizwe Mabaso", "Thandiwe Sithole"
    };

    private static readonly string[] GroupNames = new[]
    {
        "Ubuntu Savings Circle", "Thusanang Investment Club", "Kopanelo Group Savings",
        "Siyakha Together Stokvel", "Vukuzenzele Savings", "Masibambane Investment Group"
    };

    private static int _nameCounter = 0;
    private static int _phoneCounter = 800000000;
    private static int _idCounter = 10000;

    /// <summary>
    /// Builds a User entity with realistic South African data
    /// </summary>
    public static User BuildUser(
        string? phoneNumber = null,
        string? pinHash = null,
        string? fullName = null,
        string? idNumber = null,
        UserStatus status = UserStatus.Active,
        bool ficaVerified = true)
    {
        var name = fullName ?? SouthAfricanNames[_nameCounter++ % SouthAfricanNames.Length];
        var phone = phoneNumber ?? GenerateValidSAPhoneNumber();
        var pin = pinHash ?? BCrypt.Net.BCrypt.HashPassword("1234", 12);
        var id = idNumber ?? GenerateValidSAIdNumber();

        return new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phone,
            PinHash = pin,
            FullName = name,
            IdNumber = id,
            Email = $"{name.Replace(" ", ".").ToLower()}@example.com",
            Status = status,
            FailedLoginAttempts = 0,
            LockedUntil = null,
            BiometricEnabled = false,
            TwoFactorEnabled = false,
            FICAVerified = ficaVerified,
            POPIAConsentAccepted = true,
            POPIAConsentAcceptedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Builds a Group entity with realistic data
    /// </summary>
    public static Group BuildGroup(
        string? name = null,
        decimal contributionAmount = 500,
        GroupType groupType = GroupType.RotatingPayout,
        decimal currentBalance = 0,
        GroupStatus status = GroupStatus.Active,
        Guid? chairpersonId = null,
        Guid? treasurerId = null)
    {
        return new Group
        {
            Id = Guid.NewGuid(),
            Name = name ?? GroupNames[_nameCounter++ % GroupNames.Length],
            Description = "A community savings group focused on financial growth",
            GroupType = groupType,
            ContributionAmount = contributionAmount,
            ContributionFrequency = ContributionFrequency.Monthly,
            PayoutSchedule = PayoutSchedule.Rotating,
            CurrentBalance = currentBalance,
            TotalInterestEarned = 0,
            Status = status,
            BankAccountNumber = GenerateBankAccountNumber(),
            ChairpersonId = chairpersonId ?? Guid.NewGuid(),
            TreasurerId = treasurerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Builds a Member entity
    /// </summary>
    public static Member BuildMember(
        Guid? groupId = null,
        string? userId = null,
        string? fullName = null,
        MemberRole role = MemberRole.Member,
        MemberStatus status = MemberStatus.Active)
    {
        return new Member
        {
            Id = Guid.NewGuid(),
            GroupId = groupId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid().ToString(),
            FullName = fullName ?? SouthAfricanNames[_nameCounter++ % SouthAfricanNames.Length],
            PhoneNumber = GenerateValidSAPhoneNumber(),
            Role = role,
            Status = status,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Builds a Contribution entity
    /// </summary>
    public static Contribution BuildContribution(
        Guid? groupId = null,
        Guid? memberId = null,
        decimal amount = 500,
        ContributionStatus status = ContributionStatus.Completed)
    {
        return new Contribution
        {
            Id = Guid.NewGuid(),
            GroupId = groupId ?? Guid.NewGuid(),
            MemberId = memberId ?? Guid.NewGuid(),
            Amount = amount,
            TransactionId = Guid.NewGuid().ToString(),
            Status = status,
            PaymentMethod = PaymentMethod.App,
            DueDate = DateTime.UtcNow,
            ConfirmedAt = status == ContributionStatus.Completed ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Builds a Payout entity
    /// </summary>
    public static Payout BuildPayout(
        Guid? groupId = null,
        Guid? recipientMemberId = null,
        Guid? initiatedByMemberId = null,
        decimal amount = 5000,
        PayoutStatus status = PayoutStatus.PendingApproval,
        PayoutType payoutType = PayoutType.Rotating)
    {
        return new Payout
        {
            Id = Guid.NewGuid(),
            GroupId = groupId ?? Guid.NewGuid(),
            RecipientMemberId = recipientMemberId ?? Guid.NewGuid(),
            Amount = amount,
            PayoutType = payoutType,
            Status = status,
            InitiatedByMemberId = initiatedByMemberId ?? Guid.NewGuid(),
            InitiatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Notes = "Test payout",
            TransactionId = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Builds a Vote entity
    /// </summary>
    public static Vote BuildVote(
        Guid? groupId = null,
        string? title = null,
        VoteStatus status = VoteStatus.Active)
    {
        return new Vote
        {
            Id = Guid.NewGuid(),
            GroupId = groupId ?? Guid.NewGuid(),
            Proposal = title ?? "Vote on group policy change",
            Description = "Please vote on the proposed change",
            Status = status,
            Deadline = DateTime.UtcNow.AddDays(7),
            VoteDeadline = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Builds a Dispute entity
    /// </summary>
    public static Dispute BuildDispute(
        Guid? groupId = null,
        Guid? raisedByMemberId = null,
        DisputeStatus status = DisputeStatus.Open)
    {
        return new Dispute
        {
            Id = Guid.NewGuid(),
            GroupId = groupId ?? Guid.NewGuid(),
            RaisedByMemberId = raisedByMemberId ?? Guid.NewGuid(),
            IssueType = "MissedPayment",
            Description = "Dispute regarding contribution payment - There is a discrepancy in the contribution record",
            Status = status,
            RaisedAt = DateTime.UtcNow,
            EscalationDeadline = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Builds a RefreshToken entity
    /// </summary>
    public static RefreshToken BuildRefreshToken(
        string? userId = null,
        bool isActive = true)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid().ToString(),
            Token = GenerateRandomToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),

            RevokedAt = !isActive ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates a valid South African phone number (+27XXXXXXXXX)
    /// </summary>
    public static string GenerateValidSAPhoneNumber()
    {
        var number = _phoneCounter++;
        return $"+27{number:D9}";
    }

    /// <summary>
    /// Generates a valid South African ID number with correct Luhn checksum
    /// </summary>
    public static string GenerateValidSAIdNumber()
    {
        var counter = _idCounter++;
        // Format: YYMMDD SSSS C A (12 digits) + Z (checksum)
        // Use 900101 (1990-01-01) + sequence + 0 (SA citizen) + 8 (old format)
        var idWithoutChecksum = $"900101{counter:D4}08";
        
        // Calculate Luhn checksum
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = int.Parse(idWithoutChecksum[i].ToString());
            if (i % 2 == 0)
            {
                sum += digit;
            }
            else
            {
                int doubled = digit * 2;
                sum += doubled > 9 ? doubled - 9 : doubled;
            }
        }
        
        int checkDigit = (10 - (sum % 10)) % 10;
        return idWithoutChecksum + checkDigit;
    }

    /// <summary>
    /// Generates a bank account number
    /// </summary>
    private static string GenerateBankAccountNumber()
    {
        return $"620{Random.Shared.Next(10000000, 99999999)}";
    }

    /// <summary>
    /// Generates a random token string
    /// </summary>
    private static string GenerateRandomToken()
    {
        return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
    }
}
