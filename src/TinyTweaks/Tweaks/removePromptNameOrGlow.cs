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
        private static ConfigEntry<bool> removeBoothText;
        private static ConfigEntry<bool> removeLuggageText;
        private static ConfigEntry<bool> removeAll;

        public static void Binds()
        {
            removeGlowItem = tinyTweaks.config.Bind("Interactions", "Remove Glow pickup", false);
            removePickupText = tinyTweaks.config.Bind("Interactions", "Remove pickup text", false);
            removePitonText = tinyTweaks.config.Bind("Interactions", "Remove piton text", false);
            removeRopeText = tinyTweaks.config.Bind("Interactions", "Remove rope text", false);
            removeBoothText = tinyTweaks.config.Bind("Interactions", "Remove booth text", false);
            removeLuggageText = tinyTweaks.config.Bind("Interactions", "Remove Luggage text", false);
            removeAll = tinyTweaks.config.Bind("Interactions", "Remove All", false);
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
                if ((removeAll.Value && hovered is not null) ||
                    (hovered is RopeSegment && removeRopeText.Value)||
                    (hovered is ClimbHandle && removePitonText.Value)||
                    (removeBoothText.Value &&
                        hovered is AirportCheckInKiosk ||
                        hovered is AirportInviteFriendsKiosk ||
                        hovered is AirportPhotoboothKiosk)||
                    (hovered is Item) ||
                    (hovered is Luggage && removeLuggageText.Value))
                {
                    hide(__instance);
                    return false;
                }
                return true;
            }
        }
        static void hide(GUIManager manager)
        {
            if (manager.currentInteractable.UnityObjectExists<IInteractible>())
            {
                manager.currentInteractable.HoverExit();
            }
            manager.currentInteractable = Interaction.instance.currentHovered;
            if (manager.currentInteractable.UnityObjectExists<IInteractible>())
            {
                if (!removeGlowItem.Value)
                    manager.currentInteractable.HoverEnter();
            }
            if (!removePickupText.Value)
                manager.RefreshInteractablePrompt();
        }
    }
}
