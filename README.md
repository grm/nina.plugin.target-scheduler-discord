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

- N.I.N.A. ≥ 3.2.0.9001
- [Target Scheduler](https://github.com/tcpalmer/nina.plugin.targetscheduler) plugin with its REST API enabled
- A Discord webhook

## Configuration

1. Options → Plugins → **Target Scheduler Discord**
2. Paste the Discord webhook URL
3. Adjust the Target Scheduler API URL if needed
4. Optionally click **Send status now** to test without running a sequence
5. In Advanced Sequencer, add **TS: Send target status to Discord**

## Example Discord message

Posted as a Discord embed (Night Summary–style), for example:

```text
🔭 Target Scheduler Progress
2026-11-14 20:00 UTC

**Overview**
Targets: 2
Complete: 0
In progress: 2
Not started: 0
Accepted / Desired: 68/120
`█████░░░░░` **57%** avg

**🌌 Nebulae / M31** 🔄
`███████░░░` **72%**
Ha: 45/60 (50 acq) | OIII: 20/30
**Accepted:** 65/90

**🌌 Galaxies / M81** 🔄
`█░░░░░░░░░` **10%**
L: 3/30
**Accepted:** 3/30
```

Icons: ✅ complete · 🔄 in progress · 🌑 not started. Same-name filters are aggregated.

## Build & release (CI)

GitHub Actions workflows:

- `ci.yml` — restore / build / test / validation packaging on every push
- `release.yml` — on a `vMAJOR.MINOR.PATCH.BUILD` tag (e.g. `v1.0.5.0`), produces the ZIP + manifest, verifies its SHA-256 checksum, validates it with the official N.I.N.A. `gather.js`, creates the GitHub Release, then opens a pull request to `isbeorn/nina.plugin.manifests` for stable releases. The catalogue PR is skipped for Beta channel releases.

### Configure catalogue publication

Create an **Actions repository secret** named `NINA_MANIFEST_PAT` in **Settings → Secrets and variables → Actions → New repository secret**. It must authenticate as a GitHub user allowed to create/update the user's fork of `isbeorn/nina.plugin.manifests` and open PRs to the upstream repository. For a fine-grained token, grant **Contents: Read and write** on the fork and **Pull requests: Read and write** as needed. Creating a fork for the first time may require additional account permissions; alternatively fork the catalogue manually before running the workflow.

The release pipeline creates a version-specific branch `target-scheduler-discord-v<version>` in that fork and opens a PR to the upstream `main` branch. A previously created PR is detected and not duplicated. **The plugin repository and release assets must be publicly accessible** before submitting a catalogue PR; the repository is currently private.

Set repository variable `PLUGIN_CHANNEL` to `Release` for the stable channel, or `Beta` for prereleases. A beta GitHub Release does not create a catalogue PR.

Review the official N.I.N.A. manifest PR and wait for its merge: creating the release does not itself make the plugin appear inside the official N.I.N.A. catalog.


```powershell
git tag v1.0.5.0
git push origin v1.0.5.0
```

## Local development

.NET SDK **8+** (targets `net8.0-windows`) + PowerShell 7.

```powershell
dotnet restore NINA.Plugin.TargetScheduler.Discord.slnx
dotnet build NINA.Plugin.TargetScheduler.Discord.slnx -c Release
dotnet test NINA.Plugin.TargetScheduler.Discord.slnx -c Release
dotnet msbuild src/NINA.Plugin.TargetScheduler.Discord/NINA.Plugin.TargetScheduler.Discord.csproj -t:DeployPlugin -p:Configuration=Release
```

Copy `local.props.example` to `local.props` to set `NinaExecutable` if needed.
