using UnityEngine;
using MalumMenu.features;

namespace MalumMenu;

public class RolesTab : ITab
{
    public string name => "Roles";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawImpostor();

        GUILayout.Space(15);

        DrawShapeshifter();

        GUILayout.Space(15);

        DrawCrewmate();

        GUILayout.Space(15);

        DrawTracker();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawEngineer();

        GUILayout.Space(15);

        DrawScientist();

        GUILayout.Space(15);

        DrawDetective();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.setFakeRole = UIHelpers.Toggle(CheatToggles.setFakeRole, " Set Fake Role");

        CheatToggles.setFakeAlive = UIHelpers.Toggle(CheatToggles.setFakeAlive, " Set Fake Alive");
    }

    private void DrawImpostor()
    {
        GUILayout.Label("Impostor", GUIStylePreset.TabSubtitle);

        CheatToggles.killReach = UIHelpers.Toggle(CheatToggles.killReach, " Kill Reach");

        Roles.SkipSabotageChecks.SabotageInVents = UIHelpers.Toggle(Roles.SkipSabotageChecks.SabotageInVents, " Allow Sabotaging In Vents As Imposter");

        CheatToggles.impostorTasks = UIHelpers.Toggle(CheatToggles.impostorTasks, " Allow Tasks");
    }

    private void DrawShapeshifter()
    {
        GUILayout.Label("Shapeshifter", GUIStylePreset.TabSubtitle);

        CheatToggles.noShapeshiftAnim = UIHelpers.Toggle(CheatToggles.noShapeshiftAnim, " No Ss Animation");

        CheatToggles.endlessSsDuration = UIHelpers.Toggle(CheatToggles.endlessSsDuration, " Endless Ss Duration");
    }

    private void DrawCrewmate()
    {
        GUILayout.Label("Crewmate", GUIStylePreset.TabSubtitle);

        Roles.SkipSabotageChecks.SabotageAsCrewmate = UIHelpers.Toggle(Roles.SkipSabotageChecks.SabotageAsCrewmate, " Sabotage As Crewmate");

        CheatToggles.showTasksMenu = UIHelpers.Toggle(CheatToggles.showTasksMenu, " Show Tasks Menu");
    }

    private void DrawTracker()
    {
        GUILayout.Label("Tracker", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessTracking = UIHelpers.Toggle(CheatToggles.endlessTracking, " Endless Tracking");

        CheatToggles.noTrackingDelay = UIHelpers.Toggle(CheatToggles.noTrackingDelay, " No Track Delay");

        CheatToggles.noTrackingCooldown = UIHelpers.Toggle(CheatToggles.noTrackingCooldown, " No Track Cooldown");

        CheatToggles.trackReach = UIHelpers.Toggle(CheatToggles.trackReach, " Track Reach");
    }

    private void DrawEngineer()
    {
        GUILayout.Label("Engineer", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessVentTime = UIHelpers.Toggle(CheatToggles.endlessVentTime, " Endless Vent Time");

        CheatToggles.noVentCooldown = UIHelpers.Toggle(CheatToggles.noVentCooldown, " No Vent Cooldown");
    }

    private void DrawScientist()
    {
        GUILayout.Label("Scientist", GUIStylePreset.TabSubtitle);

        CheatToggles.endlessBattery = UIHelpers.Toggle(CheatToggles.endlessBattery, " Endless Battery");

        CheatToggles.noVitalsCooldown = UIHelpers.Toggle(CheatToggles.noVitalsCooldown, " No Vitals Cooldown");
    }

    private void DrawDetective()
    {
        GUILayout.Label("Detective", GUIStylePreset.TabSubtitle);

        CheatToggles.interrogateReach = UIHelpers.Toggle(CheatToggles.interrogateReach, " Interrogate Reach");
    }
}
