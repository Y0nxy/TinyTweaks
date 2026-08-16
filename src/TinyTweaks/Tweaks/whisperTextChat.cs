//using PeakTextChat;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using ExitGames.Client;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using SCPE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class whisperTextChat
    {
        static ConfigEntry<bool> enableWhisperTextChat;
        static ConfigEntry<bool> whisperForYouEveryMsg;
        public static void CheckforPeakTextChat(Harmony harmony)
        {
            enableWhisperTextChat = tinyTweaks.config.Bind("Shorties", "WhisperCmd", true);
            whisperForYouEveryMsg = tinyTweaks.config.Bind("Shorties", "send a (secret msg for you) everytime", true);
            if (Chainloader.PluginInfos.ContainsKey("com.borealityy.peaktextchat"))
            {
                tinyTweaks.log("found peaktextchat. Initializing patch!");
                var sendChatMessageMethod = AccessTools.Method("PeakTextChat.TextChatManager:SendChatMessage");
                var SlashCommandMethod = new HarmonyMethod(AccessTools.Method(typeof(InterceptMessage), ("SlashCommand")));
                if (sendChatMessageMethod == null || SlashCommandMethod == null)
                {
                    tinyTweaks.log(sendChatMessageMethod==null?"didn't find peaktextchat method":"didn't find whisper method");
                    return;
                }
                harmony.Patch(sendChatMessageMethod,prefix:SlashCommandMethod);
            }
        }

        static byte chatEventCode = 81;
        static class InterceptMessage
        {
            //[HarmonyPatch(typeof(TextChatManager), "SendChatMessage")]
            [HarmonyPrefix]
            static bool SlashCommand(string message)
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
            //var logMethod = AccessTools.Method("PeakTextChat.TextChatDisplay.instance:AddMessage");
            //if (logMethod == null) {
            //    Plugin.log("logMethod could not be found");
            //    return;
            //}
            //logMethod.Invoke(message);
            var logMethod = AccessTools.Method("PeakTextChat.TextChatDisplay:AddMessage", new Type[] { typeof(string) });
            Type type = AccessTools.TypeByName("PeakTextChat.TextChatDisplay");
            var instance = type != null ? AccessTools.Field(type, "instance")?.GetValue(null) : null;


            if (logMethod != null && instance != null)
            {
                logMethod.Invoke(instance, new object[] { message });
            }
            else
            {
                tinyTweaks.log("logMethod could not be found");
            }
            //PeakTextChat.TextChatDisplay.instance.AddMessage(message);
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
