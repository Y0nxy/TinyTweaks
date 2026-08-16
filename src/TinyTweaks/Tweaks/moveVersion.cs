using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using HarmonyLib;

namespace TinyTweaks.Tweaks
{
    internal class moveVersion : MonoBehaviour
    {
        static ConfigEntry<bool> hideVersionText;
        static ConfigEntry<bool> moveVersionText;
        static ConfigEntry<float> Xpos;
        static ConfigEntry<float> Ypos;
        static GameObject version = null;
        static Vector3 previousPosVersion;
        //AscentUI

        static ConfigEntry<bool> moveAscentUI;
        static ConfigEntry<float> XposAscent;
        static ConfigEntry<float> YposAscent;
        static GameObject ascentUI = null;
        static Vector3 previousPosAscent;

        static ConfigEntry<bool> usePeakFont;
        static TMP_FontAsset defaultFont = null;
        static TMP_FontAsset peakFont = null;
        static ConfigEntry<float> fontSize;
        static ConfigEntry<HorizontalAlignmentOptions> textAlignment;
        static ConfigEntry<string> versionTextColor;
        static ConfigEntry<string> ascentTextColor;
        static bool versionVisible = true;
        static bool ascentVisible = true;

        public static void Binds()
        {
            var config = tinyTweaks.config;
            hideVersionText = config.Bind("Version", "Hide Version", false);
            moveVersionText = config.Bind("Version", "Move Version Text", true);
            Xpos = config.Bind("Version", "X position", 890f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));
            Ypos = config.Bind("Version", "Y position", 540f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));

            moveAscentUI = config.Bind("Version", "Move Ascent Text", true);
            XposAscent = config.Bind("Version", "X position Ascent", 955f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));
            YposAscent = config.Bind("Version", "Y position Ascent", 490f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));
            usePeakFont = config.Bind("Version", "Use Peak Font", true);
            fontSize = config.Bind("Version", "Font Size", 24f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 100f)));
            textAlignment = config.Bind("Version", "Text Alignment", HorizontalAlignmentOptions.Center);
            versionTextColor = config.Bind("Version", "Version Text Color", "DBD7BF");
            ascentTextColor = config.Bind("Version", "Ascent Text Color", "DBD7BF");
        }

        void Start()
        {
            tinyTweaks.log("Trying to find VersionString");
            hideVersionText.SettingChanged += (_, _) => updateVersionText();
            moveVersionText.SettingChanged += (_, _) => updateVersionText();
            Xpos.SettingChanged += (_, _) => updateVersionText();
            Ypos.SettingChanged += (_, _) => updateVersionText();
            textAlignment.SettingChanged += (_, _) => updateVersionText();
            //AscentUI
            moveAscentUI.SettingChanged += (_, _) => moveAscentText();
            XposAscent.SettingChanged += (_, _) => moveAscentText();
            YposAscent.SettingChanged += (_, _) => moveAscentText();
            usePeakFont.SettingChanged += (_, _) => peakfontUpdate();
            fontSize.SettingChanged += (_, _) => peakfontUpdate();
            versionTextColor.SettingChanged += (_, _) => updateVersionText();
            ascentTextColor.SettingChanged += (_, _) => moveAscentText();
        }

        [HarmonyPatch]
        static class UIPatches
        {
            [HarmonyPatch(typeof(VersionString), "Start")]
            [HarmonyPostfix]
            static void setVersionObj(VersionString __instance)
            {
                version = __instance.gameObject;
                previousPosVersion = version.transform.localPosition;
                tinyTweaks.log("VersionString found!");
                updateVersionText();
                peakfontUpdate();
            }
            [HarmonyPatch(typeof(AscentUI), "Start")]
            [HarmonyPostfix]
            static void setAscentObj(AscentUI __instance)
            {
                tinyTweaks.log("found AscentUI");
                ascentUI = __instance.gameObject;
                previousPosAscent = ascentUI.transform.localPosition;
                moveAscentText();
            }
        }

        static void updateVersionText()
        {
            if (version == null) return;
            if (hideVersionText.Value)
            {
                version.SetActive(false);
                return;
            }
            version.SetActive(true);
            TextMeshProUGUI tmpro = version.GetComponent<TextMeshProUGUI>();
            RectTransform rectTransform = version.GetComponent<RectTransform>();
            ApplyColor(tmpro, versionTextColor.Value);
            if (moveVersionText.Value)
            {
                tmpro.horizontalAlignment = textAlignment.Value;//TEST THIS
                Vector2 pivot = rectTransform.pivot;
                pivot.y = 1f;
                switch (textAlignment.Value)
                {
                    case HorizontalAlignmentOptions.Left:
                        pivot.x = 0f;
                        break;
                    case HorizontalAlignmentOptions.Center:
                        pivot.x = 0.5f;
                        break;
                    case HorizontalAlignmentOptions.Right:
                        pivot.x = 1f;
                        break;
                }
                rectTransform.pivot = pivot;
                version.transform.localPosition = new Vector3(Xpos.Value, Ypos.Value, 0);
                return;
            }
            tmpro.alignment = TextAlignmentOptions.TopLeft;
            rectTransform.pivot = new Vector2(0f, 1f); // Default TopLeft pivot
            //tmpro.horizontalAlignment = HorizontalAlignmentOptions.Left;
            version.transform.localPosition = previousPosVersion;
        }

        static void moveAscentText()
        {
            if (ascentUI == null) return;
            if (moveAscentUI.Value)
            {
                tinyTweaks.log("Moved AscentUI");
                ascentUI.transform.localPosition = new Vector3(XposAscent.Value, YposAscent.Value, 0);
                ApplyColor(ascentUI.GetComponent<TextMeshProUGUI>(), ascentTextColor.Value);
                return;
            }
            ascentUI.transform.localPosition = previousPosAscent;
            ApplyColor(ascentUI.GetComponent<TextMeshProUGUI>(), ascentTextColor.Value);
        }
        static void ApplyColor(TextMeshProUGUI tmpro, string colorValue)
        {
            if (tmpro == null) return;

            string normalized = colorValue?.Trim() ?? "FFFFFF";
            if (normalized.StartsWith("#")) normalized = normalized.Substring(1);

            if (ColorUtility.TryParseHtmlString("#" + normalized, out Color color))
            {
                tmpro.color = color;
            }
        }

        static void peakfontUpdate()
        {
            if (version == null) return;
            TextMeshProUGUI tmpro = version.GetComponent<TextMeshProUGUI>();
            if (defaultFont == null) defaultFont = tmpro.font;
            if (usePeakFont.Value)
            {
                if (peakFont == null)
                {
                    TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                    peakFont = Array.Find(fonts, f => f.name == "DarumaDropOne-Regular SDF");
                }
                tmpro.font = peakFont;
                tmpro.fontSize = fontSize.Value;
                return;
            }
            tmpro.font = defaultFont;
            tmpro.fontSize = 18f;
        }
    }
}