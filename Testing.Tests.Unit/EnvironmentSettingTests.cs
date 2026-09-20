namespace Testing.Tests.Unit;

[Trait("Category", "Unit")]
public sealed class EnvironmentSettingTests : IDisposable
{
    private const int Minimum = 1;
    private const int Maximum = 5000;
    private const double MaximumPercent = 100.0;
    private const int BelowMinimum = Minimum - 1;
    private const int AboveMaximum = Maximum + 1;

    private readonly string _variableName = TestValues.NewVariableName();

    public void Dispose() => Environment.SetEnvironmentVariable(_variableName, null);

    [Fact]
    public void Count_VariableUnset_ReturnsTheDeclaredDefault()
    {
        // Arrange
        var declaredDefault = TestValues.NewCount();

        // Act
        var resolved = EnvironmentSetting.Count(_variableName, declaredDefault, Minimum, Maximum);

        // Assert
        Assert.Equal(declaredDefault, resolved);
    }

    [Fact]
    public void Count_VariableSetWithinBounds_OverridesTheDefault()
    {
        // Arrange
        var declaredDefault = TestValues.NewCount();
        var configured = TestValues.NewCount();
        Environment.SetEnvironmentVariable(_variableName, TestValues.AsInvariant(configured));

        // Act
        var resolved = EnvironmentSetting.Count(_variableName, declaredDefault, Minimum, Maximum);

        // Assert
        Assert.Equal(configured, resolved);
    }

    [Fact]
    public void Count_VariableBelowMinimum_ThrowsNamingTheVariable()
    {
        // Arrange
        Environment.SetEnvironmentVariable(_variableName, TestValues.AsInvariant(BelowMinimum));

        // Act
        var thrown = Assert.Throws<InvalidOperationException>(
            () => EnvironmentSetting.Count(_variableName, TestValues.NewCount(), Minimum, Maximum));

        // Assert
        Assert.Contains(_variableName, thrown.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Count_VariableAboveMaximum_ThrowsNamingTheVariable()
    {
        // Arrange
        Environment.SetEnvironmentVariable(_variableName, TestValues.AsInvariant(AboveMaximum));

        // Act
        var thrown = Assert.Throws<InvalidOperationException>(
            () => EnvironmentSetting.Count(_variableName, TestValues.NewCount(), Minimum, Maximum));

        // Assert
        Assert.Contains(_variableName, thrown.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Count_VariableNotANumber_ThrowsRatherThanFallingBackToTheDefault()
    {
        // Arrange
        Environment.SetEnvironmentVariable(_variableName, TestValues.NewUnparsableNumber());

        // Act
        var thrown = Assert.Throws<InvalidOperationException>(
            () => EnvironmentSetting.Count(_variableName, TestValues.NewCount(), Minimum, Maximum));

        // Assert
        Assert.Contains(_variableName, thrown.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Percent_VariableSetWithinBounds_OverridesTheDefault()
    {
        // Arrange
        var configured = TestValues.NewPercent();
        Environment.SetEnvironmentVariable(_variableName, TestValues.AsInvariant(configured));

        // Act
        var resolved = EnvironmentSetting.Percent(_variableName, TestValues.NewPercent(), MaximumPercent);

        // Assert
        Assert.Equal(configured, resolved);
    }

    [Fact]
    public void Percent_VariableUnset_ReturnsTheDeclaredDefault()
    {
        // Arrange
        var declaredDefault = TestValues.NewPercent();

        // Act
        var resolved = EnvironmentSetting.Percent(_variableName, declaredDefault, MaximumPercent);

        // Assert
        Assert.Equal(declaredDefault, resolved);
    }

    [Fact]
    public void Text_VariableSet_OverridesTheDefault()
    {
        // Arrange
        var configured = TestValues.NewText();
        Environment.SetEnvironmentVariable(_variableName, configured);

        // Act
        var resolved = EnvironmentSetting.Text(_variableName, TestValues.NewText());

        // Assert
        Assert.Equal(configured, resolved);
    }

    [Fact]
    public void Text_VariableIsWhitespace_ReturnsTheDeclaredDefault()
    {
        // Arrange
        var declaredDefault = TestValues.NewText();
        Environment.SetEnvironmentVariable(_variableName, " ");

        // Act
        var resolved = EnvironmentSetting.Text(_variableName, declaredDefault);

        // Assert
        Assert.Equal(declaredDefault, resolved);
    }
}
