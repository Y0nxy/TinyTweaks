using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class PlayerCounter : MonoBehaviour
    {
        //GameObject canvasObj;
        TextMeshProUGUI tmpro;
        static int playerCount => PhotonNetwork.CurrentRoom.PlayerCount;
        static int maxPlayers => PhotonNetwork.CurrentRoom.MaxPlayers;
        static PlayerCounter instance;
        static ConfigEntry<bool> show;
        static ConfigEntry<float> xPos;
        static ConfigEntry<float> yPos;
        static ConfigEntry<string> color;
        static ConfigEntry<float> size;
        static ConfigEntry<bool> usepeakFont;
        static ConfigEntry<Brackets> bracketType;
        static TMP_FontAsset peakFont;
        static TMP_FontAsset defaultFont;
        bool inRoom => PhotonNetwork.CurrentRoom != null && PhotonNetwork.IsConnectedAndReady;
        public static void Binds()
        {
            show = tinyTweaks.config.Bind("PlayerCounter", "show", true);
            xPos = tinyTweaks.config.Bind("PlayerCounter", "X Position", 1920f, new ConfigDescription("", new AcceptableValueRange<float>(-1920f, 1920f)));
            yPos = tinyTweaks.config.Bind("PlayerCounter", "Y Position", 600f, new ConfigDescription("", new AcceptableValueRange<float>(-600, 600f)));
            color = tinyTweaks.config.Bind("PlayerCounter", "Color", "FFFFFF");
            size = tinyTweaks.config.Bind("PlayerCounter", "Size", 30f, new ConfigDescription("", new AcceptableValueRange<float>(0, 300f)));
            usepeakFont = tinyTweaks.config.Bind("PlayerCounter", "peak Font", true);
            bracketType = tinyTweaks.config.Bind("PlayerCounter", "Bracket Type", Brackets.None);
            
            show.SettingChanged += (_,__)=> TryUpdateUI();
            xPos.SettingChanged += (_,__)=> TryUpdateUI();
            yPos.SettingChanged += (_,__)=> TryUpdateUI();
            color.SettingChanged += (_, __) =>
            {
                if (color.Value.Contains("#"))
                {
                    color.Value = color.Value.Replace("#", "");
                    return;
                }
                TryUpdateUI();
            };
            size.SettingChanged += (_,__)=> TryUpdateUI();
            usepeakFont.SettingChanged += (_,__)=> TryUpdateUI();
            bracketType.SettingChanged += (_,__)=> TryUpdateUI();
        }
        void Start()
        {
            instance = this;

            var canvasObj = new GameObject("PlayerCounter");
            canvasObj.transform.SetParent(transform, false);
            var canvas = canvasObj.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.matchWidthOrHeight = 1f;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var tmproObj = new GameObject("TMPro");
            tmproObj.transform.SetParent(canvasObj.transform, false);
            tmpro = tmproObj.AddComponent<TextMeshProUGUI>();
            tmpro.alignment = TextAlignmentOptions.Center;
            tmpro.textWrappingMode = TextWrappingModes.NoWrap;
            tmpro.raycastTarget = false;

            UpdateUI();
        }
        //onjoin/onleave
        static void TryUpdateUI()
        {
            if (instance != null)
                instance.UpdateUI();
        }
        void UpdateUI()
        {
            if (!inRoom)
            {
                tinyTweaks.log("PlayerCounter: error, not connected to room", true);
                StopAllCoroutines();
                StartCoroutine(tryAgainIn5Seconds());
                return;
            }
            tmpro.enabled = show.Value;
            var rt = tmpro.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(xPos.Value, yPos.Value);
            //tmpro.rectTransform.position = new Vector2(xPos.Value, yPos.Value);
            tmpro.text = $"{playerCount}/{maxPlayers}";
            tmpro.fontSize = size.Value;
            if (usepeakFont.Value)
            {
                if (peakFont == null)
                    peakFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(font => font.name == "DarumaDropOne-Regular SDF");
                if (defaultFont == null)
                    defaultFont = tmpro.font;
                tmpro.font = peakFont;
            }
            else
                tmpro.font = defaultFont;

            switch (bracketType.Value)
            {
                case Brackets.Square:
                    tmpro.text = $"[{tmpro.text}]";
                    break;
                case Brackets.Parentheses:
                    tmpro.text = $"({tmpro.text})";
                    break;
                case Brackets.Curly:
                    tmpro.text = '{'+tmpro.text+'}';
                    break;
                default:
                    break;
                    //
            }
            if (color.Value.Length == 6)
            {
                tmpro.text = $"<color=#{color.Value}>{tmpro.text}</color>";
            }
            else tinyTweaks.log("color is not a hex code! Skipping...");
            tinyTweaks.log($"PlayerCounter: In Room! playerCount is ({playerCount}/{maxPlayers})");

            Canvas.ForceUpdateCanvases();
            tmpro.ForceMeshUpdate();
            var canvasRect = (RectTransform)rt.parent;

            const float overhang = 0.3f;

            float limitX = canvasRect.rect.width / 2f - tmpro.preferredWidth * (0.5f - overhang);
            float limitY = canvasRect.rect.height / 2f - tmpro.preferredHeight * (0.5f - overhang);
            rt.anchoredPosition = new Vector2(
                Mathf.Clamp(xPos.Value, -limitX, limitX),
                Mathf.Clamp(yPos.Value, -limitY, limitY));
        }
        IEnumerator tryAgainIn5Seconds()
        {
            yield return new WaitForSeconds(5);
            TryUpdateUI();
        }
        enum Brackets
        {
            Square,
            Parentheses,
            Curly,
            None

        }
        #region join/leave
        [HarmonyPatch(typeof(PlayerConnectionLog))]
        private class PlayerConnectionLogPatches
        {
            [HarmonyPatch("OnPlayerEnteredRoom")]
            [HarmonyPostfix]
            static void OnPlayerEnter()
            {
                TryUpdateUI();
                tinyTweaks.log($"Someone joined! Updated Counter to: ({playerCount}/{maxPlayers})");
            }
            [HarmonyPatch("OnPlayerLeftRoom")]
            [HarmonyPostfix]
            private static void OnPlayerLeft()
            {
                TryUpdateUI();
                tinyTweaks.log($"Someone left! Updated Counter to: ({playerCount}/{maxPlayers})");
            }
        }
        #endregion
    }
}
