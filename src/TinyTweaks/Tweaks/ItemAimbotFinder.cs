using BepInEx.Configuration;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    public class ItemAimbotFinder : MonoBehaviour
    {
        [Header("Global Scan Settings")]
        public float scanInterval = 0.5f;

        private float nextScanTime;
        private HashSet<int> pendingRequests = new HashSet<int>();
        ConfigEntry<string> Basketball = null!;

        // --- CONFIGURATION MANAGEMENT ---
        public static ConfigEntry<bool> EnableAssist { get; private set; } = null!;
        public static ConfigEntry<bool> RageAimbot { get; private set; } = null!;
        public static ConfigEntry<float> PeakHeightAboveHoop { get; private set; } = null!;
        public static ConfigEntry<float> HoopEntryHeightOffset { get; private set; } = null!;
        public static ConfigEntry<bool> ShowVisualTargetMarker { get; private set; } = null!;
        public static ConfigEntry<float> MinimumThrowSpeedBasketBall { get; private set; } = null!;
        public static ConfigEntry<float> GlobalXOffset { get; private set; } = null!;



        void Awake()
        {
            var config = Plugin.config;
            EnableAssist = config.Bind("Basketball", "Enable Assist", true, "Master toggle for the assist system.");
            RageAimbot = config.Bind("Basketball", "RageAim-Bot", false, "When true, runs infinitely without self-destructing. When false, acts as one-time recorrections.");
            PeakHeightAboveHoop = config.Bind("Basketball", "Arc", 1.5f, new ConfigDescription("How high above the hoop the ball will reach at the peak of its arc.", new AcceptableValueRange<float>(0f, 100f)));
            HoopEntryHeightOffset = config.Bind("Basketball", "Hoop Entry Height Offset", 0.5f, new ConfigDescription("An additional upward vertical offset directly above the hoop component to aim for before dropping.", new AcceptableValueRange<float>(-2f, 5f)));
            MinimumThrowSpeedBasketBall = config.Bind("Basketball", "Minimum Throw Speed", 4.5f, new ConfigDescription("Minimum velocity magnitude required at release to trigger the physics assist.", new AcceptableValueRange<float>(0f, 30f)));
            GlobalXOffset = config.Bind("Basketball", "Global X Axis Offset", 0.8f, new ConfigDescription("Manually shifts the target position along Unity's absolute Global X axis to counter steady directional drifting.", new AcceptableValueRange<float>(-5f, 5f)));
            ShowVisualTargetMarker = config.Bind("Basketball", "Show Target Marker Sphere", true, "Spawns a temporary physical red marker sphere in-game where the calculation is aiming.");
            Basketball = config.Bind("Basketball", "Basketball ItemName", "Basketball", "The substring used to identify Basketball items in the scene.");
        }


        private void Update()
        {
            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + scanInterval;
                ScanAndSetupDynamicItems();
            }
        }

        private void ScanAndSetupDynamicItems()
        {
            Item[] allItems = GameObject.FindObjectsByType<Item>(FindObjectsSortMode.None);

            foreach (Item itemComponent in allItems)
            {
                if (itemComponent == null) continue;

                if (itemComponent.holderCharacter != null)
                {
                    BasketballMagnet existingMagnet = itemComponent.GetComponent<BasketballMagnet>();
                    if (existingMagnet != null) existingMagnet.ResetState();
                    continue;
                }

                GameObject itemObj = itemComponent.gameObject;
                string itemName = itemObj.name;

                Rigidbody rb = itemObj.GetComponent<Rigidbody>();
                if (rb == null) continue;

                PhotonView pv = itemObj.GetComponent<PhotonView>();
                if (pv == null) continue;

                if (pv.IsMine && pendingRequests.Contains(pv.ViewID))
                {
                    pendingRequests.Remove(pv.ViewID);
                }

                // --- CONDITION B: BASKETBALL AIMBOT ENGINE ---
                if (itemName.Contains(Basketball.Value) && (EnableAssist.Value || RageAimbot.Value))
                {
                    if (rb.linearVelocity.magnitude <= MinimumThrowSpeedBasketBall.Value) continue;

                    if (!pv.IsMine)
                    {
                        if (!pendingRequests.Contains(pv.ViewID))
                        {
                            pendingRequests.Add(pv.ViewID);
                            pv.RequestOwnership();
                        }
                    }
                    else
                    {
                        EnsureBasketballComponent(itemObj);
                    }
                }
            }
        }

        private BasketballMagnet EnsureBasketballComponent(GameObject target)
        {
            BasketballMagnet magnet = target.GetComponent<BasketballMagnet>();
            if (magnet == null) magnet = target.AddComponent<BasketballMagnet>();
            return magnet;
        }
    }
}