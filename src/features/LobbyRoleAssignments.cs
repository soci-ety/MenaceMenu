using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;

namespace MalumMenu;

public static class LobbyRoleAssignments
{
    private static readonly Dictionary<byte, RoleTypes> PendingRoles = new();

    public static int Count => PendingRoles.Count;

    public static IEnumerable<KeyValuePair<byte, RoleTypes>> GetPendingAssignments()
    {
        return PendingRoles;
    }

    public static void Queue(byte playerId, RoleTypes role)
    {
        PendingRoles[playerId] = role;
    }

    public static void Clear()
    {
        PendingRoles.Clear();
    }

    public static void PruneMissingPlayers(Il2CppSystem.Collections.Generic.List<PlayerControl> players)
    {
        HashSet<byte> activePlayerIds = new();
        foreach (PlayerControl player in players)
        {
            if (player != null)
                activePlayerIds.Add(player.PlayerId);
        }

        List<byte> missingPlayerIds = new();
        foreach (byte playerId in PendingRoles.Keys)
        {
            if (!activePlayerIds.Contains(playerId))
                missingPlayerIds.Add(playerId);
        }

        foreach (byte playerId in missingPlayerIds)
            PendingRoles.Remove(playerId);
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
