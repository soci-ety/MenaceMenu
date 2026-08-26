using System;
using AmongUs.GameOptions;
using MalumMenu.features;
using UnityEngine;

namespace MalumMenu;

public class LobbyTab : ITab
{
    public string name => "Lobby";
    public string MaterialSectionName => _materialSection switch
    {
        0 => "Role Assignments",
        1 => "Rules",
        2 => "Host",
        3 => "Start",
        4 => "Presets",
        _ => "Sabotage"
    };

    private readonly HostOnlyTab _hostControls = new();
    private readonly HostOnlyTab2 _lobbyControls = new();
    private readonly SabotageTab _sabotageControls = new();
    private int _selectedPlayerIndex;
    private int _selectedRoleIndex;
    private int _materialSection;
    private TextField _presetNameField;
    private string _presetText = string.Empty;
    private string _presetMessage = string.Empty;
    private bool _rulesLoaded;
    private int _impostors;
    private float _killCooldown;
    private float _playerSpeed;
    private int _emergencyMeetings;
    private int _emergencyCooldown;
    private int _discussionTime;
    private int _votingTime;
    private bool _confirmEjects;
    private bool _anonymousVotes;
    private readonly int[] _roleCounts = new int[9];
    private readonly int[] _roleChances = new int[9];
    public static bool FakeStartCounter;
    public static int FakeStartValue = 69;

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

    private static readonly RoleTypes[] ConfigurableRoles =
    {
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

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game. These options will not work.");
        }
        else if (AmongUsClient.Instance?.AmHost != true)
        {
            GUILayout.Label("You are not the host of the current lobby. Host-only options will do nothing or may trigger anticheat.");
        }

        if (MenuUI.IsMaterialLayoutActive)
            DrawMaterialSections();
        else
        {
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
        }

