using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using PEAKBending;
using TinyTweaks.Tweaks;
using UnityEngine;

namespace TinyTweaks
{
    /// <summary>
    /// TODO:
    ///     NoobSash
    ///     showNamesAlways
    ///     dead+passed out eyes
    ///     Basketball aimbot!=
    ///     postfix drop all items that can't be dropped
    ///     Bingbong Always same answer!=
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
            BingBongSays.Binds();
            TweaksObj = new GameObject("Tweaks!");
            DontDestroyOnLoad(TweaksObj);
            TweaksObj.AddComponent<ItemAimbotFinder>();
            Log.LogInfo($"Plugin {Name} is loaded!");
        }
    }
}
