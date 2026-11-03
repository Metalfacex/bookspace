using BookSpace.Core.Validation;

namespace BookSpace.UnitTests.Validation;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("abcdef1")]   // 7 სიმბოლო — ზღვარს ქვემოთ
    [InlineData("")]
    [InlineData(null)]
    public void Validate_TooShort_ReturnsError(string? password)
    {
        // Act
        var error = PasswordPolicy.Validate(password);

        // Assert
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_ExactlyMinLength_IsValid()
    {
        // Arrange
        const string password = "abcdefg1"; // 8 სიმბოლო

        // Act
        var error = PasswordPolicy.Validate(password);

        // Assert
        Assert.Null(error);
    }

    [Fact]
    public void Validate_MaxLengthBoundary()
    {
        // Arrange
        var atMax = new string('a', 99) + "1";   // 100
        var overMax = new string('a', 100) + "1"; // 101

        // Act
        var atMaxError = PasswordPolicy.Validate(atMax);
        var overMaxError = PasswordPolicy.Validate(overMax);

        // Assert
        Assert.Null(atMaxError);
        Assert.NotNull(overMaxError);
    }

    [Theory]
    [InlineData("abcdefgh")]  // ციფრის გარეშე
    [InlineData("12345678")]  // ასოს გარეშე
    public void Validate_MissingLetterOrDigit_ReturnsError(string password)
    {
        // Act
        var error = PasswordPolicy.Validate(password);

        // Assert
        Assert.NotNull(error);
    }
}
