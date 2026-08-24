using UnityEngine;
using MalumMenu.features;

namespace MalumMenu;

public class ProtectionsTab : ITab
{
    public string name => "Protections";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        // Network
        Protections.ForceDTLS.Enabled = UIHelpers.Toggle(Protections.ForceDTLS.Enabled, "Force enable DTLS to encrypt network data");

        Protections.BlockServerTeleports.Enabled = UIHelpers.Toggle(Protections.BlockServerTeleports.Enabled, "Block position updates from server");

        // Overloads
        Protections.HardenedReadPackedUInt.Enabled = UIHelpers.Toggle(Protections.HardenedReadPackedUInt.Enabled, "Use hardened packed int deserializer");
        Protections.BlockLargeGameMessages = UIHelpers.Toggle(Protections.BlockLargeGameMessages, "Block large game messages");
        Protections.BlockInvalidGameDataMessages = UIHelpers.Toggle(Protections.BlockInvalidGameDataMessages, "Block invalid game data messages");
        Protections.BlockUnauthorizedSystemUpdates = UIHelpers.Toggle(Protections.BlockUnauthorizedSystemUpdates, "Block unauthorized system updates");
        Protections.ProtectAgainstNonHostKickExploit = UIHelpers.Toggle(Protections.ProtectAgainstNonHostKickExploit, "Protect against non-host kick exploit");

        Protections.Votekicks.Enabled = UIHelpers.Toggle(Protections.Votekicks.Enabled, "Prevent being votekicked as host");

        GUILayout.EndVertical();
    }
}