using UnityEngine;

namespace MalumMenu;

public class ShipTab : ITab
{
    public string name => "Ship";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawSabotage();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawVents();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        // Will implement this later, currently gets user kicked by AC. -ADHyperActive
        // CheatToggles.completeAllTasks = UIHelpers.Toggle(CheatToggles.completeAllTasks, " Allow All Tasks");
        
        CheatToggles.fakeTasks = UIHelpers.Toggle(CheatToggles.fakeTasks, " Fake Tasks");

        CheatToggles.doAnyTask = UIHelpers.Toggle(CheatToggles.doAnyTask, " Do Any Task");

        CheatToggles.unfixableLights = UIHelpers.Toggle(CheatToggles.unfixableLights, " Unfixable Lights");

        CheatToggles.callMeeting = UIHelpers.Toggle(CheatToggles.callMeeting, " Call Meeting");

        CheatToggles.reportBody = UIHelpers.Toggle(CheatToggles.reportBody, " Report Body");

        CheatToggles.closeMeeting = UIHelpers.Toggle(CheatToggles.closeMeeting, " Close Meeting");

        CheatToggles.autoReportBodies = UIHelpers.Toggle(CheatToggles.autoReportBodies, " Auto-Report Dead Bodies");

        CheatToggles.autoOpenDoorsOnUse = UIHelpers.Toggle(CheatToggles.autoOpenDoorsOnUse, " Auto-Open Doors On Use");

        CheatToggles.kickOffensiveNames = UIHelpers.Toggle(CheatToggles.kickOffensiveNames, " Kick Offensive Names");
    }

    private void DrawSabotage()
    {
        GUILayout.Label("Sabotage", GUIStylePreset.TabSubtitle);

        CheatToggles.reactorSab = UIHelpers.Toggle(CheatToggles.reactorSab, " Reactor");

        CheatToggles.oxygenSab = UIHelpers.Toggle(CheatToggles.oxygenSab, " Oxygen");

        CheatToggles.elecSab = UIHelpers.Toggle(CheatToggles.elecSab, " Lights");

        CheatToggles.commsSab = UIHelpers.Toggle(CheatToggles.commsSab, " Comms");

        CheatToggles.showDoorsMenu = UIHelpers.Toggle(CheatToggles.showDoorsMenu, " Show Doors Menu");

        CheatToggles.mushSab = UIHelpers.Toggle(CheatToggles.mushSab, " Mushroom Mixup");

        CheatToggles.mushSpore = UIHelpers.Toggle(CheatToggles.mushSpore, " Trigger Spores");

        CheatToggles.sabotageMap = UIHelpers.Toggle(CheatToggles.sabotageMap, " Open Sabotage Map");
    }

    private void DrawVents()
    {
        GUILayout.Label("Vents", GUIStylePreset.TabSubtitle);

        CheatToggles.unlockVents = UIHelpers.Toggle(CheatToggles.unlockVents, " Unlock Vents");

        CheatToggles.kickVents = UIHelpers.Toggle(CheatToggles.kickVents, " Kick All From Vents");

        CheatToggles.walkInVents = UIHelpers.Toggle(CheatToggles.walkInVents, " Walk In Vents");
    }
}
