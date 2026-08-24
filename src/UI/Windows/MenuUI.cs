using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MalumMenu;

public class MenuUI : MonoBehaviour
{
    public static int windowHeight = 600;
    public static int windowWidth = 800;
    public static Rect windowRect;

    public static bool isGUIActive = false;
    private List<ITab> _tabs = new();
    private int _selectedTab;
    private Vector2 _tabScrollPosition = Vector2.zero;
    private Vector2 _contentScrollPosition = Vector2.zero;
    public static float hue; // For RGB mode
    private bool _wasInGameplay = false;

    private void Start()
    {
        // Add all tabs on start
        _tabs.Add(new MovementTab());
        _tabs.Add(new SelfTab());
        _tabs.Add(new ESPTab());
        _tabs.Add(new RolesTab());
        _tabs.Add(new PlayersTab());
        _tabs.Add(new ShipTab());
        _tabs.Add(new SabotageTab());
        _tabs.Add(new ChatTab());
        _tabs.Add(new AnimationsTab());
        _tabs.Add(new ConsoleTab());
        _tabs.Add(new HostOnlyTab());
        _tabs.Add(new HostOnlyTab2());
        _tabs.Add(new PassiveTab());
        _tabs.Add(new TrollTab());
        _tabs.Add(new ProtectionsTab());
        _tabs.Add(new AnticheatTab());
        _tabs.Add(new ModesTab());
        _tabs.Add(new ConfigTab());
        _tabs.Add(new ProfilesTab());
        // _tabs.Add(new OverloadTab());

        // Instantiate 2D area of MenuUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
        _tabs.Add(new SettingsTab());
    }

