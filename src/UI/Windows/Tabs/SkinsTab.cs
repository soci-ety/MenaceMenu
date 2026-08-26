using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

public class SkinsTab : ITab
{
    public string name => "Skins";

    private TextField _presetNameField;
    private string _presetText = string.Empty;
    private string _message = string.Empty;

    public void Draw()
    {
        _presetNameField ??= new TextField("My Avatar");
        GUILayout.BeginVertical(MenuUI.IsMaterialLayoutActive
            ? GUILayout.ExpandWidth(true)
            : GUILayout.Width(MenuUI.windowWidth * 0.425f));
        try
        {
            DrawContent();
        }
        catch (Exception exception)
        {
            GUILayout.Label("Skin presets could not be loaded.", GUI.skin.label);
            GUILayout.Label(exception.Message, GUI.skin.label);
        }

        GUILayout.EndVertical();
    }

    private void DrawContent()
    {
        GUILayout.Label("Avatar Skin Presets", GUI.skin.label);
        GUILayout.Label("Save and share your current color, hat, skin, visor, nameplate, and pet.", GUI.skin.label);
        int nameWidth = MenuUI.IsMaterialLayoutActive ? 130 : 180;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name:", GUILayout.Width(50));
        _presetNameField.Draw(nameWidth);
        if (GUILayout.Button("Save", GUILayout.Width(70)))
        {
            if (SkinPresetStore.Capture(PlayerControl.LocalPlayer, out SkinPreset preset, out string captureError))
            {
                bool saved = SkinPresetStore.Save(_presetNameField.Content, preset, out string saveError);
                SetMessage(saved, $"Saved '{_presetNameField.Content.Trim()}'.", saveError);
            }
            else
            {
                _message = captureError;
            }
        }
        GUILayout.EndHorizontal();

        IReadOnlyList<string> presets = SkinPresetStore.ListPresets();
        GUILayout.Label(presets.Count == 0 ? "No saved skin presets." : "Saved skin presets:", GUI.skin.label);
        int visiblePresets = Mathf.Min(presets.Count, 6);
        for (int presetIndex = 0; presetIndex < visiblePresets; presetIndex++)
        {
            string presetName = presets[presetIndex];
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(presetName, GUILayout.Width(nameWidth)))
                _presetNameField.Content = presetName;

            if (GUILayout.Button("Load", GUILayout.Width(60)))
            {
                bool loaded = SkinPresetStore.Load(presetName, out SkinPreset preset, out string loadError);
                if (loaded) SkinPresetStore.Apply(PlayerControl.LocalPlayer, preset);
                SetMessage(loaded, $"Loaded '{presetName}'.", loadError);
            }

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                bool deleted = SkinPresetStore.Delete(presetName, out string deleteError);
                SetMessage(deleted, $"Deleted '{presetName}'.", deleteError);
            }
            GUILayout.EndHorizontal();
        }

        if (presets.Count > visiblePresets)
            GUILayout.Label($"{presets.Count - visiblePresets} more presets in {SkinPresetStore.DirectoryPath}.", GUI.skin.label);

        GUILayout.Space(8);
        GUILayout.Label("Share Skin Preset", GUI.skin.label);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Current"))
        {
            if (SkinPresetStore.Capture(PlayerControl.LocalPlayer, out SkinPreset preset, out string captureError))
            {
                _presetText = SkinPresetStore.Export(preset);
                GUIUtility.systemCopyBuffer = _presetText;
                _message = "Skin preset copied to clipboard.";
            }
            else
            {
                _message = captureError;
            }
        }

        if (GUILayout.Button("Paste"))
        {
            try
            {
                _presetText = GUIUtility.systemCopyBuffer ?? string.Empty;
                _message = _presetText.Length == 0
                    ? "Clipboard is empty."
                    : $"Skin preset pasted ({_presetText.Length} characters). Click Import to apply it.";
            }
            catch (Exception exception)
            {
                _message = $"Clipboard read failed: {exception.Message}";
            }
        }

        if (GUILayout.Button("Import"))
        {
            bool imported = SkinPresetStore.Import(_presetText, out SkinPreset preset, out string importError);
            if (imported) SkinPresetStore.Apply(PlayerControl.LocalPlayer, preset);
            SetMessage(imported, "Shared skin preset imported.", importError);
        }
        GUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(_message))
            GUILayout.Label(_message, GUI.skin.label);
    }

    private void SetMessage(bool succeeded, string success, string error)
    {
        _message = succeeded ? success : error;
    }
}
