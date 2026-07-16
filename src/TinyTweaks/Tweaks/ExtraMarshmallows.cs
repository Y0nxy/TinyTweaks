using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using Photon.Voice.Unity.UtilityScripts;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class ExtraMarshmallows
    {
        static Campfire nextCampfire;
        static List<GameObject> marshmallows;
        static float radius = 3f;
        static ConfigEntry<float> hotdogPercent;
        static int playerCount => PhotonNetwork.CurrentRoom.PlayerCount;
        static Vector3 originPoint => nextCampfire.transform.position;
        static int marshmallowsTaken = 0;

        //Awake from every Campfire
        //Taken = 0 on any new campfire
        public static void Start()
        {
            hotdogPercent = Plugin.config.Bind("ExtraMarshmallows", "Hotdog spawn chance", 33f);
        }
        [HarmonyPatch(typeof(Campfire), "Awake")]
        [HarmonyPostfix]
        private static void OnCampfireAwake(Campfire __instance)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (__instance.transform.parent.name.Contains("Wings")) return;//not real campfire
            Plugin.log("OnCampfire Called for " + __instance.name);
            nextCampfire = __instance;
            RemoveOldMarshmallows();
            RefreshMarshmallows();
        }
        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerEnteredRoom")]
        [HarmonyPostfix]
        private static void OnPlayerEnter()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Plugin.log("Someone joined! Updated Marshmallows to:" + playerCount.ToString());
            RefreshMarshmallows();
        }

        [HarmonyPatch(typeof(PlayerConnectionLog), "OnPlayerLeftRoom")]
        [HarmonyPostfix]
        private static void OnPlayerLeft()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Plugin.log("Someone left! Updated Marshmallows to:" + playerCount.ToString());
            RefreshMarshmallows();
        }
        static void RefreshMarshmallows()
        {
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
            Vector3 directionToCenter = originPoint - spawnPosition;

            // We force Y to be 0 so they don't tilt up/down if your campfire is on a slope.
            directionToCenter.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToCenter);
            string itemToSpawn = "0_Items/Marshmallow";
            if (Random.value <= hotdogPercent.Value / 100) itemToSpawn = "0_Items/Glizzy";
            GameObject itemObj = PhotonNetwork.InstantiateRoomObject(itemToSpawn, spawnPosition, lookRotation);
            var item = itemObj.GetComponent<Item>();
            if (item != null) item.SetKinematicNetworked(true);
            marshmallows.Add(itemObj);
        }
        static void RemoveOldMarshmallows()
        {
            marshmallows.Clear();
            foreach (ItemCooking item in GameObject.FindObjectsByType<ItemCooking>(FindObjectsSortMode.None))
            {
                var itemName = item.name;
                if (itemName.Contains("Glizzy") || itemName.Contains("Marshmallow"))
                {
                    if (Vector3.Distance(originPoint, item.transform.position) < 20f) {
                        if (!item.photonView.IsMine)
                            item.photonView.RequestOwnership();
                        PhotonNetwork.Destroy(item.gameObject);
                    }
                }
            }
        }
    }
}
