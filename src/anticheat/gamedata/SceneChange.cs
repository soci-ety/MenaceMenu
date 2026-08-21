using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;

namespace MalumMenu.anticheat.gamedata
{
    internal class SceneChange : GameDataCheck
    {
        public override void Validate(MessageReader reader, ref bool blockMessage)
        {
            int clientId = reader.ReadPackedInt32();
            string scene = reader.ReadString();

            ClientData client = AmongUsClient.Instance.FindClientById(clientId);
            if (client == null)
            {
                Anticheat.Flag($"Received SceneChange message for unknown client: {clientId}.");
                blockMessage = true;
                return;
            }

            if (scene == "Tutorial")
            {
                Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} sent a scene change of Tutorial.");
                blockMessage = true;
            }
        }

        public override GameDataTypes GetGameDataType()
        {
            return GameDataTypes.SceneChangeFlag;
        }
    }
}