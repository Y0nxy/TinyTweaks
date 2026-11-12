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
            var maxValues = new ConfigDescription("", new AcceptableValueRange<float>(-2000f, 2000f));
            xPos = tinyTweaks.config.Bind("PlayerCounter", "X Position", 1880f, maxValues);
            yPos = tinyTweaks.config.Bind("PlayerCounter", "Y Position", 1065f, maxValues);
            color = tinyTweaks.config.Bind("PlayerCounter", "Color", "FFFFFF");
            size = tinyTweaks.config.Bind("PlayerCounter", "Size", 30f, new ConfigDescription("", new AcceptableValueRange<float>(0, 300f)));
            usepeakFont = tinyTweaks.config.Bind("PlayerCounter", "peak Font", true);
            bracketType = tinyTweaks.config.Bind("PlayerCounter", "Bracket Type", Brackets.None);
            
            show.SettingChanged += (_,__)=> instance.UpdateUI();
            xPos.SettingChanged += (_,__)=> instance.UpdateUI();
            yPos.SettingChanged += (_,__)=> instance.UpdateUI();
            color.SettingChanged += (_,__)=> instance.UpdateUI();
            size.SettingChanged += (_,__)=> instance.UpdateUI();
            usepeakFont.SettingChanged += (_,__)=> instance.UpdateUI();
            bracketType.SettingChanged += (_,__)=> instance.UpdateUI();
        }
        void Awake()
        {
            instance = this;
            var canvasObj = new GameObject("PlayerCounter");
            canvasObj.transform.parent = transform;
            var canvas = canvasObj.gameObject.AddComponent<Canvas>();
            var tmproObj = new GameObject("TMPro");
            tmproObj.transform.parent = canvasObj.transform;
            tmpro = tmproObj.AddComponent<TextMeshProUGUI>();
            tmpro.alignment = TextAlignmentOptions.Center;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            UpdateUI();
        }
        //onjoin/onleave
        void UpdateUI()
        {
            if (!inRoom)
            {
                tinyTweaks.log("PlayerCounter: error, not connected to room", true);
                StartCoroutine(tryAgainIn5Seconds());
                return;
            }
            tmpro.enabled = show.Value;
            tmpro.rectTransform.position = new Vector2(xPos.Value, yPos.Value);
            tmpro.text = $"{playerCount}/{maxPlayers}";
            tmpro.fontSize = size.Value;
            if (usepeakFont.Value)
            {
                if (peakFont == null)
                    peakFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(font => font.name == "DarumaDropOne-Regular SDF");
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
            color.Value = color.Value.Replace("#", "");
            if (color.Value.Length == 6)
            {
                tmpro.text = $"<color=#{color.Value}>{tmpro.text}</color>";
            }
            else tinyTweaks.log("color is not a hex code! Skipping...");
            tinyTweaks.log($"PlayerCounter: In Room! playerCount is ({playerCount}/{maxPlayers})");
        }
        IEnumerator tryAgainIn5Seconds()
        {
            yield return new WaitForSeconds(5);
            UpdateUI();
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
                if (instance == null) return;
                PlayerCounter.instance.UpdateUI();
                tinyTweaks.log($"Someone joined! Updated Counter to: ({playerCount}/{maxPlayers})");
            }
            [HarmonyPatch("OnPlayerLeftRoom")]
            [HarmonyPostfix]
            private static void OnPlayerLeft()
            {
                if (instance == null) return;
                PlayerCounter.instance.UpdateUI();

                tinyTweaks.log($"Someone left! Updated Counter to: ({playerCount}/{maxPlayers})");
            }
        }
        #endregion
    }
}
