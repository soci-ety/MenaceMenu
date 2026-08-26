using UnityEngine;
using MalumMenu.features;

namespace MalumMenu;

public class RolesTab : ITab
{
    public string name => "Roles";
    private int _materialSection;
    public string MaterialSectionName => _materialSection switch
    {
        0 => "General",
        1 => "Impostor",
        2 => "Shapeshifter",
        3 => "Crewmate",
        _ => "Other Roles"
    };

    public void Draw()
    {
        if (MenuUI.IsMaterialLayoutActive)
        {
            DrawMaterialSections();
            return;
        }

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

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawOtherRoles();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawMaterialSections()
    {
        string[] sections = { "General", "Impostor", "Shapeshifter", "Crewmate", "Other Roles" };
        for (int i = 0; i < sections.Length; i++)
        {
            if (i % 3 == 0)
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(32));

            Color previousBackground = GUI.backgroundColor;
            if (i == _materialSection)
                GUI.backgroundColor = MenuUI.GetMaterialAccentColor();
            if (GUILayout.Button(sections[i], MenuUI.CreateMaterialTabStyle(i == _materialSection), GUILayout.MinWidth(80), GUILayout.ExpandWidth(true)))
                _materialSection = i;
            GUI.backgroundColor = previousBackground;

            if (i % 3 != 2 && i != sections.Length - 1)
                GUILayout.Space(4);
            if (i % 3 == 2 || i == sections.Length - 1)
            {
                GUILayout.EndHorizontal();
                if (i != sections.Length - 1)
                    GUILayout.Space(4);
            }
        }
        GUILayout.Space(6);

        switch (_materialSection)
        {
            case 0:
                DrawGeneral();
                break;
            case 1:
                DrawImpostor();
                break;
            case 2:
                DrawShapeshifter();
                break;
            case 3:
                DrawCrewmate();
                break;
            default:
                DrawOtherRoles();
                break;
        }
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

    private void DrawOtherRoles()
    {
        DrawTracker();

        GUILayout.Space(15);

        DrawEngineer();

        GUILayout.Space(15);

        DrawScientist();

        GUILayout.Space(15);

        DrawDetective();
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
