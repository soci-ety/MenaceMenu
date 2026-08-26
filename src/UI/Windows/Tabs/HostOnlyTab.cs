using UnityEngine;

namespace MalumMenu;

public class HostOnlyTab : ITab
{
    public string name => "Host-Only";

    public void Draw()
    {
        if (MenuUI.IsMaterialLayoutActive)
        {
            DrawMaterialLayout();
            return;
        }

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game, these options will not work.");
        }
        else if (!AmongUsClient.Instance.AmHost)
        {
            GUILayout.Label("You are not the host of the current lobby. Using these options will either do nothing or get you banned by the anticheat");
        }

        DrawGeneral();

        GUILayout.Space(15);

        DrawMurder();

        GUILayout.Space(15);

        DrawGameState();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawMeetings();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawMaterialLayout()
    {
        if (PlayerControl.LocalPlayer == null)
            GUILayout.Label("You are not currently in a game, these options will not work.");
        else if (!AmongUsClient.Instance.AmHost)
            GUILayout.Label("You are not the host of the current lobby. Using these options may do nothing.");

        GUILayout.Label("General", GUIStylePreset.TabSubtitle);
        DrawGeneral();
        GUILayout.Space(12);
        GUILayout.Label("Murder", GUIStylePreset.TabSubtitle);
        DrawMurder();
        GUILayout.Space(12);
        GUILayout.Label("Game State", GUIStylePreset.TabSubtitle);
        DrawGameState();
        GUILayout.Space(12);
        DrawMeetings();
    }

    private void DrawGeneral()
    {
        CheatToggles.bypassHostOnly = UIHelpers.Toggle(CheatToggles.bypassHostOnly, " Bypass Host Only");

        GUILayout.Space(5);

        CheatToggles.killVanished = UIHelpers.Toggle(CheatToggles.killVanished, " Kill While Vanished");

        CheatToggles.killAnyone = UIHelpers.Toggle(CheatToggles.killAnyone, " Kill Anyone");

        CheatToggles.noKillCd = UIHelpers.Toggle(CheatToggles.noKillCd, " No Kill Cooldown");

        CheatToggles.showProtectMenu = UIHelpers.Toggle(CheatToggles.showProtectMenu, " Show Protect Menu");

        // CheatToggles.forceRole = UIHelpers.Toggle(CheatToggles.forceRole, " Force Role");

        // CheatToggles.noOptionsLimits = UIHelpers.Toggle(CheatToggles.noOptionsLimits, " No Options Limits");
    }

    private void DrawMurder()
    {
        GUILayout.Label("Murder", GUIStylePreset.TabSubtitle);

        CheatToggles.killPlayer = UIHelpers.Toggle(CheatToggles.killPlayer, " Kill Player");

        CheatToggles.telekillPlayer = UIHelpers.Toggle(CheatToggles.telekillPlayer, " Telekill Player");

        CheatToggles.killAllCrew = UIHelpers.Toggle(CheatToggles.killAllCrew, " Kill All Crewmates");

        CheatToggles.killAllImps = UIHelpers.Toggle(CheatToggles.killAllImps, " Kill All Impostors");

        CheatToggles.killAll = UIHelpers.Toggle(CheatToggles.killAll, " Kill Everyone");
    }

    private void DrawGameState()
    {
        GUILayout.Label("Game State", GUIStylePreset.TabSubtitle);

        CheatToggles.forceStartGame = UIHelpers.Toggle(CheatToggles.forceStartGame, " Force Start Game");

        CheatToggles.noGameEnd = UIHelpers.Toggle(CheatToggles.noGameEnd, " No Game End");
    }

    private void DrawMeetings()
    {
        GUILayout.Label("Meetings", GUIStylePreset.TabSubtitle);

        CheatToggles.skipMeeting = UIHelpers.Toggle(CheatToggles.skipMeeting, " Skip Meeting");

        CheatToggles.voteImmune = UIHelpers.Toggle(CheatToggles.voteImmune, " Vote Immune");

        CheatToggles.ejectPlayer = UIHelpers.Toggle(CheatToggles.ejectPlayer, " Eject Player");
    }
}
