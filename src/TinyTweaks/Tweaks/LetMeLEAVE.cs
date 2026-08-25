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
        static float timeToHold = 5f;
        static float timeHeld = 0f;
        
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
        {//timetoHold =5f
            if (SceneManager.GetActiveScene().name == "Title") return;
            if (Input.GetKey(leaveKeybind.Value))
                timeHeld += Time.deltaTime;
            else timeHeld = 0;
            if (timeHeld > timeToHold)
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
                timeHeld = -999f;
                //SceneManager.LoadScene("Title");
            }
        }
        public static void Start()
        {
            leaveKeybind = tinyTweaks.config.Bind("Shorties", "Leave button", KeyCode.None);
        }
    }
}
 