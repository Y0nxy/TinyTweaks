using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    public class ExtraMarshmallows
    {
        //Configs
        static ConfigEntry<bool> enableExtraMarshmallows;
        static ConfigEntry<bool> enableExtraBackpacks;
        static ConfigEntry<bool> enableCampfireProtection;
        static ConfigEntry<float> hotdogPercent;

        //other stuff
        public static Campfire nextCampfire;
        static List<GameObject> marshmallows = new List<GameObject>();
        static List<PhotonView> charactersThatPickedUp = new List<PhotonView>();
        static float radius = 2f;
        static int playerCount => PhotonNetwork.CurrentRoom.PlayerCount;
        static Vector3 originPoint => nextCampfire.transform.position;
        static int marshmallowsTaken = 0;
        static bool isMasterAndEnabled => PhotonNetwork.IsMasterClient && enableExtraMarshmallows.Value;

        //Awake from every Campfire
        //Taken = 0 on any new campfire
        public static void Start()
        {
            enableExtraMarshmallows = Plugin.config.Bind("Campfire", "Extra Marshmallows", true);
            enableExtraBackpacks = Plugin.config.Bind("Campfire", "Extra Backpacks", false);
            enableCampfireProtection = Plugin.config.Bind("Campfire", "Campfire Protection", true);
            hotdogPercent = Plugin.config.Bind("Campfire", "Hotdog spawn chance", 33f, new ConfigDescription("from 0 to 100%", new AcceptableValueRange<float>(0f, 100f)));
        }
        [HarmonyPatch(typeof(MapHandler), "SpawnCampfireItems")]
        private class SetCampfire
        {
            [HarmonyPrefix]
            public static bool Prefix(GameObject campfireRoot, bool skipMallows)
            {
                if (!campfireRoot || skipMallows) return true;

                nextCampfire = campfireRoot.GetComponentInChildren<Campfire>();
                if (nextCampfire == null)
                {
                    Plugin.Log.LogError("Campfire Component not found");
                    return true;
                }
                if (enableCampfireProtection.Value)
                    nextCampfire.gameObject.AddComponent<CampfireProtection>();

                if (!PhotonNetwork.IsMasterClient) return true;
                if (enableExtraBackpacks.Value)
                {
                    for (int i = 0; i < playerCount - marshmallowsTaken; i++)
                    {
                        SpawnExtraBackpacks(i);
                    }
                }
                if (!enableExtraMarshmallows.Value) return true;
                Plugin.log("Skipping Spawning campfire items for " + campfireRoot.gameObject.name);
                marshmallows.Clear(); //leaving the old marshmallows be as is
                marshmallowsTaken = 0;
                charactersThatPickedUp.Clear();
                RefreshMarshmallows();
                return false;
            }
        }
        [HarmonyPatch(typeof(PlayerConnectionLog))]
        private class PlayerConnectionLogPatches
        {
            [HarmonyPatch("OnPlayerEnteredRoom")]
            [HarmonyPostfix]
            static void OnPlayerEnter()
            {
                if (!isMasterAndEnabled) return;
                Plugin.log("Someone joined! Updated Marshmallows to:" + playerCount.ToString());
                RefreshMarshmallows();
            }
            [HarmonyPatch("OnPlayerLeftRoom")]
            [HarmonyPostfix]
            private static void OnPlayerLeft()
            {
                if (!isMasterAndEnabled) return;
                Plugin.log("Someone left! Updated Marshmallows to:" + playerCount.ToString());
                RefreshMarshmallows();
            }
        }
        [HarmonyPatch(typeof(Item))]
        private class MarshmallowProtection
        {
            [HarmonyPatch("RequestPickup")]
            [HarmonyPrefix]
            private static bool onRequestPickup(Item __instance, PhotonView characterView)
            {
                if (!PhotonNetwork.IsMasterClient|| !enableExtraMarshmallows.Value) return true;
                if (!marshmallows.Contains(__instance.gameObject)) return true;
                if (charactersThatPickedUp.Contains(characterView))
                {
                    Plugin.log(characterView.name + " tried taking another Marshmallow! Unbelieveable...");
                    __instance.view.RPC("DenyPickupRPC", characterView.Owner, Array.Empty<object>());
                    return false;
                }
                charactersThatPickedUp.Add(characterView);
                marshmallows.Remove(__instance.gameObject);
                return true;
            }
            [HarmonyPatch("SetKinematicRPC")]
            [HarmonyPostfix]
            static void MarshmallowStandInPlace(Item __instance, bool value, Vector3 position, Quaternion rotation)
            {
                if (!isMasterAndEnabled) return;
                if (marshmallows.Contains(__instance.gameObject))
                {
                    if (!value)
                        __instance.SetKinematicNetworked(true,__instance.transform.position, __instance.transform.rotation);
                }
            }
        }

        static void RefreshMarshmallows()
        {
            if (nextCampfire == null) return;
            Vector3 originPoint = nextCampfire.transform.position;
            foreach (GameObject obj in marshmallows)
            {
                if (obj == null)
                {
                    marshmallowsTaken++;
                    continue;
                }
                PhotonView pv = obj.GetPhotonView();
                if (pv == null) continue;
                if (!pv.IsMine) pv.RequestOwnership();
                PhotonNetwork.Destroy(obj);
            }
            marshmallows.Clear();
            Plugin.log($"RefreshMarshmallows Called! Taken: {marshmallowsTaken}/{playerCount}");
            for (int i = 0; i < playerCount - marshmallowsTaken; i++)
            {
                SpawnMarshmallow(i);
            }
        }
        static void SpawnMarshmallow(int partOfCircle)
        {
            // Calculate the angle for this specific marshmallow
            // We use 2 * Mathf.PI to represent a full circle in radians
            float angle = partOfCircle * (2 * Mathf.PI / playerCount);

            // Calculate the X and Z position (assuming Y is up)
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Create the spawn position relative to the origin
            Vector3 spawnPosition = originPoint + new Vector3(x, 0, z);
            spawnPosition.y = CastToFloor(spawnPosition);
            Vector3 directionToCenter = originPoint - spawnPosition;

            // We force Y to be 0 so they don't tilt up/down if your campfire is on a slope.
            directionToCenter.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToCenter) * Quaternion.Euler(0, -90, 0);
            string itemToSpawn = "0_Items/Marshmallow";
            if (UnityEngine.Random.value <= hotdogPercent.Value / 100) itemToSpawn = "0_Items/Glizzy";
            GameObject itemObj = PhotonNetwork.InstantiateRoomObject(itemToSpawn, spawnPosition, lookRotation);
            var item = itemObj.GetComponent<Item>();
            if (item != null) item.SetKinematicNetworked(true);
            marshmallows.Add(itemObj);
        }
        static void SpawnExtraBackpacks(int partOfCircle)
        {
            // Calculate the angle for this specific marshmallow
            // We use 2 * Mathf.PI to represent a full circle in radians
            float angle = partOfCircle * (2 * Mathf.PI / playerCount);

            // Calculate the X and Z position (assuming Y is up)
            float x = Mathf.Cos(angle) * radius * 2;
            float z = Mathf.Sin(angle) * radius * 2;

            // Create the spawn position relative to the origin
            Vector3 spawnPosition = originPoint + new Vector3(x, 0, z);
            spawnPosition.y = CastToFloor(spawnPosition);
            Vector3 directionToCenter = originPoint - spawnPosition;

            // We force Y to be 0 so they don't tilt up/down if your campfire is on a slope.
            directionToCenter.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToCenter) * Quaternion.Euler(0, -90, 0);
            string itemToSpawn = "0_Items/Backpack";
            GameObject itemObj = PhotonNetwork.InstantiateRoomObject(itemToSpawn, spawnPosition, lookRotation);
        }
        static float CastToFloor(Vector3 spawnPosition)
        {
            RaycastHit floorhit;
            if (Physics.Raycast(spawnPosition + Vector3.up * 5, Vector3.down, out floorhit, 10f))
            {
                 return floorhit.point.y;
            }
            return spawnPosition.y;
        }
    }
}
