using BepInEx.Configuration;
using HarmonyLib;
using System.Collections;
using TMPro;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class moveAscent : MonoBehaviour
    {
        //AscentUI

        static ConfigEntry<bool> fixAscentUI;
        static ConfigEntry<bool> moveAscentUI;
        static ConfigEntry<float> XposAscent;
        static ConfigEntry<float> YposAscent;
        static AscentUI ascentUI = null;
        static Vector3 previousPosAscent;

        static ConfigEntry<string> ascentTextColor;
        static ConfigEntry<string> AscentText;
        static ConfigEntry<float> AscentSize;

        public static void Binds()
        {
            var config = tinyTweaks.config;
            fixAscentUI = config.Bind("Ascent", "Fix Ascent Text", true);
            moveAscentUI = config.Bind("Ascent", "Move Ascent Text", true);
            XposAscent = config.Bind("Ascent", "X position Ascent", 940f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));
            YposAscent = config.Bind("Ascent", "Y position Ascent", 490f, new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f)));
            //usePeakFont = config.Bind("Ascent", "Use Peak Font", true);
            ascentTextColor = config.Bind("Ascent", "Ascent Text Color", "DBD7BF");
            AscentText = config.Bind("Ascent", "Ascent Text", "ASCENT");
            AscentSize = config.Bind("Ascent", "Ascent Size", 24f, new ConfigDescription("", new AcceptableValueRange<float>(0, 300f)));

            moveAscentUI.SettingChanged += (_, _) => moveAscentText();
            XposAscent.SettingChanged += (_, _) => moveAscentText();
            YposAscent.SettingChanged += (_, _) => moveAscentText();
            ascentTextColor.SettingChanged += (_, _) => moveAscentText();
            AscentSize.SettingChanged += (_, _) => moveAscentText();
            AscentText.SettingChanged += (_, _) => updateAscentText();
        }

        #region Patches
        [HarmonyPatch]
        static class UIPatches
        {
            [HarmonyPatch(typeof(AscentUI), "Start")]
            [HarmonyPostfix]
            static void setAscentObj(AscentUI __instance)
            {
                tinyTweaks.log("found AscentUI");
                __instance.StartCoroutine(delayUpdatingAscent(__instance));
                ascentUI = __instance;
                previousPosAscent = ascentUI.transform.localPosition;
                moveAscentText();
            }
            [HarmonyPatch(typeof(Ascents), nameof(Ascents.currentAscent), MethodType.Setter)]
            [HarmonyPostfix]
            static void updateAscentUI() => updateAscentText();
        }
        static IEnumerator delayUpdatingAscent(AscentUI ascentUI)
        {
            yield return new WaitForSeconds(5);
            updateAscentText();
        }
        #endregion
        static void updateAscentText()
        {
            if (!fixAscentUI.Value) return;
            if (ascentUI == null)
                ascentUI = FindFirstObjectByType<AscentUI>();
            if (ascentUI == null) return;
            if (ascentUI.TryGetComponent<TextMeshProUGUI>(out var tmpro))
            {
                switch (Ascents.currentAscent)
                {
                    case -1:
                        tmpro.text = "TENDERFOOT";
                        break;
                    case 0:
                        tmpro.text = "";
                        break;
                    default:
                        tmpro.text = AscentText.Value + " " + Ascents.currentAscent;
                        break;
                }
            }
        }
        static void moveAscentText()
        {
            if (ascentUI?.GetComponent<TextMeshProUGUI>() == null) return;
            
            ApplyColorAndSize(ascentUI.GetComponent<TextMeshProUGUI>(), ascentTextColor.Value);
        }
        static void ApplyColorAndSize(TextMeshProUGUI tmpro, string colorValue)
        {
            tmpro.fontSize = AscentSize.Value;
            tmpro.transform.localPosition = moveAscentUI.Value ? new Vector3(XposAscent.Value, YposAscent.Value, 0) : previousPosAscent;
            ascentTextColor.Value = ascentTextColor.Value.Replace("#", "");
            if (ascentTextColor.Value.Length != 6)
            {
                tinyTweaks.log("color is not a hex code! Skipping...");
                return;
            }

            if (ColorUtility.TryParseHtmlString("#" + ascentTextColor.Value, out Color color))
            {
                tmpro.color = color;
            }
        }
    }
}