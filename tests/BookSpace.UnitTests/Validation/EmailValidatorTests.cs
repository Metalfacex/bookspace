using BookSpace.Core.Validation;

namespace BookSpace.UnitTests.Validation;

public class EmailValidatorTests
{
    [Theory]
    [InlineData("student@uni.edu.ge", true)]
    [InlineData("  Student@UNI.EDU.GE ", true)]   // სივრცე და დიდი ასოები ნორმალიზდება
    [InlineData("student@edu.ge", true)]          // თავად დომენი
    [InlineData("student@gmail.com", false)]      // სხვა დომენი
    [InlineData("student@fakeedu.ge", false)]     // სუფიქსი ემთხვევა, მაგრამ ქვედომენი არაა
    [InlineData("not-an-email", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsUniversityEmail_ReturnsExpected(string? email, bool expected)
    {
        // Act
        var result = EmailValidator.IsUniversityEmail(email, "edu.ge");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_TrimsAndLowercases()
    {
        // Arrange
        const string raw = "  Ana@Uni.Edu.Ge ";

        // Act
        var normalized = EmailValidator.Normalize(raw);

        // Assert
        Assert.Equal("ana@uni.edu.ge", normalized);
    }
}
