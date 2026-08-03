using BepInEx.Configuration;
using HarmonyLib;
using pworld.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using TinyTweaks;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class ChangeRopeToRed
    {
        static ConfigEntry<bool> enableRedRope;
        public static void Start()
        {
            enableRedRope = tinyTweaks.config.Bind("Customization", "Red Rope", true);
            tinyTweaks.log("ChangeRopeToRed plugin loaded");
        }
        [HarmonyPatch]
        static class changeRope
        {
            [HarmonyPatch(typeof(RopeAnchorWithRope), "SpawnRope")]
            [HarmonyPrefix]
            static void Prefix(RopeAnchorWithRope __instance)
            {
                if (!enableRedRope.Value) return;
                if (__instance.ropePrefab.name.Contains("Anti"))
                {
                    tinyTweaks.log("Rope anchor has antigravity, skipping rope change");
                    return;
                }
                __instance.ropePrefab = Resources.Load<GameObject>("RopeDynamicBreakable");
                //__instance.ropeSegmentLength = 100f;
                tinyTweaks.log("Rope prefab changed to breakable version");
            }

            [HarmonyPatch(typeof(RopeSpool), "Awake")]
            [HarmonyPrefix]
            static void changeRopeSpool(RopeSpool __instance)
            {
                if (!enableRedRope.Value) return;
                if (__instance.GetComponent<Antigrav>() != null)
                {
                    tinyTweaks.log("Rope spool has antigravity, skipping rope change");
                    return;
                }
                __instance.ropePrefab = Resources.Load<GameObject>("RopeDynamicBreakable");
                tinyTweaks.log("Rope prefab changed to breakable version");
                __instance.GetComponent<RopeTier>().anchorPrefab = Resources.Load<GameObject>("RopeAnchorWithRopeBreakable");
            }
        }
    }
}
