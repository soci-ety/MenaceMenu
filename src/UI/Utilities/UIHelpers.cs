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
    private static readonly Dictionary<string, float> ToggleProgress = new();

    public static void ResetMaterialStyles()
    {
        _materialSwitchButton = null;
        _materialSwitchTrack = null;
        _materialSwitchThumb = null;
        _materialSliderRail = null;
        _materialSliderThumb = null;
        ToggleProgress.Clear();
    }

    public static float HorizontalSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
    {
        float result = GUILayout.HorizontalSlider(value, leftValue, rightValue, options);
        if (MenuUI.IsMaterialLayoutActive && Event.current.type == EventType.Repaint)
            DrawMaterialSliderOverlay(GUILayoutUtility.GetLastRect(), result, leftValue, rightValue);
        return result;
    }

    private static void DrawMaterialSliderOverlay(Rect rect, float value, float leftValue, float rightValue)
    {
        if (rect.width <= 0f || rect.height <= 0f) return;
        _materialSliderRail ??= CreateSolidTexture(Color.white);
        _materialSliderThumb ??= CreateRoundedTexture();

        Color previousColor = GUI.color;
        GUI.color = new Color(0.18f, 0.25f, 0.34f, previousColor.a);
        GUI.DrawTexture(new Rect(rect.x, rect.center.y - 4f, rect.width, 8f), _materialSliderRail, ScaleMode.StretchToFill);

        float normalized = Mathf.InverseLerp(leftValue, rightValue, value);
        float thumbX = Mathf.Lerp(rect.x + 9f, rect.xMax - 9f, normalized);
        GUI.color = new Color(MenuUI.GetMaterialAccentColor().r, MenuUI.GetMaterialAccentColor().g,
            MenuUI.GetMaterialAccentColor().b, previousColor.a);
        GUI.DrawTexture(new Rect(thumbX - 9f, rect.center.y - 9f, 18f, 18f), _materialSliderThumb, ScaleMode.StretchToFill);
        GUI.color = previousColor;
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
        Rect switchRect = GUILayoutUtility.GetRect(48f, 22f, GUILayout.Width(48f), GUILayout.Height(22f));
        bool nextValue = GUI.Button(switchRect, GUIContent.none, _materialSwitchButton) ? !value : value;
        string toggleKey = label.Trim();
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
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
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
