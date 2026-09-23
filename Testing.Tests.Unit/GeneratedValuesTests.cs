namespace Testing.Tests.Unit;

[Trait("Category", "Unit")]
public sealed class GeneratedValuesTests
{
    [Fact]
    public void NewPassword_Generated_CarriesEveryCharacterClassIdentityRequires()
    {
        // Act
        var password = Generated.NewPassword();

        // Assert
        Assert.Matches("[A-Z]", password);
        Assert.Matches("[a-z]", password);
        Assert.Matches("[0-9]", password);
        Assert.Matches("[^A-Za-z0-9]", password);
    }

    [Fact]
    public void NewUtcTimestamp_Generated_IsUtcAndInThePast()
    {
        // Act
        var timestamp = Generated.NewUtcTimestamp();

        // Assert
        Assert.Equal(TimeSpan.Zero, timestamp.Offset);
        Assert.True(timestamp < DateTimeOffset.UtcNow);
    }

    [Fact]
    public void NewTimestampWithNonZeroOffset_Generated_HasANonZeroOffset()
    {
        // Act
        var timestamp = Generated.NewTimestampWithNonZeroOffset();

        // Assert
        Assert.NotEqual(TimeSpan.Zero, timestamp.Offset);
    }

    [Fact]
    public void NewDayOfWeek_Generated_IsAValidDayIndex()
    {
        // Act
        var day = Generated.NewDayOfWeek();

        // Assert
        Assert.InRange(day, (byte)DayOfWeek.Sunday, (byte)DayOfWeek.Saturday);
    }

    [Fact]
    public void NewEmailAddress_Generated_UsesTheReservedInvalidDomain()
    {
        // Act
        var emailAddress = Generated.NewEmailAddress();

        // Assert
        Assert.Matches(@"^[a-z]+@[a-z]+\.invalid$", emailAddress);
    }

    [Fact]
    public void NewTokenFromFirstHalfOfAlphabet_Generated_DrawsOnlyFromAToM()
    {
        // Arrange
        var length = TestValues.NewCount();

        // Act
        var token = Generated.NewTokenFromFirstHalfOfAlphabet(length);

        // Assert
        Assert.Matches($"^[a-m]{{{length}}}$", token);
    }

    [Fact]
    public void NewTokenFromSecondHalfOfAlphabet_Generated_DrawsOnlyFromNToZ()
    {
        // Arrange
        var length = TestValues.NewCount();

        // Act
        var token = Generated.NewTokenFromSecondHalfOfAlphabet(length);

        // Assert
        Assert.Matches($"^[n-z]{{{length}}}$", token);
    }
}
