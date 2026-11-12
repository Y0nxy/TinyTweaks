using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using System.Collections;
using TinyTweaks.Tweaks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TinyTweaks
{
    [BepInAutoPlugin]
    [BepInDependency("com.borealityy.peaktextchat",BepInDependency.DependencyFlags.SoftDependency)]
    public partial class tinyTweaks : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; } = null!;
        public static  ConfigFile config;
        GameObject TweaksObj;
        Harmony harmony;
        public static tinyTweaks Instance;
        static bool hasPeakTextChat = false;
        private void Awake()
        {
            Instance = this;
            Log = Logger;
            config = this.Config;
            SceneManager.sceneLoaded += OnSceneChanged;
            harmony = new Harmony("TinyTweaks!");
            StartTweaks();
            harmony.PatchAll();
            Log.LogInfo($"Plugin {Name} is loaded!");
        }

        void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Title")
            {
                LoadingScreenHandler.loading = false;
                return;
            }
            if (TweaksObj != null)
            {
                Destroy(TweaksObj);
            }
            TweaksObj = new GameObject("Tweaks!");
            if (scene.name == "Airport")
            {
                allElevators.inAirport = true;
                log("In Airport!");
                StartCoroutine(checkIsHostDelayed());
            }
            else allElevators.inAirport = false;
            TweaksObj.AddComponent<moveAscent>();
            TweaksObj.AddComponent<PlayerCounter>();
        }
        private void StartTweaks()
        {
            Customizations.Start();
            ExtraMarshmallows.Start();
            ItemAimbotFinder.Binds();
            showNamesAlways.Start();
            BingBongSays.Start();
            noBonusStaminaFromJumps.Start();
            if (Chainloader.PluginInfos.ContainsKey("com.borealityy.peaktextchat"))
            {
                hasPeakTextChat = true;
                textChatCommands.Binds(harmony);
            }
            ChangeRopeToRed.Start();
            LetMeLEAVE.Start();
            Shields.Start();
            allElevators.Binds();
            removePromptNameOrGlow.Binds();
            AnimationOverride.Binds();
            moveAscent.Binds();
            PlayerCounter.Binds();
        }
        public static void Notification(string message, string color = "FFFFFF", bool sound = false)
        {
            log("Notification: " + message);
            PlayerConnectionLog connectionLog = FindAnyObjectByType<PlayerConnectionLog>();
            if (connectionLog == null) return;
            string formattedMessage = $"<color=#{color}>{message}</color>";
            connectionLog.AddMessage(formattedMessage);
            if (connectionLog.sfxJoin != null && sound)
            {
                connectionLog.sfxJoin.Play(default(Vector3));
            }
        }
        public static void log(string message, bool isError = false)
        {
            if (isError)
                Log.LogError(message);
            else
                Log.LogInfo(message);
        }
        public static void logMessage(string message)
        {
            if (hasPeakTextChat)
                textChatCommands.logMessage(message);
            else
                log("no peaktextChat, msg: " + message);
        }
        void Update()
        {
            Shields.UpdateTimer();
            LetMeLEAVE.Update();
            if (GUIManager.instance != null && GUIManager.instance.windowBlockingInput) return; //no keypress when typing in chat or using menus
            Customizations.Update();
            AnimationOverride.Update();
        }
        void OnDestroy()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
            SceneManager.sceneLoaded -= OnSceneChanged;
            if (TweaksObj != null)
                Destroy(TweaksObj);
            log("Unloading mod " + Name);
        }

        IEnumerator checkIsHostDelayed()
        {
            yield return new WaitForSeconds(10);
            if (!PhotonNetwork.IsMasterClient) {
                log($"Host is {PhotonNetwork.MasterClient}");
                yield break;
            }
            ChangeRopeToRed.ropeAirport();
            log("Loading Basketball Aimbot!");
            TweaksObj.AddComponent<ItemAimbotFinder>();
        }
    }
}
