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

    public static Organizer BuildOrganizer(int id = 1, string name = "Test Organizer", string email = "org@example.com")
    {
        return new Organizer 
        {
            Id = id,
            Name = name,
            Email = email,
            Phone = "1234567890"
        };
    }

    public static Event BuildEvent(int id = 1, int organizerId = 1, string title = "Test Event")
    {
        return new Event
        {
            Id = id,
            Title = title,
            Description = "Test event desc",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Test Location",
            OrganizerId = organizerId
        };
    }

    public static Ticket BuildTicket(int id = 1, int eventId = 1)
    {
        return new Ticket
        {
            Id = id,
            EventId = eventId,
            Type = EventHub.Api.Enums.TicketType.Standard,
            Price = 50.0m,
            QuantityAvailable = 100
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
