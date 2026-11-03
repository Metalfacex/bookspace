using BookSpace.Core.Validation;

namespace BookSpace.UnitTests.Validation;

public class RoomValidatorTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Validate_CapacityBoundaries(int capacity, bool expectedValid)
    {
        // Act
        var errors = RoomValidator.Validate("ოთახი 101", "I კორპუსი", capacity);

        // Assert
        Assert.Equal(expectedValid, errors.Count == 0);
    }

    [Fact]
    public void Validate_NameLengthBoundary()
    {
        // Arrange
        var name100 = new string('a', 100);
        var name101 = new string('a', 101);

        // Act
        var ok = RoomValidator.Validate(name100, "I კორპუსი", 5);
        var tooLong = RoomValidator.Validate(name101, "I კორპუსი", 5);

        // Assert
        Assert.Empty(ok);
        Assert.Single(tooLong);
    }

    [Fact]
    public void Validate_EmptyNameAndLocation_ReturnsTwoErrors()
    {
        // Act
        var errors = RoomValidator.Validate("  ", null, 5);

        // Assert
        Assert.Equal(2, errors.Count);
    }
}
