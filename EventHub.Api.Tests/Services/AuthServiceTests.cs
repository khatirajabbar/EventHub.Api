using EventHub.Api.Data;
using EventHub.Api.Services;
using EventHub.Api.Settings;
using EventHub.Api.Tests.Fixtures;
using EventHub.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EventHub.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _jwtSettings = new JwtSettings
        {
            SecretKey = "EventHub-Secret-Key-Min-32-Characters-Required-For-Security!",
            Issuer = "EventHub.Api",
            Audience = "EventHub.Client",
            ExpirationMinutes = 60
        };
    }

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUserSuccessfully()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var registerDto = TestDataBuilder.BuildRegisterDto();

        // ACT
        var result = await authService.RegisterAsync(registerDto);

        // ASSERT
        result.Should().NotBeNull();
        result.Username.Should().Be(registerDto.Username);
        result.Email.Should().Be(registerDto.Email);
        result.Message.Should().Contain("Registration successful");
        
        var savedUser = context.Users.FirstOrDefault(u => u.Email == registerDto.Email);
        savedUser.Should().NotBeNull();
        savedUser.IsEmailConfirmed.Should().BeFalse();
        savedUser.EmailConfirmationToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var existingUser = TestDataBuilder.BuildUser(username: "duplicate");
        context.Users.Add(existingUser);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var registerDto = TestDataBuilder.BuildRegisterDto(username: "duplicate");

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await authService.RegisterAsync(registerDto)
        );
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var existingUser = TestDataBuilder.BuildUser(email: "duplicate@test.com");
        context.Users.Add(existingUser);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var registerDto = TestDataBuilder.BuildRegisterDto(email: "duplicate@test.com");

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await authService.RegisterAsync(registerDto)
        );
    }

    [Fact]
    public async Task RegisterAsync_SendsConfirmationEmail()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var registerDto = TestDataBuilder.BuildRegisterDto();

        // ACT
        await authService.RegisterAsync(registerDto);

        // ASSERT
        _mockEmailService.Verify(
            x => x.SendEmailAsync(registerDto.Email, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var loginDto = TestDataBuilder.BuildLoginDto();

        // ACT
        var result = await authService.LoginAsync(loginDto);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var loginDto = TestDataBuilder.BuildLoginDto(username: "nonexistent");

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.LoginAsync(loginDto)
        );
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var loginDto = TestDataBuilder.BuildLoginDto(password: "WrongPassword123");

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.LoginAsync(loginDto)
        );
    }

    [Fact]
    public async Task LoginAsync_WithUnconfirmedEmail_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        user.IsEmailConfirmed = false; // Email not confirmed
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var loginDto = TestDataBuilder.BuildLoginDto();

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.LoginAsync(loginDto)
        );
        exception.Message.Should().Contain("email address");
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewToken()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var oldRefreshToken = user.RefreshToken;

        // ACT
        var result = await authService.RefreshTokenAsync(oldRefreshToken);

        // ASSERT
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(oldRefreshToken); // New refresh token generated
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.RefreshTokenAsync("invalid-token")
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1); // Expired
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.RefreshTokenAsync(user.RefreshToken)
        );
        exception.Message.Should().Contain("expired");
    }

    #endregion

    #region Confirm Email Tests

    [Fact]
    public async Task ConfirmEmailAsync_WithValidToken_ConfirmsEmail()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        user.IsEmailConfirmed = false;
        user.EmailConfirmationToken = "valid-token";
        user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(1);
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT
        var result = await authService.ConfirmEmailAsync("valid-token", user.Email);

        // ASSERT
        result.Should().BeTrue();
        var updatedUser = context.Users.Find(user.Id);
        updatedUser.IsEmailConfirmed.Should().BeTrue();
        updatedUser.EmailConfirmationToken.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithInvalidToken_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        user.IsEmailConfirmed = false; // Make sure email is NOT confirmed
        user.EmailConfirmationToken = "valid-token"; // Set a different valid token
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.ConfirmEmailAsync("wrong-token", user.Email)
        );
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithExpiredToken_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        user.IsEmailConfirmed = false;
        user.EmailConfirmationToken = "expired-token";
        user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(-1); // Expired
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.ConfirmEmailAsync("expired-token", user.Email)
        );
        exception.Message.Should().Contain("expired");
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePasswordAsync_WithValidOldPassword_ChangesPassword()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        var originalHash = user.PasswordHash;
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT
        var result = await authService.ChangePasswordAsync(user.Id, "Test123", "NewTest456");

        // ASSERT
        result.Should().BeTrue();
        var updatedUser = context.Users.First(u => u.Id == user.Id);
        // Verify password was changed (not the same hash)
        updatedUser.PasswordHash.Should().NotBe(originalHash);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidOldPassword_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.ChangePasswordAsync(user.Id, "WrongPassword", "NewTest456")
        );
        exception.Message.Should().Contain("incorrect");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithNonexistentUser_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.ChangePasswordAsync(999, "Test123", "NewTest456")
        );
    }

    #endregion

    #region Forgot & Reset Password Tests

    [Fact]
    public async Task ForgotPasswordAsync_WithValidEmail_SendsResetEmail()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT
        var result = await authService.ForgotPasswordAsync(user.Email);

        // ASSERT
        result.Should().BeTrue();
        var updatedUser = context.Users.Find(user.Id);
        updatedUser.PasswordResetToken.Should().NotBeNullOrEmpty();
        updatedUser.PasswordResetTokenExpiry.Should().HaveValue();
        
        _mockEmailService.Verify(
            x => x.SendEmailAsync(user.Email, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithInvalidEmail_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await authService.ForgotPasswordAsync("nonexistent@example.com")
        );
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ResetsPassword()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        user.PasswordResetToken = "valid-reset-token";
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);
        var oldPasswordHash = user.PasswordHash;

        // ACT
        var result = await authService.ResetPasswordAsync("valid-reset-token", user.Email, "NewPassword456");

        // ASSERT
        result.Should().BeTrue();
        var updatedUser = context.Users.Find(user.Id);
        updatedUser.PasswordHash.Should().NotBe(oldPasswordHash);
        updatedUser.PasswordResetToken.Should().BeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.ResetPasswordAsync("wrong-token", user.Email, "NewPassword456")
        );
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ThrowsException()
    {
        // ARRANGE
        var context = DbContextFixture.CreateInMemoryDbContext();
        var user = TestDataBuilder.BuildUser();
        user.PasswordResetToken = "expired-reset-token";
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(-1); // Expired
        context.Users.Add(user);
        context.SaveChanges();

        var authService = new AuthService(context, _jwtSettings, _mockEmailService.Object, _mockLogger.Object);

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await authService.ResetPasswordAsync("expired-reset-token", user.Email, "NewPassword456")
        );
        exception.Message.Should().Contain("expired");
    }

    #endregion
}

