using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using System.Collections;
using System.Reflection;
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
            TweaksObj = new GameObject("Tweaks!");
            if (scene.name == "Airport")
            {
                allElevators.inAirport = true;
                log("In Airport!");
                StartCoroutine(checkIsHostDelayed());
            }
            else allElevators.inAirport = false;
            TweaksObj.AddComponent<moveVersion>();
        }
        private void StartTweaks()
        {
            Customizations.Start();
            ExtraMarshmallows.Start();
            ItemAimbotFinder.Binds();
            showNamesAlways.Start();
            moveVersion.Binds();
            BingBongSays.Start();
            noBonusStaminaFromJumps.Start();
            textChatCommands.CheckforPeakTextChat(harmony);
            ChangeRopeToRed.Start();
            LetMeLEAVE.Start();
            Shields.Start();
            allElevators.Binds();
        }
        public static void Notification(string message, string color = "FFFFFF", bool sound = false)
        {
            PlayerConnectionLog connectionLog = UnityEngine.Object.FindAnyObjectByType<PlayerConnectionLog>();
            if (connectionLog == null)
            {
                return;
            }
            string formattedMessage = string.Concat(new string[] { "<color=#", color, ">", message, "</color>" });
            MethodInfo addMessageMethod = typeof(PlayerConnectionLog).GetMethod("AddMessage", BindingFlags.Instance | BindingFlags.NonPublic);
            if (addMessageMethod != null)
            {
                addMessageMethod.Invoke(connectionLog, new object[] { formattedMessage });
                if (connectionLog.sfxJoin != null && sound)
                {
                    connectionLog.sfxJoin.Play(default(Vector3));
                    return;
                }
            }
            else
            {
                Log.LogMessage("AddMessage method not found.");
            }
        }
        public static void log(string message)
        {
            Log.LogInfo(message);
        }
        public static void logMessage(string message) => textChatCommands.logMessage(message);
        void Update()
        {
            if (GUIManager.instance != null && GUIManager.instance.windowBlockingInput) return; //no keypress when typing in chat or using menus
            Customizations.Update();
            LetMeLEAVE.Update();
        }
        void OnDestroy()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
                SceneManager.sceneLoaded -= OnSceneChanged;
                if (TweaksObj != null)
                    Destroy(TweaksObj);
                log("Unloading mod " + Name);
            }
        }

        IEnumerator checkIsHostDelayed()
        {
            yield return new WaitForSeconds(10);
            if (!PhotonNetwork.IsMasterClient) {
                log($"Host is {PhotonNetwork.MasterClient}");
                yield break;
            }
            log("Loading Basketball Aimbot!");
            TweaksObj.AddComponent<ItemAimbotFinder>();
        }
    }
}
