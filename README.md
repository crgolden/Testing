# Testing

Shared test-support code for the `crgolden` apps, published as the NuGet package `Testing` to the
`crgolden` GitHub Packages feed.

**Referenced by test projects only.** That is the whole reason this repo exists separately from
[`Shared`](https://github.com/crgolden/Shared): NuGet dependencies are package-level, not
assembly-level, so putting Playwright and xunit into `Shared` would put them into the restore graph
of every `Shared` consumer — including Directory and Functions, which are production services with
no business downloading a browser automation library.

## Contents

| Namespace | What it is |
|---|---|
| `Testing.EnvironmentSetting` | Typed environment-variable resolution with a declared default and bounds. A default in source is the config schema; the value itself stays settable without a recompile. |
| `Testing.Playwright.PlaywrightEnvironment` | Headless/headed resolution from `PLAYWRIGHT_HEADED`, and the Chromium install. |
| `Testing.Playwright.PlaywrightArtifactRecorder` | Per-test screenshot, trace, video, browser log and metadata capture, retained on failure and discarded on success. |
| `Testing.Playwright.PlaywrightArtifactFinalizerAttribute` | The xunit `BeforeAfterTestAttribute` that drives the recorder's retain/discard decision. |

**The finalizer attribute must be applied by each consuming test assembly**, because an
assembly-level attribute only applies to the assembly that declares it:

```csharp
[assembly: Testing.Playwright.PlaywrightArtifactFinalizer]
```

## Build and test

```
dotnet build Testing.slnx
dotnet test --project Testing.Tests.Unit --configuration Release -- --filter-trait "Category=Unit"
```

## Consuming the package

The consumer's root `NuGet.Config` lists `nuget.org` and `GitHub` and pins `Testing` to `GitHub`
with `packageSourceMapping`. Credentials are per machine in `%AppData%\NuGet\NuGet.Config`, and in
CI come from `PACKAGES_READ_TOKEN`. Add the `PackageReference` to the **test** project, never to the
project under test.

## Versioning and publishing

Identical to `Shared`: push to `main` publishes a preview version computed by GitVersion;
`workflow_dispatch` packs `Testing.csproj`'s `<Version>` verbatim and tags `v<version>`.
