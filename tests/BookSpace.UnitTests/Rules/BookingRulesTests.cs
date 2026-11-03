using BookSpace.Core.Rules;

namespace BookSpace.UnitTests.Rules;

public class BookingRulesTests
{
    [Theory]
    [InlineData(7, false)]  // გახსნამდე
    [InlineData(8, true)]   // პირველი სლოტი
    [InlineData(21, true)]  // ბოლო სლოტი (21:00–22:00)
    [InlineData(22, false)] // დახურვის საათი
    public void IsWithinOpeningHours_Boundaries(int hour, bool expected)
    {
        // Arrange
        var rules = new BookingRules { OpeningHour = 8, ClosingHour = 22 };

        // Act
        var result = rules.IsWithinOpeningHours(hour);

        // Assert
        Assert.Equal(expected, result);
    }
}