    public void InitStyles()
    {
        GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 14;
        GUI.skin.window.padding = new RectOffset { left = 12, right = 12, top = 30, bottom = 12 };
        GUI.skin.window.margin = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 };
    }

    private void Update()
    {
        if (Input.GetKeyDown(Utils.StringToKeycode(MalumMenu.menuKeybind.Value)))
        {
            // Enable or disable GUI
            isGUIActive = !isGUIActive;

            if (MalumMenu.menuOpenOnMouse.Value)
            {
                // Teleport the window to the mouse for immediate use
                Vector2 mousePosition = Input.mousePosition;
                windowRect.position = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            }
        }

        if (CheatToggles.rgbMode)
        {
            hue += Time.deltaTime * 0.3f;
            if (hue > 1f) hue -= 1f;
        }

        if (CheatToggles.stealthMode != MalumMenu.inStealthMode)
        {
            MalumMenu.inStealthMode = CheatToggles.stealthMode;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == "MainMenu" || scene.name == "MatchMaking")
            {
                SceneManager.LoadScene(scene.name);
            }
        }

        if (CheatToggles.panicMode) Utils.Panic();

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = !(MalumMenu.inStealthMode || MalumMenu.isPanicked);

        if (CheatToggles.openConfig)
        {
            Utils.OpenConfigFile();
            CheatToggles.openConfig = false;
        }

        // Check if round just ended and disable sabotage cheats
        bool currentlyInGameplay = Utils.isPlayer && Utils.isShip;
        if (_wasInGameplay && !currentlyInGameplay)
        {
            DisableSabotageCheats();
        }
        _wasInGameplay = currentlyInGameplay;

        if (CheatToggles.reloadConfig)
        {
            MalumMenu.Plugin.Config.Reload();
            CheatToggles.reloadConfig = false;
        }

        if (CheatToggles.saveProfile)
        {
            CheatToggles.saveProfile = false;
            CheatToggles.SaveTogglesToProfile();
        }

        if (CheatToggles.loadProfile)
        {
            CheatToggles.LoadTogglesFromProfile();
            CheatToggles.loadProfile = false;
        }

        // Turn off player-dependent cheats if local player doesn't exist
        if (!Utils.isPlayer)
        {
            CheatToggles.setFakeRole = false;
            CheatToggles.setFakeAlive = false;
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.teleportPlayer = false;
            CheatToggles.spectate = false;
            CheatToggles.freecam = false;
            CheatToggles.killPlayer = false;
            CheatToggles.callMeeting = false;

            if (CheatToggles.runOverload)
            {
                OverloadUI.StopOverload();
            }
        }

        // Turn off ship-dependent cheats if ship doesn't exist
        if (!Utils.isShip)
        {
            DisableSabotageCheats();
            CheatToggles.completeMyTasks = false;
            CheatToggles.kickVents = false;
            CheatToggles.reportBody = false;
            CheatToggles.closeMeeting = false;
            CheatToggles.closeAllDoors = false;
            CheatToggles.openAllDoors = false;
            CheatToggles.spamCloseAllDoors = false;
            CheatToggles.spamOpenAllDoors = false;

            MalumCheats.StopShipAnimCheats();
            MalumCheats.CleanUpInjectedTasks();
        }

        // Turn off host-dependent cheats if not host or freeplay
        if (!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.killAll = false;
            CheatToggles.telekillPlayer = false;
            CheatToggles.killAllCrew = false;
            CheatToggles.killAllImps = false;
            CheatToggles.killPlayer = false;
            CheatToggles.ejectPlayer = false;
            CheatToggles.noKillCd = false;
            CheatToggles.killAnyone = false;
            CheatToggles.killVanished = false;
            CheatToggles.forceStartGame = false;
            CheatToggles.skipMeeting = false;
            CheatToggles.voteImmune = false;
            CheatToggles.noGameEnd = false;
            CheatToggles.showProtectMenu = false;
            CheatToggles.showRolesMenu = false;
            CheatToggles.noOptionsLimits = false;
        }

        // Turn off meeting-dependent cheats if not in a meeting
        if (!Utils.isMeeting)
        {
            CheatToggles.skipMeeting = false;
            CheatToggles.ejectPlayer = false;
        }
    }

    public void OnGUI()
    {
        if (!isGUIActive || MalumMenu.isPanicked) return;

        InitStyles();
        UIHelpers.ApplyUIColor();

        GUI.WindowFunction renderer = MalumMenu.menuMaterialLayout.Value
            ? (GUI.WindowFunction)MaterialWindowFunction
            : (GUI.WindowFunction)WindowFunction;

        windowRect = GUI.Window((int)WindowId.MenuUI, windowRect, renderer, "MenaceMenu v" + MalumMenu.malumVersion);
    }

    private void DisableSabotageCheats()
    {
        CheatToggles.sabotageMap = false;
        CheatToggles.unfixableLights = false;
        CheatToggles.commsSab = false;
        CheatToggles.elecSab = false;
        CheatToggles.reactorSab = false;
        CheatToggles.oxygenSab = false;
        CheatToggles.mushSab = false;
        CheatToggles.mushSpore = false;
        CheatToggles.closeAllDoors = false;
        CheatToggles.openAllDoors = false;
        CheatToggles.spamCloseAllDoors = false;
        CheatToggles.spamOpenAllDoors = false;
    }

    public void WindowFunction(int windowID)
    {
        GUILayout.BeginHorizontal();

        // Left tab selector (20% width)
        GUILayout.BeginVertical(GUIStylePreset.ModernBox, GUILayout.Width(windowWidth * 0.2f));
        GUILayout.Space(2);

        _tabScrollPosition = GUILayout.BeginScrollView(_tabScrollPosition, false, true);

        for (var i = 0; i < _tabs.Count; i++)
        {
            Color standardColor = GUI.backgroundColor;

            if (_selectedTab == i)
            {
                GUI.backgroundColor = new Color(0.35f, 0.42f, 0.55f, 1f);
            }

            if (GUILayout.Button(_tabs[i].name, GUIStylePreset.TabButton, GUILayout.Height(40)))
                _selectedTab = i;

            GUI.backgroundColor = standardColor;
        }

        GUILayout.EndScrollView();
        GUILayout.Space(4);
        GUILayout.EndVertical();

        GUILayout.Space(10f);

        // Right tab content and controls (80% width)
        GUILayout.BeginVertical(GUIStylePreset.ModernBox, GUILayout.Width(windowWidth * 0.8f));
        GUILayout.Space(2);
        
        bool scrollContent = _tabs[_selectedTab].name is "Troll" or "Config";
        if (scrollContent)
            _contentScrollPosition = GUILayout.BeginScrollView(_contentScrollPosition, false, true, GUILayout.ExpandHeight(true));

        if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
        {
            GUILayout.Label(_tabs[_selectedTab].name, GUIStylePreset.TabTitle);
            GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(2f), GUILayout.ExpandWidth(true));
            GUILayout.Space(6);
            _tabs[_selectedTab].Draw();
        }

        if (scrollContent)
            GUILayout.EndScrollView();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private void MaterialWindowFunction(int windowID)
    {
        Color primary = GUI.backgroundColor;
        Color surface = new Color(0.09f, 0.10f, 0.12f, 1f);
        Color surfaceContainer = new Color(0.14f, 0.15f, 0.18f, 1f);
        Color surfaceHigh = new Color(0.20f, 0.22f, 0.26f, 1f);
        Color onSurface = new Color(0.92f, 0.93f, 0.96f, 1f);
        Color previousContent = GUI.contentColor;
        Color previousBackground = GUI.backgroundColor;

        GUI.contentColor = onSurface;
        GUI.backgroundColor = surface;
        GUILayout.BeginVertical(GUIStylePreset.ModernBox);

        // Compact app bar
        GUI.backgroundColor = surfaceContainer;
        GUILayout.BeginHorizontal(GUIStylePreset.ModernBox, GUILayout.Height(36));
        GUILayout.Label("MENACEMENU", GUIStylePreset.SectionHeader);
        GUILayout.FlexibleSpace();
        GUILayout.Label("discord.gg/bH4Hy9YnVD", GUIStylePreset.ModernLabel);
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = surfaceHigh;
        GUILayout.Label(Utils.isPlayer ? "IN GAME" : "LOBBY", GUIStylePreset.ModernLabel, GUILayout.Width(72));
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.BeginHorizontal(GUILayout.Height(windowHeight - 92));

        GUI.backgroundColor = surfaceContainer;
        GUILayout.BeginVertical(GUIStylePreset.ModernBox, GUILayout.Width(windowWidth * 0.21f));
        GUILayout.Label("NAVIGATION", GUIStylePreset.ModernLabel);
        GUILayout.Space(2);
        
        _tabScrollPosition = GUILayout.BeginScrollView(_tabScrollPosition, false, true);

        for (int i = 0; i < _tabs.Count; i++)
        {
            Color tabBackground = GUI.backgroundColor;
            GUI.backgroundColor = _selectedTab == i ? UIHelpers.GetHighlightColor(primary) : surfaceContainer;

            if (GUILayout.Button(_tabs[i].name, GUIStylePreset.TabButton, GUILayout.Height(34)))
                _selectedTab = i;

            GUI.backgroundColor = tabBackground;
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(8);

        GUI.backgroundColor = surfaceContainer;
        GUILayout.BeginVertical(GUIStylePreset.ModernBox, GUILayout.Width(windowWidth * 0.77f));
        
        if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(_tabs[_selectedTab].name, GUIStylePreset.TabTitle);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{_selectedTab + 1:00} / {_tabs.Count:00}", GUIStylePreset.ModernLabel);
            GUILayout.EndHorizontal();
            GUILayout.Box(string.Empty, GUIStylePreset.Separator, GUILayout.Height(2f), GUILayout.ExpandWidth(true));
            GUILayout.Space(6);

            _contentScrollPosition = GUILayout.BeginScrollView(_contentScrollPosition, false, true, GUILayout.ExpandHeight(true));
            _tabs[_selectedTab].Draw();
            GUILayout.EndScrollView();
        }
    }
}
