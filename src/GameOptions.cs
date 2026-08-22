using AmongUs.GameOptions;
using Hazel;
using MalumMenu.features;
using UnityEngine;

namespace MalumMenu
{
    internal class GameOptions
    {
        // If we want to freely modify IGameOptions without it applying to ourselves, we will need to clone it
        // There might be a better way of doing this, but I just serialize the game options into a byte array and serialize it back into IGameOptions
        // which gives us a new instance of IGameOptions based off our pre-existing options
        public static IGameOptions CreateCloneFromCurrent()
        {
            if (!GameManager.Instance || GameManager.Instance.LogicOptions == null) return null;
            return CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
        }

        public static IGameOptions CreateCloneOptions(IGameOptions options)
        {
            if (options == null) return null;
            if (!GameManager.Instance || GameManager.Instance.LogicOptions == null) return null;

            LogicOptions logicOptions = GameManager.Instance.LogicOptions;
            if (logicOptions.gameOptionsFactory == null) return null;

            byte[] byteArray = logicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn);
            return logicOptions.gameOptionsFactory.FromBytes(byteArray);
        }

        // Only send the game options update to one specific player
        public static void SendGameOptionsToClient(IGameOptions options, int targetClientId)
        {
            if (options == null || !GameManager.Instance || GameManager.Instance.LogicOptions == null
                || GameManager.Instance.LogicOptions.gameOptionsFactory == null) return;

            // We have the manually apply game options in Freeplay as there is no networking layer there
            // Freeplay has some settings that cannot be changed, such as player vision, so the Blind Player feature wont work there
            if (AmongUsClient.Instance != null
                && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay
                && PlayerControl.LocalPlayer != null
                && targetClientId == PlayerControl.LocalPlayer.OwnerId)
            {
                GameManager.Instance.LogicOptions.SetGameOptions(options);
                return;
            }

            if (Protections.BypassShapeshiftRatelimits.Enabled) options.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0.0f);

            int logicIndex = FindLogicOptionsIndex();
            if (logicIndex < 0) return;

            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
            writer.StartMessage((byte)logicIndex);
            writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
            writer.EndMessage();

            Network.BatchedMessage batch = new Network.BatchedMessage(targetClientId);
            batch.QueueDataFlag(GameManager.Instance.NetId, writer);
            batch.FinishBatch();
        }

        private static int FindLogicOptionsIndex()
        {
            if (!GameManager.Instance || GameManager.Instance.LogicComponents == null) return -1;

            var components = GameManager.Instance.LogicComponents;

            for (int i = 0; i < components.Count; i++)
            {
                GameLogicComponent component = components[i];
                if (component == null) continue;
                if (component.TryCast<LogicOptions>() == null) continue;

                return i;
            }

            return -1;
        }
    }
}
