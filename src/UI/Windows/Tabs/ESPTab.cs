using UnityEngine;
using MalumMenu.features;

namespace MalumMenu;

public class ESPTab : ITab
{
    public string name => "ESP";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawCamera();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawTracers();

        GUILayout.Space(15);

        DrawMinimap();

        GUILayout.Space(15);

        DrawRadar();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.seePlayerInfo = UIHelpers.Toggle(CheatToggles.seePlayerInfo, " See Player Info");

        CheatToggles.seeRoles = UIHelpers.Toggle(CheatToggles.seeRoles, " See Roles");

        CheatToggles.seeGhosts = UIHelpers.Toggle(CheatToggles.seeGhosts, " See Ghosts");

        CheatToggles.noShadows = UIHelpers.Toggle(CheatToggles.noShadows, " No Shadows");

        CheatToggles.taskArrows = UIHelpers.Toggle(CheatToggles.taskArrows, " Task Arrows");

        CheatToggles.revealVotes = UIHelpers.Toggle(CheatToggles.revealVotes, " Reveal Votes");

        CheatToggles.seeLobbyInfo = UIHelpers.Toggle(CheatToggles.seeLobbyInfo, " See Lobby Info");

        Visuals.SkipShhhAnimation.Enabled = UIHelpers.Toggle(Visuals.SkipShhhAnimation.Enabled, "Skip Shhh Animation");

        Visuals.NoSeekerAnimation.Enabled = UIHelpers.Toggle(Visuals.NoSeekerAnimation.Enabled, "Skip Seeker Animation");

        Visuals.ShowGhosts.Enabled = UIHelpers.Toggle(Visuals.ShowGhosts.Enabled, "Show Dead Players");

        Visuals.AccurateDisconnectReasons.Enabled = UIHelpers.Toggle(Visuals.AccurateDisconnectReasons.Enabled, "Use more accurate disconnection reasons");

        Visuals.ShowProtections.Enabled = UIHelpers.Toggle(Visuals.ShowProtections.Enabled, "Show Guardian Angel Protections");
    }

    private void DrawCamera()
    {
        GUILayout.Label("Camera", GUIStylePreset.TabSubtitle);

        CheatToggles.zoomOut = UIHelpers.Toggle(CheatToggles.zoomOut, " Zoom Out");

        CheatToggles.spectate = UIHelpers.Toggle(CheatToggles.spectate, " Spectate");

        CheatToggles.freecam = UIHelpers.Toggle(CheatToggles.freecam, " Freecam");
    }

    private void DrawTracers()
    {
        GUILayout.Label("Tracers", GUIStylePreset.TabSubtitle);

        CheatToggles.tracersCrew = UIHelpers.Toggle(CheatToggles.tracersCrew, " Crewmates");

        CheatToggles.tracersImps = UIHelpers.Toggle(CheatToggles.tracersImps, " Impostors");

        CheatToggles.tracersGhosts = UIHelpers.Toggle(CheatToggles.tracersGhosts, " Ghosts");

        CheatToggles.tracersBodies = UIHelpers.Toggle(CheatToggles.tracersBodies, " Dead Bodies");

        CheatToggles.colorBasedTracers = UIHelpers.Toggle(CheatToggles.colorBasedTracers, " Color-based");

        CheatToggles.distanceBasedTracers = UIHelpers.Toggle(CheatToggles.distanceBasedTracers, " Distance-based");
    }

    private void DrawMinimap()
    {
        GUILayout.Label("Minimap", GUIStylePreset.TabSubtitle);

        CheatToggles.mapCrew = UIHelpers.Toggle(CheatToggles.mapCrew, " Crewmates");

        CheatToggles.mapImps = UIHelpers.Toggle(CheatToggles.mapImps, " Impostors");

        CheatToggles.mapGhosts = UIHelpers.Toggle(CheatToggles.mapGhosts, " Ghosts");

        CheatToggles.colorBasedMap = UIHelpers.Toggle(CheatToggles.colorBasedMap, " Color-based");
    }

    private void DrawRadar()
    {
        GUILayout.Label("Radar", GUIStylePreset.TabSubtitle);

        CheatToggles.showRadar = UIHelpers.Toggle(CheatToggles.showRadar, " Show Radar");
        CheatToggles.radarRealistic = UIHelpers.Toggle(CheatToggles.radarRealistic, " Realistic Map");
        CheatToggles.radarGhosts = UIHelpers.Toggle(CheatToggles.radarGhosts, " Show Ghosts");
        CheatToggles.radarDeadBodies = UIHelpers.Toggle(CheatToggles.radarDeadBodies, " Show Dead Bodies");
        CheatToggles.radarRightClickTeleport = UIHelpers.Toggle(CheatToggles.radarRightClickTeleport, " Right-click Teleport");
        CheatToggles.radarHideInMeeting = UIHelpers.Toggle(CheatToggles.radarHideInMeeting, " Hide In Meetings");

            RadarHandler.RadarScale = UIHelpers.HorizontalSlider(RadarHandler.RadarScale, 0.65f, 1.6f);
        GUILayout.Label($"Radar Scale: {RadarHandler.RadarScale:F2}");
            RadarHandler.RadarAlpha = UIHelpers.HorizontalSlider(RadarHandler.RadarAlpha, 0.2f, 1f);
        GUILayout.Label($"Radar Opacity: {RadarHandler.RadarAlpha:F2}");
    }
}
