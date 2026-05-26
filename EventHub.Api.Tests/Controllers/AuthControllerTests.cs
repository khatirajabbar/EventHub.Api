using EventHub.Api.Controllers;
using EventHub.Api.Data;
using EventHub.Api.DTOs.User;
using EventHub.Api.Services;
using EventHub.Api.Settings;
using EventHub.Api.Tests.Fixtures;
using EventHub.Api.Tests.Helpers;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EventHub.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _context = DbContextFixture.CreateInMemoryDbContext();

        // Setup AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<EventHub.Api.Entities.User, UserResponseDto>();
        });
        _mapper = mapperConfig.CreateMapper();
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var registerDto = TestDataBuilder.BuildRegisterDto();
        var registerResponse = new EventHub.Api.DTOs.Auth.RegisterResponseDto
        {
            Id = 1,
            Username = registerDto.Username,
            Email = registerDto.Email,
            Role = "Member",
            Message = "Registration successful!"
        };

        _mockAuthService.Setup(x => x.RegisterAsync(registerDto))
            .ReturnsAsync(registerResponse);

        // ACT
        var result = await controller.Register(registerDto);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedValue = okResult.Value as EventHub.Api.DTOs.Auth.RegisterResponseDto;
        returnedValue.Username.Should().Be(registerDto.Username);
        returnedValue.Email.Should().Be(registerDto.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var registerDto = TestDataBuilder.BuildRegisterDto();

        _mockAuthService.Setup(x => x.RegisterAsync(registerDto))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // ACT
        var result = await controller.Register(registerDto);

        // ASSERT
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var loginDto = TestDataBuilder.BuildLoginDto();
        var authResponse = new EventHub.Api.DTOs.Auth.AuthResponseDto
        {
            Id = 1,
            Username = loginDto.Username,
            Email = "test@example.com",
            Role = "Member",
            Token = "jwt-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            RefreshToken = "refresh-token"
        };

        _mockAuthService.Setup(x => x.LoginAsync(loginDto))
            .ReturnsAsync(authResponse);

        // ACT
        var result = await controller.Login(loginDto);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedValue = okResult.Value as EventHub.Api.DTOs.Auth.AuthResponseDto;
        returnedValue.Token.Should().NotBeNullOrEmpty();
        returnedValue.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var loginDto = TestDataBuilder.BuildLoginDto();

        _mockAuthService.Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // ACT
        var result = await controller.Login(loginDto);

        // ASSERT
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewToken()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var refreshRequest = new EventHub.Api.DTOs.Auth.RefreshTokenRequestDto
        {
            RefreshToken = "valid-refresh-token"
        };
        var authResponse = new EventHub.Api.DTOs.Auth.AuthResponseDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Role = "Member",
            Token = "new-jwt-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            RefreshToken = "new-refresh-token"
        };

        _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshRequest.RefreshToken))
            .ReturnsAsync(authResponse);

        // ACT
        var result = await controller.RefreshToken(refreshRequest);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var returnedValue = okResult.Value as EventHub.Api.DTOs.Auth.AuthResponseDto;
        returnedValue.Token.Should().Be("new-jwt-token");
        returnedValue.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var refreshRequest = new EventHub.Api.DTOs.Auth.RefreshTokenRequestDto
        {
            RefreshToken = "invalid-token"
        };

        _mockAuthService.Setup(x => x.RefreshTokenAsync(refreshRequest.RefreshToken))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid refresh token"));

        // ACT
        var result = await controller.RefreshToken(refreshRequest);

        // ASSERT
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_WithValidData_ReturnsOk()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var changePasswordDto = TestDataBuilder.BuildChangePasswordDto();

        // Mock User.FindFirst to return userId
        var claims = new List<Claim>
        {
            new Claim("sub", "1")
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mockAuthService.Setup(x => x.ChangePasswordAsync(1, changePasswordDto.OldPassword, changePasswordDto.NewPassword))
            .ReturnsAsync(true);

        // ACT
        var result = await controller.ChangePassword(changePasswordDto);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ChangePassword_WithInvalidOldPassword_ReturnsBadRequest()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var changePasswordDto = TestDataBuilder.BuildChangePasswordDto();

        var claims = new List<Claim>
        {
            new Claim("sub", "1")
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        _mockAuthService.Setup(x => x.ChangePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new UnauthorizedAccessException("Old password is incorrect"));

        // ACT
        var result = await controller.ChangePassword(changePasswordDto);

        // ASSERT
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion

    #region Get Profile Tests

    [Fact]
    public async Task GetProfile_WithValidToken_ReturnsUserProfile()
    {
        // ARRANGE
        var user = TestDataBuilder.BuildUser();
        _context.Users.Add(user);
        _context.SaveChanges();

        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // ACT
        var result = await controller.GetProfile();

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        
        var profile = okResult.Value as UserResponseDto;
        profile.Id.Should().Be(user.Id);
        profile.Username.Should().Be(user.Username);
        profile.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var claims = new List<Claim>(); // No sub claim
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // ACT
        var result = await controller.GetProfile();

        // ASSERT
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion

    #region Confirm Email Tests

    [Fact]
    public async Task ConfirmEmail_WithValidToken_ReturnsOk()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);

        _mockAuthService.Setup(x => x.ConfirmEmailAsync("valid-token", "test@example.com"))
            .ReturnsAsync(true);

        // ACT
        var result = await controller.ConfirmEmail("valid-token", "test@example.com");

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_ReturnsUnauthorized()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);

        _mockAuthService.Setup(x => x.ConfirmEmailAsync("invalid-token", "test@example.com"))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid token"));

        // ACT
        var result = await controller.ConfirmEmail("invalid-token", "test@example.com");

        // ASSERT
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ConfirmEmail_WithMissingParameters_ReturnsBadRequest()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);

        // ACT
        var result = await controller.ConfirmEmail("", "");

        // ASSERT
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region Forgot Password Tests

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsOk()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var forgotPasswordDto = TestDataBuilder.BuildForgotPasswordDto();

        _mockAuthService.Setup(x => x.ForgotPasswordAsync(forgotPasswordDto.Email))
            .ReturnsAsync(true);

        // ACT
        var result = await controller.ForgotPassword(forgotPasswordDto);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ForgotPassword_WithNonexistentEmail_ReturnsBadRequest()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var forgotPasswordDto = TestDataBuilder.BuildForgotPasswordDto("nonexistent@example.com");

        _mockAuthService.Setup(x => x.ForgotPasswordAsync(forgotPasswordDto.Email))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // ACT
        var result = await controller.ForgotPassword(forgotPasswordDto);

        // ASSERT
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region Reset Password Tests

    [Fact]
    public async Task ResetPassword_WithValidData_ReturnsOk()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var resetPasswordDto = TestDataBuilder.BuildResetPasswordDto();

        _mockAuthService.Setup(x => x.ResetPasswordAsync(resetPasswordDto.Token, resetPasswordDto.Email, resetPasswordDto.NewPassword))
            .ReturnsAsync(true);

        // ACT
        var result = await controller.ResetPassword(resetPasswordDto);

        // ASSERT
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsUnauthorized()
    {
        // ARRANGE
        var controller = new AuthController(_mockAuthService.Object, _context, _mapper);
        var resetPasswordDto = TestDataBuilder.BuildResetPasswordDto(token: "invalid-token");

        _mockAuthService.Setup(x => x.ResetPasswordAsync(resetPasswordDto.Token, resetPasswordDto.Email, resetPasswordDto.NewPassword))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid token"));

        // ACT
        var result = await controller.ResetPassword(resetPasswordDto);

        // ASSERT
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion
}

