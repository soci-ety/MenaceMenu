using Il2CppSystem;
using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

public class ConsoleUI : MonoBehaviour
{
    public static int windowHeight = 380;
    public static int windowWidth = 600;
    public static Rect windowRect;

    private GUIStyle _logStyle;
    private bool _materialResizing;
    private Vector2 _resizeStart;
    private Vector2 _resizeOrigin;
    private static Vector2 _scrollPosition = Vector2.zero;
    private static List<string> _logEntries = new();
    private const int MaxLogEntries = 300;

    private void Start()
    {
        // Instantiate 2D area of ConsoleUI
        windowRect = new(
            Screen.width / 2f - windowWidth / 2f,
            Screen.height / 2f - windowHeight / 2f,
            windowWidth,
            windowHeight
        );
    }

    private void OnGUI()
    {
        if (!CheatToggles.showConsole || !(MenuUI.isGUIActive || MalumMenu.menuKeepSubwindowsOpen.Value) || MalumMenu.isPanicked) return;

        GUISkin previousSkin = GUI.skin;
        GUI.skin = MenuUI.GetWindowSkin(previousSkin);
        _logStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 15
        };

        UIHelpers.ApplyUIColor();

        windowRect = GUI.Window((int)WindowId.ConsoleUI, windowRect, (GUI.WindowFunction)ConsoleWindow, "Console");
        GUI.skin = previousSkin;
    }

    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginVertical(GUI.skin.box);

        _scrollPosition = UIHelpers.BeginScrollView(_scrollPosition, false, false);

        foreach (var log in _logEntries)
        {
            GUILayout.Label(log, _logStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Log", GUILayout.Width(285)))
        {
            _logEntries.Clear();
        }

        if (GUILayout.Button("Copy Log to Clipboard"))
        {
            GUIUtility.systemCopyBuffer = String.Join("\n", _logEntries.ToArray());
        }

        GUILayout.EndHorizontal();

        if (MalumMenu.menuMaterialLayout?.Value == true)
            HandleMaterialResize();
        GUI.DragWindow();
    }

    private void HandleMaterialResize()
    {
        const float gripSize = 22f;
        Rect grip = new(windowRect.width - gripSize, windowRect.height - gripSize, gripSize, gripSize);
        int controlId = GUIUtility.GetControlID(148238, FocusType.Passive);
        Event current = Event.current;

        if (current.type == EventType.MouseDown && current.button == 0 && grip.Contains(current.mousePosition))
        {
            _materialResizing = true;
            _resizeStart = current.mousePosition;
            _resizeOrigin = new Vector2(windowRect.width, windowRect.height);
            GUIUtility.hotControl = controlId;
            current.Use();
        }
        else if (_materialResizing && current.type == EventType.MouseDrag && GUIUtility.hotControl == controlId)
        {
            Vector2 delta = current.mousePosition - _resizeStart;
            windowRect.width = Mathf.Clamp(_resizeOrigin.x + delta.x, 360f, Screen.width - 20f);
            windowRect.height = Mathf.Clamp(_resizeOrigin.y + delta.y, 240f, Screen.height - 20f);
            GUI.changed = true;
            current.Use();
        }
        else if (_materialResizing && current.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
        {
            _materialResizing = false;
            GUIUtility.hotControl = 0;
            current.Use();
        }

        GUI.Box(new Rect(windowRect.width - 12f, windowRect.height - 12f, 8f, 8f), GUIContent.none);
    }

    public static void Log(string message)
    {
        if (_logEntries.Count >= MaxLogEntries) // Limit the number of logs to keep memory usage in check
        {
            _logEntries.RemoveAt(0); // Remove the oldest log entry
        }

        var currentTime = DateTime.Now.ToString("HH:mm:ss");

        _logEntries.Add($"<b>[ {currentTime} ]  {message}</b>");

        // Scroll to the bottom
        _scrollPosition.y = float.MaxValue;
    }
}