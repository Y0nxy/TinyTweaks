using BepInEx.Configuration;
using HarmonyLib;
using pworld.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    public class showNamesAlways
    {
        static ConfigEntry<bool> AlwaysShowNames;
        static ConfigEntry<float> VisibleAngle;
        static ConfigEntry<bool> DisplayWhenBlind;

        public static void Start()
        {
            var config = Plugin.config;
            AlwaysShowNames = config.Bind("Tweaks", "Always Show Names", true);
            //min 0, max 360
            VisibleAngle = config.Bind("Tweaks", "Visible Angle", 52f,
                new ConfigDescription("Visible Angle up to 360 deg",
                new AcceptableValueRange<float>(0f, 360f)));
            DisplayWhenBlind = config.Bind("Tweaks", "Show When Blind", false);
        }
        [HarmonyPatch(typeof(IsLookedAt))]
        public static class IsLookedAtPatches
        {
            [HarmonyPatch("Update")]
            [HarmonyPrefix]
            public static bool IsLookAtUpdatePath(IsLookedAt __instance)
            {
                if (!AlwaysShowNames.Value) //AlwaysShowNames
                {
                    return true;
                }

                if (__instance.playerNamePos == null || __instance.mouth == null)
                {
                    return true; // Avoid crashing on uninitialized/destroyed targets
                }

                var visible = false;
                var angle = Vector3.Angle(MainCamera.instance.transform.forward, __instance.transform.position - MainCamera.instance.transform.position);

                if (angle < VisibleAngle.Value) //VisibleAngle
                {
                    visible = true;
                }
                if (__instance.mouth.character.data.isBlind)
                {
                    visible = DisplayWhenBlind.Value; //DisplayWhenBlind
                }

                var indexField = AccessTools.Field(typeof(IsLookedAt), "index");
                if (indexField == null) return true;
                var index = (int)indexField.GetValue(__instance);
                GUIManager.instance.playerNames.UpdateName(index, __instance.playerNamePos.position, visible, __instance.mouth.amplitudeIndex);
                return false;
            }
        }
    }
}
