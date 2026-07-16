using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TinyTweaks.Tweaks
{
    internal class CampfireProtection : MonoBehaviour
    {
        Vector3 protectionPoint;
        //float radius = 30;
        float timeToCheck = 0;
        float delayTime = 2; //every 2 seconds
        //Dictionary<Character, float> playersAtCampfire = new Dictionary<Character,float>();
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
                    CharacterAfflictions aff = c.refs.afflictions;
                    if (aff.canGetHungry) aff.AddAffliction(Campfire.s_CampfireBuff, !c.IsLocal);
                    if (Time.time - campfire._timebuffLastApplied >= 2f)
                    {
                        campfire._timebuffLastApplied = Time.time;
                        aff.AddAffliction(Campfire.s_CampfireBuff, !c.IsLocal);
                    }
                    //playersAtCampfire.Add(c);
                }
            }
        }
    }
}
