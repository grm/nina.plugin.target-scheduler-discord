# Changelog

## 1.0.7.0

- Fix Advanced Sequencer sends failing with "Discord webhook URL is not configured" while the options test succeeds. Each send now reloads the saved settings shared by N.I.N.A.'s separate options and sequencer instances.
- Existing and cloned instructions pick up changes to the webhook, Target Scheduler API URL and inactive-target option without being recreated.
- Add regression coverage for separate settings instances, updated settings and cloned instructions.

## 1.0.6.0

- First stable release with updated N.I.N.A. plugin metadata, corrected license and changelog URL, and a new visual identity.
- Automated release verification using the official N.I.N.A. manifest tooling and submission of the stable manifest to the upstream plugin catalogue.

## 1.0.5.0

- Night Summary–style Discord report: 🔭 title, **Overview** stats, 🌌 targets with ✅/🔄/🌑 icons, filter lines as `H: 60/60 | O: 60/60`

## 1.0.4.0

- Nicer Discord status: embed card, progress bars, aggregated filters, compact complete / empty targets

## 1.0.3.0

- Fix Target Scheduler API client to use GUID `Id` fields (`/ts/v0`), matching TS 5.x responses (was calling `/projects/0/targets` → 404)

## 1.0.2.0

- Retarget plugin to N.I.N.A. 3.2 / `net8.0-windows` (was incorrectly requiring 3.3)

## 1.0.1.0

- Add **Send status now** button in plugin options for on-demand Discord tests

## 1.0.0.0

- Initial plugin: sequencer instruction posts Target Scheduler target progress to Discord
- Configurable Discord webhook and Target Scheduler API base URL
