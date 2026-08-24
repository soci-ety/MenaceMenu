using MalumMenu.anticheat;
using UnityEngine;

namespace MalumMenu
{
    internal class AnticheatTab : ITab
    {
        public string name => "Anticheat";

        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            Anticheat.Enabled = UIHelpers.Toggle(Anticheat.Enabled, "Enable Menace Menu Anticheat");

            Anticheat.CheckSpoofedPlatforms = UIHelpers.Toggle(Anticheat.CheckSpoofedPlatforms, "Flag Spoofed Platform Data");

            GUILayout.Space(5);
            GUILayout.Label("RPCs that should be checked by the anticheat:");
            foreach (var (rpcCall, handler) in Anticheat.RpcHandlers)
            {
                handler.Enabled = UIHelpers.Toggle(handler.Enabled, $"{rpcCall}");
            }

            GUILayout.Space(5);
            GUILayout.Label("When a cheater is detected:");
            Anticheat.sendNotification = UIHelpers.Toggle(Anticheat.sendNotification, "Send notification");
            Anticheat.discardRpc = UIHelpers.Toggle(Anticheat.discardRpc, "Discard RPC");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Punish the player with: {Anticheat.punishment}");
            Anticheat.punishment = (Anticheat.Punishments)UIHelpers.HorizontalSlider((float)Anticheat.punishment, 0, 3);
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }
    }
}