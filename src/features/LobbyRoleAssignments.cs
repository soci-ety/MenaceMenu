using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;

namespace MalumMenu;

public static class LobbyRoleAssignments
{
    private static readonly Dictionary<byte, RoleTypes> PendingRoles = new();

    public static int Count => PendingRoles.Count;

    public static void Queue(byte playerId, RoleTypes role)
    {
        PendingRoles[playerId] = role;
    }

    public static void Clear()
    {
        PendingRoles.Clear();
    }

    public static void Apply(ref List<NetworkedPlayerInfo> players, ref List<RoleTypes> roleList, ref int rolesAssigned)
    {
        if (!Utils.isHost || PendingRoles.Count == 0 || players == null || roleList == null)
            return;

        foreach (KeyValuePair<byte, RoleTypes> assignment in PendingRoles)
        {
            int playerIndex = -1;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null && players[i].PlayerId == assignment.Key)
                {
                    playerIndex = i;
                    break;
                }
            }

            if (playerIndex < 0)
                continue;

            NetworkedPlayerInfo player = players[playerIndex];
            players.RemoveAt(playerIndex);

            for (int i = 0; i < roleList.Count; i++)
            {
                if (roleList[i] == assignment.Value)
                {
                    roleList.RemoveAt(i);
                    break;
                }
            }

            PlayerControl playerControl = null;
            foreach (PlayerControl candidate in PlayerControl.AllPlayerControls)
            {
                if (candidate != null && candidate.PlayerId == player.PlayerId)
                {
                    playerControl = candidate;
                    break;
                }
            }
            if (playerControl != null)
            {
                playerControl.RpcSetRole(assignment.Value, true);
                rolesAssigned++;
            }
        }

        PendingRoles.Clear();
    }
}

[HarmonyPatch(typeof(LogicRoleSelectionNormal), nameof(LogicRoleSelectionNormal.AssignRolesFromList))]
public static class LobbyRoleAssignmentPatch
{
    public static void Prefix(ref List<NetworkedPlayerInfo> players, ref List<RoleTypes> roleList, ref int rolesAssigned)
    {
        LobbyRoleAssignments.Apply(ref players, ref roleList, ref rolesAssigned);
    }
}
