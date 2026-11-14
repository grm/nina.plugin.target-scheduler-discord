# Changelog

## 1.0.3.0

- Fix Target Scheduler API client to use GUID `Id` fields (`/ts/v0`), matching TS 5.x responses (was calling `/projects/0/targets` → 404)

## 1.0.2.0

- Retarget plugin to N.I.N.A. 3.2 / `net8.0-windows` (was incorrectly requiring 3.3)

## 1.0.1.0

- Add **Send status now** button in plugin options for on-demand Discord tests

## 1.0.0.0

- Initial plugin: sequencer instruction posts Target Scheduler target progress to Discord
- Configurable Discord webhook and Target Scheduler API base URL
