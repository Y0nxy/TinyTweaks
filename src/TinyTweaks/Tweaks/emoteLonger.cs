using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class emoteLonger
    {
        //const string sitEmote = "A_Scout_Emote_Sit";
        static readonly string[] emotesToKeepLonger = new string[] { "A_Scout_Emote_Sit", "A_Scout_Emote_Panic"};
        private static readonly Dictionary<CharacterAnimations, string> emotingPlayers = new Dictionary<CharacterAnimations, string>();
        
        [HarmonyPatch]
        private class Patcher
        {
            [HarmonyPatch(typeof(Character), "CreateHelperObjects")]
            [HarmonyPostfix]
            public static void CharacterUpdateAnims(Character __instance)
            {
                List<KeyValuePair<AnimationClip, AnimationClip>> clips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                AnimatorOverrideController overrideController = new AnimatorOverrideController();
                overrideController.runtimeAnimatorController = __instance.refs.animator.runtimeAnimatorController;
                //foreach (AnimationClip originalClip in overrideController.animationClips.Where<AnimationClip>((AnimationClip o) => o.name.Equals(sitEmote)))
                foreach (AnimationClip originalClip in overrideController.animationClips.Where<AnimationClip>((AnimationClip o) => emotesToKeepLonger.Contains(o.name)))
                {
                    AnimationClip newClip = UnityEngine.Object.Instantiate<AnimationClip>(originalClip);
                    newClip.wrapMode = WrapMode.Loop;
                    clips.Add(new KeyValuePair<AnimationClip, AnimationClip>(originalClip, newClip));
                }
                overrideController.ApplyOverrides(clips);
                __instance.refs.animator.runtimeAnimatorController = overrideController;
            }

            // Capture which emote is currently being triggered
            [HarmonyPatch(typeof(CharacterAnimations), "RPCA_PlayRemove")]
            [HarmonyPrefix]
            public static void TrackEmote(CharacterAnimations __instance, string emoteName)
            {
                emotingPlayers[__instance] = emoteName;
            }

            // if sit and not moving, don't continue the sinceEmoteStart, so it doesn't stop
            [HarmonyPatch(typeof(CharacterAnimations), "Update")]
            [HarmonyPrefix]
            public static void CharacterAnimationsUpdatePostfix(CharacterAnimations __instance)
            {
                if (!__instance.emoting) return;
                //if (!sittingPlayers.ContainsKey(__instance) || !sittingPlayers[__instance].Equals(sitEmote)) return;
                if (!emotingPlayers.ContainsKey(__instance) || !emotesToKeepLonger.Contains(emotingPlayers[__instance])) return;
                var c = __instance.character;
                if (c.input.movementInput.magnitude > 0.1f || c.input.jumpWasPressed || c.data.sinceGrounded > 0.2f) return;
                __instance.sinceEmoteStart = 0;
            }
        }
    }
}
