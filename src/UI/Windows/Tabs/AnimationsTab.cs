using UnityEngine;

namespace MalumMenu;

public class AnimationsTab : ITab
{
    public string name => "Animations";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawClientSided();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.animShields = UIHelpers.Toggle(CheatToggles.animShields, " Shields");

        CheatToggles.animAsteroids = UIHelpers.Toggle(CheatToggles.animAsteroids, " Asteroids");

        CheatToggles.animEmptyGarbage = UIHelpers.Toggle(CheatToggles.animEmptyGarbage, " Empty Garbage");

        CheatToggles.animMedScan = UIHelpers.Toggle(CheatToggles.animMedScan, " Medbay Scan");

        CheatToggles.animCamsInUse = UIHelpers.Toggle(CheatToggles.animCamsInUse, " Cams In Use");

        // CheatToggles.animPet = UIHelpers.Toggle(CheatToggles.animPet, " Pet");
    }

    private void DrawClientSided()
    {
        GUILayout.Label("Client-Sided", GUIStylePreset.TabSubtitle);

        CheatToggles.moonWalk = UIHelpers.Toggle(CheatToggles.moonWalk, " Moonwalk");
    }
}
