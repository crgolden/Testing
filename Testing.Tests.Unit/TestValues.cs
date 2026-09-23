namespace Testing.Tests.Unit;

using System.Globalization;

internal static class TestValues
{
    private const string UppercaseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseAlphabet = "abcdefghijklmnopqrstuvwxyz";
    private const int VariableNameLength = 12;
    private const int SmallestCount = 1;
    private const int LargestCount = 1000;

    internal static string NewVariableName() =>
        $"CRGOLDEN_TESTING_{Token(UppercaseAlphabet, VariableNameLength)}";

    internal static int NewCount() => Random.Shared.Next(SmallestCount, LargestCount);

    internal static double NewPercent() => Math.Round(Random.Shared.NextDouble() * 100, 2);

    internal static int NewCountCeiling() => Random.Shared.Next(LargestCount, LargestCount * 10);

    internal static double NewPercentCeiling() => Random.Shared.Next(101, 1000);

    internal static string NewText() => Token(LowercaseAlphabet, VariableNameLength);

    internal static string NewUnparsableNumber() => Token(LowercaseAlphabet, VariableNameLength);

    internal static string Padded(string value) => $" {value} ";

    internal static string AsInvariant(int value) => value.ToString(CultureInfo.InvariantCulture);

    internal static string AsInvariant(double value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Token(string alphabet, int length) =>
        string.Concat(
            Enumerable.Range(0, length).Select(_ => alphabet[Random.Shared.Next(alphabet.Length)]));
}
