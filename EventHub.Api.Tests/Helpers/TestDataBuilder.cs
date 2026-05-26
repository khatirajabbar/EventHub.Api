using EventHub.Api.Entities;
using EventHub.Api.DTOs.Auth;
using System;
using System.Security.Cryptography;
using System.Text;

namespace EventHub.Api.Tests.Helpers;

/// <summary>
/// Helper class to build test data
/// </summary>
public class TestDataBuilder
{
    public static User BuildUser(int id = 1, string username = "testuser", string email = "test@example.com")
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = email,
            PasswordHash = HashPassword("Test123"),
            Role = "Member",
            CreatedAt = DateTime.UtcNow,
            RefreshToken = "test-refresh-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
            IsEmailConfirmed = true,
            EmailConfirmationToken = string.Empty, // InMemory requires non-null string
            EmailConfirmationTokenExpiry = DateTime.MinValue,
            PasswordResetToken = string.Empty, // InMemory requires non-null string
            PasswordResetTokenExpiry = null
        };
    }

    public static LoginDto BuildLoginDto(string username = "testuser", string password = "Test123")
    {
        return new LoginDto
        {
            Username = username,
            Password = password
        };
    }

    public static RegisterDto BuildRegisterDto(string username = "newuser", string email = "new@example.com", string password = "Test123")
    {
        return new RegisterDto
        {
            Username = username,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
    }

    public static ChangePasswordDto BuildChangePasswordDto(string oldPassword = "Test123", string newPassword = "NewTest456")
    {
        return new ChangePasswordDto
        {
            OldPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };
    }

    public static ResetPasswordDto BuildResetPasswordDto(string token = "test-token", string email = "test@example.com", string newPassword = "NewTest456")
    {
        return new ResetPasswordDto
        {
            Token = token,
            Email = email,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };
    }

    public static ForgotPasswordDto BuildForgotPasswordDto(string email = "test@example.com")
    {
        return new ForgotPasswordDto
        {
            Email = email
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}

