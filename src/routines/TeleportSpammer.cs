using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu.routines
{
    public class TeleportSpammer : IRoutine
    {
        public TeleportSpammer() : base("TeleportSpammer") { }

        private float teleportDelay = 0.5f;
        private float timeElapsed = 0f;
        private Vector2 destination = new Vector2(-0.78f, 2.48f);

        public string DestinationName { get; private set; } = "Cafeteria";

        public void SetDestination(string name, Vector2 position)
        {
            DestinationName = name;
            destination = position;
        }

        public override void Run()
        {
            if(ShipStatus.Instance == null || !AmongUsClient.Instance.AmHost) return;

            timeElapsed += Time.deltaTime;
            if(timeElapsed < teleportDelay) return;
            timeElapsed = 0f;

            foreach(PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if(player == PlayerControl.LocalPlayer) continue;

                Teleporter.TeleportPlayerTo(player, destination);
            }
        }

        protected override void OnEnable()
        {
            if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                MalumMenu.notifications.Send("Teleport Spammer", "Teleport Spammer is only available to the lobby host.", 10);
                Enabled = false;
                return;
            }

            Dictionary<string, Vector2> locations = Teleporter.GetTeleportLocations();
            if (!locations.TryGetValue(DestinationName, out Vector2 selectedDestination))
            {
                foreach (var location in locations)
                {
                    SetDestination(location.Key, location.Value);
                    break;
                }
            }
            else
            {
                destination = selectedDestination;
            }
        }

        public override void OnDisconnect()
        {
            MalumMenu.notifications.Send("Teleport Spammer", "Teleport Spammer was disabled as you left the game.", 10);
            Enabled = false;
        }
    }
}
