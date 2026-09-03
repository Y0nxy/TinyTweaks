using BepInEx.Configuration;
using HarmonyLib;
using pworld.Scripts.Extensions;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
        //static TMP_FontAsset defaultFont = null;
        static Dictionary<TextMeshProUGUI, TMP_FontAsset> defaultFonts = new Dictionary<TextMeshProUGUI, TMP_FontAsset>();

        static TMP_FontAsset peakFont = null;
        static ConfigEntry<float> fontSize;
        static ConfigEntry<TextAlignment> textAlignment;
        static ConfigEntry<string> versionTextColor;
        static ConfigEntry<string> ascentTextColor;
        static bool peaker = false;

        public static void Binds()
        {
            var config = tinyTweaks.config;
            hideVersionText = config.Bind("Version", "Hide Version", false);
            moveVersionText = config.Bind("Version", "Move Version Text", true);
            Xpos = config.Bind("Version", "X position", 890f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));
            Ypos = config.Bind("Version", "Y position", 545f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));

            moveAscentUI = config.Bind("Version", "Move Ascent Text", true);
            XposAscent = config.Bind("Version", "X position Ascent", 940f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));
            YposAscent = config.Bind("Version", "Y position Ascent", 490f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));
            usePeakFont = config.Bind("Version", "Use Peak Font", true);
            fontSize = config.Bind("Version", "Font Size", 24f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 100f)));
            textAlignment = config.Bind("Version", "Text Alignment", TextAlignment.Center);
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
            versionTextColor.SettingChanged += (_, _) => updateVersionText();
            fontSize.SettingChanged += (_, _) => updateVersionText();
            usePeakFont.SettingChanged += (_, _) => updateVersionText();
            //AscentUI
            moveAscentUI.SettingChanged += (_, _) => moveAscentText();
            XposAscent.SettingChanged += (_, _) => moveAscentText();
            YposAscent.SettingChanged += (_, _) => moveAscentText();
            ascentTextColor.SettingChanged += (_, _) => moveAscentText();
            defaultFonts.Clear();
        }

        [HarmonyPatch]
        static class UIPatches
        {
            [HarmonyPatch(typeof(VersionString), "Start")]
            [HarmonyPostfix]
            static void setVersionObj(VersionString __instance)
            {
                __instance.gameObject.AddComponent<moveVersion>();
                version = __instance.gameObject;
                if (version.transform.parent.name == "VersionStack")
                {
                    version = version.transform.parent.gameObject; // peaker check, WHY LAMMAS WHY!??!
                    //tinyTweaks.log("peaker probably on, why did you change how label works lammas??");
                    peaker = true;
                }
                previousPosVersion = version.transform.localPosition;
                if (peaker)
                    previousPosVersion = version.GetComponent<RectTransform>().localPosition;
                tinyTweaks.log("VersionString found!" + (peaker ? " (PEAKER detected)" : ""));
                updateVersionText();
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
            if (peaker)
            {
                moveVersionTextAndAlign(version.transform.GetChild(0).gameObject);
                tinyTweaks.Instance.StartCoroutine(WaitForPEAKtext());
            }
            else
            {
                moveVersionTextAndAlign(version);
            }

            
            //tmpro.horizontalAlignment = HorizontalAlignmentOptions.Left;
            if (!moveVersionText.Value)
            {
                if (peaker)
                {
                    version.GetComponent<RectTransform>().localPosition = previousPosVersion;
                    version.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
                    version.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f); // Default TopLeft pivot
                }
                else
                {
                    version.transform.localPosition = previousPosVersion;
                    version.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
                    version.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f); // Default TopLeft pivot
                }
            }
        }
        static void moveVersionTextAndAlign(GameObject ver)
        {
            TextMeshProUGUI tmpro = ver.GetComponent<TextMeshProUGUI>();
            RectTransform rectTransform = ver.GetComponent<RectTransform>();
            ApplyColor(tmpro, versionTextColor.Value);
            peakfontUpdate(tmpro);
            if (moveVersionText.Value)
            {
                if (peaker)
                {
                    var rect = version.GetComponent<RectTransform>();
                    rect.localPosition = new Vector3(Xpos.Value, Ypos.Value, 0);
                    var layoutGroup = version.GetComponent<VerticalLayoutGroup>();
                    layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                    
                    switch (textAlignment.Value)
                    {
                        case TextAlignment.Left:
                            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
                            rect.pivot = new Vector2(0f, 1f); // TopLeft pivot
                            break;
                        case TextAlignment.Center:
                            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                            rect.pivot = new Vector2(0.5f, 1f); // TopCenter pivot
                            break;
                        case TextAlignment.Right:
                            layoutGroup.childAlignment = TextAnchor.MiddleRight;
                            rect.pivot = new Vector2(1f, 1f);
                            break;
                    }
                }
                else
                {
                    Vector2 pivot = rectTransform.pivot;
                    pivot.y = 1f;
                    switch (textAlignment.Value)
                    {
                        case TextAlignment.Left:
                            tmpro.horizontalAlignment = HorizontalAlignmentOptions.Left;
                            pivot.x = 0f;
                            break;
                        case TextAlignment.Center:
                            tmpro.horizontalAlignment = HorizontalAlignmentOptions.Center;
                            pivot.x = 0.5f;
                            break;
                        case TextAlignment.Right:
                            tmpro.horizontalAlignment = HorizontalAlignmentOptions.Right;
                            pivot.x = 1f;
                            break;
                    }
                    rectTransform.pivot = pivot;
                    ver.transform.localPosition = new Vector3(Xpos.Value, Ypos.Value, 0);
                }
                return;
            }
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

        static void peakfontUpdate(TextMeshProUGUI tmpro)
        {
            if (tmpro == null) return;
            if (!defaultFonts.ContainsKey(tmpro)) defaultFonts.Add(tmpro, tmpro.font);
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
            tmpro.font = defaultFonts[tmpro];
            tmpro.fontSize = 18f;
        }
        static System.Collections.IEnumerator WaitForPEAKtext()
        {
            yield return new WaitForSeconds(5f);
            var peakTextObj = version.transform.GetChild(1);
            if (peakTextObj != null)
            {
                moveVersionTextAndAlign(peakTextObj.gameObject);
            }
        }
    }
}