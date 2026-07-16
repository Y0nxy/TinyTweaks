using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using TinyTweaks.Tweaks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TinyTweaks
{
    /// <summary>
    /// TODO:
    ///     Extra Marshmallows
    /// </summary>
    [BepInAutoPlugin]
    public partial class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; } = null!;
        public static  ConfigFile config;
        GameObject TweaksObj;

        private void Awake()
        {
            Log = Logger;
            config = this.Config;
            StartTweaks();
            SceneManager.sceneLoaded += OnSceneChanged;
            Harmony harmony = new Harmony("TinyTweaks!");
            harmony.PatchAll();
            Log.LogInfo($"Plugin {Name} is loaded!");
        }

        void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Airport")
            {
                Log.LogInfo("In Airport! Loading Basketball Aimbot!");
                TweaksObj = new GameObject("Tweaks!");
                TweaksObj.AddComponent<ItemAimbotFinder>();
            }
        }
        private void StartTweaks()
        {
            Customizations.Start();
            BingBongSays.Start();
            showNamesAlways.Start();
            ExtraMarshmallows.Start();
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
    }
}
