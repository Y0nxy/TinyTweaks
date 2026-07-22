using System;
using UnityEngine;
using Photon.Pun;

namespace TinyTweaks.Tweaks
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PhotonView))]
    public class BasketballMagnet : MonoBehaviour, IPunOwnershipCallbacks
    {
        private Rigidbody rb = null!;
        private PhotonView pv = null!;
        private BasketballHoop targetHoop;
        private bool hasLeftHand;
        private bool hasExecutedLegitShot;
        private float lastForceTime = -1f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            pv = GetComponent<PhotonView>();
            ResetState();
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            FindNearestHoop();
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void ResetState()
        {
            hasLeftHand = false;
            hasExecutedLegitShot = false;
            lastForceTime = -1f;
        }

        private void FindNearestHoop()
        {
            BasketballHoop[] hoops = GameObject.FindObjectsByType<BasketballHoop>(FindObjectsSortMode.None);
            float closestDistance = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            foreach (BasketballHoop hoop in hoops)
            {
                float distance = Vector3.Distance(currentPos, hoop.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetHoop = hoop;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!pv.IsMine || !ItemAimbotFinder.EnableAssist.Value || !ItemAimbotFinder.RageAimbot.Value) return;

            BasketballHoop hoop = other.GetComponentInParent<BasketballHoop>();
            if (hoop != null)
            {
                if (rb.linearVelocity.y < 0f && transform.position.y < hoop.transform.position.y)
                {
                    Debug.Log("[Rage Mode] Basket scored! Automatically tracking next objective...");
                    hasLeftHand = true;
                    FindNearestHoop();
                    ApplyParabolicAimbotForce();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!pv.IsMine || !ItemAimbotFinder.EnableAssist.Value) return;

            if (transform.parent != null && (transform.root.GetComponent<Character>() != null || transform.root.name.Contains("Player")))
            {
                ResetState();
                return;
            }

            if (!ItemAimbotFinder.RageAimbot.Value && hasExecutedLegitShot) return;

            if (!hasLeftHand)
            {
                hasLeftHand = true;
                FindNearestHoop();

                float currentSpeed = rb.linearVelocity.magnitude;
                if (currentSpeed < ItemAimbotFinder.MinimumThrowSpeedBasketBall.Value)
                {
                    if (!ItemAimbotFinder.RageAimbot.Value)
                    {
                        hasExecutedLegitShot = true;
                    }
                    return;
                }

                ApplyParabolicAimbotForce();

                if (!ItemAimbotFinder.RageAimbot.Value)
                {
                    hasExecutedLegitShot = true;
                }
            }
        }
        private void Update()
        {
            if (pv.IsMine && lastForceTime > 0f)
            {
                if (Time.time - lastForceTime >= 3.5f)
                {
                    ReturnOwnershipToMaster();
                    lastForceTime = -1f;
                }
            }
        }

        private void ApplyParabolicAimbotForce()
        {
            if (targetHoop == null)
            {
                FindNearestHoop();
                if (targetHoop == null) return;
            }

            Vector3 startPos = transform.position;

            Vector3 targetPos = targetHoop.transform.position + (Vector3.up * ItemAimbotFinder.HoopEntryHeightOffset.Value);
            targetPos.x += ItemAimbotFinder.GlobalXOffset.Value;

            if (ItemAimbotFinder.ShowVisualTargetMarker.Value)
            {
                SpawnDebugMarker(targetPos);
            }

            float gravity = Mathf.Abs(Physics.gravity.y);
            if (Mathf.Approximately(gravity, 0f)) gravity = 9.81f;

            float displacementY = targetPos.y - startPos.y;
            Vector3 displacementXZ = new Vector3(targetPos.x - startPos.x, 0f, targetPos.z - startPos.z);

            float highestPoint = Mathf.Max(targetPos.y, startPos.y) + ItemAimbotFinder.PeakHeightAboveHoop.Value;
            float heightToPeak = highestPoint - startPos.y;
            float heightToTargetFromPeak = highestPoint - targetPos.y;

            if (heightToPeak <= 0 || heightToTargetFromPeak <= 0)
            {
                Vector3 fallbackDir = (targetPos - startPos).normalized;
                rb.linearVelocity = fallbackDir * 15f;
                lastForceTime = Time.time;
                return;
            }

            float initialVelocityY = Mathf.Sqrt(2f * gravity * heightToPeak);
            float timeToPeak = Mathf.Sqrt((2f * heightToPeak) / gravity);
            float timeToTargetFromPeak = Mathf.Sqrt((2f * heightToTargetFromPeak) / gravity);
            float totalFlightTime = timeToPeak + timeToTargetFromPeak;

            if (Mathf.Approximately(totalFlightTime, 0f)) return;

            float dragMultiplier = 1f - (rb.linearDamping * totalFlightTime * 0.5f);
            dragMultiplier = Mathf.Clamp(dragMultiplier, 0.7f, 1.0f);

            Vector3 initialVelocityXZ = displacementXZ / (totalFlightTime * dragMultiplier);
            Vector3 targetVelocity = new Vector3(initialVelocityXZ.x, initialVelocityY, initialVelocityXZ.z);

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.AddForce(targetVelocity, ForceMode.VelocityChange);

            lastForceTime = Time.time;
        }

        private void ReturnOwnershipToMaster()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.MasterClient != null && pv.Owner != PhotonNetwork.MasterClient)
            {
                pv.TransferOwnership(PhotonNetwork.MasterClient);
                Plugin.Log.LogInfo("[BasketballMagnet] Inactivity timeout (3.5s). Transferred ownership back to Master Client.");
            }
        }
        private void SpawnDebugMarker(Vector3 position)
        {
            try
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Aimbot_Target_Indicator";
                marker.transform.position = position;
                marker.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

                if (marker.TryGetComponent<Collider>(out var col))
                {
                    Destroy(col);
                }

                if (marker.TryGetComponent<Renderer>(out var renderer))
                {
                    renderer.material = new Material(Shader.Find("Sprites/Default"));
                    renderer.material.color = Color.red;
                }

                Destroy(marker, 3f);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Arc Aimbot] Failed to render mod debug tracker sphere: {ex.Message}");
            }
        }

        #region Photon Ownership Callbacks
        public void OnOwnershipRequest(PhotonView targetView, Photon.Realtime.Player requestingPlayer)
        {
            targetView.RequestOwnership();
            targetView.TransferOwnership(requestingPlayer);
            Plugin.Log.LogInfo($"[BasketballMagnet] Ownership requested by Player {requestingPlayer.UserId}. Attempting transfer...");
        }
        public void OnOwnershipTransfered(PhotonView targetView, Photon.Realtime.Player previousOwner)
        {
            if (targetView == pv && pv.IsMine)
            {
                ResetState();
                FindNearestHoop();
                Plugin.Log.LogInfo("[BasketballMagnet] Ownership transfer successful. Assist system active.");
            }
        }
        public void OnOwnershipTransferFailed(PhotonView targetView, Photon.Realtime.Player previousOwner)
        {
            Plugin.Log.LogInfo("[BasketballMagnet] Ownership transfer failed.");
        }
        #endregion
    }
}