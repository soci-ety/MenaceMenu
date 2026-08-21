using Hazel;
using UnityEngine;

namespace MalumMenu.routines
{
    public class PetPlayerRoutine : IRoutine
    {
        public PetPlayerRoutine() : base("PetPlayer") { }

        private const float PetDelay = 0.6f;

        public PlayerControl target;
        private float timeElapsed;

        public override void Run()
        {
            if (PlayerControl.LocalPlayer == null || target == null) return;

            timeElapsed += Time.deltaTime;
            if (timeElapsed < PetDelay) return;
            timeElapsed = 0.0f;

            Vector2 petPosition = target.transform.position;
            petPosition.y -= PlayerControl.LocalPlayer.cosmetics.currentPet.yOffset * 2;

            PlayerControl.LocalPlayer.cosmetics.CurrentPet.SetGettingPet(true, petPosition);
            PlayerControl.LocalPlayer.cosmetics.PettingHand.StartPet(PlayerControl.LocalPlayer.cosmetics.currentPet);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.MyPhysics.NetId,
                (byte)RpcCalls.Pet,
                SendOption.Reliable,
                -1
            );

            NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), writer);
            NetHelpers.WriteVector2(petPosition, writer);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        protected override void OnEnable()
        {
            if (PlayerControl.LocalPlayer == null)
            {
                MalumMenu.notifications.Send("Pet Player", "Pet Player can only be used inside of a game.", 10);
                Enabled = false;
                return;
            }

            PlayerControl.LocalPlayer.moveable = false;
            PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;
        }

        protected override void OnDisable()
        {
            target = null;

            if (PlayerControl.LocalPlayer != null)
            {
                PlayerControl.LocalPlayer.moveable = true;
                PlayerControl.LocalPlayer.MyPhysics.RpcCancelPet();
            }
        }

        public override void OnDisconnect()
        {
            MalumMenu.notifications.Send("Pet Player", "Pet Player was disabled as you left the game.", 10);
            Enabled = false;
        }
    }
}