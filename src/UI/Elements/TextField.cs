using UnityEngine;

namespace MalumMenu;

public class TextField
{
    private string _content = "";
    private bool _focused = false;
    private float _lastBlinkTime = 0f;
    private bool _cursorVisible = true;
    private Rect _fieldRect = Rect.zero;
    private float _cursorBlinkTime = 0.5f;

    public bool IsFocused => _focused;
    public string Content
    {
        get => _content;
        set => _content = value;
    }

    public TextField(string initialContent = "")
    {
        _content = initialContent;
    }

    public void Draw(int width = 200, int height = 20)
    {
        if (MenuUI.IsMaterialLayoutActive)
        {
            Rect materialRect = GUILayoutUtility.GetRect(width, Mathf.Max(28, height), GUILayout.ExpandWidth(false));
            Color previousColor = GUI.color;
            GUI.color = new Color(0.025f, 0.032f, 0.042f, 1f);
            GUI.Box(materialRect, GUIContent.none, MaterialFieldStyle());
            GUI.color = previousColor;
            if (Event.current.type == EventType.MouseDown && materialRect.Contains(Event.current.mousePosition))
            {
                _focused = true;
                _lastBlinkTime = Time.time;
                _cursorVisible = true;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseDown)
            {
                _focused = false;
            }

            if (_focused && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Backspace && _content.Length > 0)
                    _content = _content.Substring(0, _content.Length - 1);
                else if (Event.current.character != '\0' && !char.IsControl(Event.current.character))
                    _content += Event.current.character;
                else
                    return;
                Event.current.Use();
            }

            GUI.Label(new Rect(materialRect.x + 8f, materialRect.y + 3f,
                materialRect.width - 16f, materialRect.height - 6f), _content, GUI.skin.label);
            if (_focused && Time.time - _lastBlinkTime > _cursorBlinkTime)
            {
                _cursorVisible = !_cursorVisible;
                _lastBlinkTime = Time.time;
            }
            if (_focused && _cursorVisible)
            {
                float cursorX = materialRect.x + 8f + GUI.skin.label.CalcSize(new GUIContent(_content)).x;
                GUI.Label(new Rect(cursorX, materialRect.y + 3f, 10f, materialRect.height - 6f), "|", GUI.skin.label);
            }
            return;
        }

        GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(height));

        if (Event.current.type == EventType.Repaint)
        {
            _fieldRect = GUILayoutUtility.GetLastRect();
        }

        // Handle mouse click to focus
        if (Event.current.type == EventType.MouseDown)
        {
            if (_fieldRect.Contains(Event.current.mousePosition))
            {
                _focused = true;
                _lastBlinkTime = Time.time;
                _cursorVisible = true;
                Event.current.Use();
            }
            else
            {
                _focused = false;
            }
        }

        // Handle keyboard input
        if (_focused && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Backspace)
            {
                if (_content.Length > 0)
                {
                    _content = _content.Substring(0, _content.Length - 1);
                    Event.current.Use();
                }
            }
            else if (Event.current.character != '\0' && !char.IsControl(Event.current.character))
            {
                _content += Event.current.character;
                Event.current.Use();
            }
        }

        // Display text content
        GUI.Label(new Rect(_fieldRect.x + 5, _fieldRect.y + 2, _fieldRect.width - 10, _fieldRect.height), _content);

        // Handle cursor blinking
        if (_focused)
        {
            if (Time.time - _lastBlinkTime > _cursorBlinkTime)
            {
                _cursorVisible = !_cursorVisible;
                _lastBlinkTime = Time.time;
            }

            // Draw blinking cursor
            if (_cursorVisible)
            {
                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(_content));
                GUI.Label(new Rect(_fieldRect.x + textSize.x + 7, _fieldRect.y + 2, 10, _fieldRect.height - 4), "|");
            }
        }
    }

    private static GUIStyle MaterialFieldStyle()
    {
        return new GUIStyle
        {
            normal = { background = Texture2D.whiteTexture },
            padding = new RectOffset { left = 8, right = 8, top = 3, bottom = 3 },
            margin = new RectOffset(),
            border = new RectOffset { left = 1, right = 1, top = 1, bottom = 1 }
        };
    }

    public void Unfocus()
    {
        _focused = false;
    }
}
