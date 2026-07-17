using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using System;

namespace TinyTweaks.Tweaks
{
    public class Customizations
    {
        public static ConfigEntry<bool> NoobSash;
        public static ConfigEntry<bool> DeadEyes;
        public static ConfigEntry<bool> passedOutEyes;
        public static ConfigEntry<bool> NormalEyes;
        public static void Start()
        {
            var config = Plugin.config;
            NoobSash = config.Bind("Customization", "Noob Sash", false, "Toggle the Noob Sash (hides badges) on/off.");
            DeadEyes = config.Bind("Customization", "Dead Eyes", false, "Toggle the Dead Eyes on/off.");
            passedOutEyes = config.Bind("Customization", "Passed Out Eyes", false, "Toggle the Passed Out Eyes on/off.");
            NormalEyes = config.Bind("Customization", "Normal Eyes", false, "Toggle the Normal Eyes on/off.");
            NoobSash.SettingChanged += (_, _) =>
            {
                Plugin.Notification("Noob Sash is " + (NoobSash.Value ? "ON" : "OFF"));
                CharacterData localCharacterData = Character.localCharacter?.GetComponent<CharacterData>();
                if (localCharacterData != null)
                {
                    AccessTools.Method(typeof(CharacterData), "SetBadgeStatus").Invoke(localCharacterData, null);
                }
            };
            DeadEyes.Value = false;
            passedOutEyes.Value = false;
            NormalEyes.Value = false;
            DeadEyes.SettingChanged += (_, _) =>
            {
                Plugin.Notification("Dead Eyes is " + (DeadEyes.Value ? "ON" : "OFF"));
                if (!DeadEyes.Value) return; // only trigger when turning on
                DeadEyes.Value = false;
                passedOutEyes.Value = false; // turn off passed out eyes if dead eyes is turned on
                NormalEyes.Value = false;
                Character local = Character.localCharacter;
                if (local != null)
                {
                    local.photonView.RPC("CharacterDied", RpcTarget.AllBuffered, Array.Empty<object>());
                }
            };
            passedOutEyes.SettingChanged += (_, _) =>
            {
                Plugin.Notification("Passed Out Eyes is " + (passedOutEyes.Value ? "ON" : "OFF"));
                if (!passedOutEyes.Value) return; // only trigger when turning on
                passedOutEyes.Value = false;
                DeadEyes.Value = false; // turn off dead eyes if passed out eyes is turned on
                NormalEyes.Value = false;
                Character local = Character.localCharacter;
                if (local != null)
                {
                    local.photonView.RPC("CharacterPassedOut", RpcTarget.AllBuffered, Array.Empty<object>());
                }
            };
            NormalEyes.SettingChanged += (_, _) =>
            {
                Plugin.Notification("Normal Eyes is " + (NormalEyes.Value ? "ON" : "OFF"));
                if (!NormalEyes.Value) return; // only trigger when turning on
                NormalEyes.Value = false;
                DeadEyes.Value = false; // turn off dead eyes if normal eyes is turned on
                passedOutEyes.Value = false; // turn off passed out eyes if normal eyes is turned on
                Character local = Character.localCharacter;
                if (local != null)
                {
                    local.photonView.RPC("OnRevive_RPC", RpcTarget.AllBuffered, Array.Empty<object>());
                }
            };
        }
        [HarmonyPatch(typeof(CharacterData), "SetBadgeStatus")]
        public static class NoobSashPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(CharacterData __instance)
            {
                if (!NoobSash.Value) return true; // if Noob Sash is off, let the original method run
                if (!__instance.GetComponent<Character>().IsLocal) return true; //not me, og method runs
                __instance.badgeStatus = new bool[GUIManager.instance.mainBadgeManager.badgeData.Length];
                for (int i = 0; i < __instance.badgeStatus.Length; i++)
                {
                    __instance.badgeStatus[i] = false;
                }
                __instance.photonView.RPC("SyncBadgeStatus", RpcTarget.All, new object[] { __instance.badgeStatus });
                return false; // skip original method to prevent it from overwriting our changes
            }
        }

    }
}
