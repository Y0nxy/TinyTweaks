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
        static ConfigEntry<bool> versionTextLeft;
        GameObject version = null;
        Vector3 previousPos;
        float timeToCheck = 0;

        public static void Binds()
        {
            var config = Plugin.config;
            hideVersionText = config.Bind("Version", "Hide Version", false);
            versionTextLeft = config.Bind("Version", "Move Version left", true);
        }

        void Start()
        {
            timeToCheck = Time.time + 3f;
            version = null;
            Plugin.log("Trying to find VersionString");
            hideVersionText.SettingChanged += (_, _) => CheckHiddenText();
            versionTextLeft.SettingChanged += (_, _) => CheckTextLeft();
        }

        void Update()
        {
            if (version != null || timeToCheck > Time.time) return;
            timeToCheck = Time.time + 3f;
            var versionString = FindAnyObjectByType<VersionString>();
            if (versionString == null)
            {
                Plugin.log("No VersionString in Scene");
                //Destroy(this);
                return;
            }
            Plugin.log("VersionString found!");
            version = versionString.gameObject;
            previousPos = version.transform.localPosition;
            CheckHiddenText();
            CheckTextLeft();

        }
        void CheckTextLeft()
        {
            if (version == null) return;
            TextMeshProUGUI tmpro = version.GetComponent<TextMeshProUGUI>();
            if (versionTextLeft.Value)
            {
                version.SetActive(true);
                hideVersionText.Value = false;
                tmpro.alignment = TextAlignmentOptions.TopRight;
                tmpro.horizontalAlignment = HorizontalAlignmentOptions.Right;
                version.transform.localPosition = new Vector3(425f, 540, 0);
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
