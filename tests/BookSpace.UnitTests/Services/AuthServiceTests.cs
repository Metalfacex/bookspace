using BookSpace.Core.Common;
using BookSpace.Core.Domain;
using BookSpace.Core.Rules;
using BookSpace.Core.Services;
using BookSpace.UnitTests.Fakes;

namespace BookSpace.UnitTests.Services;

/// <summary>US-01 რეგისტრაცია და US-02 შესვლა.</summary>
public class AuthServiceTests
{
    private readonly InMemoryUserRepository _users = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_users, new FakePasswordHasher(), new FakeClock(),
            new AccountRules { AllowedEmailDomain = "edu.ge" });
    }

    [Fact]
    public async Task Register_ValidUniversityEmail_CreatesStudentAccount()
    {
        // Arrange
        const string email = "Nino@Uni.Edu.Ge";

        // Act
        var result = await _sut.RegisterAsync(email, "Password1");

        // Assert
        Assert.True(result.IsSuccess);
        var saved = Assert.Single(_users.Items);
        Assert.Equal("nino@uni.edu.ge", saved.Email);
        Assert.Equal(UserRole.Student, saved.Role);
        Assert.NotEqual("Password1", saved.PasswordHash); // პაროლი ღია სახით არ ინახება
    }

    [Fact]
    public async Task Register_EmailAlreadyUsed_ReturnsConflict()
    {
        // Arrange
        _users.Seed("nino@uni.edu.ge");

        // Act
        var result = await _sut.RegisterAsync("NINO@uni.edu.ge", "Password1");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Conflict, result.Error);
        Assert.Single(_users.Items);
    }

    [Fact]
    public async Task Register_NonUniversityEmail_ReturnsValidationError()
    {
        // Act
        var result = await _sut.RegisterAsync("nino@gmail.com", "Password1");

        // Assert
        Assert.Equal(ErrorCode.Validation, result.Error);
        Assert.Empty(_users.Items);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsValidationError()
    {
        // Act
        var result = await _sut.RegisterAsync("nino@uni.edu.ge", "short1");

        // Assert
        Assert.Equal(ErrorCode.Validation, result.Error);
        Assert.Empty(_users.Items);
    }

    [Fact]
    public async Task Login_CorrectCredentials_ReturnsUser()
    {
        // Arrange
        var user = _users.Seed("nino@uni.edu.ge", "hashed:Password1");

        // Act
        var result = await _sut.LoginAsync(" NINO@uni.edu.ge ", "Password1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value!.Id);
    }

    [Fact]
    public async Task Login_WrongPassword_IsRejected()
    {
        // Arrange
        _users.Seed("nino@uni.edu.ge", "hashed:Password1");

        // Act
        var result = await _sut.LoginAsync("nino@uni.edu.ge", "WrongPass9");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Unauthorized, result.Error);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsSameMessageAsWrongPassword()
    {
        // Act
        var result = await _sut.LoginAsync("nobody@uni.edu.ge", "Password1");

        // Assert
        Assert.Equal(AuthService.InvalidCredentialsMessage, result.Message);
    }
}
