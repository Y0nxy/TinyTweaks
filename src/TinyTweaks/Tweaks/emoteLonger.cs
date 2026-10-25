using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

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
            [HarmonyPatch(typeof(CharacterAnimations), "RPCA_PlayRemove")]
            [HarmonyPrefix]
            public static void TrackEmote(CharacterAnimations __instance, string emoteName)
            {
                if (emotesToKeepLonger.Contains(emoteName))
                    emotingPlayers[__instance] = emoteName;
                else
                {
                    if (emotingPlayers.ContainsKey(__instance))
                        emotingPlayers.Remove(__instance);
                }
            }

            // if sit and not moving, don't continue the sinceEmoteStart, so it doesn't stop
            [HarmonyPatch(typeof(CharacterAnimations), "Update")]
            [HarmonyPrefix]
            public static void CharacterAnimationsUpdatePostfix(CharacterAnimations __instance)
            {
                if (!__instance.emoting) return;
                //if (!sittingPlayers.ContainsKey(__instance) || !sittingPlayers[__instance].Equals(sitEmote)) return;
                //if (!emotingPlayers.ContainsKey(__instance) || !emotesToKeepLonger.Contains(emotingPlayers[__instance])) return;
                if (!emotingPlayers.ContainsKey(__instance)) return;
                var c = __instance.character;
                if (c.input.movementInput.magnitude > 0.1f || c.input.jumpWasPressed || c.data.sinceGrounded > 0.2f) return;
                __instance.sinceEmoteStart = 0;
            }
        }
    }
}
