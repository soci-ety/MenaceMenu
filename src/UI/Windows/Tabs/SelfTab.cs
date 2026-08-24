using System.Collections;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using MalumMenu.features;

namespace MalumMenu
{
    internal class SelfTab : ITab
    {
        public string name => "Self";

        private uint level = 0;

        public void Draw()
        {
            GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));
            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.");
            }
            else
            {
                GUILayout.Label($"Role: {PlayerControl.LocalPlayer.Data.RoleType}");
            }

            GUILayout.Space(8);
            GUILayout.Label("Color Utilities", GUIStylePreset.TabSubtitle);

            bool newFreeColorCycle = UIHelpers.Toggle(CheatToggles.freeColorCycle, "Free Color Cycle");
            if (newFreeColorCycle != CheatToggles.freeColorCycle)
            {
                CheatToggles.freeColorCycle = newFreeColorCycle;
                if (CheatToggles.freeColorCycle)
                {
                    CheatToggles.snipeColor = false;
                }
            }

            bool newSnipeColor = false;
            if (CheatToggles.snipeColor || CheatToggles.freeColorCycle)
            {
                newSnipeColor = UIHelpers.Toggle(CheatToggles.snipeColor, "Snipe Color");
                if (newSnipeColor != CheatToggles.snipeColor)
                {
                    CheatToggles.snipeColor = newSnipeColor;
                    if (CheatToggles.snipeColor)
                    {
                        CheatToggles.freeColorCycle = false;
                    }
                }
            }
            else
            {
                CheatToggles.snipeColor = UIHelpers.Toggle(CheatToggles.snipeColor, "Snipe Color");
            }

            if (CheatToggles.snipeColor)
            {
                int selectedColorId = Mathf.Clamp(CheatToggles.snipeColorId, 0, 17);
                Color selectedColor = Palette.PlayerColors != null && selectedColorId < Palette.PlayerColors.Length
                    ? Palette.PlayerColors[selectedColorId]
                    : Color.white;

                GUILayout.BeginHorizontal();
                Color previousBackground = GUI.backgroundColor;
                GUI.backgroundColor = selectedColor;
                GUILayout.Box(string.Empty, GUILayout.Width(24), GUILayout.Height(20));
                GUI.backgroundColor = previousBackground;
                GUILayout.Label($"Selected Color: {((CrewmateColor)selectedColorId)}");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Target Color:", GUILayout.Width(90));
                CheatToggles.snipeColorId = Mathf.RoundToInt(UIHelpers.HorizontalSlider(CheatToggles.snipeColorId, 0, 17, GUILayout.Width(180)));
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Apply Snipe Color"))
                {
                    if (PlayerControl.LocalPlayer != null)
                    {
                        PlayerControl.LocalPlayer.CmdCheckColor((byte)Mathf.Clamp(CheatToggles.snipeColorId, 0, 17));
                    }
                }
            }

            // Self.BypassIntentionalDisconnectionBlocks.Enabled = UIHelpers.Toggle(Self.BypassIntentionalDisconnectionBlocks.Enabled, "Bypass intentional disconnection temp bans");
            Self.UpdateStatsFreeplay.Enabled = UIHelpers.Toggle(Self.UpdateStatsFreeplay.Enabled, "Update Stats in Freeplay");
            Immortality.Enabled = UIHelpers.Toggle(Immortality.Enabled, "Become Immortal");
            Self.AlwaysShowTaskAnimations = UIHelpers.Toggle(Self.AlwaysShowTaskAnimations, "Always Show Task Animations");
            Self.NoLadderCooldown.Enabled = UIHelpers.Toggle(Self.NoLadderCooldown.Enabled, "No Ladder Cooldown");
            Self.UnlimitedMeetings.enabled = UIHelpers.Toggle(Self.UnlimitedMeetings.enabled, "Unlimited Meetings");

            if (GUILayout.Button("Call Meeting"))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    MalumMenu.Log.LogInfo("We are the host, we can force a meeting");
                    Utilities.OpenMeeting(PlayerControl.LocalPlayer, null);
                }
                else
                {
                    PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                }
            }

            if (GUILayout.Button("Complete All Tasks"))
            {
                PlayerControl.LocalPlayer.StartCoroutine(CompleteAllTasks().WrapToIl2Cpp());
            }

            if (GUILayout.Button("Randomize Avatar"))
            {
                if (AmongUsClient.Instance.AmConnected)
                {
                    Utilities.RandomizePlayer(true);

                    MalumMenu.notifications.Send("Player Randomizer", "Your avatar has been randomized for this game.", 5);
                }
                else
                {
                    AccountManager.Instance.RandomizeName();
                    Utilities.RandomizePlayer();

                    MalumMenu.notifications.Send("Player Randomizer", "Your name and avatar has been randomized.", 5);
                }
            }

            GUILayout.Label("Task Animations:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Medbay Scan"))
            {
                if (!Utils.isLobby)
                {
                    Network.RPCEmitter.SendSetScanner(true);
                }else
                {
                    MalumMenu.notifications.Send("Anticheat Notice", "This cheat is disabled in lobby due to anticheat detection. You can use it once the game starts.");
                }
            }

            if (GUILayout.Button("Finish Medbay Scan"))
            {
                if (!Utils.isLobby)
                {
                    Network.RPCEmitter.SendSetScanner(false);
                }else
                {
                    MalumMenu.notifications.Send("Anticheat Notice", "This cheat is disabled in lobby due to anticheat detection. You can use it once the game starts.");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Asteroids"))
            {
                if (!Utils.isLobby)
                {
                    Network.RPCEmitter.SendPlayAnimation((byte)TaskTypes.ClearAsteroids);
                }else
                {
                    MalumMenu.notifications.Send("Anticheat Notice", "This cheat is disabled in lobby due to anticheat detection. You can use it once the game starts.");
                }
            }

            if (GUILayout.Button("Empty Garbage"))
            {
                if (!Utils.isLobby)
                {
                    Network.RPCEmitter.SendPlayAnimation((byte)TaskTypes.EmptyGarbage);
                }else
                {
                    MalumMenu.notifications.Send("Anticheat Notice", "This cheat is disabled in lobby due to anticheat detection. You can use it once the game starts.");
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Prime Shields"))
            {
                if (!Utils.isLobby)
                {
                    Network.RPCEmitter.SendPlayAnimation((byte)TaskTypes.PrimeShields);
                } else
                {
                    MalumMenu.notifications.Send("Anticheat Notice", "This cheat is disabled in lobby due to anticheat detection. You can use it once the game starts.");
                }
            }

            GUILayout.Space(5);
            GUILayout.Label($"Update level to: {level + 1}");
            level = (uint)UIHelpers.HorizontalSlider(level, 0, 199);

            if (GUILayout.Button("Send Level Update"))
            {
                PlayerControl.LocalPlayer.RpcSetLevel(level);
                MalumMenu.notifications.Send("Level Updater", $"Your level has been changed to {level + 1}", 5);
            }
            GUILayout.EndVertical();
        }
        private IEnumerator CompleteAllTasks()
        {
            Il2CppSystem.Collections.Generic.List<PlayerTask> allTasks = PlayerControl.LocalPlayer.myTasks;

            MalumMenu.Log.LogInfo("Completing all tasks...");
            foreach (PlayerTask task in allTasks)
            {
                if (task.IsComplete)
                {
                    MalumMenu.Log.LogInfo($"Task {task.Id} has already been completed, skipping");
                    continue;
                }

                MalumMenu.Log.LogInfo($"Sent CompleteTask RPC for task {task.Id}");
                PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);

                // If we want to complete more than six tasks then a delay needs to be implemented
                // otherwise the vanilla anticheat will kick us for violating ratelimits
                yield return Effects.Wait(0.05f);
            }

            MalumMenu.notifications.Send("Task Finisher", "All your tasks have been finished.", 5);
        }

    }
}
