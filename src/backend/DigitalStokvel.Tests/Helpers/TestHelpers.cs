using DigitalStokvel.Core.DTOs;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalStokvel.Tests.Helpers;

/// <summary>
/// Helper methods for testing
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Generates a test JWT token for authentication testing
    /// </summary>
    public static string GenerateTestJwtToken(string userId, string phoneNumber)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestSecretKeyForJwtTokenGeneration_MustBeAtLeast32Characters"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim("phone_number", phoneNumber),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "DigitalStokvel",
            audience: "DigitalStokvelUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a valid South African phone number for testing
    /// </summary>
    public static string GenerateValidSAPhoneNumber()
    {
        return $"+27{Random.Shared.Next(700000000, 899999999):D9}";
    }

    /// <summary>
    /// Generates an invalid phone number for negative testing
    /// </summary>
    public static string GenerateInvalidPhoneNumber()
    {
        return "0123456789"; // Missing +27 prefix
    }

    /// <summary>
    /// Generates a valid 4-digit PIN
    /// </summary>
    public static string GenerateValidPin()
    {
        return $"{Random.Shared.Next(1000, 9999)}";
    }

    /// <summary>
    /// Generates an invalid PIN (sequential digits like 1234)
    /// </summary>
    public static string GenerateInvalidPinSequential()
    {
        return "1234";
    }

    /// <summary>
    /// Generates an invalid PIN (repeated digits like 1111)
    /// </summary>
    public static string GenerateInvalidPinRepeated()
    {
        return "1111";
    }

    /// <summary>
    /// FluentAssertions wrapper for Result Success
    /// </summary>
    public static void AssertResultSuccess<T>(Result<T> result, string because = "")
    {
        result.IsSuccess.Should().BeTrue(because);
        result.Data.Should().NotBeNull(because);
        result.ErrorMessage.Should().BeNullOrEmpty(because);
    }

    /// <summary>
    /// FluentAssertions wrapper for Result Failure
    /// </summary>
    public static void AssertResultFailure<T>(Result<T> result, string expectedError, string because = "")
    {
        result.IsSuccess.Should().BeFalse(because);
        result.Data.Should().BeNull(because);
        result.ErrorMessage.Should().NotBeNullOrEmpty(because);
        if (!string.IsNullOrEmpty(expectedError))
        {
            result.ErrorMessage.Should().Contain(expectedError, because);
        }
    }

    /// <summary>
    /// FluentAssertions wrapper for tuple (Success, Value, Error) pattern
    /// </summary>
    public static void AssertTupleSuccess<T>(bool success, T? value, string? errorMessage, string because = "")
    {
        success.Should().BeTrue(because);
        value.Should().NotBeNull(because);
        errorMessage.Should().BeNullOrEmpty(because);
    }

    /// <summary>
    /// FluentAssertions wrapper for tuple (Success, Value, Error) failure pattern
    /// </summary>
    public static void AssertTupleFailure<T>(bool success, T? value, string? errorMessage, string expectedErrorContains = "", string because = "")
    {
        success.Should().BeFalse(because);
        value.Should().BeNull(because);
        errorMessage.Should().NotBeNullOrEmpty(because);
        if (!string.IsNullOrEmpty(expectedErrorContains))
        {
            errorMessage.Should().Contain(expectedErrorContains, because);
        }
    }

    /// <summary>
    /// Creates a mock DateTime provider for time-dependent tests
    /// </summary>
    public static DateTime GetFixedDateTime()
    {
        return new DateTime(2026, 3, 24, 12, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Verifies a phone number is in valid South African format
    /// </summary>
    public static bool IsValidSAPhoneNumber(string phoneNumber)
    {
        return phoneNumber.StartsWith("+27") && phoneNumber.Length == 12;
    }
}
