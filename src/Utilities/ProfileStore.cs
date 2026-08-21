using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using UnityEngine;

namespace MalumMenu;

public static class ProfileStore
{
    private const string Header = "# MenaceMenu Shared Profile v1";
    private const string SharedPrefix = "MENACEMENU_PROFILE_V1:";
    private static readonly HashSet<string> ExcludedToggles = new(StringComparer.OrdinalIgnoreCase)
    {
        "reloadConfig", "openConfig", "loadProfile", "saveProfile", "panicMode"
    };

    public static string DirectoryPath => Path.Combine(Paths.ConfigPath, "MenaceProfiles");

    public static IReadOnlyList<string> ListProfiles()
    {
        Directory.CreateDirectory(DirectoryPath);
        return Directory.GetFiles(DirectoryPath, "*.profile.txt")
            .Select(path => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path)))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool Save(string profileName, out string error)
    {
        if (!TryNormalizeName(profileName, out string safeName, out error)) return false;

        Directory.CreateDirectory(DirectoryPath);
        File.WriteAllText(GetPath(safeName), Serialize());
        return true;
    }

    public static bool Load(string profileName, out string error)
    {
        if (!TryNormalizeName(profileName, out string safeName, out error)) return false;

        string path = GetPath(safeName);
        if (!File.Exists(path))
        {
            error = $"Profile '{safeName}' does not exist.";
            return false;
        }

        return TryApply(File.ReadAllText(path), out error);
    }

    public static bool Delete(string profileName, out string error)
    {
        if (!TryNormalizeName(profileName, out string safeName, out error)) return false;

        string path = GetPath(safeName);
        if (!File.Exists(path))
        {
            error = $"Profile '{safeName}' does not exist.";
            return false;
        }

        File.Delete(path);
        return true;
    }

    public static string Export()
    {
        return SharedPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(Serialize()));
    }

    public static bool Import(string text, out string error)
    {
        text = text?.Trim();
        if (text?.StartsWith(SharedPrefix, StringComparison.Ordinal) == true)
        {
            try
            {
                byte[] payload = Convert.FromBase64String(text[SharedPrefix.Length..]);
                text = Encoding.UTF8.GetString(payload);
            }
            catch (FormatException)
            {
                error = "The copied profile payload is not valid.";
                return false;
            }
        }

        return TryApply(text, out error);
    }

    public static void EnsureLegacyProfile()
    {
        if (File.Exists(MalumMenu.ProfilePath)) return;

        Directory.CreateDirectory(Path.GetDirectoryName(MalumMenu.ProfilePath));
        SaveLegacy();
    }

    public static void SaveLegacy()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(MalumMenu.ProfilePath));
        File.WriteAllText(MalumMenu.ProfilePath, Serialize());
    }

    public static bool LoadLegacy(out string error)
    {
        error = string.Empty;
        if (!File.Exists(MalumMenu.ProfilePath)) return false;
        return TryApply(File.ReadAllText(MalumMenu.ProfilePath), out error);
    }

    private static string Serialize()
    {
        StringBuilder builder = new();
        builder.AppendLine(Header);
        builder.AppendLine("# Format: ToggleName = True/False = KeyCode.KEY");
        builder.AppendLine("# Shared profiles contain menu toggles and keybinds only.");
        builder.AppendLine();

        foreach (FieldInfo field in CheatToggles.ToggleFields.Values.OrderBy(field => field.Name))
        {
            if (ExcludedToggles.Contains(field.Name)) continue;

            CheatToggles.Keybinds.TryGetValue(field.Name, out KeyCode key);
            builder.AppendLine($"{field.Name} = {field.GetValue(null)} = KeyCode.{key}");
        }

        return builder.ToString();
    }

    private static bool TryApply(string text, out string error)
    {
        Dictionary<string, bool> toggleValues = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, KeyCode> keyValues = new(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(text))
        {
            error = "The profile text is empty.";
            return false;
        }

        int recognizedEntries = 0;
        foreach (string rawLine in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;

            string[] parts = line.Split('=', 3);
            if (parts.Length < 2) continue;

            string name = parts[0].Trim();
            if (!CheatToggles.ToggleFields.ContainsKey(name) || ExcludedToggles.Contains(name)) continue;

            if (!bool.TryParse(parts[1].Trim(), out bool toggleValue))
            {
                error = $"Invalid value for '{name}'. Expected True or False.";
                return false;
            }

            KeyCode key = KeyCode.None;
            if (parts.Length == 3)
            {
                string keyName = parts[2].Trim();
                if (keyName.StartsWith("KeyCode.", StringComparison.OrdinalIgnoreCase))
                    keyName = keyName["KeyCode.".Length..];

                if (!Enum.TryParse(keyName, true, out key))
                {
                    error = $"Invalid keybind for '{name}'.";
                    return false;
                }
            }

            toggleValues[name] = toggleValue;
            keyValues[name] = key;
            recognizedEntries++;
        }

        if (recognizedEntries == 0)
        {
            error = "No MenaceMenu settings were found in the pasted profile.";
            return false;
        }

        foreach ((string name, bool value) in toggleValues)
            CheatToggles.ToggleFields[name].SetValue(null, value);

        foreach ((string name, KeyCode key) in keyValues)
            CheatToggles.Keybinds[name] = key;

        error = string.Empty;
        return true;
    }

    private static string GetPath(string profileName)
    {
        return Path.Combine(DirectoryPath, $"{profileName}.profile.txt");
    }

    private static bool TryNormalizeName(string profileName, out string safeName, out string error)
    {
        safeName = profileName?.Trim() ?? string.Empty;
        error = string.Empty;

        if (safeName.Length == 0 || safeName.Length > 48)
        {
            error = "Profile names must be between 1 and 48 characters.";
            return false;
        }

        if (safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || safeName is "." or "..")
        {
            error = "Profile name contains invalid characters.";
            return false;
        }

        return true;
    }
}