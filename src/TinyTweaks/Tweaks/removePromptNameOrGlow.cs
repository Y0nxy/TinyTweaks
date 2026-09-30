using BepInEx.Configuration;
using HarmonyLib;
using Zorro.Core;

namespace TinyTweaks.Tweaks
{
    internal class removePromptNameOrGlow
    {
        private static ConfigEntry<bool> removeGlowItem;
        private static ConfigEntry<bool> removePickupText;
        private static ConfigEntry<bool> removePitonText;
        private static ConfigEntry<bool> removeRopeText;

        public static void Binds()
        {
            removeGlowItem = tinyTweaks.config.Bind("Interactions", "Remove Glow pickup", false);
            removePickupText = tinyTweaks.config.Bind("Interactions", "Remove pickup text", false);
            removePitonText = tinyTweaks.config.Bind("Interactions", "Remove piton text", false);
            removeRopeText = tinyTweaks.config.Bind("Interactions", "Remove rope text", false);
        }

        [HarmonyPatch]
        static class patch
        {
            [HarmonyPatch(typeof(GUIManager), "OnInteractChange")]
            [HarmonyPrefix]
            static bool disable(GUIManager __instance)
            {
                var hovered = Interaction.instance.currentHovered;
                //return true;
                if (hovered is RopeSegment && !removeRopeText.Value) return true;
                if (hovered is ClimbHandle && !removePitonText.Value) return true;
                if (hovered is not Item && hovered is not ClimbHandle && hovered is not RopeSegment) return true;
                if (__instance.currentInteractable.UnityObjectExists<IInteractible>())
                {
                    __instance.currentInteractable.HoverExit();
                }
                __instance.currentInteractable = Interaction.instance.currentHovered;
                if (__instance.currentInteractable.UnityObjectExists<IInteractible>())
                {
                    if (!removeGlowItem.Value)
                        __instance.currentInteractable.HoverEnter();
                }
                if (!removePickupText.Value)
                    __instance.RefreshInteractablePrompt();
                return false;
            }
        }
    }
}
