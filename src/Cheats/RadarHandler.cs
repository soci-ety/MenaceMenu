using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MalumMenu;

public sealed class RadarHandler : MonoBehaviour
{
    private const int RadarWindowId = 843207;
    private static readonly Dictionary<int, Texture2D> Textures = new();
    private static readonly Dictionary<int, Rect> MapData = new()
    {
        [0] = new Rect(277f, 77f, 11.5f, 0f),
        [1] = new Rect(115f, 240f, 9.25f, 0f),
        [2] = new Rect(8f, 21f, 10f, 0f),
        [3] = new Rect(277f, 77f, 11.5f, 0f),
        [4] = new Rect(162f, 107f, 6f, 0f),
        [5] = new Rect(237f, 140f, 8.5f, 0f)
    };

    private static Rect _radarRect = new(15f, 90f, 220f, 180f);
    private static GUIStyle _mapStyle;
    private static GUIStyle _glyphStyle;
    private static float _nextTeleportAt;

    public static float RadarScale { get; set; } = 1f;
    public static float RadarAlpha { get; set; } = 0.78f;

    private void OnGUI()
    {
        if (!CheatToggles.showRadar || !CanDraw()) return;

        int mapId = Mathf.Clamp(Utils.GetCurrentMapID(), 0, 5);
        Texture2D texture = LoadMap(mapId);
        if (texture == null) return;

        RadarScale = Mathf.Clamp(RadarScale, 0.65f, 1.6f);
        RadarAlpha = Mathf.Clamp(RadarAlpha, 0.2f, 1f);
        _radarRect.width = Mathf.Max(120f, texture.width * 0.5f * RadarScale + 10f);
        _radarRect.height = Mathf.Max(90f, texture.height * 0.5f * RadarScale + 10f);
        ClampWindow();

        InitStyles(texture);
        Color oldColor = GUI.color;
        try
        {
            GUI.color = Color.white;
            _radarRect = GUI.Window(RadarWindowId, _radarRect, (GUI.WindowFunction)DrawWindow, new GUIContent("Radar"), GUI.skin.window);
            ClampWindow();
        }
        catch { }
        finally
        {
            GUI.color = oldColor;
        }
    }

    private static bool CanDraw()
    {
        if (CheatToggles.radarHideInMeeting && (MeetingHud.Instance != null || ExileController.Instance != null || IntroCutscene.Instance != null))
            return false;

        return PlayerControl.LocalPlayer != null && PlayerControl.AllPlayerControls != null &&
               AmongUsClient.Instance != null && (AmongUsClient.Instance.IsGameStarted || Utils.isPlayer);
    }

