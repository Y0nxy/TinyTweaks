using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using SCPE;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.Rendering.STP;

namespace TinyTweaks.Tweaks
{
    internal class BingBongSays
    {
        public static ConfigEntry<float> bingBongAnswer = null!;
        public static ConfigEntry<bool> enableBingBongPatch = null!;
        static ConfigFile config;

        public static void Binds()
        {
            config = Plugin.config;
            enableBingBongPatch = config.Bind("Actions!", "BingBong", true);
            bingBongAnswer = config.Bind("Actions!", "BingBongAnswer(rounded down)", 0f, new ConfigDescription("The answer to the Bing Bong action.",
                        new AcceptableValueRange<float>(0, 30)));

            bingBongAnswer.SettingChanged += (sender, e) =>
            {
                if (bingBongAnswer.Value % 1f != 0)
                {
                    Plugin.Log.LogWarning($"BingBongAnswer value {bingBongAnswer.Value} is not an integer. Rounding down to nearest integer.");
                    bingBongAnswer.Value = Mathf.FloorToInt(bingBongAnswer.Value);
                }
            };
        }
        [HarmonyPatch(typeof(Action_AskBingBong), "RunAction")]
        public static class BingBongPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(Action_AskBingBong __instance)
            {
                if (!enableBingBongPatch.Value) return true;
                if (bingBongAnswer.Value >= __instance.responses.Length)
                {
                    bingBongAnswer.Value = __instance.responses.Length - 1;
                    bingBongAnswer = config.Bind("Actions!", "BingBongAnswer(rounded down)", bingBongAnswer.Value, new ConfigDescription("The answer to the Bing Bong action. Min: 0, Max: " + (__instance.responses.Length - 1),
                        new AcceptableValueRange<float>(0, __instance.responses.Length - 1)));
                    config.Save();
                    bingBongAnswer.SettingChanged += (sender, e) =>
                    {
                        if (bingBongAnswer.Value % 1f != 0)
                        {
                            Plugin.Log.LogWarning($"BingBongAnswer value {bingBongAnswer.Value} is not an integer. Rounding down to nearest integer.");
                            bingBongAnswer.Value = Mathf.FloorToInt(bingBongAnswer.Value);
                        }
                    };
                }
                int answer = Mathf.FloorToInt(bingBongAnswer.Value);
                __instance.item.photonView.RPC("Ask", RpcTarget.All, new object[]
                {
                    answer,
                    Time.time < __instance.lastAsked + 1f
                });

                return false;
            }
        }
    }
}
