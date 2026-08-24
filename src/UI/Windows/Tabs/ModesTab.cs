using UnityEngine;

namespace MalumMenu;

public class ModesTab : ITab
{
    public string name => "Modes";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.rgbMode = UIHelpers.Toggle(CheatToggles.rgbMode, " RGB Mode");

        CheatToggles.stealthMode = UIHelpers.Toggle(CheatToggles.stealthMode, " Stealth Mode");

        if (MalumMenu.isDevRelease)
        {
            CheatToggles.streamerMode = UIHelpers.Toggle(CheatToggles.streamerMode, " Streamer Mode");
        }
        else
        {
            GUILayout.Label("Coming Soon: Streamer Mode");

        }
        
        CheatToggles.panicMode = UIHelpers.Toggle(CheatToggles.panicMode, " Panic Mode");
    }
}
