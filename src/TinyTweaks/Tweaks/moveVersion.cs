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
        static GameObject version;
        static Vector3 previousPos;

        public static void Binds()
        {
            var config = Plugin.config;
            hideVersionText = config.Bind("Version Label", "Hide Version", false);
            versionTextLeft = config.Bind("Version Label", "Move Version left", true);

            hideVersionText.SettingChanged += (_, _) => CheckHiddenText();
            versionTextLeft.SettingChanged += (_, _) => CheckTextLeft();
        }

        void Awake()
        {
            var versionString = FindAnyObjectByType<VersionString>();
            if (versionString == null)
            {
                Plugin.log("No VersionString in Scene"); 
                Destroy(this);
                return;
            }
            version = versionString.gameObject;
            previousPos = version.transform.localPosition;
            CheckHiddenText();
            CheckTextLeft();
        }

        static void CheckTextLeft()
        {
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
        static void CheckHiddenText()
        {
            if (hideVersionText.Value)
            {
                version.SetActive(false);
                return;
            }
            version.SetActive(true);
        }
    }
}
