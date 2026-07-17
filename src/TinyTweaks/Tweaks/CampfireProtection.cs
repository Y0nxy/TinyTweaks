using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using static CharacterAfflictions.STATUSTYPE;

namespace TinyTweaks.Tweaks
{
    internal class CampfireProtection : MonoBehaviour
    {
        Vector3 protectionPoint;
        //float radius = 30;
        float timeToCheck = 0;
        float delayTime = 1; //every 2 seconds
        Dictionary<Character, float> playersAtCampfire = new Dictionary<Character,float>(); //float is hunger when reached
        Campfire campfire;

        void Awake(){
            protectionPoint = transform.position;
            campfire = GetComponent<Campfire>();
        }
        void Update()
        {
            if (timeToCheck > Time.time) return;
            timeToCheck = Time.time + delayTime;
            //Plugin.log("Checking who got to the campfire");
            foreach (Character c in Character.AllCharacters)
            {
                if (c.data.dead) continue;
                if (Vector3.Distance(protectionPoint, c.Center) < campfire.moraleBoostRadius)
                {
                    if (playersAtCampfire.ContainsKey(c))
                    {
                        addCampfireEffect(c.refs.afflictions, campfire);
                    }
                    else
                    {
                        float currentHunger = c.refs.afflictions.GetCurrentStatus(Hunger);
                        playersAtCampfire.Add(c, currentHunger);
                    }
                }
            }
        }
        void addCampfireEffect(CharacterAfflictions c, Campfire campfire)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                float[] statusData = new float[CharacterAfflictions.NumStatusTypes];

                //hunger
                float currentHunger = c.GetCurrentStatus(Hunger);

                //if hunger is lower than set, set it to that one
                if (currentHunger < playersAtCampfire[c.character]) playersAtCampfire[c.character] = currentHunger;
                float hungerToRemove = currentHunger - playersAtCampfire[c.character];
                statusData[(int)Hunger] = -hungerToRemove;

                //remove all cold
                float currentCold = c.GetCurrentStatus(Cold);
                if (currentCold > 0)
                    statusData[(int)Cold] = -currentCold;

                c.photonView.RPC("RPC_ApplyStatusesFromFloatArray", RpcTarget.All, new object[] { statusData });
            }
        }
    }
}
