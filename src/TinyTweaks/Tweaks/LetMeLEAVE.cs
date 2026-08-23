using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace TinyTweaks.Tweaks
{
    internal class LetMeLEAVE
    {
        static ConfigEntry<KeyCode> leaveKeybind;
        static GameObject loadingScreen;
        
        [HarmonyPatch(typeof(LoadingScreen), "Awake")]
        static class patches
        {
            [HarmonyPostfix]
            static void removeLoadingScreenPatch(LoadingScreen __instance)
            {
                tinyTweaks.log("found loadingScreen");
                loadingScreen = __instance.gameObject;
            }
        }

        public static void Update()
        {
            if (SceneManager.GetActiveScene().name != "Title" && Input.GetKeyDown(leaveKeybind.Value))
            {
                Player.LeaveCurrentGame();
                var text = loadingScreen?.transform.Find("LoadingText");
                if (text != null)
                {
                    text.gameObject.SetActive(false);
                }
                var black = loadingScreen?.transform.Find("Black");
                if (black != null)
                {
                    black.gameObject.SetActive(false);
                }

                //SceneManager.LoadScene("Title");
            }
        }
        public static void Start()
        {
            leaveKeybind = tinyTweaks.config.Bind("Shorties", "Leave button", KeyCode.None);
        }
    }
}
 