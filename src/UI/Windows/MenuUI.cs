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
    public static bool IsMaterialLayoutActive { get; private set; }
    public static GUISkin MaterialSkin => _materialSkin;

    public static GUISkin GetWindowSkin(GUISkin fallback)
    {
        if (MalumMenu.menuMaterialLayout?.Value != true)
            return fallback;

        return GetMaterialSkin(_legacySkin ?? fallback);
    }

    private List<ITab> _tabs = new();
    private int _selectedTab = 1;
    private Vector2 _tabScrollPosition = Vector2.zero;
    private Vector2 _contentScrollPosition = Vector2.zero;
    private Vector2 _materialTabScrollPosition = Vector2.zero;
    private Vector2 _materialTabScrollTarget = Vector2.zero;
    private Vector2 _materialContentScrollPosition = Vector2.zero;
    private Vector2 _materialContentScrollTarget = Vector2.zero;
    public static float hue; // For RGB mode
    private bool _wasInGameplay = false;
    private int _materialRenderedTab = -1;
    private float _materialPageProgress = 1f;
    private string _lastSceneName;
    private bool _lastGameplayState;
    private bool _lastPlayerAvailable;
    private bool _lastMaterialLayout;
    private bool _lastNewLayout;
    private bool _materialResizing;
    private Vector2 _materialResizeStart;
    private Vector2 _materialResizeOrigin;
    private GUISkin _unitySkin;
    private static GUISkin _legacySkin;

    private static readonly string[] DefaultTabOrder =
    {
        "Movement", "Self", "ESP", "Roles", "Players", "Ship", "Sabotage", "Chat", "Animations",
        "Console", "Lobby", "Passive", "Troll", "Protections", "Anticheat", "Modes", "Config",
        "Profiles", "Skins", "Settings", "Test"
    };

    private static readonly string[] NewTabOrder =
    {
        "Movement", "Self", "Roles", "Players", "ESP", "Chat", "Lobby", "Modes", "Ship", "Sabotage",
        "Passive", "Troll", "Protections", "Anticheat", "Animations", "Console", "Profiles", "Skins",
        "Config", "Settings", "Test"
    };

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
        _tabs.Add(new LobbyTab());
        _tabs.Add(new PassiveTab());
        _tabs.Add(new TrollTab());
        _tabs.Add(new ProtectionsTab());
        _tabs.Add(new AnticheatTab());
        _tabs.Add(new ModesTab());
        _tabs.Add(new ConfigTab());
        _tabs.Add(new ProfilesTab());
        _tabs.Add(new SkinsTab());
        // _tabs.Add(new OverloadTab());

        // Instantiate 2D area of MenuUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
        _tabs.Add(new SettingsTab());
        _tabs.Add(new TestTab());
    }

    public void InitStyles()
    {
        GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 14;
        GUI.skin.window.padding = new RectOffset { left = 12, right = 12, top = 30, bottom = 12 };
        GUI.skin.window.margin = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 };
    }

    public void SelectTab(string tabName)
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            if (_tabs[i].name != tabName || !IsTabVisible(_tabs[i])) continue;
            _selectedTab = i;
            ResetMaterialContentScroll();
            return;
        }
    }

    private static bool IsTabVisible(ITab tab)
    {
        if (tab is TestTab)
            return MalumMenu.menuShowTestTab?.Value == true;
        return true;
    }

    private void ApplyMaterialTabOrder(bool newLayout)
    {
        if (_lastNewLayout == newLayout) return;

        ITab selectedTab = _selectedTab >= 0 && _selectedTab < _tabs.Count ? _tabs[_selectedTab] : null;
        string[] order = newLayout ? NewTabOrder : DefaultTabOrder;
        List<ITab> reorderedTabs = new();
        foreach (string tabName in order)
        {
            ITab tab = _tabs.Find(candidate => candidate.name == tabName);
            if (tab != null) reorderedTabs.Add(tab);
        }
        _tabs = reorderedTabs;
        _selectedTab = selectedTab == null ? 0 : Mathf.Max(0, _tabs.IndexOf(selectedTab));
        _lastNewLayout = newLayout;
        ResetMaterialContentScroll();
    }

    private void Update()
    {
        RefreshUiForContextChanges();

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
        if (_wasInGameplay != currentlyInGameplay)
            ResetMaterialUiState();
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

    private void RefreshUiForContextChanges()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool gameplayState = Utils.isPlayer && Utils.isShip;
        bool playerAvailable = PlayerControl.LocalPlayer != null;
        bool materialLayout = MalumMenu.menuMaterialLayout?.Value == true;

        if (_lastSceneName == null)
        {
            _lastSceneName = sceneName;
            _lastGameplayState = gameplayState;
            _lastPlayerAvailable = playerAvailable;
            _lastMaterialLayout = materialLayout;
            return;
        }

        if (sceneName == _lastSceneName && gameplayState == _lastGameplayState &&
            playerAvailable == _lastPlayerAvailable && materialLayout == _lastMaterialLayout)
            return;

        _lastSceneName = sceneName;
        _lastGameplayState = gameplayState;
        _lastPlayerAvailable = playerAvailable;
        _lastMaterialLayout = materialLayout;
        ResetMaterialUiState();
    }

    private void ResetMaterialUiState()
    {
        _materialRenderedTab = -1;
        _materialPageProgress = 1f;
        _materialTabScrollPosition = Vector2.zero;
        _materialTabScrollTarget = Vector2.zero;
        _materialContentScrollPosition = Vector2.zero;
        _materialContentScrollTarget = Vector2.zero;
        _contentScrollPosition = Vector2.zero;
        UIHelpers.ResetMaterialStyles();
        _materialSkin = null;
        _materialSkinSource = null;
        IsMaterialLayoutActive = false;
        GUI.changed = true;
    }

    public void OnGUI()
    {
        if (!isGUIActive || MalumMenu.isPanicked) return;

        bool materialLayout = MalumMenu.menuMaterialLayout?.Value == true;
        if (materialLayout)
            ApplyMaterialTabOrder(MalumMenu.menuMaterialNewLayout?.Value == true);
        else if (_lastNewLayout)
            ApplyMaterialTabOrder(false);
        IsMaterialLayoutActive = materialLayout;
        if (_unitySkin == null)
            _unitySkin = GUI.skin;
        if (_legacySkin == null)
            _legacySkin = UnityEngine.Object.Instantiate(_unitySkin);
        GUI.skin = _legacySkin;
        InitStyles();
        UIHelpers.ApplyUIColor();

        if (materialLayout)
            GUI.skin = GetMaterialSkin(_legacySkin);
        else
            GUI.skin = _legacySkin;

        if (materialLayout && _selectedTab >= 0 && _selectedTab < _tabs.Count &&
            !IsTabVisible(_tabs[_selectedTab]))
        {
            _selectedTab = 0;
            ResetMaterialContentScroll();
        }

        GUI.WindowFunction renderer = materialLayout
            ? (MaterialNewLayoutEnabled
                ? (GUI.WindowFunction)NewLayoutWindowFunction
                : (GUI.WindowFunction)MaterialWindowFunction)
            : (GUI.WindowFunction)WindowFunction;

        string title = materialLayout ? string.Empty : "Menace Menu v" + MalumMenu.menaceVersion;
        windowRect.width = Mathf.Min(windowRect.width, Mathf.Max(320f, Screen.width - 20f));
        windowRect.height = Mathf.Min(windowRect.height, Mathf.Max(260f, Screen.height - 20f));
        windowRect.x = Mathf.Clamp(windowRect.x, 0f, Mathf.Max(0f, Screen.width - windowRect.width));
        windowRect.y = Mathf.Clamp(windowRect.y, 0f, Mathf.Max(0f, Screen.height - windowRect.height));
        Color previousWindowColor = GUI.color;
        if (materialLayout)
            GUI.color = new Color(1f, 1f, 1f, MaterialWindowOpacity);
        windowRect = GUI.Window((int)WindowId.MenuUI, windowRect, renderer, title);
        if (materialLayout)
        {
            GUISkin resizeSkin = GUI.skin;
            GUI.skin = GetMaterialSkin(_legacySkin);
            DrawMaterialResizeGrip();
            GUI.skin = resizeSkin;
        }
        GUI.color = previousWindowColor;
        GUI.skin = _unitySkin;

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

        _tabScrollPosition = UIHelpers.BeginScrollView(_tabScrollPosition, false, true);

        for (var i = 0; i < _tabs.Count; i++)
        {
            if (!IsTabVisible(_tabs[i]))
                continue;
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
        
        bool scrollContent = true;
        if (scrollContent)
            _contentScrollPosition = UIHelpers.BeginScrollView(_contentScrollPosition, false, true, GUILayout.Height(windowHeight - 58));

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
        GUISkin previousSkin = GUI.skin;
        GUI.skin = GetMaterialSkin(_legacySkin ?? previousSkin);
        Color oldBackground = GUI.backgroundColor;
        Color oldContent = GUI.contentColor;
        Color oldColor = GUI.color;
        GUI.color = Color.white;
        GUI.contentColor = MaterialText;

        float width = windowRect.width;
        float height = windowRect.height;
        float sidebarWidth = Mathf.Clamp(width * 0.20f, 122f, 160f);
        float bodyX = sidebarWidth + 18f;
        float bodyWidth = width - bodyX - 12f;
        float bodyHeight = height - 48f;
        if (_materialRenderedTab != _selectedTab)
        {
            _materialRenderedTab = _selectedTab;
            _materialPageProgress = MaterialPageAnimationsEnabled ? 0f : 1f;
            _contentScrollPosition = Vector2.zero;
            _materialContentScrollPosition = Vector2.zero;
            _materialContentScrollTarget = Vector2.zero;
        }
        float pageEase = 1f;
        if (MaterialPageAnimationsEnabled)
        {
            float animationStep = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 10f);
            _materialPageProgress = Mathf.Lerp(_materialPageProgress, 1f, animationStep);
            pageEase = Mathf.SmoothStep(0f, 1f, _materialPageProgress);
        }
        if (_materialPageProgress < 0.999f)
            GUI.changed = true;

        GUI.Box(new Rect(0f, 0f, width, height), GUIContent.none, CreatePanelStyle(MaterialSurface));
        GUI.Box(new Rect(0f, 0f, width, 34f), GUIContent.none, CreatePanelStyle(MaterialHeader));
        GUI.Label(new Rect(18f, 6f, Mathf.Max(280f, width - 190f), 24f), "MENACE MENU // V1.2.1-pre-release.1", CreateHeaderStyle());
        GUI.Label(new Rect(width - 120f, 8f, 100f, 20f), Utils.isPlayer ? "IN GAME" : "LOBBY", MaterialStatusStyle());
        GUI.Box(new Rect(0f, 34f, width, 3f), GUIContent.none, CreateGradientStyle(MaterialAccent, MaterialAccentEnd));

        GUI.Box(new Rect(10f, 48f, sidebarWidth, bodyHeight), GUIContent.none, CreatePanelStyle(MaterialNavigation));
        GUILayout.BeginArea(new Rect(18f, 56f, sidebarWidth - 16f, bodyHeight - 12f));
        GUILayout.Label("NAVIGATION", MaterialCaptionStyle());
        Vector2 tabScrollInput = UIHelpers.BeginScrollView(_materialTabScrollPosition, false, true);
        for (int i = 0; i < _tabs.Count; i++)
        {
            if (!IsTabVisible(_tabs[i]))
                continue;
            if (GUILayout.Button(_tabs[i].name, _selectedTab == i ? CreateSelectedNavigationStyle() : CreateNavigationStyle(), GUILayout.Height(27)))
                _selectedTab = i;
        }
        GUILayout.EndScrollView();
        UpdateMaterialScroll(ref _materialTabScrollPosition, ref _materialTabScrollTarget, tabScrollInput);
        GUILayout.EndArea();

        GUI.Box(new Rect(bodyX, 48f, bodyWidth, bodyHeight), GUIContent.none, CreatePanelStyle(MaterialContent));
        GUILayout.BeginArea(new Rect(bodyX + 14f, 58f + Mathf.Lerp(10f, 0f, pageEase), bodyWidth - 28f, bodyHeight - 18f));
        if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
        {
            Color pageColor = GUI.color;
            GUI.color = new Color(pageColor.r, pageColor.g, pageColor.b, pageEase);
            GUILayout.BeginHorizontal(CreatePageHeaderStyle(), GUILayout.Height(54));
            GUILayout.BeginVertical();
            GUILayout.Label(GetMaterialPageTitle(), CreateContentTitleStyle());
            GUILayout.Label("gg/bH4Hy9YnVD", MaterialCaptionStyle());
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Box(string.Empty, CreateGradientStyle(MaterialAccent, MaterialAccentEnd), GUILayout.Height(2), GUILayout.ExpandWidth(true));
            GUILayout.Space(8);
            GUILayout.BeginVertical(MaterialNewLayoutEnabled ? CreateOrganizedWorkSurfaceStyle() : CreateWorkSurfaceStyle());
            Vector2 contentScrollInput = UIHelpers.BeginScrollView(_materialContentScrollPosition, false, true,
                GUILayout.Height(bodyHeight - 98f));
            _tabs[_selectedTab].Draw();
            GUILayout.EndScrollView();
            UpdateMaterialScroll(ref _materialContentScrollPosition, ref _materialContentScrollTarget, contentScrollInput);
            GUILayout.EndVertical();
            GUI.color = pageColor;
        }
        GUILayout.EndArea();

        GUI.backgroundColor = oldBackground;
        GUI.contentColor = oldContent;
        GUI.color = oldColor;
        GUI.skin = previousSkin;
        GUI.DragWindow(new Rect(0, 0, windowWidth, 36));
    }

    private void NewLayoutWindowFunction(int windowID)
    {
        GUISkin previousSkin = GUI.skin;
        GUI.skin = GetMaterialSkin(_legacySkin ?? previousSkin);
        Color oldBackground = GUI.backgroundColor;
        Color oldContent = GUI.contentColor;
        Color oldColor = GUI.color;
        GUI.color = Color.white;
        GUI.contentColor = MaterialText;

        float width = windowRect.width;
        float height = windowRect.height;
        float railWidth = 178f;
        float contentX = railWidth + 14f;
        float contentWidth = width - contentX - 12f;
        float bodyHeight = height - 48f;

        GUI.Box(new Rect(0f, 0f, width, height), GUIContent.none, CreatePanelStyle(MaterialSurface));
        GUI.Box(new Rect(0f, 0f, width, 42f), GUIContent.none, CreatePanelStyle(MaterialHeader));
        GUI.Label(new Rect(18f, 7f, width - 250f, 28f), "MENACE MENU // NEW LAYOUT", CreateHeaderStyle());
        GUI.Label(new Rect(width - 142f, 10f, 124f, 20f), Utils.isPlayer ? "IN GAME" : "LOBBY", MaterialStatusStyle());
        GUI.Box(new Rect(0f, 42f, width, 3f), GUIContent.none, CreateGradientStyle(MaterialAccent, MaterialAccentEnd));

        GUI.Box(new Rect(10f, 56f, railWidth, bodyHeight - 8f), GUIContent.none, CreatePanelStyle(MaterialNavigation));
        GUILayout.BeginArea(new Rect(22f, 66f, railWidth - 24f, bodyHeight - 28f));
        GUILayout.Label("NAVIGATION", MaterialCaptionStyle());
        Vector2 navigationInput = UIHelpers.BeginScrollView(_materialTabScrollPosition, false, true);
        DrawNewLayoutCategory("CORE", "Movement", "Self", "Roles", "Players");
        DrawNewLayoutCategory("LOBBY", "Lobby", "Modes", "Ship", "Sabotage");
        DrawNewLayoutCategory("TOOLS", "ESP", "Chat", "Animations", "Console");
        DrawNewLayoutCategory("SYSTEM", "Passive", "Troll", "Protections", "Anticheat", "Config", "Profiles", "Skins", "Settings", "Test");
        GUILayout.EndScrollView();
        UpdateMaterialScroll(ref _materialTabScrollPosition, ref _materialTabScrollTarget, navigationInput);
        GUILayout.EndArea();

        GUI.Box(new Rect(contentX, 56f, contentWidth, bodyHeight - 8f), GUIContent.none, CreatePanelStyle(MaterialContent));
        GUILayout.BeginArea(new Rect(contentX + 16f, 68f, contentWidth - 32f, bodyHeight - 30f));
        if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
        {
            GUILayout.Label(_tabs[_selectedTab].name, CreateContentTitleStyle());
            GUILayout.Label("gg/bH4Hy9YnVD", MaterialCaptionStyle());
            GUILayout.Box(string.Empty, CreateGradientStyle(MaterialAccent, MaterialAccentEnd), GUILayout.Height(2), GUILayout.ExpandWidth(true));
            GUILayout.Space(10f);
            GUILayout.BeginVertical(CreateOrganizedWorkSurfaceStyle());
            Vector2 contentInput = UIHelpers.BeginScrollView(_materialContentScrollPosition, false, true,
                GUILayout.Height(bodyHeight - 82f));
            _tabs[_selectedTab].Draw();
            GUILayout.EndScrollView();
            UpdateMaterialScroll(ref _materialContentScrollPosition, ref _materialContentScrollTarget, contentInput);
            GUILayout.EndVertical();
        }
        GUILayout.EndArea();

        GUI.backgroundColor = oldBackground;
        GUI.contentColor = oldContent;
        GUI.color = oldColor;
        GUI.skin = previousSkin;
        GUI.DragWindow(new Rect(0, 0, windowWidth, 44));
    }

    private void DrawNewLayoutCategory(string title, params string[] tabNames)
    {
        GUILayout.Space(6f);
        GUILayout.Label(title, MaterialCaptionStyle());
        foreach (string tabName in tabNames)
        {
            int tabIndex = _tabs.FindIndex(tab => tab.name == tabName && IsTabVisible(tab));
            if (tabIndex < 0) continue;
            if (GUILayout.Button(tabName, _selectedTab == tabIndex ? CreateSelectedNavigationStyle() : CreateNavigationStyle(), GUILayout.Height(27f)))
            {
                _selectedTab = tabIndex;
                ResetMaterialContentScroll();
            }
        }
    }

    private void DrawMaterialResizeGrip()
    {
        const float gripSize = 22f;
        Rect grip = new(windowRect.xMax - gripSize, windowRect.yMax - gripSize, gripSize, gripSize);
        int controlId = GUIUtility.GetControlID(148237, FocusType.Passive);
        Event current = Event.current;

        if (current.type == EventType.MouseDown && current.button == 0 && grip.Contains(current.mousePosition))
        {
            _materialResizing = true;
            _materialResizeStart = current.mousePosition;
            _materialResizeOrigin = new Vector2(windowRect.width, windowRect.height);
            GUIUtility.hotControl = controlId;
            current.Use();
        }
        else if (_materialResizing && current.type == EventType.MouseDrag && GUIUtility.hotControl == controlId)
        {
            Vector2 delta = current.mousePosition - _materialResizeStart;
            windowRect.width = Mathf.Clamp(_materialResizeOrigin.x + delta.x, 520f, Screen.width - 20f);
            windowRect.height = Mathf.Clamp(_materialResizeOrigin.y + delta.y, 360f, Screen.height - 20f);
            GUI.changed = true;
            current.Use();
        }
        else if (_materialResizing && current.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
        {
            _materialResizing = false;
            GUIUtility.hotControl = 0;
            current.Use();
        }

        Color previousColor = GUI.color;
        GUI.color = MaterialAccent;
        _materialResizeGripTexture ??= Texture2D.whiteTexture;
        GUI.Box(new Rect(windowRect.xMax - 24f, windowRect.yMax - 24f, 20f, 20f),
            GUIContent.none, CreateTextureStyle(_materialResizeGripTexture));
        GUI.color = previousColor;
    }

    private static GUIStyle CreateTextureStyle(Texture2D texture)
    {
        return new GUIStyle
        {
            normal = { background = texture },
            padding = new RectOffset(),
            margin = new RectOffset(),
            border = new RectOffset()
        };
    }

    private static void UpdateMaterialScroll(ref Vector2 position, ref Vector2 target, Vector2 input)
    {
        if ((input - position).sqrMagnitude > 0.01f)
            target = input;

        if (!MaterialSmoothScrollingEnabled)
        {
            position = target;
            return;
        }

        float scrollStep = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 14f);
        Vector2 nextPosition = Vector2.Lerp(position, target, scrollStep);
        if ((nextPosition - position).sqrMagnitude > 0.01f)
            GUI.changed = true;
        position = nextPosition;
    }

    private static bool MaterialSmoothScrollingEnabled =>
        MalumMenu.menuMaterialLowPerformance?.Value != true &&
        MalumMenu.menuMaterialSmoothScrolling?.Value != false;

    private static bool MaterialPageAnimationsEnabled =>
        MalumMenu.menuMaterialLowPerformance?.Value != true &&
        MalumMenu.menuMaterialPageAnimations?.Value != false;

    private static bool MaterialNewLayoutEnabled =>
        MalumMenu.menuMaterialLayout?.Value == true && MalumMenu.menuMaterialNewLayout?.Value == true;

    private string GetMaterialPageTitle()
    {
        return _tabs[_selectedTab] switch
        {
            RolesTab roles => $"Roles / {roles.MaterialSectionName}",
            LobbyTab lobby => $"Lobby / {lobby.MaterialSectionName}",
            _ => _tabs[_selectedTab].name
        };
    }

    private static readonly Color MaterialSurface = new(0.045f, 0.05f, 0.06f, 1f);
    private static readonly Color MaterialHeader = new(0.09f, 0.10f, 0.12f, 1f);
    private static readonly Color MaterialNavigation = new(0.065f, 0.075f, 0.085f, 1f);
    private static readonly Color MaterialContent = new(0.10f, 0.105f, 0.115f, 1f);
    private static Color MaterialAccent = new(0.10f, 0.78f, 0.68f, 1f);
    private static Color MaterialAccentEnd = new(0.95f, 0.55f, 0.25f, 1f);
    private static readonly Color MaterialText = new(0.95f, 0.94f, 0.90f, 1f);

    private static GUIStyle CreatePanelStyle(Color color)
    {
        return new GUIStyle(GUI.skin.box)
        {
            normal = { background = CreateRoundedTexture(color) },
            padding = new RectOffset { left = 12, right = 12, top = 10, bottom = 10 },
            margin = new RectOffset(),
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
        };
    }

    private static GUIStyle CreateGradientStyle(Color start, Color end)
    {
        return new GUIStyle(GUI.skin.box)
        {
            normal = { background = CreateGradientTexture(start, end) },
            padding = new RectOffset(),
            margin = new RectOffset(),
            border = new RectOffset()
        };
    }

    private static GUIStyle CreateNavigationStyle(Color unused = default)
    {
        return new GUIStyle(GUI.skin.button)
        {
            normal = { textColor = new Color(0.70f, 0.77f, 0.86f, 1f) },
            hover = { textColor = Color.white },
            active = { textColor = Color.white },
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset { left = 12, right = 8, top = 5, bottom = 5 },
            margin = new RectOffset { left = 0, right = 0, top = 2, bottom = 2 },
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            wordWrap = false
        };
    }

    private static GUIStyle CreateSelectedNavigationStyle()
    {
        GUIStyle style = CreateNavigationStyle();
        style.normal.background = CreateRoundedTexture(MaterialAccent);
        style.normal.textColor = Color.white;
        return style;
    }

    public static GUIStyle CreateMaterialTabStyle(bool selected)
    {
        GUIStyle style = new(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.70f, 0.77f, 0.86f, 1f) },
            hover = { textColor = Color.white },
            active = { textColor = Color.white },
            padding = new RectOffset { left = 8, right = 8, top = 5, bottom = 5 },
            margin = new RectOffset(),
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            wordWrap = false,
            fixedHeight = 28f
        };
        if (selected)
        {
            style.normal.background = CreateRoundedTexture(MaterialAccent);
            style.normal.textColor = Color.white;
        }
        return style;
    }

    private static GUIStyle CreateHeaderStyle() => new(GUI.skin.label)
    {
        fontSize = 21,
        fontStyle = FontStyle.Bold,
        normal = { textColor = MaterialText },
        padding = new RectOffset(),
        margin = new RectOffset()
    };

    private static GUIStyle CreateContentTitleStyle() => new(GUI.skin.label)
    {
        fontSize = 20,
        fontStyle = FontStyle.Bold,
        normal = { textColor = MaterialText },
        padding = new RectOffset { bottom = 4 },
        margin = new RectOffset()
    };

    private static GUIStyle CreatePageHeaderStyle() => new(GUI.skin.box)
    {
        normal = { background = CreateRoundedTexture(new Color(0.13f, 0.145f, 0.16f, 1f)) },
        padding = new RectOffset { left = 10, right = 8, top = 6, bottom = 6 },
        margin = new RectOffset(),
        border = new RectOffset()
    };

    private static GUIStyle CreatePageNumberStyle() => new(GUI.skin.label)
    {
        fontSize = 24,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleLeft,
        normal = { textColor = MaterialAccent },
        padding = new RectOffset(),
        margin = new RectOffset()
    };

    private static GUIStyle CreateWorkSurfaceStyle() => new(GUI.skin.box)
    {
        normal = { background = CreateRoundedTexture(new Color(0.06f, 0.068f, 0.075f, 1f)) },
        padding = new RectOffset { left = 10, right = 10, top = 8, bottom = 8 },
        margin = new RectOffset(),
        border = new RectOffset()
    };

    private static GUIStyle CreateOrganizedWorkSurfaceStyle() => new(GUI.skin.box)
    {
        normal = { background = CreateRoundedTexture(new Color(0.075f, 0.09f, 0.11f, 1f)) },
        padding = new RectOffset { left = 16, right = 16, top = 14, bottom = 14 },
        margin = new RectOffset(),
        border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
    };

    private static GUIStyle MaterialCaptionStyle() => new(GUI.skin.label)
    {
        fontSize = 10,
        fontStyle = FontStyle.Bold,
        normal = { textColor = new Color(0.45f, 0.58f, 0.70f, 1f) },
        padding = new RectOffset(),
        margin = new RectOffset { bottom = 2 }
    };

    private static GUIStyle MaterialStatusStyle() => new(GUI.skin.label)
    {
        fontSize = 11,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter,
        normal = { textColor = MaterialAccent },
        padding = new RectOffset { left = 10, right = 10, top = 5, bottom = 5 },
        margin = new RectOffset()
    };

    private static Texture2D CreateTexture(Color color)
    {
        color.a *= MaterialWindowOpacity;
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateRoundedTexture(Color color)
    {
        const int size = 32;
        const float radius = 7f;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, 0f);
                dx = Mathf.Max(dx, x - (size - radius - 1f));
                float dy = Mathf.Max(radius - y, 0f);
                dy = Mathf.Max(dy, y - (size - radius - 1f));
                Color pixel = color;
                pixel.a *= MaterialWindowOpacity * Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                texture.SetPixel(x, y, pixel);
            }
        }
        texture.Apply();
        return texture;
    }

    public static int MaterialColorPreset { get; private set; }
    public static float MaterialWindowOpacity { get; set; } = 0.94f;

    public static Color GetMaterialAccentColor()
    {
        return MaterialAccent;
    }

    public static void ResetMaterialContentScroll()
    {
        if (MalumMenu.menuUI != null)
        {
            MalumMenu.menuUI._materialContentScrollPosition = Vector2.zero;
            MalumMenu.menuUI._materialContentScrollTarget = Vector2.zero;
        }
    }

    public static readonly string[] MaterialColorPresets =
        { "Teal / Amber", "Ocean / Lime", "Cardinal / Gold", "Violet / Rose", "Cobalt / Mint", "Copper / Sky", "Mono / Ice", "Plum / Peach" };

    public static void SetMaterialColorPreset(int preset)
    {
        MaterialColorPreset = Mathf.Clamp(preset, 0, MaterialColorPresets.Length - 1);
        switch (MaterialColorPreset)
        {
            case 1:
                MaterialAccent = new Color(0.12f, 0.60f, 0.95f, 1f);
                MaterialAccentEnd = new Color(0.45f, 0.82f, 0.28f, 1f);
                break;
            case 2:
                MaterialAccent = new Color(0.92f, 0.24f, 0.30f, 1f);
                MaterialAccentEnd = new Color(0.98f, 0.68f, 0.18f, 1f);
                break;
            case 3:
                MaterialAccent = new Color(0.62f, 0.34f, 0.94f, 1f);
                MaterialAccentEnd = new Color(0.98f, 0.34f, 0.62f, 1f);
                break;
            case 4:
                MaterialAccent = new Color(0.16f, 0.42f, 0.95f, 1f);
                MaterialAccentEnd = new Color(0.22f, 0.92f, 0.74f, 1f);
                break;
            case 5:
                MaterialAccent = new Color(0.90f, 0.38f, 0.16f, 1f);
                MaterialAccentEnd = new Color(0.24f, 0.70f, 0.94f, 1f);
                break;
            case 6:
                MaterialAccent = new Color(0.72f, 0.76f, 0.80f, 1f);
                MaterialAccentEnd = new Color(0.25f, 0.78f, 0.92f, 1f);
                break;
            case 7:
                MaterialAccent = new Color(0.72f, 0.25f, 0.62f, 1f);
                MaterialAccentEnd = new Color(1.00f, 0.58f, 0.38f, 1f);
                break;
            default:
                MaterialAccent = new Color(0.10f, 0.78f, 0.68f, 1f);
                MaterialAccentEnd = new Color(0.95f, 0.55f, 0.25f, 1f);
                break;
        }
        UIHelpers.ResetMaterialStyles();
        _materialSkin = null;
        _materialSkinSource = null;
    }

    private static Texture2D CreateSliderRailTexture(Color color)
    {
                    const int size = 32;
                    const int railTop = 12;
                    const int railBottom = 20;
                    Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            bool insideRail = y >= railTop && y < railBottom;
                            float edgeDistance = Mathf.Min(Mathf.Min(x, size - 1 - x),
                                Mathf.Min(y - railTop, railBottom - 1 - y));
                            float alpha = insideRail ? Mathf.Clamp01(edgeDistance + 1f) : 0f;
                            texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * MaterialWindowOpacity * alpha));
                        }
                    }
                    texture.Apply();
                    return texture;
                }

    private static Texture2D CreateGradientTexture(Color start, Color end)
    {
        start.a *= MaterialWindowOpacity;
        end.a *= MaterialWindowOpacity;
        Texture2D texture = new(2, 1);
        texture.SetPixel(0, 0, start);
        texture.SetPixel(1, 0, end);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateRoundedGradientTexture(Color start, Color end)
    {
        const int size = 32;
        const float radius = 7f;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, 0f);
                dx = Mathf.Max(dx, x - (size - radius - 1f));
                float dy = Mathf.Max(radius - y, 0f);
                dy = Mathf.Max(dy, y - (size - radius - 1f));
                Color pixel = Color.Lerp(start, end, (float)x / (size - 1));
                pixel.a *= MaterialWindowOpacity * Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                texture.SetPixel(x, y, pixel);
            }
        }
        texture.Apply();
        return texture;
    }

    private static GUISkin _materialSkin;
    private static GUISkin _materialSkinSource;
    private static Texture2D _materialResizeGripTexture;

    private static GUISkin GetMaterialSkin(GUISkin source)
    {
        if (_materialSkin != null && _materialSkinSource == source)
            return _materialSkin;

        _materialSkin = UnityEngine.Object.Instantiate(source);
        _materialSkinSource = source;
        _materialSkin.name = "MenaceMaterialSkin";
        _materialSkin.window = new GUIStyle(source.window)
        {
            padding = new RectOffset(),
            margin = new RectOffset(),
            border = new RectOffset(),
            normal = { background = CreateRoundedTexture(MaterialSurface) }
        };

        _materialSkin.button = new GUIStyle(source.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset { left = 10, right = 10, top = 6, bottom = 6 },
            margin = new RectOffset { left = 2, right = 2, top = 3, bottom = 3 },
            normal = { background = CreateRoundedTexture(new Color(0.12f, 0.16f, 0.22f, 1f)), textColor = MaterialText },
            hover = { background = CreateRoundedGradientTexture(MaterialAccent, MaterialAccentEnd), textColor = Color.white },
            active = { background = CreateRoundedTexture(MaterialAccentEnd), textColor = Color.white },
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
        };

        _materialSkin.toggle = new GUIStyle(source.toggle)
        {
            fontSize = 13,
            padding = new RectOffset { left = 22, right = 8, top = 5, bottom = 5 },
            margin = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 },
            normal = { textColor = new Color(0.76f, 0.84f, 0.92f, 1f) },
            onNormal = { textColor = Color.white },
            hover = { textColor = Color.white },
            onHover = { textColor = Color.white }
        };

        _materialSkin.label = new GUIStyle(source.label)
        {
            fontSize = 13,
            normal = { textColor = MaterialText },
            padding = new RectOffset { left = 4, right = 4, top = 3, bottom = 3 },
            margin = new RectOffset { left = 2, right = 2, top = 1, bottom = 1 },
            wordWrap = true
        };

        _materialSkin.box = new GUIStyle(source.box)
        {
            normal = { background = CreateRoundedTexture(MaterialContent) },
            padding = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            margin = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 },
            border = new RectOffset()
        };
        _materialSkin.scrollView = new GUIStyle()
        {
            normal = { background = CreateRoundedTexture(MaterialContent) },
            padding = new RectOffset(),
            margin = new RectOffset(),
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
        };

        _materialSkin.horizontalSlider = new GUIStyle()
        {
            normal = { background = CreateSliderRailTexture(new Color(0.18f, 0.25f, 0.34f, 1f)) },
            hover = { background = CreateSliderRailTexture(new Color(0.22f, 0.30f, 0.40f, 1f)) },
            active = { background = CreateSliderRailTexture(new Color(0.25f, 0.34f, 0.44f, 1f)) },
            focused = { background = CreateSliderRailTexture(new Color(0.25f, 0.34f, 0.44f, 1f)) },
            margin = new RectOffset(),
            fixedHeight = 18
        };
        _materialSkin.horizontalSliderThumb = new GUIStyle()
        {
            normal = { background = CreateRoundedTexture(MaterialAccent) },
            hover = { background = CreateRoundedTexture(MaterialAccentEnd) },
            active = { background = CreateRoundedTexture(Color.white) },
            focused = { background = CreateRoundedTexture(MaterialAccentEnd) },
            margin = new RectOffset(),
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            fixedWidth = 18,
            fixedHeight = 18
        };
        _materialSkin.horizontalScrollbar = new GUIStyle()
        {
            normal = { background = CreateRoundedTexture(new Color(0.10f, 0.14f, 0.19f, 1f)) },
            hover = { background = CreateRoundedTexture(new Color(0.14f, 0.19f, 0.25f, 1f)) },
            active = { background = CreateRoundedTexture(new Color(0.18f, 0.24f, 0.31f, 1f)) },
            focused = { background = CreateRoundedTexture(new Color(0.18f, 0.24f, 0.31f, 1f)) },
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            fixedHeight = 10,
            stretchHeight = false
        };
        _materialSkin.horizontalScrollbarThumb = new GUIStyle()
        {
            normal = { background = CreateRoundedTexture(MaterialAccent) },
            hover = { background = CreateRoundedTexture(MaterialAccentEnd) },
            active = { background = CreateRoundedTexture(Color.white) },
            focused = { background = CreateRoundedTexture(MaterialAccentEnd) },
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            fixedWidth = 10,
            fixedHeight = 10,
            stretchHeight = false
        };
        _materialSkin.verticalScrollbar = new GUIStyle()
        {
            normal = { background = CreateRoundedTexture(new Color(0.10f, 0.14f, 0.19f, 1f)) },
            hover = { background = CreateRoundedTexture(new Color(0.14f, 0.19f, 0.25f, 1f)) },
            active = { background = CreateRoundedTexture(new Color(0.18f, 0.24f, 0.31f, 1f)) },
            focused = { background = CreateRoundedTexture(new Color(0.18f, 0.24f, 0.31f, 1f)) },
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            fixedWidth = 10,
            stretchWidth = false
        };
        _materialSkin.verticalScrollbarThumb = new GUIStyle()
        {
            normal = { background = CreateRoundedTexture(MaterialAccent) },
            hover = { background = CreateRoundedTexture(MaterialAccentEnd) },
            active = { background = CreateRoundedTexture(Color.white) },
            focused = { background = CreateRoundedTexture(MaterialAccentEnd) },
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            fixedWidth = 10,
            fixedHeight = 10,
            stretchWidth = false
        };

        ApplyMaterialState(_materialSkin.horizontalSlider, _materialSkin.horizontalSlider.normal.background,
            _materialSkin.horizontalSlider.hover.background, _materialSkin.horizontalSlider.active.background);
        ApplyMaterialState(_materialSkin.horizontalSliderThumb, _materialSkin.horizontalSliderThumb.normal.background,
            _materialSkin.horizontalSliderThumb.hover.background, _materialSkin.horizontalSliderThumb.active.background);
        ApplyMaterialState(_materialSkin.horizontalScrollbar, _materialSkin.horizontalScrollbar.normal.background,
            _materialSkin.horizontalScrollbar.hover.background, _materialSkin.horizontalScrollbar.active.background);
        ApplyMaterialState(_materialSkin.horizontalScrollbarThumb, _materialSkin.horizontalScrollbarThumb.normal.background,
            _materialSkin.horizontalScrollbarThumb.hover.background, _materialSkin.horizontalScrollbarThumb.active.background);
        ApplyMaterialState(_materialSkin.verticalScrollbar, _materialSkin.verticalScrollbar.normal.background,
            _materialSkin.verticalScrollbar.hover.background, _materialSkin.verticalScrollbar.active.background);
        ApplyMaterialState(_materialSkin.verticalScrollbarThumb, _materialSkin.verticalScrollbarThumb.normal.background,
            _materialSkin.verticalScrollbarThumb.hover.background, _materialSkin.verticalScrollbarThumb.active.background);
        _materialSkin.horizontalScrollbarLeftButton = CreateHiddenScrollbarButtonStyle();
        _materialSkin.horizontalScrollbarRightButton = CreateHiddenScrollbarButtonStyle();
        _materialSkin.verticalScrollbarUpButton = CreateHiddenScrollbarButtonStyle();
        _materialSkin.verticalScrollbarDownButton = CreateHiddenScrollbarButtonStyle();

        return _materialSkin;
    }

    private static void ApplyMaterialState(GUIStyle style, Texture2D normal, Texture2D hover, Texture2D active)
    {
        style.onNormal.background = normal;
        style.onHover.background = hover;
        style.onActive.background = active;
        style.onFocused.background = hover;
        style.focused.background = hover;
    }

    private static GUIStyle CreateHiddenScrollbarButtonStyle()
    {
        Texture2D transparent = CreateTexture(new Color(0f, 0f, 0f, 0f));
        GUIStyle style = new GUIStyle
        {
            normal = { background = transparent },
            hover = { background = transparent },
            active = { background = transparent },
            focused = { background = transparent },
            onNormal = { background = transparent },
            onHover = { background = transparent },
            onActive = { background = transparent },
            onFocused = { background = transparent },
            fixedWidth = 0f,
            fixedHeight = 0f,
            stretchWidth = false,
            stretchHeight = false,
            margin = new RectOffset(),
            padding = new RectOffset()
        };
        return style;
    }
}
