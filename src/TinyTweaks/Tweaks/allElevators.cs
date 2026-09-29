using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class allElevators
    {
        static int lastIndex = 0;
        public static bool inAirport = false;
        static ConfigEntry<bool> randomElevators;
        static ConfigEntry<bool> spacedElevators;
        static readonly float[] points = new float[] { -15f, -9.5f, -4.1f, 1.5f };

        public static void Binds()
        {
            randomElevators = tinyTweaks.config.Bind("Shorties", "Random Elevators", true);
            spacedElevators = tinyTweaks.config.Bind("Shorties", "In Order Elevators", false);
        }
        [HarmonyPatch]
        static class patch
        {
            [HarmonyPatch(typeof(Character), nameof(Character.Start))]
            [HarmonyPostfix]
            static void spawnInElevator(Character __instance)
            {
                //tinyTweaks.logMessage($"StartCharacter");
                if (!inAirport ||!PhotonNetwork.IsMasterClient ||(!randomElevators.Value && !spacedElevators.Value)) return;
                if (__instance != null)
                    MoveCharacterToElevator(__instance);
            }
        }
        static void MoveCharacterToElevator(Character character)
        {
            float xPos;
            int index;
            //var spawnPoint = GameObject.Find("SpawnPoint");
            if (randomElevators.Value)
            {
                var randomIndex = UnityEngine.Random.Range(0, points.Length);
                xPos = points[randomIndex];
                index = randomIndex;
            }
            else
            {
                if (++lastIndex >= points.Length) lastIndex = 0;
                xPos = points[lastIndex];
                index = lastIndex;
            }

            var position = new Vector3(xPos, 1.8f, 50.5f);
            character.photonView.RPC("WarpPlayerRPC", RpcTarget.All, position, false);
            //tinyTweaks.logMessage($"tp player: {character.name} to index: {index}. pos is: {position}");
        }
    }
}
