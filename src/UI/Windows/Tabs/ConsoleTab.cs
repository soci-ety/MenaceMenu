using UnityEngine;

namespace MalumMenu;

public class ConsoleTab : ITab
{
    public string name => "Console";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.showConsole = UIHelpers.Toggle(CheatToggles.showConsole, " Show Console");

        CheatToggles.logDeaths = UIHelpers.Toggle(CheatToggles.logDeaths, " Log Deaths");

        CheatToggles.logShapeshifts = UIHelpers.Toggle(CheatToggles.logShapeshifts, " Log Shapeshifts");

        CheatToggles.logVents = UIHelpers.Toggle(CheatToggles.logVents, " Log Vents");
    }
}
