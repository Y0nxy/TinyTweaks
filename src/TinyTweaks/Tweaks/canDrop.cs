using HarmonyLib;

namespace TinyTweaks.Tweaks
{
    internal class canDrop
    {
        [HarmonyPatch]
        public static class ItemPatches
        {
            #region canDrop And Throw Items
            [HarmonyPatch(typeof(CharacterData), nameof(CharacterData.currentItem), MethodType.Setter)]
            [HarmonyPostfix]
            static void canDropItems(CharacterData __instance, Item value)
            {
                if (value == null) return;
                if (!__instance.character.IsLocal) return;

                if (value.UIData.canThrow == false || value.UIData.canDrop == false)
                {
                    value.UIData.canThrow = true;
                    value.UIData.canDrop = true;
                }
            }
            #endregion
            #region make Backpack Pickup inside other backpacks possible
            [HarmonyPatch(typeof(BackpackWheel), nameof(BackpackWheel.Choose))]
            [HarmonyPrefix]
            static bool pickupBackpack(BackpackWheel __instance)
            {
                if (!__instance.chosenSlice.IsSome) return true;
                BackpackWheelSlice.SliceData sliceData = __instance.chosenSlice.Value;
                if (!sliceData.backpackReference.exists)
                {
                    tinyTweaks.logMessage("Slice Data doesnt Exist");
                    return true;
                }
                if (__instance.chosenSlice.Value.isBackpackWear) return true;
                sliceData = __instance.chosenSlice.Value;
                Item item;
                if (sliceData.backpackReference.GetVisuals().TryGetSpawnedItem(__instance.chosenSlice.Value.slotID, out item))
                {
                    //tinyTweaks.logMessage("try interact");
                    if (item is not Backpack) return true;
                    Backpack backpack = item.GetComponent<Backpack>();
                    var character = Character.localCharacter;
                    if (character != null)
                    {
                        //tinyTweaks.logMessage("backpack");
                        backpack.Wear(character);
                        //backpack.Interact(character);
                        //character.refs.items.EquipSlot(Zorro.Core.Optionable<byte>.None);
                        character.refs.items.EquipSlot(Zorro.Core.Optionable<byte>.Some(3));
                    }
                    return false;
                }
                return true;
            }
            #endregion
        }
    }
}
