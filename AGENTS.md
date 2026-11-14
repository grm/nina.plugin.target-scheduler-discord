# AGENTS.md

Guidance for agents working in this repository.

## Project

N.I.N.A. plugin that posts **Target Scheduler** target progress to Discord via a sequencer instruction.

| Piece | Location |
| --- | --- |
| Plugin identity / manifest metadata | `Plugin.props` |
| Sequencer instruction | `src/.../Features/instruction/SendTargetStatusInstruction.cs` |
| Plugin options (webhook, TS API URL) | `src/.../Features/settings/PluginSettings.cs` |
| TS REST client | `src/.../Services/TargetSchedulerClient.cs` |
| Discord webhook | `src/.../Services/DiscordWebhookClient.cs` |
| Progress math / Discord formatting | `TargetProgressCalculator.cs`, `TargetProgressFormatter.cs` |
| CI / release packaging | `.github/workflows/`, `build/` |

Also see [CURSOR.md](CURSOR.md).

## Constraints

- Target **net8.0-windows** and `NINA.Plugin` **3.2.0.9001** (matching `Directory.Build.props`) for N.I.N.A. 3.2 compatibility.
- Do not rename exported sequencer types/namespaces after publishing (saved sequences store FQTNs).
- Keep webhook URL and secrets out of git; settings live in NINA plugin settings JSON under `%LOCALAPPDATA%`.
- Prefer the Target Scheduler **HTTP API** (`/ts/v0`) over referencing the TS plugin assembly.
- Completion % uses **Accepted / Desired** (TS semantics). Discord lines also show acquired counts.
- Release tags must be `vMAJOR.MINOR.PATCH.BUILD`. CI builds and publishes; local build is optional.

## Style

- Follow surrounding C# / XAML style; CRLF for new files unless the location dictates otherwise.
- Keep sequencer entities MEF-exported with `Clone()` fidelity.
- Add/adjust unit tests under `tests/` for calculator and formatter changes.

## Verify

```powershell
dotnet test NINA.Plugin.TargetScheduler.Discord.slnx -c Release
```

For a release dry-run packaging check:

```powershell
dotnet msbuild src/NINA.Plugin.TargetScheduler.Discord/NINA.Plugin.TargetScheduler.Discord.csproj -t:PackagePlugin -p:Configuration=Release
```