        GUILayout.EndVertical();
    }

    private void DrawMaterialSections()
    {
        string[] sections = { "Role Assignments", "Rules", "Host", "Start", "Presets", "Sabotage" };
        for (int i = 0; i < sections.Length; i++)
        {
            if (i % 3 == 0)
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(32));

            Color previousBackground = GUI.backgroundColor;
            if (i == _materialSection)
                GUI.backgroundColor = MenuUI.GetMaterialAccentColor();
            if (GUILayout.Button(sections[i], MenuUI.CreateMaterialTabStyle(i == _materialSection), GUILayout.MinWidth(80), GUILayout.ExpandWidth(true)))
            {
                _materialSection = i;
                MenuUI.ResetMaterialContentScroll();
            }
            GUI.backgroundColor = previousBackground;

            if (i % 3 != 2 && i != sections.Length - 1)
                GUILayout.Space(4);
            if (i % 3 == 2 || i == sections.Length - 1)
            {
                GUILayout.EndHorizontal();
                if (i != sections.Length - 1)
                    GUILayout.Space(4);
            }
        }
        GUILayout.Space(8);

        switch (_materialSection)
        {
            case 0:
                DrawLobbySettings();
                break;
            case 1:
                DrawGameSettings();
                break;
            case 2:
                DrawHostAndGameControls();
                break;
            case 3:
                DrawStartControls();
                break;
            case 4:
                DrawPresetControls();
                break;
            default:
                _sabotageControls.Draw();
                break;
        }
    }

    private void DrawHostAndGameControls()
    {
        GUILayout.Label("Host Controls", GUIStylePreset.TabSubtitle);
        _hostControls.Draw();
        GUILayout.Space(12);
        GUILayout.Label("Lobby and Game Controls", GUIStylePreset.TabSubtitle);
        _lobbyControls.Draw();
    }

    private void DrawGameSettings()
    {
        IGameOptions current = GameOptionsManager.Instance?.CurrentGameOptions ??
            GameManager.Instance?.LogicOptions?.currentGameOptions;
        if (current == null)
        {
            GUILayout.Label("Game settings are not ready.", GUIStylePreset.ModernLabel);
            return;
        }

        if (!_rulesLoaded)
        {
            _impostors = current.NumImpostors;
            _killCooldown = current.GetFloat(FloatOptionNames.KillCooldown);
            _playerSpeed = current.GetFloat(FloatOptionNames.PlayerSpeedMod);
            _emergencyMeetings = current.GetInt(Int32OptionNames.NumEmergencyMeetings);
            _emergencyCooldown = current.GetInt(Int32OptionNames.EmergencyCooldown);
            _discussionTime = current.GetInt(Int32OptionNames.DiscussionTime);
            _votingTime = current.GetInt(Int32OptionNames.VotingTime);
            _confirmEjects = current.GetBool(BoolOptionNames.ConfirmImpostor);
            _anonymousVotes = current.GetBool(BoolOptionNames.AnonymousVotes);
            for (int i = 0; i < ConfigurableRoles.Length; i++)
            {
                _roleCounts[i] = current.RoleOptions.GetNumPerGame(ConfigurableRoles[i]);
                _roleChances[i] = current.RoleOptions.GetChancePerGame(ConfigurableRoles[i]);
            }
            _rulesLoaded = true;
        }

        GUILayout.Label("Lobby Rules", GUIStylePreset.TabSubtitle);
        bool boundaries = MalumMenu.menuSettingBoundaries?.Value != false;
        if (MalumMenu.menuSettingBoundaries != null)
            MalumMenu.menuSettingBoundaries.Value = UIHelpers.Toggle(boundaries, " Respect Among Us setting boundaries");
        CheatToggles.noOptionsLimits = !boundaries;

        int maxImpostors = boundaries ? 3 : 15;
        float maxCooldown = boundaries ? 60f : 300f;
        float maxSpeed = boundaries ? 3f : 15f;
        _impostors = DrawIntSetting("Impostors", _impostors, 1, maxImpostors);
        _killCooldown = DrawFloatSetting("Kill cooldown", _killCooldown, 0f, maxCooldown);
        _playerSpeed = DrawFloatSetting("Player speed", _playerSpeed, 0.1f, maxSpeed);
        _emergencyMeetings = DrawIntSetting("Emergency meetings", _emergencyMeetings, 0, 9);
        _emergencyCooldown = DrawIntSetting("Emergency cooldown", _emergencyCooldown, 0, 60);
        _discussionTime = DrawIntSetting("Discussion time", _discussionTime, 0, 120);
        _votingTime = DrawIntSetting("Voting time", _votingTime, 0, 300);
        _confirmEjects = UIHelpers.Toggle(_confirmEjects, " Confirm ejects");
        _anonymousVotes = UIHelpers.Toggle(_anonymousVotes, " Anonymous votes");

        GUILayout.Space(8);
        GUILayout.Label("Role Settings", GUIStylePreset.TabSubtitle);
        for (int i = 0; i < ConfigurableRoles.Length; i++)
        {
            RoleTypes role = ConfigurableRoles[i];
            GUILayout.Label($"{role}: {_roleCounts[i]} per game, {_roleChances[i]}% chance");
            _roleCounts[i] = Mathf.RoundToInt(UIHelpers.HorizontalSlider(_roleCounts[i], 0, 15));
            _roleChances[i] = Mathf.RoundToInt(UIHelpers.HorizontalSlider(_roleChances[i], 0, 100));
        }

        if (GUILayout.Button("Apply Rules To Lobby"))
            ApplyGameSettings();
    }

    private static int DrawIntSetting(string label, int value, int min, int max)
    {
        GUILayout.Label($"{label}: {value}");
        return Mathf.RoundToInt(UIHelpers.HorizontalSlider(value, min, max));
    }

    private static float DrawFloatSetting(string label, float value, float min, float max)
    {
        GUILayout.Label($"{label}: {value:F1}");
        return UIHelpers.HorizontalSlider(value, min, max);
    }

    private void ApplyGameSettings()
    {
        if (!CanChangeLobbySettings()) return;

        IGameOptions current = GameOptionsManager.Instance?.CurrentGameOptions ??
            GameManager.Instance?.LogicOptions?.currentGameOptions;
        IGameOptions options = GameOptions.CreateCloneOptions(current);
        if (options == null) return;

        options.SetInt(Int32OptionNames.NumImpostors, _impostors);
        options.SetFloat(FloatOptionNames.KillCooldown, _killCooldown);
        options.SetFloat(FloatOptionNames.PlayerSpeedMod, _playerSpeed);
        options.SetInt(Int32OptionNames.NumEmergencyMeetings, _emergencyMeetings);
        options.SetInt(Int32OptionNames.EmergencyCooldown, _emergencyCooldown);
        options.SetInt(Int32OptionNames.DiscussionTime, _discussionTime);
        options.SetInt(Int32OptionNames.VotingTime, _votingTime);
        options.SetBool(BoolOptionNames.ConfirmImpostor, _confirmEjects);
        options.SetBool(BoolOptionNames.AnonymousVotes, _anonymousVotes);
        for (int i = 0; i < ConfigurableRoles.Length; i++)
            options.RoleOptions.SetRoleRate(ConfigurableRoles[i], _roleCounts[i], _roleChances[i]);
        GameManager.Instance.LogicOptions.SetGameOptions(options);
        if (GameOptionsManager.Instance != null)
        {
            GameOptionsManager.Instance.CurrentGameOptions = options;
            GameOptionsManager.Instance.GameHostOptions = options;
        }
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player?.Data != null)
                GameOptions.SendGameOptionsToClient(options, player.OwnerId);
        }
        _rulesLoaded = false;
        _presetMessage = "Lobby rules applied.";
    }

    private void DrawStartControls()
    {
        GUILayout.Label("Start Flow", GUIStylePreset.TabSubtitle);
        FakeStartCounter = UIHelpers.Toggle(FakeStartCounter, " Use Custom Start Counter");
        GUILayout.Label($"Start counter value: {FakeStartValue}");
        FakeStartValue = Mathf.RoundToInt(UIHelpers.HorizontalSlider(FakeStartValue, -128, 127));
        GUILayout.Label("The host can use this to test or customize the lobby countdown.", GUIStylePreset.ModernLabel);
    }

    private void DrawPresetControls()
    {
        _presetNameField ??= new TextField("My Lobby");
        GUILayout.Label("Lobby Rule Presets", GUIStylePreset.TabSubtitle);
        GUILayout.Label("Save the current game rules or share them through the clipboard.", GUIStylePreset.ModernLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name:", GUILayout.Width(50));
        _presetNameField.Draw(180);
        if (GUILayout.Button("Save", GUILayout.Width(70)))
        {
            if (AmongUsClient.Instance?.AmHost == true && _rulesLoaded)
                ApplyGameSettings();
            SetPresetMessage(LobbyPresetStore.Save(_presetNameField.Content, out string error), "Lobby preset saved.", error);
        }
        GUILayout.EndHorizontal();

        foreach (string presetName in LobbyPresetStore.List())
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(presetName, GUILayout.Width(180))) _presetNameField.Content = presetName;
            if (GUILayout.Button("Load", GUILayout.Width(60)))
            {
                bool loaded = LobbyPresetStore.Load(presetName, out string loadError);
                SetPresetMessage(loaded, "Lobby preset loaded.", loadError);
                if (loaded) _rulesLoaded = false;
            }
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
                SetPresetMessage(LobbyPresetStore.Delete(presetName, out string deleteError), "Lobby preset deleted.", deleteError);
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Current"))
        {
            _presetText = LobbyPresetStore.Export(out string exportError);
            if (_presetText.Length > 0) { GUIUtility.systemCopyBuffer = _presetText; _presetMessage = "Lobby preset copied."; }
            else _presetMessage = exportError;
        }
        if (GUILayout.Button("Paste"))
        {
            _presetText = GUIUtility.systemCopyBuffer ?? string.Empty;
            _presetMessage = _presetText.Length > 0 ? "Lobby preset pasted. Click Import to apply it." : "Clipboard is empty.";
        }
        if (GUILayout.Button("Import"))
        {
            bool imported = LobbyPresetStore.Import(_presetText, out string importError);
            SetPresetMessage(imported, "Lobby preset imported.", importError);
            if (imported) _rulesLoaded = false;
        }
        GUILayout.EndHorizontal();
        if (!string.IsNullOrEmpty(_presetMessage)) GUILayout.Label(_presetMessage, GUIStylePreset.ModernLabel);
    }

    private void SetPresetMessage(bool succeeded, string success, string error)
    {
        _presetMessage = succeeded ? success : error;
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
            LobbyRoleAssignments.PruneMissingPlayers(PlayerControl.AllPlayerControls);
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
            foreach (var assignment in LobbyRoleAssignments.GetPendingAssignments())
            {
                string playerName = "Unknown player";
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.PlayerId == assignment.Key)
                    {
                        playerName = player.Data?.PlayerName ?? playerName;
                        break;
                    }
                }

                GUILayout.Label($"{playerName} -> {assignment.Value}");
            }

            if (GUILayout.Button("Clear Pending Role Assignments"))
                LobbyRoleAssignments.Clear();

            if (GUILayout.Button("Force All Meeting Votes To Player"))
                ForceAllVotes(selectedPlayer);
        }
        else
        {
            GUILayout.Label("No players are currently available for role assignment.");
        }

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

    private static bool CanChangeLobbySettings()
    {
        if (AmongUsClient.Instance?.AmHost == true) return true;

        MalumMenu.notifications.Send("Lobby Settings", "This option requires lobby host privileges.", 5);
        return false;
    }
}