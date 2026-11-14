# Target Scheduler Discord

N.I.N.A. plugin that adds an Advanced Sequencer instruction to post **Target Scheduler** target progress to Discord.

## Features

- Instruction **TS: Send target status to Discord**
- One line per active target (`ACTIVE` projects)
- Overall completion percentage (Accepted / Desired, matching Target Scheduler)
- Per-filter counts: `Accepted/Desired (acq Acquired)`
- Sorted by completion percentage (descending)
- Discord webhook configurable in plugin options
- Target Scheduler API URL configurable (default `http://localhost:8188/ts/v0`)

## Requirements

- N.I.N.A. ≥ 3.3.0.1060
- [Target Scheduler](https://github.com/tcpalmer/nina.plugin.targetscheduler) plugin with its REST API enabled
- A Discord webhook

## Configuration

1. Options → Plugins → **Target Scheduler Discord**
2. Paste the Discord webhook URL
3. Adjust the Target Scheduler API URL if needed
4. Optionally click **Send status now** to test without running a sequence
5. In Advanced Sequencer, add **TS: Send target status to Discord**

## Example Discord message

```text
**Target Scheduler progress** — 2026-11-14 20:00 UTC
`  72.5%` **Nebulae / M31** — Ha 45/60 (acq 50) · OIII 20/30 (acq 22)
`  10.0%` **Galaxies / M81** — L 3/30 (acq 3)
```

## Build & release (CI)

GitHub Actions workflows:

- `ci.yml` — restore / build / test / validation packaging on every push
- `release.yml` — on a `vMAJOR.MINOR.PATCH.BUILD` tag (e.g. `v1.0.0.0`), produces the ZIP + manifest and creates the GitHub Release

```powershell
git tag v1.0.0.0
git push origin v1.0.0.0
```

## Local development

.NET SDK **10.0.4xx** + PowerShell 7.

```powershell
dotnet restore NINA.Plugin.TargetScheduler.Discord.slnx
dotnet build NINA.Plugin.TargetScheduler.Discord.slnx -c Release
dotnet test NINA.Plugin.TargetScheduler.Discord.slnx -c Release
dotnet msbuild src/NINA.Plugin.TargetScheduler.Discord/NINA.Plugin.TargetScheduler.Discord.csproj -t:DeployPlugin -p:Configuration=Release
```

Copy `local.props.example` to `local.props` to set `NinaExecutable` if needed.
