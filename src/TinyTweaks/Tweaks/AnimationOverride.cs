using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TinyTweaks.Tweaks
{
    internal class AnimationOverride
    {
        private static ScoutAnimations currentAnimation;
        static float timeSinceBackflip = 0;
        static bool isBackflipping
        { 
            get
            {
                return Time.time < timeSinceBackflip + 1.35f;
            }
        }
        static ConfigEntry<bool> overrideAnimations;
        static ConfigEntry<ScoutAnimations> idle;
        static ConfigEntry<ScoutAnimations> sprinting;
        static ConfigEntry<ScoutAnimations> walkingForward;
        static ConfigEntry<ScoutAnimations> walkingBackward;
        static ConfigEntry<ScoutAnimations> walkingLeft;
        static ConfigEntry<ScoutAnimations> walkingRight;
        static List<string> scoutAnimCache = new List<string>();
        static ConfigEntry<bool> useZombiePreset;
        static ScoutAnimations[] selectedPreset = new ScoutAnimations[6];
        static ScoutAnimations[] defaultPreset = new ScoutAnimations[6]; 
        static ScoutAnimations[] zombiePreset = new ScoutAnimations[6] {
            ScoutAnimations.A_Zombie_Idle, ScoutAnimations.A_Zombie_Run, ScoutAnimations.A_Zombie_Walk,
            ScoutAnimations.DontApply, ScoutAnimations.DontApply, ScoutAnimations.DontApply};
        public static void Binds()
        {
            overrideAnimations = tinyTweaks.config.Bind("Animation", "Override Animations", true);
            useZombiePreset = tinyTweaks.config.Bind("Animation", "Zombie Preset", true);
            idle = tinyTweaks.config.Bind("Animation", "Idle Animation", ScoutAnimations.A_Scout_Swim_Idle);
            sprinting = tinyTweaks.config.Bind("Animation", "Sprinting Animation", ScoutAnimations.A_Scout_Emote_Panic);
            walkingForward = tinyTweaks.config.Bind("Animation", "Walking Forward Animation", ScoutAnimations.A_Scout_Swim_FWD);
            walkingBackward = tinyTweaks.config.Bind("Animation", "Walking Backward Animation", ScoutAnimations.A_Scout_Swim_Back);
            walkingRight = tinyTweaks.config.Bind("Animation", "Walking Right Animation", ScoutAnimations.A_Scout_Swim_R);
            walkingLeft = tinyTweaks.config.Bind("Animation", "Walking Left Animation", ScoutAnimations.A_Scout_Swim_L);
            //scoutAnims = tinyTweaks.config.Bind("Animation", "Scout Animations", new List<string> { "Scout_Idle", "Scout_Run" }, "List of animations to use for the scout character.");
            useZombiePreset.SettingChanged += (_, _) => updatePresets();
            overrideAnimations.SettingChanged += (_, _) => updatePresets();
            idle.SettingChanged += (_, __) => updatePresets();
            sprinting.SettingChanged += (_, __) => updatePresets();
            walkingForward.SettingChanged += (_, __) => updatePresets();
            walkingBackward.SettingChanged += (_, __) => updatePresets();
            walkingRight.SettingChanged += (_, __) => updatePresets();
            walkingLeft.SettingChanged += (_, __) => updatePresets();
            updatePresets();
        }

        private static void updatePresets()
        {
            defaultPreset = new ScoutAnimations[6] {idle.Value, sprinting.Value, walkingForward.Value, walkingBackward.Value, walkingRight.Value, walkingLeft.Value};
            if (useZombiePreset.Value) setPreset(zombiePreset);
            else setPreset(defaultPreset);
        }

        static void setPreset(ScoutAnimations[] newPreset)
        {
            if (newPreset != null && newPreset.Length == 6)
            {
                selectedPreset = (ScoutAnimations[])newPreset;
            }
        }
        #region removeUnusedAnimations
        //static List<string> invalidAnimations = new List<string>();
        //static int index = 0;
        //public static void Update()
        //{
        //    bool pressedBack = Input.GetKeyDown(KeyCode.G);
        //    if (pressedBack || Input.GetKeyDown(KeyCode.H))
        //    {
        //        if (pressedBack)
        //            index--;
        //        else
        //            index++;
        //        ScoutAnimations anim = (ScoutAnimations)index;
        //        if (index >= Enum.GetValues(typeof(ScoutAnimations)).Length)
        //        {
        //            string finalText = "Invalid Animations:\n";
        //            foreach (var invalidAnimation in invalidAnimations)
        //            {
        //                finalText += invalidAnimation + ",\n";
        //            }
        //            GUIUtility.systemCopyBuffer = finalText;
        //            return;
        //        }
        //        tinyTweaks.logMessage("playing animation: " + anim.ToString());
        //        playAnimation(anim);
        //    }
        //    if (Input.GetKeyDown(KeyCode.B))
        //    {
        //        //tinyTweaks.logMessage("adding invalid animation: " + currentAnimation);
        //        //invalidAnimations.Add(currentAnimation);
        //    }
        //}
        #endregion
        #region get all animations for enum
        static void getAllAnimationsForBuild()
        {
            var clips = Resources.FindObjectsOfTypeAll<AnimationClip>();
            foreach (var clip in clips)
            {
                //if (clip.name.Contains("Scout"))
                //{
                    if (!scoutAnimCache.Contains(clip.name))
                        scoutAnimCache.Add(clip.name);
                //}
            }
            tinyTweaks.log($"found {scoutAnimCache.Count} scout animations\n{string.Join("\n", scoutAnimCache)}");
            string finalText = "";
            scoutAnimCache.Sort();
            foreach (var anim in scoutAnimCache)
            {
                finalText += anim + ",\n";
            }
            GUIUtility.systemCopyBuffer = finalText;

        }
        #endregion
        public static void emote(string emoteName)
        {
            var character = Character.localCharacter;
            var localCharacterView = character?.refs?.view;
            if (localCharacterView == null) return;
            localCharacterView.RPC("RPCA_PlayRemove", RpcTarget.All, emoteName, false);
            var anims = character.refs.animations;
            if (anims != null)
            {
                anims.emoting = false;
            }
            //Log.LogInfo($"played {emoteName} emote");
        }
        static void playAnimation(ScoutAnimations animation)
        {
            if (animation == currentAnimation) return;
            currentAnimation = animation;
            if (animation == ScoutAnimations.DontApply)
            {
                //var animator = Character.localCharacter?.refs?.animator;
                //if (animator != null)
                //    animator.SetBool("Emote", false);
                emote(ScoutAnimations.A_Scout_Land.ToString());
                
                return;
            }
            var animationName = animation.ToString();
            emote(animationName);
        }
        static IEnumerator playJumpDelayed(bool isSprinting)
        {
            if (currentAnimation == ScoutAnimations.DontApply) yield break;
            yield return new WaitForSeconds(.2f);
            if (isSprinting)
                playAnimation(ScoutAnimations.A_Scout_Fall_Sprint);
            else playAnimation(ScoutAnimations.DontApply);
        }
        [HarmonyPatch(typeof(CharacterAnimations))]
        static class patches
        {
            [HarmonyPatch(nameof(CharacterAnimations.PlayEmote))]
            [HarmonyPostfix]
            static void trackCurrentEmote(string emoteName)
            {
                if (emoteName == "A_Scout_Emote_BackFlip")
                {
                    timeSinceBackflip = Time.time;
                }
            }
            [HarmonyPatch("Update")]
            [HarmonyPostfix]
            public static void overrideAnimation(CharacterAnimations __instance)
            {
                if (!overrideAnimations.Value) return;
                //tinyTweaks.log("AnimationOverride Postfix called");
                var character = __instance.character;
                Animator animator = character?.refs?.animator;
                if (animator == null || !character.IsLocal) return;

                bool isGrounded = animator.GetBool("Is Grounded");
                bool isSprinting = character.data.isSprinting;
                //if (__instance.sinceEmoteStart < 1f)
                if (isBackflipping)
                    return;
                if (character.input.jumpWasPressed || character.data.isJumping || !isGrounded)
                {
                    tinyTweaks.Instance.StartCoroutine(playJumpDelayed(isSprinting));
                    return;
                }
                var forward = animator.GetFloat(__instance.AN_INPUTY);
                var side = animator.GetFloat(__instance.AN_INPUTX);
                //tinyTweaks.log($"Forward: {forward:2F}, Side: {side:2F}, IsSprinting: {isSprinting}");
                bool moving = forward != 0 || side != 0;
                if (moving)
                {
                    if (forward > 0)
                    {
                        if (isSprinting)
                        {
                            //tinyTweaks.log("Sprinting");
                            playAnimation(selectedPreset[1]);
                            return;
                        }
                        playAnimation(selectedPreset[2]);
                        //tinyTweaks.log("Walking Forward");
                    }
                    else if (forward < 0)
                    {
                        //tinyTweaks.log("Walking Backward");
                        playAnimation(selectedPreset[3]);
                    }
                    else if (side > 0)
                    {
                        //tinyTweaks.log("Walking Right");
                        playAnimation(selectedPreset[4]);
                    }
                    else if (side < 0)
                    {
                        //tinyTweaks.log("Walking Left");
                        playAnimation(selectedPreset[5]);
                    }
                }
                else
                {
                    //tinyTweaks.log("Idle");
                    playAnimation(selectedPreset[0]);
                }
            }
        }
        enum ScoutAnimations
        {
            DontApply,
            A_Scout_Climb_Down,
            A_Scout_Climb_Fall,
            A_Scout_Climb_Jump,
            A_Scout_Climb_Jump_L,
            A_Scout_Climb_Jump_R,
            A_Scout_Climb_L,
            A_Scout_Climb_R,
            A_Scout_Climb_Rope_Down,
            A_Scout_Climb_Rope_Idle,
            A_Scout_Climb_Rope_Up,
            A_Scout_Climb_Slide_Fast,
            A_Scout_Climb_Up,
            A_Scout_Climb_Velocity,
            A_Scout_Crouch_BCK,
            A_Scout_Crouch_HelpFriend,
            A_Scout_Crouch_LFT,
            A_Scout_Crouch_RGT,
            A_Scout_Emote_BackFlip,
            A_Scout_Emote_Cinema,
            A_Scout_Emote_Clap,
            A_Scout_Emote_Crashout,
            A_Scout_Emote_CrossedArms,
            A_Scout_Emote_Dance1,
            A_Scout_Emote_Dance2,
            A_Scout_Emote_Flex,
            A_Scout_Emote_Nono,
            A_Scout_Emote_Panic,
            A_Scout_Emote_Salute,
            A_Scout_Emote_Shrug,
            A_Scout_Emote_Sit,
            A_Scout_Emote_Think,
            A_Scout_Emote_ThumbsUp,
            A_Scout_Fall_Neutral,
            A_Scout_Fall_Up,
            A_Scout_Getup_Back,
            A_Scout_Hands,
            A_Scout_Idle_Lean_B,
            A_Scout_Idle_Lean_F,
            A_Scout_Idle_Lean_L,
            A_Scout_Idle_Lean_R,
            A_Scout_Idle1,
            A_Scout_Idle2,
            A_Scout_Idle3,
            A_Scout_Idle4,
            A_Scout_Idle5,
            A_Scout_Idle6,
            A_Scout_Idle7,
            A_Scout_Idle8,
            A_Scout_Jog,
            A_Scout_Jog_Slope_Down,
            A_Scout_Jog_Slope_L,
            A_Scout_Jog_Slope_R,
            A_Scout_Jog_Slope_Up,
            A_Scout_Jump_Squat,
            A_Scout_Kick,
            //A_Scout_KickDown,
            A_Scout_Land,
            A_Scout_LedgeGrab,
            A_Scout_OnBack,
            A_Scout_Reach_Air,
            A_Scout_Reach_Air_Down,
            A_Scout_Reach_Air_Up,
            A_Scout_Reach_Crouch,
            A_Scout_Sneak,
            A_Scout_Sprint_Lean_B,
            A_Scout_Sprint_Lean_F,
            A_Scout_Strafe_L,
            A_Scout_Strafe_R,
            A_Scout_Swim_Back,
            A_Scout_Swim_FWD,
            A_Scout_Swim_Idle,
            A_Scout_Swim_L,
            A_Scout_Swim_R,
            A_Scout_Swim_Start,
            A_Scout_Throw_Throw,
            A_Scout_ThrowCharge_0,
            A_Scout_ThrowCharge_Down,
            A_Scout_ThrowCharge_Up,
            A_Scout_Upperbody_Point_L_DOWN,
            A_Scout_Upperbody_Point_L_UP,
            A_Scout_Upperbody_Point_R_UP,
            A_Scout_Upperbody_Point_UP,
            A_Scout_Use_Item,
            A_Scout_Use_Item_Antidote,
            A_Scout_Use_Item_Drink,
            A_Scout_Use_Item_Eat,
            A_Scout_Use_Item_Heal,
            A_Scout_VineHang_Fwd,
            //A_Scout_VineHang_Fwd 1,
            //A_Scout_VineHang_Idle 1,
            A_Scout_VineHang_Slide,
            //A_Scout_VineHang_Slide 1,
            A_Scout_VineHang_Slide_Slow,
            //A_Scout_VineHang_Slide_Slow 1,
            A_Scout_WakeUp,
            A_Scout_Walk,
            A_Scout_Wilhelm,
            A_Zombie_Idle,
            A_Zombie_Walk,
            A_Zombie_Run,
            A_Scout_Fall_Sprint,
        }
    }
}
