namespace Testing.Playwright;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Microsoft.Playwright;
using Xunit;
using Xunit.v3;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class PlaywrightArtifactFinalizerAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        ArgumentNullException.ThrowIfNull(test);
        PlaywrightArtifactRecorder.Clear(test.UniqueID);
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        ArgumentNullException.ThrowIfNull(test);
        var state = TestContext.Current.TestState;
        PlaywrightArtifactRecorder.Finalize(test.UniqueID, state);
    }
}

internal sealed class PlaywrightArtifactSession : IAsyncDisposable
{
    private readonly PlaywrightArtifactRecorder _recorder;
    private readonly IBrowserContext _context;
    private readonly IPage _page;
    private bool _disposed;

    public PlaywrightArtifactSession(PlaywrightArtifactRecorder recorder, IBrowserContext context, IPage page)
    {
        _recorder = recorder;
        _context = context;
        _page = page;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await _recorder.CompleteAsync(_context, _page).ConfigureAwait(false);
    }
}

public sealed class PlaywrightArtifactRecorder
{
    private const string TempFolderName = ".tmp";
    private const string TestResultsFolderName = "TestResults";
    private const string ArtifactsFolderName = "PlaywrightArtifacts";
    private const string ScreenshotFileName = "screenshot.png";
    private const string TraceFileName = "trace.zip";
    private const string BrowserLogFileName = "browser-log.txt";
    private const string MetadataFileName = "metadata.json";
    private const string FailureFileName = "failure.json";
    private const string UnnamedTestDisplayName = "Unknown test";
    private const int RecordedVideoWidth = 1280;
    private const int RecordedVideoHeight = 720;
    private const int MaxArtifactNameLength = 120;
    private const string ConsoleEventTag = "CONSOLE";
    private const string PageErrorEventTag = "PAGEERROR";
    private const string RequestEventTag = "REQ";
    private const string ResponseEventTag = "RESP";
    private const string RequestFailedEventTag = "FAIL";
    private const string ScreenshotErrorEventTag = "SCREENSHOT_ERROR";
    private const string TraceErrorEventTag = "TRACE_ERROR";
    private const string ContextDisposeErrorEventTag = "CONTEXT_DISPOSE_ERROR";

    private static readonly ConcurrentDictionary<string, ConcurrentBag<PendingArtifact>> PendingArtifacts =
        new ConcurrentDictionary<string, ConcurrentBag<PendingArtifact>>();

    private readonly string _appName;
    private readonly string _suiteName;
    private readonly string _testId;
    private readonly string _testName;
    private readonly string _artifactName;
    private readonly string _tempDirectory;
    private readonly string _finalDirectory;
    private readonly bool _belongsToTest;
    private readonly List<string> _events = new List<string>();
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;

    private PlaywrightArtifactRecorder(string appName, string suiteName)
    {
        var test = TestContext.Current.Test;
        _appName = appName;
        _suiteName = suiteName;
        _belongsToTest = test is not null;
        _testId = test?.UniqueID ?? $"unknown-{Guid.NewGuid():N}";
        _testName = test?.TestDisplayName ?? UnnamedTestDisplayName;
        _artifactName = Sanitize(_testName);

        var root = Path.Combine(AppContext.BaseDirectory, TestResultsFolderName, ArtifactsFolderName);
        var runFolderName = Guid.NewGuid().ToString("N");
        _tempDirectory = Path.Combine(root, TempFolderName, _testId, runFolderName);
        _finalDirectory = Path.Combine(root, _suiteName, _artifactName);
        Directory.CreateDirectory(_tempDirectory);
    }

