using BepInEx;
using BepInEx.Configuration;
using DG.Tweening.Plugins.Core;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinyTweaks.Tweaks
{
    internal class daggerShieldPatch
    {
        static ConfigEntry<bool> daggerShield;

        public static void Start()
        {
            daggerShield = tinyTweaks.config.Bind("Shorties", "dagger Shield", false);
        }
        [HarmonyPatch(typeof(CharacterAfflictions))]
        static class blockDaggerPatch
        {
            [HarmonyPatch("DieToRitualDagger")]
            [HarmonyPrefix]
            static bool block()
            {
                if (!daggerShield.Value) return true;
                tinyTweaks.Notification("blocked stabbie stab!");
                return false;
            }
            [HarmonyPatch("RPC_PetrifyInstantly")]
            [HarmonyPrefix]
            static void blockPetrifyRequest(CharacterAfflictions __instance, bool killedByDagger, ref PhotonMessageInfo info)
            {
                if (info.Sender != __instance.photonView.Owner)
                {
                    tinyTweaks.Notification($"{info.Sender.NickName} tried to petrify player {__instance.name} by cheats!");
                }
            }
        }

    }
}
