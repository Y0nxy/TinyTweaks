using BepInEx;
using BepInEx.Configuration;
using DG.Tweening.Plugins.Core;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class Shields
    {
        static ConfigEntry<bool> daggerShield;
        static ConfigEntry<bool> blowgunShield;

        public static void Start()
        {
            daggerShield = tinyTweaks.config.Bind("Shorties", "dagger Shield", false);
            blowgunShield = tinyTweaks.config.Bind("Shorties", "blowgun Shield", false);
        }
        [HarmonyPatch(typeof(CharacterAfflictions))]
        static class blockDaggerPatch
        {
            [HarmonyPatch("DieToRitualDagger")]
            [HarmonyPrefix]
            static bool block()
            {
                if (!daggerShield.Value) return true; //no blocking
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
        [HarmonyPatch]
        static class BlowgunShield
        {

            [HarmonyPatch(typeof(Action_RaycastDart), "RPC_DartImpact")]
            [HarmonyPrefix]
            static bool block(Action_RaycastDart __instance, int characterID, Vector3 endpoint, ref PhotonMessageInfo info)
            {
                if (!blowgunShield.Value) return true;//no need to block
                var c = Character.localCharacter;
                if (c == null || characterID != c.photonView?.ViewID) return true;
                if (!c.data.fullyConscious) return true;

                tinyTweaks.Notification("blocked blowgun from " + info.Sender.NickName);
                UnityEngine.Object.Instantiate<GameObject>(__instance.dartVFX, endpoint, Quaternion.identity);
                GamefeelHandler.instance.AddPerlinShakeProximity(endpoint, 5f, 0.2f, 15f, 10f);
                return false;
            }
        }

    }
}
