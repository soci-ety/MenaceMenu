<p align="center">
  <img src="HyperMenu.jpeg">
</p>

---
# NOTICE:
* MenaceMenu is a fork of HyperMenu, which is based on the original MalumMenu.
  * Original MalumMenu: [scp222thj/MalumMenu](https://github.com/scp222thj/MalumMenu)
  * HyperMenu: [The-HyperMenu-Team/HyperMenu](https://github.com/The-HyperMenu-Team/HyperMenu)
  * MenaceMenu: [soci-ety/MenaceMenu](https://github.com/soci-ety/MenaceMenu)
---
## Our Discord:
https://discord.gg/gkpdeAX5u9

<p align="center">
  <a href="https://discord.gg/gkpdeAX5u9">
    <img hspace="6" src="https://img.shields.io/badge/Join%20Us%20on-Discord-blue?style=flat&logo=discord" alt="Discord">
  </a>

  <a href="https://github.com/soci-ety/MenaceMenu/releases">
    <img hspace="6" src="https://img.shields.io/github/downloads/soci-ety/MenaceMenu/total?style=flat&logo=github&label=Total%20Downloads&color=2ECC71" alt="Downloads">
  </a>
</p>

<p align="center">
  <b>An easy-to-use Among Us BepInEx plugin with a simple GUI and useful modules.</b>
</p>

<!-- omit in toc -->

---
# 😎 Table Of Contents

- [🎁 Releases](#-releases)
- [⬇️ Installation](#️-installation)
  - [🪟 Windows](#-windows)
  - [🔧 Building From Source](#-building-from-source)
  - [🐧 Linux](#-linux)
- [📋 Features](#-features)
- [❓ FAQ](#-faq)
- [⚠️ Disclaimer](#️-disclaimer)

# 🎁 Releases

| Mod Version        | Among Us - Version | Link                                                                  |
|--------------------|--------------------|-----------------------------------------------------------------------|
| Main **[CURRENT]** | 17.5 / 2026.8.18   | [Repository](https://github.com/soci-ety/MenaceMenu)                 |
| v4.2.2 base        | 17.4 / 2026.6.5    | [Original release](https://github.com/The-HyperMenu-Team/HyperMenu/releases/tag/v4.2.2) |

MenaceMenu is currently built against the Among Us `2026.8.18` game libraries.
Use the matching game version for the best compatibility.

# ⬇️ Installation

## 🪟 Windows

1. Install **BepInEx 6 IL2CPP x64** into your Among Us game folder.
   The folder must contain `Among Us.exe`, `winhttp.dll`, `doorstop_config.ini`, and `BepInEx\`.

2. Launch Among Us once, wait for BepInEx to finish creating its folders, then close the game.

3. Download `MalumMenu.dll` and `MalumMenu.deps.json` from the [MenaceMenu repository](https://github.com/soci-ety/MenaceMenu), or build them from source below.

4. Copy both files into:

   ```text
   Among Us\BepInEx\plugins\
   ```

5. Launch Among Us normally. Check `BepInEx\LogOutput.log` for the plugin load message.

6. Open the menu with the configured menu key. The default key is **DELETE**.

Do not install multiple MalumMenu, HyperMenu, or MenaceMenu DLLs at the same time. Remove older copies from `BepInEx\plugins\` first.

## 🔧 Building From Source

Requirements:

- Windows 11 or Windows 10
- .NET SDK 6 or newer
- Git
- Among Us `2026.8.18 / 17.5`
- BepInEx 6 IL2CPP installed in the game directory

Build from the repository root:

```powershell
dotnet restore .\MalumMenu.sln
dotnet build .\MalumMenu.sln --no-restore
```

The plugin files are generated in:

```text
src\bin\Debug\net6.0\MalumMenu.dll
src\bin\Debug\net6.0\MalumMenu.deps.json
```

Copy both generated files into `Among Us\BepInEx\plugins\`.

## 🐧 Linux

1. Run Among Us under **Proton or Wine**.
   - In Steam, right-click Among Us → `Properties` → `Compatibility` → enable `Force the use of a specific Steam Play compatibility tool`.

2. Set up BepInEx for Proton/Wine using the official guide [here](https://docs.bepinex.dev/articles/advanced/proton_wine.html).

3. If BepInEx cannot load the IL2CPP chainloader, add this to the Steam launch options:

   ```text
   PROTON_NO_ESYNC=1 PROTON_USE_WINED3D=1 WINEDLLOVERRIDES="winhttp.dll=n,b" %command%
   ```

4. Follow the Windows plugin installation steps above.

# 📋 Features

<img alt="image" src="HyperInGame.jpeg">

## Changes from OG MalumMenu

- HyperMenu fork with additional modules and UI improvements
- Among Us 17.5 voting compatibility
- Updated MeetingHud and vote RPC handling for Among Us `2026.8.18`
- MenaceMenu watermark with stealth-mode support
- Configurable menu and chat colors
- Source build support for local BepInEx plugin installation

## OG Menu Features

- An intuitive GUI with gameplay and utility modules
- See ghosts and reveal impostors
- Track players using the minimap
- Teleport around the map
- Change roles
- Remove kill cooldowns and use kill utilities
- Unlock cosmetics locally
- Avoid disconnect penalties
- Meeting and voting utilities

For the original feature list, see [MalumMenu FEATURES.md](https://github.com/scp222thj/MalumMenu/blob/main/FEATURES.md).

# ❓ FAQ

Click to expand each topic.

<details>

<summary><h2>❗ MenaceMenu does not load</h2></summary>

Make sure you installed **BepInEx 6 IL2CPP x64**, not BepInEx 5. Confirm that `winhttp.dll` and `doorstop_config.ini` are beside `Among Us.exe`, then launch the game once before copying the plugin files.

Check `BepInEx\LogOutput.log` for errors. The plugin should be installed only once in `BepInEx\plugins\`.

The current build targets Among Us `17.5 / 2026.8.18`. Older or newer game versions may require a different build.

</details>

<details>

<summary><h2>🗳️ Voting does not work</h2></summary>

Use the MenaceMenu build compiled against Among Us `2026.8.18`. The v17.5 fix updates the MeetingHud vote-area fields and the private voting completion RPC used by the newer game version.

Remove old plugin DLLs before installing the new one. Having MalumMenu, HyperMenu, and MenaceMenu loaded together can cause duplicate Harmony patches and broken voting UI.

</details>

<details>

<summary><h2>👾 I found a bug or want to suggest a feature</h2></summary>

Open an issue on the [MenaceMenu repository](https://github.com/soci-ety/MenaceMenu/issues). Include your Among Us version, BepInEx version, platform, and the relevant section of `BepInEx\LogOutput.log`.

</details>

# ⚠️ Disclaimer

MenaceMenu is not affiliated with Among Us or Innersloth LLC. The content contained herein is not endorsed or sponsored by Innersloth LLC. Portions of the materials used by this project are property of Innersloth LLC. © Innersloth LLC.

This plugin is provided for local testing and experimentation. Its use can violate the Among Us terms of service, interfere with other players, or result in temporary or permanent bans. Do not use it to disrupt public games, bypass protections, or negatively affect other users.

Use the software at your own risk. The authors are not responsible for account action, game instability, data loss, or other consequences resulting from its use.