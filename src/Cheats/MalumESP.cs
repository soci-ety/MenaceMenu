using UnityEngine;
using Sentry.Internal.Extensions;
using System.Collections.Generic;

namespace MalumMenu;
public static class MalumESP
{
    private static bool _freecamActive;
    private static bool _resolutionChangeNeeded;
    private static readonly Dictionary<byte, string> _playerNameTags = new();
    private static readonly Dictionary<byte, int> _playerNameTagLayouts = new();
    private static readonly Dictionary<byte, string> _meetingNameTags = new();
    private static readonly Dictionary<byte, int> _meetingNameTagLayouts = new();

    private static int GetNameTagLayout()
    {
        return CheatToggles.seeRoles && CheatToggles.seePlayerInfo ? 2 : CheatToggles.seeRoles || CheatToggles.seePlayerInfo ? 1 : 0;
    }

    public static void SporeCloudVision(Mushroom mushroom)
    {
        if (CheatToggles.noShadows)
        {
            // Change the Z axis position of spore clouds as to make players appear above them
            mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, -1);
            return;
        }

        // Normal Z axis position: 5f
        mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, 5f);
    }

    public static bool IsFullbrightActive()
    {
        // Fullbright is automatically activated when being a ghost, zooming out, spectating other players, or "freecamming"
        // This is done to avoid issues with shadows
        return CheatToggles.noShadows || (PlayerControl.LocalPlayer?.Data != null && PlayerControl.LocalPlayer.Data.IsDead) || Camera.main.orthographicSize > 3f || Camera.main.gameObject.GetComponent<FollowerCamera>().Target != PlayerControl.LocalPlayer;
    }

    public static void ZoomOut(HudManager hudManager)
    {
        Camera cam = Camera.main;
        if (cam == null || hudManager?.UICamera == null) return;

        if (CheatToggles.zoomOut)
        {
            // Suspend zoomOut whenever a UI screen requires scrolling
            if ((hudManager.Chat != null && hudManager.Chat.IsOpenOrOpening)
                || PlayerCustomizationMenu.Instance
                || (Utils.isLobby && FriendsListUI.Instance != null && FriendsListUI.Instance.IsOpen)
                || (Utils.isLobby && GameStartManager.Instance != null && (
                    (GameStartManager.Instance.LobbyInfoPane != null
                     && GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane != null
                     && GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.active)
                    || GameStartManager.Instance.RulesEditPanel))) return;

            _resolutionChangeNeeded = true;

            if (Input.GetAxis("Mouse ScrollWheel") < 0f ) // Zoom out
            {
                // Both the main camera and the UI camera need to be adjusted
                cam.orthographicSize++;
                hudManager.UICamera.orthographicSize++;
                Utils.AdjustResolution();
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0f )
            {
                // Zoom in
                if (!(cam.orthographicSize > 3f)) return; // Never go below the default orthographicSize: 3f

                cam.orthographicSize--;
                hudManager.UICamera.orthographicSize--;
                Utils.AdjustResolution();
            }
        }
        else
        {
            // orthographicSize is reset to default value: 3f
            cam.orthographicSize = 3f;
            hudManager.UICamera.orthographicSize = 3f;

            // Utils.AdjustResolution() is invoked one last time to prevent issues with UI
            if (_resolutionChangeNeeded)
            {
                Utils.AdjustResolution();
                _resolutionChangeNeeded = false;
            }
        }
    }

    public static void MeetingNametags(MeetingHud meetingHud)
    {
        try
        {
            foreach (var playerState in Utils.GetPlayerStates(meetingHud))
            {
                // Fetch the NetworkedPlayerInfo of each playerState
                var data = GameData.Instance.GetPlayerById(playerState.PlayerId);

                if (data.IsNull() || data.Disconnected || data.Outfits[PlayerOutfitType.Default].IsNull()) continue;

                byte playerId = playerState.PlayerId;
                string nameTag = Utils.GetNameTag(data, data.DefaultOutfit.PlayerName);
                int layout = GetNameTagLayout();

                playerState.NameText.text = nameTag;
                _meetingNameTags[playerId] = nameTag;
                playerState.NameText.ForceMeshUpdate(true, true);

                // Move and resize the nametag to prevent it overlapping with colorblind text
                if (!_meetingNameTagLayouts.TryGetValue(playerId, out int previousLayout) || previousLayout != layout)
                {
                    if (layout == 2)
                    {
                        playerState.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                        playerState.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                    }
                    else if (layout == 1)
                    {
                        playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                        playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                    }
                    else
                    {
                        // Reset the position and scale of the nametag to default values
                        playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                        playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                    }

                    _meetingNameTagLayouts[playerId] = layout;
                }
            }
        } catch { }
    }

    public static void PlayerNametags(PlayerPhysics playerPhysics)
    {
        try
        {
            byte playerId = playerPhysics.myPlayer.PlayerId;
            string nameTag = Utils.GetNameTag(playerPhysics.myPlayer.Data, playerPhysics.myPlayer.CurrentOutfit.PlayerName);
            int layout = GetNameTagLayout();

            if (!_playerNameTags.TryGetValue(playerId, out string previousNameTag) || previousNameTag != nameTag)
            {
                playerPhysics.myPlayer.cosmetics.SetName(nameTag);
                _playerNameTags[playerId] = nameTag;
            }

            // Move the nameText up to prevent it overlapping with colorblind text
            if (!_playerNameTagLayouts.TryGetValue(playerId, out int previousLayout) || previousLayout != layout)
            {
                playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = layout == 2
                    ? new Vector3(0f, 0.186f, 0f)
                    : layout == 1
                        ? new Vector3(0f, 0.093f, 0f)
                        : new Vector3(0f, 0f, 0f);
                _playerNameTagLayouts[playerId] = layout;
            }
        } catch { }
    }

    public static void ChatNametags(ChatBubble chatBubble)
    {
        try
        {
            // Update the player's nametag appropriately
            chatBubble.NameText.text = Utils.GetNameTag(chatBubble.playerInfo, chatBubble.NameText.text, true);

            // Adjust the chatBubble's size to the new nametag to prevent issues
            chatBubble.NameText.ForceMeshUpdate(true, true);
            chatBubble.Background.size = new Vector2(5.52f, 0.2f + chatBubble.NameText.GetNotDumbRenderedHeight() + chatBubble.TextArea.GetNotDumbRenderedHeight());
            chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);

        } catch { }
    }

    public static void SeeGhostsCheat(PlayerPhysics playerPhysics)
    {
        try
        {
            if (playerPhysics.myPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                playerPhysics.myPlayer.Visible = CheatToggles.seeGhosts;
            }
        } catch {}
    }

    public static void FreecamCheat()
    {
        if (CheatToggles.freecam)
        {
            // Completely disable FollowerCamera
            if (!_freecamActive)
            {
                Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = false;
                Camera.main.gameObject.GetComponent<FollowerCamera>().Target = null;
                _freecamActive = true;
            }

            // Prevent the player from moving while in freecam
            PlayerControl.LocalPlayer.moveable = false;

            // Get keyboard input
            var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);

            // Change the camera's position depending on the keyboard input
            Camera.main.transform.position = Camera.main.transform.position + movement * 10f * Time.deltaTime;
        }
        else
        {
            // Re-enable FollowerCamera & movement once freecam is disabled
            if (!_freecamActive) return;
            PlayerControl.LocalPlayer.moveable = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            _freecamActive = false;
        }
    }
}