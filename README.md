# Target Scheduler Discord

Plugin N.I.N.A. qui ajoute une instruction Advanced Sequencer pour publier l'avancement des cibles **Target Scheduler** sur Discord.

## Fonctionnalités

- Instruction **TS: Send target status to Discord**
- Une ligne par cible active (projets `ACTIVE`)
- Pourcentage global (Accepted / Desired, comme Target Scheduler)
- Compteurs filtre par filtre : `Accepted/Desired (acq Acquired)`
- Tri par pourcentage d'avancement décroissant
- Webhook Discord configurable dans les options du plugin
- URL de l'API Target Scheduler configurable (défaut `http://localhost:8188/ts/v0`)

## Prérequis

- N.I.N.A. ≥ 3.3.0.1060
- Plugin [Target Scheduler](https://github.com/tcpalmer/nina.plugin.targetscheduler) avec son API REST active
- Un webhook Discord

## Configuration

1. Options → Plugins → **Target Scheduler Discord**
2. Coller l'URL du webhook Discord
3. Vérifier l'URL de l'API TS si besoin
4. Dans Advanced Sequencer, ajouter **TS: Send target status to Discord**

## Exemple de message Discord

```text
**Target Scheduler progress** — 2026-11-14 20:00 UTC
`  72.5%` **Nebulae / M31** — Ha 45/60 (acq 50) · OIII 20/30 (acq 22)
`  10.0%` **Galaxies / M81** — L 3/30 (acq 3)
```

## Build & release (CI)

Les workflows GitHub Actions :

- `ci.yml` — restore / build / test / packaging de validation à chaque push
- `release.yml` — sur un tag `vMAJOR.MINOR.PATCH.BUILD` (ex. `v1.0.0.0`), produit le ZIP + manifest et crée la GitHub Release

```powershell
git tag v1.0.0.0
git push origin v1.0.0.0
```

## Développement local

SDK .NET **10.0.4xx** + PowerShell 7.

```powershell
dotnet restore NINA.Plugin.TargetScheduler.Discord.slnx
dotnet build NINA.Plugin.TargetScheduler.Discord.slnx -c Release
dotnet test NINA.Plugin.TargetScheduler.Discord.slnx -c Release
dotnet msbuild src/NINA.Plugin.TargetScheduler.Discord/NINA.Plugin.TargetScheduler.Discord.csproj -t:DeployPlugin -p:Configuration=Release
```

Copier `local.props.example` vers `local.props` pour pointer `NinaExecutable` si besoin.
