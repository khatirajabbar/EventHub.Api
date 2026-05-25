namespace EventHub.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // "Admin" or "Member"
    public DateTime CreatedAt { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string EmailConfirmationToken { get; set; }
    public DateTime EmailConfirmationTokenExpiry { get; set; }
    public string PasswordResetToken { get; set; }
    public DateTime PasswordResetTokenExpiry { get; set; }
}

