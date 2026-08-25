using System;
using System.Reflection;
using AmongUs.GameOptions;
using MalumMenu.features;
using UnityEngine;

namespace MalumMenu;

public class LobbyTab : ITab
{
    public string name => "Lobby";

    private readonly HostOnlyTab _hostControls = new();
    private readonly HostOnlyTab2 _lobbyControls = new();
    private readonly SabotageTab _sabotageControls = new();
    private int _selectedPlayerIndex;
    private int _selectedRoleIndex;
    private int _selectedMap;

    private static readonly RoleTypes[] AssignableRoles =
    {
        RoleTypes.Crewmate,
        RoleTypes.Impostor,
        RoleTypes.Engineer,
        RoleTypes.Scientist,
        RoleTypes.GuardianAngel,
        RoleTypes.Shapeshifter,
        RoleTypes.Noisemaker,
        RoleTypes.Phantom,
        RoleTypes.Tracker,
        RoleTypes.Detective,
        RoleTypes.Viper
    };

    private static readonly MapNames[] LobbyMaps =
    {
        MapNames.Skeld,
        MapNames.MiraHQ,
        MapNames.Polus,
        MapNames.Dleks,
        MapNames.Airship,
        MapNames.Fungle
    };

    public void Draw()
    {
        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game. These options will not work.");
        }
        else if (!AmongUsClient.Instance.AmHost)
        {
            GUILayout.Label("You are not the host of the current lobby. Host-only options will do nothing or may trigger anticheat.");
        }

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        DrawLobbySettings();

        GUILayout.Space(12);
        GUILayout.Label("Host Controls", GUIStylePreset.TabSubtitle);
        _hostControls.Draw();

        GUILayout.Space(12);
        GUILayout.Label("Lobby and Game Controls", GUIStylePreset.TabSubtitle);
        _lobbyControls.Draw();

        GUILayout.Space(12);
        GUILayout.Label("Sabotage Controls", GUIStylePreset.TabSubtitle);
        _sabotageControls.Draw();

        GUILayout.EndVertical();
    }

    private void DrawLobbySettings()
    {
        GUILayout.Label("Lobby Settings", GUIStylePreset.TabSubtitle);

        if (AmongUsClient.Instance?.AmHost == true)
        {
            Host.AlwaysImposter.Enabled = UIHelpers.Toggle(Host.AlwaysImposter.Enabled, " Always Impostor");
            Host.AlwaysImposter.assignedRole = RoleTypes.Impostor;
            GUILayout.Space(5);
        }

        if (PlayerControl.AllPlayerControls.Count > 0)
        {
            _selectedPlayerIndex = Mathf.Clamp(_selectedPlayerIndex, 0, PlayerControl.AllPlayerControls.Count - 1);
            PlayerControl selectedPlayer = PlayerControl.AllPlayerControls[_selectedPlayerIndex];
            GUILayout.Label($"Role target: {selectedPlayer?.Data?.PlayerName ?? "Unknown"}");
            _selectedPlayerIndex = Mathf.RoundToInt(UIHelpers.HorizontalSlider(_selectedPlayerIndex, 0, PlayerControl.AllPlayerControls.Count - 1));

            _selectedRoleIndex = Mathf.Clamp(_selectedRoleIndex, 0, AssignableRoles.Length - 1);
            GUILayout.Label($"Selected role: {AssignableRoles[_selectedRoleIndex]}");
            _selectedRoleIndex = Mathf.RoundToInt(UIHelpers.HorizontalSlider(_selectedRoleIndex, 0, AssignableRoles.Length - 1));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Assign Role To Player"))
                AssignRole(selectedPlayer);
            if (GUILayout.Button("Assign Role To Everyone"))
                AssignRoleToEveryone();
            GUILayout.EndHorizontal();

            GUILayout.Label($"Pending role assignments: {LobbyRoleAssignments.Count}");
            if (GUILayout.Button("Clear Pending Role Assignments"))
                LobbyRoleAssignments.Clear();

            if (GUILayout.Button("Force All Meeting Votes To Player"))
                ForceAllVotes(selectedPlayer);
        }
        else
        {
            GUILayout.Label("No players are currently available for role assignment.");
        }

        _selectedMap = Mathf.Clamp(_selectedMap, 0, LobbyMaps.Length - 1);
        GUILayout.Label($"Selected map: {LobbyMaps[_selectedMap]}");
        _selectedMap = Mathf.RoundToInt(UIHelpers.HorizontalSlider(_selectedMap, 0, LobbyMaps.Length - 1));
        if (GUILayout.Button("Apply Map To Lobby"))
            ApplyMap();
    }

    private void AssignRole(PlayerControl player)
    {
        if (!CanChangeLobbySettings() || player == null) return;

        LobbyRoleAssignments.Queue(player.PlayerId, AssignableRoles[Mathf.Clamp(_selectedRoleIndex, 0, AssignableRoles.Length - 1)]);
    }

    private void AssignRoleToEveryone()
    {
        if (!CanChangeLobbySettings()) return;

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            AssignRole(player);
    }

    private void ForceAllVotes(PlayerControl target)
    {
        if (!CanChangeLobbySettings() || MeetingHud.Instance == null || target == null) return;

        foreach (PlayerVoteArea votingArea in MeetingHud.Instance.playerStates)
            votingArea.SetVote(target.PlayerId);

        MeetingHud.Instance.SetDirtyBit(1);
        MeetingHud.Instance.CheckForEndVoting();
    }

    private void ApplyMap()
    {
        if (!CanChangeLobbySettings() || GameManager.Instance?.LogicOptions == null) return;

        IGameOptions options = GameOptions.CreateCloneFromCurrent();
        if (options == null) return;

        Type optionsType = options.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        PropertyInfo mapProperty = optionsType.GetProperty("MapId", flags);
        FieldInfo mapField = optionsType.GetField("MapId", flags);

        if (mapProperty?.CanWrite == true)
            mapProperty.SetValue(options, (int)LobbyMaps[_selectedMap]);
        else if (mapField != null)
            mapField.SetValue(options, (int)LobbyMaps[_selectedMap]);
        else
            return;

        GameManager.Instance.LogicOptions.SetGameOptions(options);
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player?.Data != null)
                GameOptions.SendGameOptionsToClient(options, player.OwnerId);
        }
    }

    private static bool CanChangeLobbySettings()
    {
        if (AmongUsClient.Instance?.AmHost == true) return true;

        MalumMenu.notifications.Send("Lobby Settings", "This option requires lobby host privileges.", 5);
        return false;
    }
}