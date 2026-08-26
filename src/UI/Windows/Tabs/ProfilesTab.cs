using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

public class ProfilesTab : ITab
{
    public string name => "Profiles";

    private TextField _profileNameField;
    private string _profileText = string.Empty;
    private string _profileMessage = string.Empty;

    public void Draw()
    {
        _profileNameField ??= new TextField("Default");
        GUILayout.BeginVertical(MenuUI.IsMaterialLayoutActive
            ? GUILayout.ExpandWidth(true)
            : GUILayout.Width(MenuUI.windowWidth * 0.425f));
        try
        {
            DrawProfileContent();
        }
        catch (Exception exception)
        {
            GUILayout.Label("Profiles could not be loaded.", GUI.skin.label);
            GUILayout.Label(exception.Message, GUI.skin.label);
        }

        GUILayout.EndVertical();
    }

    private void DrawProfileContent()
    {
        GUILayout.Label("Named Profiles", GUI.skin.label);
        GUILayout.Label("Save local toggle profiles or share the current configuration through the clipboard.", GUI.skin.label);
        int nameWidth = MenuUI.IsMaterialLayoutActive ? 130 : 180;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name:", GUILayout.Width(50));
        _profileNameField.Draw(nameWidth);
        if (GUILayout.Button("Save", GUILayout.Width(70)))
        {
            bool saved = ProfileStore.Save(_profileNameField.Content, out string saveError);
            SetMessage(saved, $"Saved '{_profileNameField.Content.Trim()}'.", saveError);
        }
        GUILayout.EndHorizontal();

        IReadOnlyList<string> profiles = ProfileStore.ListProfiles();

        GUILayout.Label(profiles.Count == 0 ? "No named profiles saved." : "Saved profiles:", GUI.skin.label);
        int visibleProfiles = Mathf.Min(profiles.Count, 5);
        for (int profileIndex = 0; profileIndex < visibleProfiles; profileIndex++)
        {
            string profile = profiles[profileIndex];
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(profile, GUILayout.Width(nameWidth)))
                _profileNameField.Content = profile;

            if (GUILayout.Button("Load", GUILayout.Width(60)))
                SetMessage(ProfileStore.Load(profile, out string loadError), $"Loaded '{profile}'.", loadError);

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
                SetMessage(ProfileStore.Delete(profile, out string deleteError), $"Deleted '{profile}'.", deleteError);
            GUILayout.EndHorizontal();
        }

        if (profiles.Count > visibleProfiles)
            GUILayout.Label($"{profiles.Count - visibleProfiles} more profiles in {ProfileStore.DirectoryPath}.", GUI.skin.label);

        GUILayout.Space(8);
        GUILayout.Label("Shareable Profile", GUI.skin.label);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Current"))
        {
            _profileText = ProfileStore.Export();
            GUIUtility.systemCopyBuffer = _profileText;
            _profileMessage = "Profile copied to clipboard.";
        }

        if (GUILayout.Button("Paste"))
        {
            try
            {
                _profileText = GUIUtility.systemCopyBuffer ?? string.Empty;
                _profileMessage = _profileText.Length == 0
                    ? "Clipboard is empty."
                    : $"Profile pasted ({_profileText.Length} characters). Click Import to apply it.";
            }
            catch (Exception exception)
            {
                _profileMessage = $"Clipboard read failed: {exception.Message}";
            }
        }

        if (GUILayout.Button("Import"))
        {
            SetMessage(ProfileStore.Import(_profileText, out string importError), "Shared profile imported.", importError);
        }
        GUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(_profileMessage))
            GUILayout.Label(_profileMessage, GUI.skin.label);
    }

    private void SetMessage(bool succeeded, string success, string error)
    {
        _profileMessage = succeeded ? success : error;
    }
}