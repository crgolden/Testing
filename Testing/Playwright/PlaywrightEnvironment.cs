namespace Testing.Playwright;

using Microsoft.Playwright;

public static class PlaywrightEnvironment
{
    private const string InstallCommand = "install";

    private const string ChromiumTarget = "chromium";

    private const string HeadedVariable = "PLAYWRIGHT_HEADED";

    private const string HeadedEnabledValue = "1";

    public static bool Headless =>
        !string.Equals(
            Environment.GetEnvironmentVariable(HeadedVariable),
            HeadedEnabledValue,
            StringComparison.OrdinalIgnoreCase);

    public static void InstallChromium()
    {
        var exitCode = Program.Main([InstallCommand, ChromiumTarget]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright install failed with exit code {exitCode}.");
        }
    }
}
