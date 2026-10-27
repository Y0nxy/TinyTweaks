using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class Shields
    {
        static ConfigEntry<bool> daggerShield;
        static ConfigEntry<bool> blowgunShield;
        static ConfigEntry<bool> rescueClawShield;
        static float timeSinceLastClawOnME = 999;

        public static void Start()
        {
            daggerShield = tinyTweaks.config.Bind("Shields", "dagger Shield", false);
            blowgunShield = tinyTweaks.config.Bind("Shields", "blowgun Shield", false);
            rescueClawShield = tinyTweaks.config.Bind("Shields", "rescue claw Shield", false);
        }
        [HarmonyPatch]
        static class shieldPatches
        {
            #region blockDaggerPatch
            [HarmonyPatch(typeof(CharacterAfflictions), "DieToRitualDagger")]
            [HarmonyPrefix]
            static bool block()
            {
                if (!daggerShield.Value) return true; //no blocking
                tinyTweaks.Notification("blocked stabbie stab!");
                return false;
            }
            [HarmonyPatch(typeof(CharacterAfflictions), "RPC_PetrifyInstantly")]
            [HarmonyPrefix]
            static void blockPetrifyRequest(CharacterAfflictions __instance, bool killedByDagger, ref PhotonMessageInfo info)
            {
                if (info.Sender != __instance.photonView.Owner)
                {
                    tinyTweaks.Notification($"{info.Sender.NickName} tried to petrify player {__instance.name} by cheats!");
                }
            }
            #endregion
            #region BlowGun Shield
            [HarmonyPatch(typeof(Action_RaycastDart), "RPC_DartImpact")]
            [HarmonyPrefix]
            static bool block(Action_RaycastDart __instance, int characterID, Vector3 endpoint, ref PhotonMessageInfo info)
            {
                if (!blowgunShield.Value) return true;//no need to block
                var c = Character.localCharacter;
                if (c == null || characterID != c.photonView?.ViewID) return true;
                if (!c.data.fullyConscious) return true;

                tinyTweaks.Notification("blocked blowgun from " + info.Sender.NickName);
                UnityEngine.Object.Instantiate<GameObject>(__instance.dartVFX, endpoint, Quaternion.identity);
                GamefeelHandler.instance.AddPerlinShakeProximity(endpoint, 5f, 0.2f, 15f, 10f);
                return false;
            }
            #endregion
            #region rescueClaw Shield
            [HarmonyPatch(typeof(RescueHook), nameof(RescueHook.RPCA_RescueCharacter))]
            [HarmonyPrefix]
            static bool rescueShield(RescueHook __instance , PhotonView characterView, ref PhotonMessageInfo info)
            {
                if (!rescueClawShield.Value) return true;
                var localC = Character.localCharacter;
                if (localC?.data == null || localC.data.dead || !localC.data.fullyConscious) return true;
                if (characterView != localC.photonView) return true;
                timeSinceLastClawOnME = 0;
                tinyTweaks.Notification("blocked rescue claw from " + info.Sender.NickName);
                //play sound
                for (int i = 0; i < __instance.rescueHit.Length; i++)
                {
                    __instance.rescueHit[i].Play(localC.Center);
                }
                __instance.photonView.RPC(nameof(RescueHook.RPCA_LetGo), RpcTarget.All, new object[] { });//remove hook
                return false;
            }
            [HarmonyPatch(typeof(Character), nameof(Character.RPCA_AddForceToBodyPart))]
            [HarmonyPrefix]
            static bool blockForce(Character __instance, ref PhotonMessageInfo info)
            {
                if (!rescueClawShield.Value || !__instance.IsLocal) return true;
                //tinyTweaks.Notification($"{info.Sender.NickName} Applied force on me");
                if (timeSinceLastClawOnME < 2.5f)
                    return false;
                return true;
            }
            [HarmonyPatch(typeof(Character), nameof(Character.RPCA_Fall))]
            [HarmonyPrefix]
            static bool blockFall(Character __instance)//, ref PhotonMessageInfo info)
            {
                if (!rescueClawShield.Value || !__instance.IsLocal) return true;
                if (timeSinceLastClawOnME < 1) return false;
                return true;
            }

            #endregion
        }
        public static void UpdateTimer() => timeSinceLastClawOnME += Time.deltaTime;
    }
}
