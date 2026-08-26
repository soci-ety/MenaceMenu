using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;

namespace MalumMenu;

public readonly struct SkinPreset
{
    public readonly int ColorId;
    public readonly string HatId;
    public readonly string SkinId;
    public readonly string VisorId;
    public readonly string NamePlateId;
    public readonly string PetId;

    public SkinPreset(int colorId, string hatId, string skinId, string visorId, string namePlateId, string petId)
    {
        ColorId = colorId;
        HatId = hatId ?? string.Empty;
        SkinId = skinId ?? string.Empty;
        VisorId = visorId ?? string.Empty;
        NamePlateId = namePlateId ?? string.Empty;
        PetId = petId ?? string.Empty;
    }
}

public static class SkinPresetStore
{
    private const string SharedPrefix = "MENACEMENU_SKIN_V1:";
    private const string Header = "# MenaceMenu Skin Preset v1";

    public static string DirectoryPath => Path.Combine(Paths.ConfigPath, "MenaceSkins");

    public static bool Capture(PlayerControl player, out SkinPreset preset, out string error)
    {
        preset = default;
        error = string.Empty;
        if (player?.Data?.DefaultOutfit == null)
        {
            error = "Your avatar is not ready.";
            return false;
        }

        NetworkedPlayerInfo.PlayerOutfit outfit = player.Data.DefaultOutfit;
        preset = new SkinPreset(outfit.ColorId, outfit.HatId, outfit.SkinId, outfit.VisorId,
            outfit.NamePlateId, outfit.PetId);
        return true;
    }

    public static void Apply(PlayerControl player, SkinPreset preset)
    {
        if (player == null) return;

        player.RpcSetColor((byte)Mathf.Clamp(preset.ColorId, 0, 255));
        player.RpcSetHat(preset.HatId);
        player.RpcSetSkin(preset.SkinId);
        player.RpcSetVisor(preset.VisorId);
        player.RpcSetNamePlate(preset.NamePlateId);
        player.RpcSetPet(preset.PetId);
    }

    public static IReadOnlyList<string> ListPresets()
    {
        Directory.CreateDirectory(DirectoryPath);
        return Directory.GetFiles(DirectoryPath, "*.skin.txt")
            .Select(path => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path)))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool Save(string name, SkinPreset preset, out string error)
    {
        if (!TryNormalizeName(name, out string safeName, out error)) return false;
        Directory.CreateDirectory(DirectoryPath);
        File.WriteAllText(GetPath(safeName), Serialize(preset));
        return true;
    }

    public static bool Load(string name, out SkinPreset preset, out string error)
    {
        preset = default;
        if (!TryNormalizeName(name, out string safeName, out error)) return false;
        string path = GetPath(safeName);
        if (!File.Exists(path))
        {
            error = $"Skin preset '{safeName}' does not exist.";
            return false;
        }

        return TryDeserialize(File.ReadAllText(path), out preset, out error);
    }

    public static bool Delete(string name, out string error)
    {
        if (!TryNormalizeName(name, out string safeName, out error)) return false;
        string path = GetPath(safeName);
        if (!File.Exists(path))
        {
            error = $"Skin preset '{safeName}' does not exist.";
            return false;
        }

        File.Delete(path);
        return true;
    }

    public static string Export(SkinPreset preset)
    {
        return SharedPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(Serialize(preset)));
    }

    public static bool Import(string text, out SkinPreset preset, out string error)
    {
        preset = default;
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
                error = "The copied skin preset is not valid.";
                return false;
            }
        }

        return TryDeserialize(text, out preset, out error);
    }

    private static string Serialize(SkinPreset preset)
    {
        return Header + Environment.NewLine + string.Join("\t", new[]
        {
            preset.ColorId.ToString(),
            Clean(preset.HatId),
            Clean(preset.SkinId),
            Clean(preset.VisorId),
            Clean(preset.NamePlateId),
            Clean(preset.PetId)
        });
    }

    private static bool TryDeserialize(string text, out SkinPreset preset, out string error)
    {
        preset = default;
        error = string.Empty;
        string dataLine = text?.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .FirstOrDefault(line => !line.TrimStart().StartsWith("#"))?.Trim();
        string[] parts = dataLine?.Split('\t');
        if (parts == null || parts.Length < 6 || !int.TryParse(parts[0], out int colorId))
        {
            error = "The skin preset format is invalid.";
            return false;
        }

        preset = new SkinPreset(colorId, parts[1], parts[2], parts[3], parts[4], parts[5]);
        return true;
    }

    private static string Clean(string value)
    {
        return (value ?? string.Empty).Replace("\t", " ").Replace("\r", " ").Replace("\n", " ").Trim();
    }

    private static string GetPath(string name) => Path.Combine(DirectoryPath, $"{name}.skin.txt");

    private static bool TryNormalizeName(string name, out string safeName, out string error)
    {
        safeName = name?.Trim() ?? string.Empty;
        error = string.Empty;
        if (safeName.Length == 0 || safeName.Length > 48)
        {
            error = "Preset names must be between 1 and 48 characters.";
            return false;
        }

        if (safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || safeName is "." or "..")
        {
            error = "Preset name contains invalid characters.";
            return false;
        }

        return true;
    }
}
