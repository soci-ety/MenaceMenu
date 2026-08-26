using UnityEngine;

namespace MalumMenu;

public class TestTab : ITab
{
    public string name => "Test";
    private string _status = string.Empty;

    public void Draw()
    {
        GUILayout.Label("UI Test", GUI.skin.label);
        GUILayout.Label("Use this to verify that notifications are using the active UI style.", GUI.skin.label);
        if (GUILayout.Button("Send Test Notification", GUILayout.Height(32)))
        {
            if (MalumMenu.notifications != null)
            {
                MalumMenu.notifications.Send("Test Notification", "The notification system is working.", 5f);
                _status = "Test notification sent.";
            }
            else
                _status = "Notification manager is not ready.";
        }

        if (!string.IsNullOrEmpty(_status))
            GUILayout.Label(_status, GUI.skin.label);
    }
}