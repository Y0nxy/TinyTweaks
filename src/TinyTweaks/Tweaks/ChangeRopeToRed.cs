using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class ChangeRopeToRed
    {
        static ConfigEntry<bool> enableRedRope;
        static ConfigEntry<bool> alwaysRopeInAirport;
        public static void Start()
        {
            enableRedRope = tinyTweaks.config.Bind("Customization", "Red Rope", true);
            alwaysRopeInAirport = tinyTweaks.config.Bind("Shorties", "Always Rope in Airport", true);
            tinyTweaks.log("ChangeRopeToRed plugin loaded");
        }

        public static void ropeAirport()
        {
            var rope = tinyTweaks.FindFirstObjectByType<RopeAnchorWithRope>();
            if (rope != null)
            {
                var old_rope = tinyTweaks.FindFirstObjectByType<Rope>();

                if (enableRedRope.Value)
                {
                    Transform t = rope.transform;
                    var RedRope = PhotonNetwork.Instantiate("RopeAnchorWithRopeBreakable", t.position, t.rotation).GetComponent<RopeAnchorWithRope>();
                    RedRope.ropeSegmentLength = 30f;
                    if (RedRope.TryGetComponent<BreakableRopeAnchor>(out var breakable))
                    {
                        breakable.isBreaking = true; //make it unbreakable
                    }
                    if (rope?.photonView != null)
                    {
                        PhotonNetwork.Destroy(rope.gameObject);
                    }
                    if (alwaysRopeInAirport.Value || old_rope != null)
                        RedRope.SpawnRope();
                    if (old_rope?.photonView != null)
                    {
                        PhotonNetwork.Destroy(old_rope.gameObject);
                    }
                    return;
                }

                if (old_rope == null && alwaysRopeInAirport.Value)
                {
                    rope.ropeSegmentLength = 30f;
                    rope.SpawnRope();
                }
                //tinyTweaks.logMessage("rope length is :" + RedRope.ropeSegmentLength);
            }
        }
        [HarmonyPatch]
        static class changeRope
        {
            [HarmonyPatch(typeof(RopeAnchorWithRope), nameof(RopeAnchorWithRope.SpawnRope))]
            [HarmonyPrefix]
            static void Prefix(RopeAnchorWithRope __instance)
            {
                if (!enableRedRope.Value) return;
                //tinyTweaks.Notification($"name:[{__instance.ropePrefab.name}] SpawnRope");
                //tinyTweaks.log($"name:[{__instance.ropePrefab.name}] SpawnRope");
                if (__instance.ropePrefab.name.Contains("Anti"))
                {
                    tinyTweaks.log("Rope anchor has antigravity, skipping rope change");
                    return;
                }
                if (__instance.ropePrefab.name.Contains("Helicopter"))
                {
                    return;
                    //tinyTweaks.log($"Helicopter rope, length is :{__instance.ropeSegmentLength}");
                    //tinyTweaks.Notification($"Helicopter rope, length is :{__instance.ropeSegmentLength}");
                    //__instance.ropeSegmentLength = 50f;
                    //tinyTweaks.log($"Helicopter rope, length is now :{__instance.ropeSegmentLength}");
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
                //tinyTweaks.Notification($"name:[{__instance.ropePrefab.name}] rope spool awake");
                //tinyTweaks.log($"name:[{__instance.ropePrefab.name}] rope spool awake");
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
