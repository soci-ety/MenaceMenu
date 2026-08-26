using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using AmongUs.GameOptions;
using UnityEngine;

namespace MalumMenu;

public static class LobbyPresetStore
{
    private const string Prefix = "MENACEMENU_LOBBY_V1:";
    public static string DirectoryPath => Path.Combine(Paths.ConfigPath, "MenaceLobbyPresets");

    public static IReadOnlyList<string> List()
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            return Directory.GetFiles(DirectoryPath, "*.lobby.txt")
                .Select(path => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path)))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }

    public static bool Save(string name, out string error)
    {
        if (!TryName(name, out string safeName, out error)) return false;
        if (!TryReadCurrent(out byte[] bytes, out error)) return false;
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(GetPath(safeName), Convert.ToBase64String(bytes));
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            error = $"Could not save lobby preset: {exception.Message}";
            return false;
        }
    }

    public static bool Load(string name, out string error)
    {
        error = string.Empty;
        if (!TryName(name, out string safeName, out error)) return false;
        string path = GetPath(safeName);
        if (!File.Exists(path)) { error = $"Lobby preset '{safeName}' does not exist."; return false; }
        try
        {
            string encoded = File.ReadAllText(path).Trim();
            if (encoded.Length == 0) { error = "The lobby preset is empty."; return false; }
            byte[] bytes = Convert.FromBase64String(encoded);
            return Apply(bytes, out error);
        }
        catch (FormatException) { error = "The lobby preset is invalid."; return false; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { error = $"Could not read lobby preset: {exception.Message}"; return false; }
    }

    public static bool Delete(string name, out string error)
    {
        if (!TryName(name, out string safeName, out error)) return false;
        string path = GetPath(safeName);
        if (!File.Exists(path)) { error = $"Lobby preset '{safeName}' does not exist."; return false; }
        try
        {
            File.Delete(path);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            error = $"Could not delete lobby preset: {exception.Message}";
            return false;
        }
    }

    public static string Export(out string error)
    {
        if (!TryReadCurrent(out byte[] bytes, out error)) return string.Empty;
        return Prefix + Convert.ToBase64String(bytes);
    }

    public static bool Import(string text, out string error)
    {
        error = string.Empty;
        text = text?.Trim() ?? string.Empty;
        if (!text.StartsWith(Prefix, StringComparison.Ordinal)) { error = "Invalid lobby preset data."; return false; }
        try { return Apply(Convert.FromBase64String(text[Prefix.Length..]), out error); }
        catch (FormatException) { error = "The copied lobby preset is invalid."; return false; }
    }

    private static bool TryReadCurrent(out byte[] bytes, out string error)
    {
        bytes = null;
        error = string.Empty;
        try
        {
            LogicOptions logic = GameManager.Instance?.LogicOptions;
            if (logic?.gameOptionsFactory == null)
            {
                error = "Lobby options are not ready.";
                return false;
            }
            IGameOptions current = GameOptionsManager.Instance?.CurrentGameOptions ?? logic.currentGameOptions;
            if (current == null)
            {
                error = "Lobby options are not ready.";
                return false;
            }
            bytes = logic.gameOptionsFactory.ToBytes(current, AprilFoolsMode.IsAprilFoolsModeToggledOn);
            return bytes != null && bytes.Length > 0;
        }
        catch (Exception exception) { error = exception.Message; return false; }
    }

    private static bool Apply(byte[] bytes, out string error)
    {
        error = string.Empty;
        try
        {
            LogicOptions logic = GameManager.Instance?.LogicOptions;
            if (logic?.gameOptionsFactory == null || bytes == null || bytes.Length == 0)
            { error = "Lobby options are not ready."; return false; }
            IGameOptions options = logic.gameOptionsFactory.FromBytes(bytes);
            if (options == null) { error = "Could not decode the lobby preset."; return false; }
            logic.SetGameOptions(options);
            if (GameOptionsManager.Instance != null)
            {
                GameOptionsManager.Instance.CurrentGameOptions = options;
                GameOptionsManager.Instance.GameHostOptions = options;
            }

            if (Utils.isHost)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player?.Data != null)
                        GameOptions.SendGameOptionsToClient(options, player.OwnerId);
                }
            }
            return true;
        }
        catch (Exception exception) { error = exception.Message; return false; }
    }

    private static string GetPath(string name) => Path.Combine(DirectoryPath, $"{name}.lobby.txt");

    private static bool TryName(string name, out string safeName, out string error)
    {
        safeName = name?.Trim() ?? string.Empty;
        error = string.Empty;
        if (safeName.Length == 0 || safeName.Length > 48 || safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || safeName is "." or "..")
        { error = "Preset names must be between 1 and 48 valid characters."; return false; }
        return true;
    }
}
