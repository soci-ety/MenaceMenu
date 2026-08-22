# Changelog

All notable MenaceMenu changes are documented here.

## [1.10.0] - 2026-08-21

### Added

- Added Hydra parity features:
  - Pet Player routine and player control.
  - SceneChange anticheat validation.
  - Skip Hide and Seek seeker animation.
  - Show dead players visual option.
  - Teleport Flooder destination selection using map locations.
  - Kick All Players host control.
- Added local role choices for Shapeshifter, Phantom, Viper, Impostor, Guardian Angel, Judge, Crewmate Ghost, and Impostor Ghost.
- Added the new menu layout as an opt-in UI mode.
- Added the `New UI (WIP)` setting and enabled the new UI by default.
- Added scrollable content support.
- Added a dedicated Profiles tab with:
  - Multiple named local profiles.
  - Profile save, load, and delete actions.
  - Portable profile export through the system clipboard.
  - Clipboard paste and import actions.
  - Validation for malformed or empty profile payloads.
- Added portable `MENACEMENU_PROFILE_V1` profile payloads for sharing settings between installations.
- Added `MenaceMenu V1.1.1` to the Among Us home-screen version display.
- Added `discord.gg/bH4Hy9YnVD` centered in the new menu app bar.

### Changed

- Restored Config to its original responsibilities and moved profile management into the separate Profiles tab.
- Preserved compatibility with the legacy `MalumProfile.txt` format.
- Updated profile parsing to validate settings before applying them.
- Updated the menu title to `MenaceMenu v1.1.1`.
- Updated the Material layout with a compact app bar, navigation rail, improved contrast, and scrollable content.

### Fixed

- Fixed Config and Profiles tabs collapsing or hiding the rest of the menu during rendering.
- Removed conflicting nested scroll views from Config in both classic and Material layouts.
- Replaced fragile profile-name rendering with the existing custom text field control.
- Fixed clipboard profile import feedback and invalid-payload handling.
- Prevented Teleport Flooder from sending unauthorized remote movement RPCs that could trigger server kicks.
- Restricted player kicking and Teleport Flooder to legitimate lobby-host operations.
- Preserved the classic UI as a fallback while the new UI remains available through Settings.

### Build

- Target: Among Us `2026.8.18`
- Plugin loader: BepInEx 6 IL2CPP
- Build command: `dotnet build .\\MenaceMenu\\MalumMenu.sln --no-restore`
