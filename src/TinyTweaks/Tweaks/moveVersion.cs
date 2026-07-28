using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class moveVersion : MonoBehaviour
    {
        static ConfigEntry<bool> hideVersionText;
        static ConfigEntry<bool> versionTextRight;
        static ConfigEntry<float> Xpos;
        static ConfigEntry<float> Ypos;
        GameObject version = null;
        Vector3 previousPos;
        float timeToCheck = 0;

        public static void Binds()
        {
            var config = tinyTweaks.config;
            hideVersionText = config.Bind("Version", "Hide Version", false);
            versionTextRight = config.Bind("Version", "Move Version right", true);
            Xpos = config.Bind("Version", "X position", 425f);
            Ypos = config.Bind("Version", "Y position", 540f);
        }

        void Start()
        {
            timeToCheck = Time.time + 3f;
            version = null;
            tinyTweaks.log("Trying to find VersionString");
            hideVersionText.SettingChanged += (_, _) => CheckHiddenText();
            versionTextRight.SettingChanged += (_, _) => CheckTextLeft();
            Xpos.SettingChanged += (_, _) => CheckTextLeft();
            Ypos.SettingChanged += (_, _) => CheckTextLeft();
        }

        void Update()
        {
            if (version != null || timeToCheck > Time.time) return;
            timeToCheck = Time.time + 3f;
            var versionString = FindAnyObjectByType<VersionString>();
            if (versionString == null)
            {
                tinyTweaks.log("No VersionString in Scene");
                //Destroy(this);
                return;
            }
            tinyTweaks.log("VersionString found!");
            version = versionString.gameObject;
            previousPos = version.transform.localPosition;
            CheckHiddenText();
            CheckTextLeft();

        }
        void CheckTextLeft()
        {
            if (version == null) return;
            TextMeshProUGUI tmpro = version.GetComponent<TextMeshProUGUI>();
            if (versionTextRight.Value)
            {
                version.SetActive(true);
                tmpro.alignment = TextAlignmentOptions.TopRight;
                tmpro.horizontalAlignment = HorizontalAlignmentOptions.Right;
                version.transform.localPosition = new Vector3(Xpos.Value, Ypos.Value, 0);
                return;
            }
            tmpro.alignment = TextAlignmentOptions.TopLeft;
            tmpro.horizontalAlignment = HorizontalAlignmentOptions.Left;
            version.transform.localPosition = previousPos;
        }
        void CheckHiddenText()
        {
            if (version == null) return;
            if (hideVersionText.Value)
            {
                version.SetActive(false);
                return;
            }
            version.SetActive(true);
        }
    }
}
