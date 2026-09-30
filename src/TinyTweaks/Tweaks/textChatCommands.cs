using BepInEx.Configuration;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class textChatCommands
    {
        static ConfigEntry<bool> enableWhisperTextChat;
        static ConfigEntry<bool> whisperForYouEveryMsg;
        public static void Binds(Harmony harmony)
        {
            enableWhisperTextChat = tinyTweaks.config.Bind("Shorties", "WhisperCmd", true);
            whisperForYouEveryMsg = tinyTweaks.config.Bind("Shorties", "send a (secret msg for you) everytime", true);
            tinyTweaks.log("found peaktextchat. Initializing patch!");

            var sendChatMessageMethod = AccessTools.Method(typeof(PeakTextChat.TextChatManager), "SendChatMessage");
            var SlashCommandMethod = AccessTools.Method(typeof(InterceptMessage), "SlashCommand");
            if (sendChatMessageMethod == null || SlashCommandMethod == null)
            {
                tinyTweaks.log(sendChatMessageMethod==null?"didn't find peaktextchat method":"didn't find whisper method");
                return;
            }
            harmony.Patch(sendChatMessageMethod,prefix: new HarmonyMethod(SlashCommandMethod));
        }

        static byte chatEventCode = 81;
        static class InterceptMessage
        {
            //[HarmonyPatch(typeof(TextChatManager), "SendChatMessage")]
            //[HarmonyPrefix]
            static public bool SlashCommand(string message)
            {
                if (string.IsNullOrWhiteSpace(message) ||!enableWhisperTextChat.Value) return true; //if empty
                string cmd = message.Split(' ')[0];
                if (cmd.StartsWith("/w", StringComparison.OrdinalIgnoreCase) || cmd.StartsWith("/whisper", StringComparison.OrdinalIgnoreCase))
                {
                    string content = message.Substring(cmd.Length).Trim();
                    string[] args = content.Split(' ');
                    string plr = args[0];
                    if (args.Length < 2)
                    {
                        logMessage("you need 2 parameters usage: /w playerName msg");
                        return false;
                    }
                    string msg = content.Substring(plr.Length);

                    Whisper($"{plr} {msg}");
                    return false;
                }
                else if (cmd.StartsWith("/sit", StringComparison.OrdinalIgnoreCase))
                {
                    var c = Character.localCharacter;
                    if (c == null) return false;
                    c.refs.view.RPC("RPCA_PlayRemove", RpcTarget.All, "A_Scout_Emote_Sit", false);
                    c.refs.animations.PlayEmote("A_Scout_Emote_Sit");
                    tinyTweaks.log("played sit emote");
                    return false;
                }
                return true;
            }
        }
        static void Whisper(Photon.Realtime.Player whisperTo, string msg)
        {
            bool isDead = false;
            msg = $"<color=#8973a1>{msg} ";
            msg = msg + (whisperForYouEveryMsg.Value ? $"<size=16><i>(secret msg for you)</i></size></color>" : "</color>");

            object[] array = new object[]
            {
                PhotonNetwork.LocalPlayer.NickName,
                msg,
                PhotonNetwork.LocalPlayer.UserId,
                isDead
            };

            PhotonNetwork.RaiseEvent(chatEventCode, array, new RaiseEventOptions
            {
                TargetActors = new int[] { whisperTo.ActorNumber }
            }, SendOptions.SendReliable);
            logMessage($"(/w) to {returnNameWithColor(whisperTo)}: {msg}");
        }
        static void Whisper(string content)
        {
            var plr = content.Split(' ')[0];
            var msg = content.Substring(plr.Length);
            var playerToWhisperTo = returnPlayerFromString(plr);
            if (playerToWhisperTo == null) return;
            Whisper(playerToWhisperTo, msg);
        }
        public static void logMessage(string message)
        {
            var instance = PeakTextChat.TextChatDisplay.instance;
            if (instance != null)
            {
                instance.AddMessage(message);
            }
            else
            {
                tinyTweaks.log("instance could not be found");
            }
        }
        static string returnNameWithColor(Photon.Realtime.Player plr)
        {
            string name = "";
            var c = returnCharacter(plr);
            string color;
            if (c != null)
                color = ColorUtility.ToHtmlStringRGB(c.refs.customization.PlayerColor);
            else
                color = ColorUtility.ToHtmlStringRGB(new Color(0.64f, 0.69f, 0.83f));
            name += $"<color=#{color}>{plr.NickName}</color>";
            return name;
        }
        static Character returnCharacter(Photon.Realtime.Player player)
        {
            foreach (Character c in Character.AllCharacters)
            {
                PhotonView photonView = c.photonView;

                if (photonView != null || c != null && c.transform.Find("Scout") != null && c.enabled == true)
                {
                    if (photonView.Owner == player)
                        return c;
                }
            }
            return null;
        }
        static Photon.Realtime.Player returnPlayerFromString(string id)
        {
            foreach (Photon.Realtime.Player plr in PhotonNetwork.PlayerList)
            {
                string cleanName = Regex.Replace(plr.NickName.ToLower(), @"</?color(=\w+|=[#\w]+)?>", string.Empty, RegexOptions.IgnoreCase);
                if (cleanName.Contains(id, StringComparison.OrdinalIgnoreCase))
                    return plr;
            }
            logMessage("player not found!");
            return null;
        }
    }
}
