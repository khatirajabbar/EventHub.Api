namespace EventHub.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Member"; // "Admin" or "Member"
    public DateTime CreatedAt { get; set; }
    
    // Tokens made optional/nullable for PostgreSQL
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    
    public bool IsEmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; } // Added '?' to fix the crash!
    public DateTime EmailConfirmationTokenExpiry { get; set; }
    
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
}