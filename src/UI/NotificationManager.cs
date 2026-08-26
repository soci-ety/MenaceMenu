using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu.ui
{
    public class NotificationManager : MonoBehaviour
    {
        public List<Notification> notifications = new List<Notification>();
        public bool DisableNotifications = false;

        public static Vector2 BoxSize
        {
            get { return new Vector2(325, 90); }
        }

        public static Vector2 BoxHeaderSize
        {
            get { return new Vector2(BoxSize.x, 17); }
        }

        public static Vector2 BoxContentPadding
        {
            get { return new Vector2(10, 0); }
        }

        public static Vector2 BoxContentSize
        {
            get { return new Vector2(BoxSize.x - BoxContentPadding.x, BoxSize.y - BoxHeaderSize.y - BoxSliderSize.y); }
        }

        public static Vector2 BoxSliderSize
        {
            get { return new Vector2(BoxSize.x, 20); }
        }

        public void Update()
        {
            for (int i = notifications.Count - 1; i >= 0; i--)
            {
                Notification notification = notifications[i];
                notification.lifetime += Time.deltaTime;

                if (notification.HasExpired)
                    notifications.RemoveAt(i);
            }
        }

        public void OnGUI()
        {
            if (DisableNotifications) return;

            GUISkin previousSkin = GUI.skin;
            Color previousColor = GUI.color;
            Color previousContentColor = GUI.contentColor;
            Color previousBackgroundColor = GUI.backgroundColor;
            int previousDepth = GUI.depth;
            Matrix4x4 previousMatrix = GUI.matrix;
            bool material = MalumMenu.menuMaterialLayout?.Value == true;
            try
            {
                GUI.skin = material ? previousSkin : MenuUI.GetWindowSkin(previousSkin);
                GUI.depth = -10000;
                GUI.matrix = Matrix4x4.identity;
                GUI.color = Color.white;
                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.white;
                if (material)
                    GUI.BeginGroup(new Rect(0f, 0f, Screen.width, Screen.height));

                int visibleNotifications = Math.Min(GetMaxNotifications(), notifications.Count);
                for (int i = 0; i < visibleNotifications; i++)
                    RenderNotification(i, notifications[i], material);

                if (material)
                    GUI.EndGroup();
            }
            finally
            {
                GUI.skin = previousSkin;
                GUI.color = previousColor;
                GUI.contentColor = previousContentColor;
                GUI.backgroundColor = previousBackgroundColor;
                GUI.depth = previousDepth;
                GUI.matrix = previousMatrix;
            }
        }

        private void RenderNotification(int position, Notification notification, bool material)
        {
            float boxX = Mathf.Max(8f, Screen.width - BoxSize.x - 12f);
            float boxY = Mathf.Max(8f, Screen.height - (int)(BoxSize.y * (position + 1)) - 12f);
            if (material)
            {
                RenderMaterialNotification(boxX, boxY, notification);
                return;
            }

            GUI.Box(new Rect(boxX, boxY, BoxSize.x, BoxSize.y), notification.title);

            GUI.Label(new Rect(boxX + BoxContentPadding.x, boxY + BoxHeaderSize.y, BoxContentSize.x, BoxContentSize.y),
                notification.message);

            GUI.HorizontalSlider(new Rect(boxX, boxY + BoxHeaderSize.y + BoxContentSize.y, BoxSize.x, BoxSize.y),
                notification.ttl - notification.lifetime, 0, notification.ttl);
        }

        private static void RenderMaterialNotification(float boxX, float boxY, Notification notification)
        {
            const float contentInset = 12f;
            float contentWidth = BoxSize.x - contentInset * 2f;
            float enterProgress = Mathf.Clamp01(notification.lifetime / 0.28f);
            float exitProgress  = notification.ttl - notification.lifetime < 0.45f
                ? Mathf.Clamp01((notification.ttl - notification.lifetime) / 0.45f)
                : 1f;
            float easedEnter = 1f - Mathf.Pow(1f - enterProgress, 3f);
            float animatedX = Mathf.Lerp(Screen.width + 18f, boxX, easedEnter);
            float alpha = easedEnter * exitProgress;
            float pulse = 0.82f + Mathf.Sin(Time.unscaledTime * 5f) * 0.18f;
            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Box(new Rect(animatedX, boxY, BoxSize.x, BoxSize.y), GUIContent.none, MaterialCardStyle());

            Color accent = MenuUI.GetMaterialAccentColor();
            accent.a = alpha * pulse;
            GUI.color = accent;
            GUI.Box(new Rect(animatedX, boxY, BoxSize.x, 4f), GUIContent.none, SolidBoxStyle());

            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(animatedX + contentInset, boxY + 9f, contentWidth, 22f), notification.title,
                MaterialNotificationTitleStyle());
            GUI.Label(new Rect(animatedX + contentInset, boxY + 32f, contentWidth, 38f), notification.message,
                MaterialNotificationMessageStyle());

            float progress = Mathf.Clamp01((notification.ttl - notification.lifetime) / Mathf.Max(0.01f, notification.ttl));
            GUI.color = new Color(0.18f, 0.22f, 0.27f, alpha);
            GUI.Box(new Rect(animatedX + contentInset, boxY + 76f, contentWidth, 4f), GUIContent.none, SolidBoxStyle());
            accent.a = alpha;
            GUI.color = accent;
            GUI.Box(new Rect(animatedX + contentInset, boxY + 76f, contentWidth * progress, 4f), GUIContent.none, SolidBoxStyle());
            GUI.color = previousColor;
        }

        private static GUIStyle MaterialCardStyle()
        {
            return new GUIStyle
            {
                normal = { background = CreateRoundedTexture(new Color(0.075f, 0.09f, 0.11f, 0.98f)) },
                padding = new RectOffset(),
                margin = new RectOffset(),
                border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 }
            };
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
                    pixel.a *= Mathf.Clamp01(radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                    texture.SetPixel(x, y, pixel);
                }
            }
            texture.Apply();
            return texture;
        }

        private static GUIStyle SolidBoxStyle()
        {
            return new GUIStyle
            {
                normal = { background = Texture2D.whiteTexture },
                padding = new RectOffset(),
                margin = new RectOffset(),
                border = new RectOffset()
            };
        }

        private static GUIStyle MaterialNotificationTitleStyle()
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                padding = new RectOffset(),
                margin = new RectOffset()
            };
        }

        private static GUIStyle MaterialNotificationMessageStyle()
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.84f, 0.87f, 0.91f, 1f) },
                padding = new RectOffset(),
                margin = new RectOffset()
            };
        }

        public int GetMaxNotifications()
        {
            return Math.Max(1, (Screen.height / 2) / (int)BoxSize.y);
        }

        // The time to live value for a notification should be five seconds if it is a success message, and ten seconds if it is a failure message
        public void Send(string title, string message, float ttl = 10)
        {
            MalumMenu.Log.LogMessage($"[Notification] [{title}] {message}");

            if (DisableNotifications) return;

            Notification notification = new Notification(title, message, ttl);
            notifications.Add(notification);
        }

        public void ClearNotifications()
        {
            notifications.Clear();
        }
    }
}