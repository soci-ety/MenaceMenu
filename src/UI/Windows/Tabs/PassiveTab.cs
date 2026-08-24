using UnityEngine;

namespace MalumMenu;

public class PassiveTab : ITab
{
    public string name => "Passive";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.antiOverload = UIHelpers.Toggle(CheatToggles.antiOverload, " Anti-Overload");

        CheatToggles.freeCosmetics = UIHelpers.Toggle(CheatToggles.freeCosmetics, " Free Cosmetics");

        CheatToggles.avoidPenalties = UIHelpers.Toggle(CheatToggles.avoidPenalties, " Avoid Penalties");

        CheatToggles.unlockFeatures = UIHelpers.Toggle(CheatToggles.unlockFeatures, " Unlock Extra Features");

        CheatToggles.copyLobbyCodeOnDisconnect = UIHelpers.Toggle(CheatToggles.copyLobbyCodeOnDisconnect, " Copy Lobby Code on Disconnect");

        CheatToggles.spoofAprilFoolsDate = UIHelpers.Toggle(CheatToggles.spoofAprilFoolsDate, " Spoof Date to April 1st");

        CheatToggles.randomizeCosmetics = UIHelpers.Toggle(CheatToggles.randomizeCosmetics, " Randomize on Lobby Join");

        if (GUILayout.Button(" Randomize Now", GUILayout.Width(200)))
        {
            MalumRandomizer.Randomize();
        }
    }
}
