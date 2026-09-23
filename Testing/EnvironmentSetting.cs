namespace Testing;

using System.Globalization;

public static class EnvironmentSetting
{
    private const NumberStyles PercentStyles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
        | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public static int Count(string variableName, int defaultValue, int minimum, int maximum)
    {
        var raw = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            || value < minimum
            || value > maximum)
        {
            throw new InvalidOperationException(
                $"{variableName} must be an integer between {minimum} and {maximum}; got {raw}.");
        }

        return value;
    }

    public static double Percent(string variableName, double defaultValue, double maximum)
    {
        var raw = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (!double.TryParse(raw, PercentStyles, CultureInfo.InvariantCulture, out var value)
            || value < 0
            || value > maximum)
        {
            throw new InvalidOperationException(
                $"{variableName} must be a percentage between 0 and {maximum}; got {raw}.");
        }

        return value;
    }

    public static string Text(string variableName, string defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrWhiteSpace(raw) ? defaultValue : raw;
    }
}