    private static Texture2D LoadMap(int mapId)
    {
        int textureKey = mapId + (CheatToggles.radarRealistic ? 100 : 0);
        if (Textures.TryGetValue(textureKey, out Texture2D cached)) return cached;

        string prefix = CheatToggles.radarRealistic ? "radar_realistic_" : "radar_";
        string fileName = mapId switch
        {
            1 => prefix + "mira_hq.png",
            2 => prefix + "polus.png",
            4 => prefix + "airship.png",
            5 => prefix + "fungle.png",
            _ => prefix + "skeld.png"
        };

        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = Array.Find(assembly.GetManifestResourceNames(), name => name.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase));
            if (resourceName == null) return null;

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            using MemoryStream buffer = new();
            stream.CopyTo(buffer);

            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, buffer.ToArray(), false)) return null;
            texture.hideFlags = HideFlags.HideAndDontSave;
            Textures[textureKey] = texture;
            return texture;
        }
        catch
        {
            return null;
        }
    }

    public static Texture2D LoadMapPreview(int mapId)
    {
        return LoadMap(Mathf.Clamp(mapId, 0, 5));
    }

    private static void InitStyles(Texture2D texture)
    {
        _mapStyle ??= new GUIStyle(GUIStyle.none);
        _mapStyle.normal.background = texture;
        _glyphStyle ??= new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            richText = false
        };
    }

    private static void DrawWindow(int windowId)
    {
        int mapId = Mathf.Clamp(Utils.GetCurrentMapID(), 0, 5);
        Texture2D texture = LoadMap(mapId);
        if (texture == null) return;

        Rect map = MapData[mapId];
        float pad = 5f;
        Rect image = new(pad, pad, texture.width * 0.5f * RadarScale, texture.height * 0.5f * RadarScale);
        Color oldColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, RadarAlpha);
        GUI.Box(image, GUIContent.none, _mapStyle);
        GUI.color = oldColor;

        DrawPlayers(map, pad);
        if (CheatToggles.radarDeadBodies) DrawBodies(map, pad);
        if (CheatToggles.radarRightClickTeleport) HandleTeleport(map, pad);

        GUI.DragWindow(new Rect(0f, 0f, _radarRect.width, 24f));
    }

    private static void DrawPlayers(Rect map, float pad)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Disconnected) continue;
            if (player.Data.IsDead && !CheatToggles.radarGhosts) continue;

            Vector2 point = RadarPoint(map, player.GetTruePosition(), pad);
            if (!Inside(point)) continue;
            DrawGlyph(point, GetPlayerColor(player), IsImpostor(player) ? "■" : "●");
        }
    }

    private static void DrawBodies(Rect map, float pad)
    {
        foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body == null) continue;
            Vector2 point = RadarPoint(map, body.TruePosition, pad);
            if (Inside(point)) DrawGlyph(point, new Color(1f, 0.25f, 0.25f), "X");
        }
    }

    private static Vector2 RadarPoint(Rect map, Vector2 position, float pad)
    {
        return new Vector2((map.x + position.x * map.width) * RadarScale + pad,
            (map.y - position.y * map.width) * RadarScale + pad);
    }

    private static bool Inside(Vector2 point)
    {
        return point.x >= 2f && point.y >= 2f && point.x <= _radarRect.width - 2f && point.y <= _radarRect.height - 2f;
    }

    private static void DrawGlyph(Vector2 point, Color color, string glyph)
    {
        float size = 20f * RadarScale;
        _glyphStyle.fontSize = Mathf.Max(10, Mathf.RoundToInt(18f * RadarScale));
        Rect rect = new(point.x - size * 0.5f, point.y - size * 0.5f, size, size);
        GUI.color = new Color(0f, 0f, 0f, 0.9f);
        GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), glyph, _glyphStyle);
        GUI.color = color;
        GUI.Label(rect, glyph, _glyphStyle);
        GUI.color = Color.white;
    }

    private static void HandleTeleport(Rect map, float pad)
    {
        Event current = Event.current;
        if (current == null || current.button != 1 || current.type != EventType.MouseDown || current.shift || current.control || current.alt || Time.unscaledTime < _nextTeleportAt)
            return;

        _nextTeleportAt = Time.unscaledTime + 0.1f;
        Vector2 point = current.mousePosition;
        Vector2 target = new(((point.x - pad) / RadarScale - map.x) / map.width,
            (((point.y - pad) / RadarScale - map.y) * -1f) / map.width);
        try { PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(target); } catch { }
        current.Use();
    }

    private static Color GetPlayerColor(PlayerControl player)
    {
        int colorId = player.Data.DefaultOutfit.ColorId;
        return Palette.PlayerColors != null && colorId >= 0 && colorId < Palette.PlayerColors.Length ? Palette.PlayerColors[colorId] : Color.white;
    }

    private static bool IsImpostor(PlayerControl player)
    {
        return player.Data.Role != null ? player.Data.Role.IsImpostor : RoleManager.IsImpostorRole(player.Data.RoleType);
    }

    private static void ClampWindow()
    {
        _radarRect.x = Mathf.Clamp(_radarRect.x, 0f, Mathf.Max(0f, Screen.width - _radarRect.width));
        _radarRect.y = Mathf.Clamp(_radarRect.y, 0f, Mathf.Max(0f, Screen.height - _radarRect.height));
    }
}
