using MalumMenu.features;
using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

public class TrollTab : ITab
{
    public string name => "Troll";

    public void Draw()
    {
        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game, these options will not work.");
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        Troll.AutoReportBodies.Enabled = UIHelpers.Toggle(Troll.AutoReportBodies.Enabled, "Automatically Report Bodies");
         MalumMenu.routines.autoTriggerSpores.Enabled = UIHelpers.Toggle(MalumMenu.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
        Troll.BlockSabotages.Enabled = UIHelpers.Toggle(Troll.BlockSabotages.Enabled, "Block Sabotages");
        Troll.BlockVenting.Enabled = UIHelpers.Toggle(Troll.BlockVenting.Enabled, "Disable Vents");

        if (GUILayout.Button(" Trigger All Spores"))
        {
            if (Utilities.GetCurrentMap() != MapNames.Fungle)
               {
                   MalumMenu.notifications.Send("Trigger Spores", "This option only works on the Fungle map.");
               }
               else
               {
                   FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

                   foreach (Mushroom mushroom in shipStatus.sporeMushrooms.Values)
                   {
                       PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
                   }

                   MalumMenu.notifications.Send("Trigger Spores", "All spores have been triggered.", 5);
            }
        }

        if (GUILayout.Button(" Copy Random Player"))
        {
            PlayerControl randomPl = Utilities.GetRandomPlayer();
            Utilities.CopyPlayer(randomPl);
        }

        GUILayout.Space(5);

        GUILayout.Label("Teleport Flooder:");
        MalumMenu.routines.teleportSpammer.Enabled = UIHelpers.Toggle(MalumMenu.routines.teleportSpammer.Enabled, "Teleport Flooder");

        GUILayout.Label($"Destination: {MalumMenu.routines.teleportSpammer.DestinationName}");
        Dictionary<string, Vector2> teleportLocations = Teleporter.GetTeleportLocations();
        byte locationIndex = 0;
        foreach (var (name, position) in teleportLocations)
        {
            if (locationIndex % 2 == 0) GUILayout.BeginHorizontal();

            if (GUILayout.Button(name))
            {
                MalumMenu.routines.teleportSpammer.SetDestination(name, position);
            }

            if (locationIndex % 2 != 0) GUILayout.EndHorizontal();
            locationIndex++;
        }

        if (locationIndex % 2 != 0) GUILayout.EndHorizontal();

        if (GUILayout.Button("Kick All Players"))
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                MalumMenu.notifications.Send("Kick All Players", "Only the lobby host can kick players.");
            }
            else
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == PlayerControl.LocalPlayer || player.OwnerId == AmongUsClient.Instance.HostId) continue;

                    Utilities.KickPlayer(player);
                }
            }
        }

        GUILayout.Space(5);

        GUILayout.Label("Door Troller:");
        MalumMenu.routines.doorTroller.Enabled = UIHelpers.Toggle(MalumMenu.routines.doorTroller.Enabled, "Enabled");

        GUILayout.Label($"Lock and Unlock Delay: {MalumMenu.routines.doorTroller.doorDelay:F2}s");
        MalumMenu.routines.doorTroller.doorDelay = UIHelpers.HorizontalSlider(MalumMenu.routines.doorTroller.doorDelay, 0.1f, 2.0f);

        GUILayout.EndVertical();
    }
}
