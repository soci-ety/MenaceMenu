using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

public static class UIHelpers
{
    private static readonly Color DefaultDarkColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    private static readonly Color ModernAccentColor = new Color(0.2f, 0.4f, 0.8f, 1f);
    private static GUIStyle _materialSwitchButton;
    private static GUIStyle _materialSwitchTrack;
    private static GUIStyle _materialSwitchThumb;
    private static Texture2D _materialSliderRail;
    private static Texture2D _materialSliderThumb;
    private static GUIStyle _materialSliderRailStyle;
    private static GUIStyle _materialSliderThumbStyle;
    private static GUIStyle _materialHorizontalScrollbar;
    private static GUIStyle _materialVerticalScrollbar;
    private static GUIStyle _materialHorizontalScrollbarThumb;
    private static GUIStyle _materialVerticalScrollbarThumb;
    private static readonly Dictionary<string, float> ToggleProgress = new();

    public static void ResetMaterialStyles()
    {
        _materialSwitchButton = null;
        _materialSwitchTrack = null;
        _materialSwitchThumb = null;
        _materialSliderRail = null;
        _materialSliderThumb = null;
        _materialSliderRailStyle = null;
        _materialSliderThumbStyle = null;
        _materialHorizontalScrollbar = null;
        _materialVerticalScrollbar = null;
        _materialHorizontalScrollbarThumb = null;
        _materialVerticalScrollbarThumb = null;
        ToggleProgress.Clear();
    }

    public static float HorizontalSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
    {
        if (!MenuUI.IsMaterialLayoutActive)
            return GUILayout.HorizontalSlider(value, leftValue, rightValue, options);

        Rect rect = GUILayoutUtility.GetRect(250f, 26f, options);
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        Event current = Event.current;
        Rect track = new(rect.x + 9f, rect.center.y - 4f, Mathf.Max(0f, rect.width - 18f), 8f);

        if ((current.type == EventType.MouseDown || current.type == EventType.MouseDrag) &&
            (current.type == EventType.MouseDrag || track.Contains(current.mousePosition)))
        {
            if (current.type == EventType.MouseDown)
                GUIUtility.hotControl = controlId;

            if (GUIUtility.hotControl == controlId)
            {
                value = Mathf.Lerp(leftValue, rightValue,
                    Mathf.Clamp01((current.mousePosition.x - track.x) / Mathf.Max(1f, track.width)));
                current.Use();
            }
        }
        else if (current.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
        {
            GUIUtility.hotControl = 0;
            current.Use();
        }

        if (current.type == EventType.Repaint)
            DrawMaterialSliderOverlay(rect, value, leftValue, rightValue);

        return value;
    }

    public static Vector2 BeginScrollView(Vector2 scrollPosition, params GUILayoutOption[] options)
    {
        if (!MenuUI.IsMaterialLayoutActive)
            return GUILayout.BeginScrollView(scrollPosition, options);

        InitializeMaterialScrollStyles();
        ApplyMaterialScrollStylesToSkin();
        return GUILayout.BeginScrollView(scrollPosition, false, true, _materialHorizontalScrollbar,
            _materialVerticalScrollbar, options);
    }

    public static Vector2 BeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal,
        bool alwaysShowVertical, params GUILayoutOption[] options)
    {
        if (!MenuUI.IsMaterialLayoutActive)
            return GUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical, options);