    public static async Task<(IAsyncDisposable Context, IPage Page)> CreateSessionAsync(
        IBrowser browser,
        string appName,
        string suiteName,
        BrowserNewContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(options);

        var recorder = new PlaywrightArtifactRecorder(appName, suiteName);
        options.RecordVideoDir = recorder._tempDirectory;
        options.RecordVideoSize = new RecordVideoSize { Width = RecordedVideoWidth, Height = RecordedVideoHeight };

        var context = await browser.NewContextAsync(options).ConfigureAwait(false);
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        }).ConfigureAwait(false);

        var page = await context.NewPageAsync().ConfigureAwait(false);
        recorder.Attach(page);
        return (new PlaywrightArtifactSession(recorder, context, page), page);
    }

    public static void Clear(string testId)
    {
        if (PendingArtifacts.TryRemove(testId, out var artifacts))
        {
            foreach (var artifact in artifacts)
            {
                DeleteDirectory(artifact.TempDirectory);
            }
        }
    }

    public static void Finalize(string testId, TestResultState? state)
    {
        if (!PendingArtifacts.TryRemove(testId, out var artifacts))
        {
            return;
        }

        var failed = state?.Result == TestResult.Failed;
        foreach (var artifact in artifacts)
        {
            var tempParent = Path.GetDirectoryName(artifact.TempDirectory);
            if (!failed)
            {
                DeleteDirectory(artifact.TempDirectory);
                DeleteDirectoryIfEmpty(tempParent);
                continue;
            }

            Directory.CreateDirectory(artifact.FinalDirectory);
            var targetDirectory = Path.Combine(artifact.FinalDirectory, artifact.ContextId.ToString("N"));
            if (Directory.Exists(targetDirectory))
            {
                DeleteDirectory(targetDirectory);
            }

            Directory.Move(artifact.TempDirectory, targetDirectory);
            DeleteDirectoryIfEmpty(tempParent);
            WriteFailureMetadata(targetDirectory, state);
        }
    }

    public void Attach(IPage page)
    {
        ArgumentNullException.ThrowIfNull(page);
        page.Console += (_, msg) => AddEvent($"{ConsoleEventTag} {msg.Type} {msg.Text}");
        page.PageError += (_, error) => AddEvent($"{PageErrorEventTag} {error}");
        page.Request += (_, request) => AddEvent($"{RequestEventTag} {request.Method} {request.Url}");
        page.Response += (_, response) => AddEvent($"{ResponseEventTag} {response.Status} {response.Url}");
        page.RequestFailed += (_, request) =>
            AddEvent($"{RequestFailedEventTag} {request.Method} {request.Url} err={request.Failure}");
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Artifact capture is diagnostics. Any failure to screenshot, trace or dispose is recorded in the browser log and must never fail or mask the test that was actually running.")]
    public async Task CompleteAsync(IBrowserContext context, IPage page)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(page);

        try
        {
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(_tempDirectory, ScreenshotFileName),
                FullPage = true
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            AddEvent($"{ScreenshotErrorEventTag} {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            await context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(_tempDirectory, TraceFileName)
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            AddEvent($"{TraceErrorEventTag} {ex.GetType().Name}: {ex.Message}");
        }

        try
        {
            await context.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            AddEvent($"{ContextDisposeErrorEventTag} {ex.GetType().Name}: {ex.Message}");
        }

        string[] recordedEvents;
        lock (_events)
        {
            recordedEvents = _events.ToArray();
        }

        await File.WriteAllLinesAsync(Path.Combine(_tempDirectory, BrowserLogFileName), recordedEvents)
            .ConfigureAwait(false);
        await WriteMetadataAsync(Path.Combine(_tempDirectory, MetadataFileName)).ConfigureAwait(false);
        if (!_belongsToTest)
        {
            var parent = Path.GetDirectoryName(_tempDirectory);
            DeleteDirectory(_tempDirectory);
            DeleteDirectoryIfEmpty(parent);
            return;
        }

        var pendingArtifactId = Guid.NewGuid();
        PendingArtifacts.GetOrAdd(_testId, _ => new ConcurrentBag<PendingArtifact>()).Add(
            new PendingArtifact(_tempDirectory, _finalDirectory, pendingArtifactId));
    }

    private static void WriteFailureMetadata(string directory, TestResultState? state)
    {
        var path = Path.Combine(directory, FailureFileName);
        var payload = new
        {
            outcome = state?.Result.ToString(),
            executionTimeSeconds = state?.ExecutionTime,
            exceptionTypes = state?.ExceptionTypes,
            exceptionMessages = state?.ExceptionMessages,
            exceptionStackTraces = state?.ExceptionStackTraces,
            failureCause = state?.FailureCause?.ToString()
        };
        File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions()));
    }

    private static JsonSerializerOptions JsonOptions() => new JsonSerializerOptions { WriteIndented = true };

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) || char.IsWhiteSpace(ch) ? '_' : ch).ToArray();
        var sanitized = new string(chars);
        return sanitized.Length <= MaxArtifactNameLength ? sanitized : sanitized[..MaxArtifactNameLength];
    }

    private static void DeleteDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void DeleteDirectoryIfEmpty(string? directory)
    {
        if (directory is not null && Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    private void AddEvent(string message)
    {
        lock (_events)
        {
            _events.Add($"[{DateTimeOffset.UtcNow:O}] {message}");
        }
    }

    private async Task WriteMetadataAsync(string path)
    {
        var payload = new
        {
            app = _appName,
            suite = _suiteName,
            testId = _testId,
            testName = _testName,
            artifactName = _artifactName,
            startedAt = _startedAt,
            completedAt = DateTimeOffset.UtcNow,
            githubRunId = Environment.GetEnvironmentVariable("GITHUB_RUN_ID"),
            githubRunAttempt = Environment.GetEnvironmentVariable("GITHUB_RUN_ATTEMPT"),
            githubRepository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY"),
            githubSha = Environment.GetEnvironmentVariable("GITHUB_SHA"),
            githubRef = Environment.GetEnvironmentVariable("GITHUB_REF")
        };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(payload, JsonOptions())).ConfigureAwait(false);
    }

    private sealed class PendingArtifact(string tempDirectory, string finalDirectory, Guid contextId)
    {
        public string TempDirectory { get; } = tempDirectory;

        public string FinalDirectory { get; } = finalDirectory;

        public Guid ContextId { get; } = contextId;
    }
}
