using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TinyTweaks.Tweaks
{
    internal class canDrop
    {
        [HarmonyPatch(typeof(CharacterData), nameof(CharacterData.currentItem), MethodType.Setter)]
        public static class CurrentItemPatch
        {
            [HarmonyPostfix]
            static void Postfix(CharacterData __instance, Item value)
            {
                if (value == null) return;
                if (!__instance.character.IsLocal) return;

                if (value.UIData.canThrow == false || value.UIData.canDrop == false)
                {
                    value.UIData.canThrow = true;
                    value.UIData.canDrop = true;
                }
            }
        }
    }
}
