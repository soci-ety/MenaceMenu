using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MalumMenu.features;

namespace MalumMenu;

public class ConfigTab : ITab
{
    public string name => "Config";

    public readonly Dictionary<string, int> versions = new Dictionary<string, int>()
        {
			// Current version at runtime
			// VersionShower::Start uses ReferenceDataManager.Refdata.userFacingVersion to get version strings such as "17.1" however that doesn't seem to before the game fully loads, so we have to use Constants::AddressablesVersion to get a less human-understandable version string
			{ $"{Constants.AddressablesVersion} (Current)", Constants.GetBroadcastVersion() },
            { "16.1.0", 50632950 },
            { "17.1", 50643450 },
            { "17.1.2", 50647000 },
            { "17.2", 50645050 },
            { "17.2.1", 50652900 },
            { "17.2.2", 50653700 }
        };

    private int versionSelection = 0;
    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(10);
        
		Chat.OnChat.LogChatMessages = UIHelpers.Toggle(Chat.OnChat.LogChatMessages, "Log chat messages to console");

		if(GUILayout.Button("Clear Notifications"))
		{
			MalumMenu.notifications.ClearNotifications();
			MalumMenu.notifications.Send("Notifications", "All notifications have been cleared.", 5);
		}

        GUILayout.Space(10);

        Spoofer.shouldSpoofVersion = UIHelpers.Toggle(Spoofer.shouldSpoofVersion, "Enable Version Spoofing");

        GUILayout.Label($"Spoofed Version: {versions.ElementAt(versionSelection).Key} ({Spoofer.spoofedVersion})");
        versionSelection = (int)UIHelpers.HorizontalSlider(versionSelection, 0, versions.Count - 1);
        Spoofer.spoofedVersion = versions.ElementAt(versionSelection).Value;

        Spoofer.useModdedProtocol = UIHelpers.Toggle(Spoofer.useModdedProtocol, "Use Modded Protocol");

        GUILayout.Label($"Spoofed Platform: {Spoofer.spoofedPlatform}");
        Spoofer.spoofedPlatform = (Platforms)UIHelpers.HorizontalSlider((float)Spoofer.spoofedPlatform, 0, 10);

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.openConfig = UIHelpers.Toggle(CheatToggles.openConfig, " Open Config");

        CheatToggles.reloadConfig = UIHelpers.Toggle(CheatToggles.reloadConfig, " Reload Config");

        CheatToggles.saveProfile = UIHelpers.Toggle(CheatToggles.saveProfile, " Save to Profile");

        CheatToggles.loadProfile = UIHelpers.Toggle(CheatToggles.loadProfile, " Load from Profile");
    }

}