        InitializeMaterialScrollStyles();
        ApplyMaterialScrollStylesToSkin();
        return GUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical,
            _materialHorizontalScrollbar, _materialVerticalScrollbar, options);
    }

    private static void ApplyMaterialScrollStylesToSkin()
    {
        GUISkin skin = MenuUI.MaterialSkin ?? GUI.skin;
        skin.horizontalScrollbar = _materialHorizontalScrollbar;
        skin.verticalScrollbar = _materialVerticalScrollbar;
        skin.horizontalScrollbarThumb = _materialHorizontalScrollbarThumb;
        skin.verticalScrollbarThumb = _materialVerticalScrollbarThumb;
        GUI.skin = skin;
    }

    private static void InitializeMaterialScrollStyles()
    {
        if (_materialHorizontalScrollbar != null) return;

        Texture2D track = CreateSolidTexture(new Color(0.07f, 0.09f, 0.12f, 1f));
        Texture2D thumb = CreateRoundedTexture(MenuUI.GetMaterialAccentColor());
        _materialHorizontalScrollbar = CreateScrollbarStyle(track, 10f, 0f);
        _materialVerticalScrollbar = CreateScrollbarStyle(track, 0f, 10f);
        _materialHorizontalScrollbarThumb = CreateScrollbarStyle(thumb, 10f, 0f);
        _materialVerticalScrollbarThumb = CreateScrollbarStyle(thumb, 0f, 10f);
        _materialHorizontalScrollbar.normal.background = track;
        _materialVerticalScrollbar.normal.background = track;
        _materialHorizontalScrollbarThumb.normal.background = thumb;
        _materialVerticalScrollbarThumb.normal.background = thumb;
    }

    private static GUIStyle CreateScrollbarStyle(Texture2D texture, float fixedHeight, float fixedWidth)
    {
        GUIStyle style = new()
        {
            normal = { background = texture },
            hover = { background = texture },
            active = { background = texture },
            focused = { background = texture },
            onNormal = { background = texture },
            onHover = { background = texture },
            onActive = { background = texture },
            onFocused = { background = texture },
            fixedHeight = fixedHeight,
            fixedWidth = fixedWidth,
            stretchHeight = fixedHeight <= 0f,
            stretchWidth = fixedWidth <= 0f,
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
        };
        return style;
    }

    private static void DrawMaterialSliderOverlay(Rect rect, float value, float leftValue, float rightValue)
    {
        if (rect.width <= 0f || rect.height <= 0f) return;
        _materialSliderRail ??= CreateSolidTexture(new Color(0.18f, 0.25f, 0.34f, 1f));
        _materialSliderThumb ??= CreateRoundedTexture(MenuUI.GetMaterialAccentColor());
        _materialSliderRailStyle ??= CreateTextureStyle(_materialSliderRail);
        _materialSliderThumbStyle ??= CreateTextureStyle(_materialSliderThumb);

        Color previousColor = GUI.color;
        GUI.color = Color.white;
        GUI.Box(new Rect(rect.x, rect.center.y - 4f, rect.width, 8f), GUIContent.none, _materialSliderRailStyle);

        float normalized = Mathf.InverseLerp(leftValue, rightValue, value);
        float thumbX = Mathf.Lerp(rect.x + 9f, rect.xMax - 9f, normalized);
        GUI.Box(new Rect(thumbX - 9f, rect.center.y - 9f, 18f, 18f), GUIContent.none, _materialSliderThumbStyle);
        GUI.color = previousColor;
    }

    private static GUIStyle CreateTextureStyle(Texture2D texture)
    {
        return new GUIStyle
        {
            normal = { background = texture },
            hover = { background = texture },
            active = { background = texture },
            focused = { background = texture },
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            padding = new RectOffset(),
            margin = new RectOffset()
        };
    }

    public static void ApplyUIColor()
    {
        if (CheatToggles.rgbMode)
        {
            // Use HSV color for RGB mode with better saturation and value
            Color hsvColor = Color.HSVToRGB(MenuUI.hue, 0.8f, 0.95f);
            GUI.backgroundColor = hsvColor;
        }
        else
        {
            var configHtmlColor = MalumMenu.menuHtmlColor.Value;

            // Try to parse the custom color
            if (!string.IsNullOrEmpty(configHtmlColor))
            {
                if (ColorUtility.TryParseHtmlString(configHtmlColor, out var uiColor))
                {
                    GUI.backgroundColor = uiColor;
                    return;
                }

                // Try with # prefix if not present
                if (!configHtmlColor.StartsWith("#"))
                {
                    if (ColorUtility.TryParseHtmlString("#" + configHtmlColor, out uiColor))
                    {
                        GUI.backgroundColor = uiColor;
                        return;
                    }
                }
            }

            // Fall back to modern default color
            GUI.backgroundColor = DefaultDarkColor;
        }
    }

    public static bool Toggle(bool value, string label)
    {
        if (!MenuUI.IsMaterialLayoutActive)
            return GUILayout.Toggle(value, label);

        InitializeMaterialSwitchStyles();
        GUILayout.BeginHorizontal(GUILayout.Height(28));
        int toggleId = GUIUtility.GetControlID(FocusType.Passive);
        Rect switchRect = GUILayoutUtility.GetRect(48f, 22f, GUILayout.Width(48f), GUILayout.Height(22f));
        bool nextValue = GUI.Button(switchRect, GUIContent.none, _materialSwitchButton) ? !value : value;
        string toggleKey = $"{label.Trim()}:{toggleId}";
        ToggleProgress.TryGetValue(toggleKey, out float progress);
        float targetProgress = nextValue ? 1f : 0f;
        progress = Mathf.MoveTowards(progress, targetProgress, Time.unscaledDeltaTime * 5f);
        ToggleProgress[toggleKey] = progress;
        if (!Mathf.Approximately(progress, targetProgress))
            GUI.changed = true;
        Color previousColor = GUI.color;
        float visualProgress = Mathf.SmoothStep(0f, 1f, progress);
        bool isHovered = switchRect.Contains(Event.current.mousePosition);
        Color offColor = new Color(0.18f, 0.21f, 0.25f, 1f);
        Color onColor = MenuUI.GetMaterialAccentColor();
        Color trackColor = Color.Lerp(offColor, onColor, visualProgress);
        if (isHovered)
            trackColor = Color.Lerp(trackColor, onColor, 0.65f);
        trackColor.a = previousColor.a;
        GUI.color = trackColor;
        GUI.Box(switchRect, GUIContent.none, _materialSwitchTrack);

        float thumbX = Mathf.Lerp(switchRect.x + 3f, switchRect.xMax - 21f, visualProgress);
        Color thumbColor = Color.Lerp(new Color(0.62f, 0.67f, 0.72f, 1f), new Color(0.98f, 0.94f, 0.84f, 1f), visualProgress);
        thumbColor.a = previousColor.a;
        GUI.color = thumbColor;
        GUI.Box(new Rect(thumbX, switchRect.y + 3f, 16f, 16f), GUIContent.none, _materialSwitchThumb);
        GUI.color = previousColor;
        GUILayout.Label(label.Trim(), GUI.skin.label);
        GUILayout.EndHorizontal();
        return nextValue;
    }

    private static void InitializeMaterialSwitchStyles()
    {
        if (_materialSwitchButton != null) return;

        _materialSwitchButton = new GUIStyle(GUI.skin.button)
        {
            normal = { background = CreateSolidTexture(new Color(0f, 0f, 0f, 0f)) },
            hover = { background = CreateSolidTexture(new Color(0f, 0f, 0f, 0f)) },
            active = { background = CreateSolidTexture(new Color(0f, 0f, 0f, 0f)) },
            padding = new RectOffset(),
            margin = new RectOffset(),
            border = new RectOffset()
        };
        _materialSwitchTrack = new GUIStyle(GUI.skin.box)
        {
            normal = { background = CreateRoundedTexture() },
            padding = new RectOffset(),
            margin = new RectOffset(),
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
        };
        _materialSwitchThumb = new GUIStyle(GUI.skin.box)
        {
            normal = { background = CreateRoundedTexture() },
            padding = new RectOffset(),
            margin = new RectOffset(),
            border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
        };
    }

    private static Texture2D CreateSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateRoundedTexture()
    {
        return CreateRoundedTexture(Color.white);
    }

    private static Texture2D CreateRoundedTexture(Color color)
    {
        const int size = 24;
        const float radius = 8f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, 0f);
                dx = Mathf.Max(dx, x - (size - radius - 1f));
                float dy = Mathf.Max(radius - y, 0f);
                dy = Mathf.Max(dy, y - (size - radius - 1f));
                float alpha = Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Gets a contrast color based on the current UI color for better readability
    /// </summary>
    public static Color GetContrastColor(Color baseColor)
    {
        float luminance = 0.299f * baseColor.r + 0.587f * baseColor.g + 0.114f * baseColor.b;
        return luminance > 0.5f ? Color.black : Color.white;
    }

    /// <summary>
    /// Creates a modern highlighted color for interactive elements
    /// </summary>
    public static Color GetHighlightColor(Color baseColor)
    {
        return new Color(baseColor.r + 0.1f, baseColor.g + 0.1f, baseColor.b + 0.1f, baseColor.a);
    }
}
