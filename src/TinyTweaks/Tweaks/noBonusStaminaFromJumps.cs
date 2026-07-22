using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.TextCore.Text;

namespace TinyTweaks.Tweaks
{
    internal class noBonusStaminaFromJumps
    {
        static ConfigEntry<bool> enableNoStamJump;

        public static void Start()
        {
            enableNoStamJump = Plugin.config.Bind("Shorties", "NoBonusStaminaFromJumps", false);
        }

        [HarmonyPatch]
        static class noBonusStamFromJumps
        {
            [HarmonyPatch(typeof(Character), "UseStamina")]
            [HarmonyPrefix]
            static bool NoBonusStaminaFromJumps(Character __instance, float usage, ref bool useBonusStamina)
            {
                if (!__instance.IsLocal || !useBonusStamina || !enableNoStamJump.Value) return true;
                //return false;
                if (__instance.data.isSprinting && __instance.data.sinceJump < 0.2f) useBonusStamina = false;
                return true;
            }
        }
    }
}
