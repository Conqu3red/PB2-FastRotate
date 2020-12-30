using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FastRotate
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    //[BepInDependency(PolyTechFramework.PolyTechMain.pluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("Poly Bridge 2.exe")]
    public class PluginMain : BaseUnityPlugin
    {
        [Header("Hello decompiler!")]
        public const String PluginGuid = "polytech.fastrotate";
        public const String PluginName = "Fast Rotate";
        public const String PluginVersion = "0.5.0";

        private static BepInEx.Logging.ManualLogSource staticLogger;

        public static ConfigEntry<bool> ModIsEnabled;
        public static ConfigEntry<bool> ShowUnroundedDegs;


        public static ConfigEntry<float> ContDegs;
        public static ConfigEntry<KeyboardShortcut> HotkeyContCW;
        public static ConfigEntry<KeyboardShortcut> HotkeyContCCW;

        public static ConfigEntry<float> SingleDegs;
        public static ConfigEntry<KeyboardShortcut> HotkeySingleCW;
        public static ConfigEntry<KeyboardShortcut> HotkeySingleCCW;

        public static bool movedIcons;
        void Awake()
        {
            staticLogger = Logger;

            ModIsEnabled = Config.Bind("͔General", "Mod Enabled", true, new ConfigDescription("Enable/Disable the mod", null, new ConfigurationManagerAttributes { Order = 4 }));
            ShowUnroundedDegs = Config.Bind("͔General", "Show Un-Rounded Degrees", false, new ConfigDescription("Show the true degree rotation value", null, new ConfigurationManagerAttributes { Order = 4 }));

            SingleDegs = Config.Bind("͔Single Degree Rotation", "Single Degree Multiplier", 45f, new ConfigDescription("Degrees to rotate", null, new ConfigurationManagerAttributes { Order = 4 }));
            HotkeySingleCCW = Config.Bind("͔Single Degree Rotation", "Single Degree Counter-Clockwise Keybind", new KeyboardShortcut(KeyCode.Q, KeyCode.LeftControl), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
            HotkeySingleCW = Config.Bind("͔Single Degree Rotation", "Single Degree Clockwise Keybind", new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));

            ContDegs = Config.Bind("Rotation Speed Multiplier", "Rotation Speed Multiplier", 5f, new ConfigDescription("Speed multiplier to rotate at.", null, new ConfigurationManagerAttributes { Order = 4 }));
            HotkeyContCCW = Config.Bind("Rotation Speed Multiplier", "Fast Rotation Counter-Clockwise Keybind", new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
            HotkeyContCW = Config.Bind("Rotation Speed Multiplier", "Fast Rotation Clockwise Keybind", new KeyboardShortcut(KeyCode.E, KeyCode.LeftShift), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));

            ShowUnroundedDegs.SettingChanged += (o, e) =>
            {
                EditClipboardUI(ShowUnroundedDegs.Value);
            };
            ModIsEnabled.SettingChanged += (o, e) =>
            {
                EditClipboardUI(ShowUnroundedDegs.Value);
            };

            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            //PolyTechMain.registerMod(this);
        }

        public static void EditClipboardUI(bool enabled)
        {
            GameObject clipboardPanelGameOBJ = GameObject.Find("/GameUI/Panel_Clipboard/");
            Panel_Clipboard clipboardPanel = clipboardPanelGameOBJ.GetComponent<Panel_Clipboard>();

            var offset = new Vector3(0, 7);
            var scale = new Vector3(1.5f, 1.5f, 1.5f);

            if (enabled) 
            {
                clipboardPanel.m_RotationText.enableWordWrapping = false; //Disable word wrap
                clipboardPanel.m_RotationText.transform.SetAsLastSibling(); //Move to top
                clipboardPanel.m_RotationText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 30); //Make it wider
                clipboardPanel.m_RotationText.transform.localScale = scale;

                if (!movedIcons) //Move icons up
                {
                    clipboardPanel.m_RotateLeft.transform.localPosition += offset;
                    clipboardPanel.m_RotateRight.transform.localPosition += offset;
                    movedIcons = true;
                }
            }
            else 
            {
                if (movedIcons) //Move icons back down
                {
                    clipboardPanel.m_RotateLeft.transform.localPosition -= offset;
                    clipboardPanel.m_RotateRight.transform.localPosition -= offset;
                    movedIcons = false;
                }
                clipboardPanel.m_RotationText.transform.localScale = Vector3.one;
            }
        }

        //void Update() {}

        static bool ConfigKeyIsDown(ConfigEntry<KeyboardShortcut> key)
        {
            if (!ModIsEnabled.Value) { return false; }

            bool modifiersdown = true;
            foreach (KeyCode modifier in key.Value.Modifiers)
            {
                if (Input.GetKey(modifier) == false) { modifiersdown = false; }
            }
            return Input.GetKey(key.Value.MainKey) && modifiersdown;
        }

        [HarmonyPatch(typeof(ClipboardManager), "ContinuousRotateLeft")]
        static class Patch_ClipboardManager_ContinuousRotateLeft
        {
            [HarmonyPrefix]
            static void Prefix(ref float ___ROTATION_DEGREES_PER_SECOND)
            {
                //staticLogger.LogInfo("Patch_ClipboardManager_ContinuousRotateLeft");
                //staticLogger.LogInfo("HotkeyContCW: " + ConfigKeyIsDown(HotkeyContCW));
                //staticLogger.LogInfo("HotkeyContCCW: " + ConfigKeyIsDown(HotkeyContCCW));
                if (ConfigKeyIsDown(HotkeyContCCW))
                {
                    //staticLogger.LogInfo("rotating " + ContDegs.Value);
                    ___ROTATION_DEGREES_PER_SECOND = 45 * ContDegs.Value;
                }
                else
                {
                    //staticLogger.LogInfo("rotating " + 1);
                    ___ROTATION_DEGREES_PER_SECOND = 45 * 1;
                }
            }
        }

        [HarmonyPatch(typeof(ClipboardManager), "ContinuousRotateRight")]
        static class Patch_ClipboardManager_ContinuousRotateRight
        {
            [HarmonyPrefix]
            static void Prefix(ref float ___ROTATION_DEGREES_PER_SECOND)
            {
                //staticLogger.LogInfo("Patch_ClipboardManager_ContinuousRotateRight");
                //staticLogger.LogInfo("HotkeyContCW: " + ConfigKeyIsDown(HotkeyContCW));
                //staticLogger.LogInfo("HotkeyContCCW: " + ConfigKeyIsDown(HotkeyContCCW));
                if (ConfigKeyIsDown(HotkeyContCW))
                {
                    //staticLogger.LogInfo("rotating " + ContDegs.Value);
                    ___ROTATION_DEGREES_PER_SECOND = 45 * ContDegs.Value;
                }
                else
                {
                    //staticLogger.LogInfo("rotating " + 1);
                    ___ROTATION_DEGREES_PER_SECOND = 45 * 1;
                }
            }
        }

        [HarmonyPatch(typeof(ClipboardManager), "Rotate")]
        static class Patch_ClipboardManager_Rotate
        {
            [HarmonyPrefix]
            static void Prefix(ref float value)
            {
                //staticLogger.LogInfo("HotkeySingleCW: " + ConfigKeyIsDown(HotkeySingleCW));
                //staticLogger.LogInfo("HotkeySingleCCW: " + ConfigKeyIsDown(HotkeySingleCCW));
                if (ConfigKeyIsDown(HotkeySingleCW) || ConfigKeyIsDown(HotkeySingleCCW))
                {
                    value *= SingleDegs.Value;
                }
            }
        }

        [HarmonyPatch(typeof(Panel_Clipboard), "GetRotationText")]
        static class Patch_Panel_Clipboard_GetRotationText
        {
            [HarmonyPrefix]
            static bool Prefix(ref Panel_Clipboard __instance, ref string __result)
            {
                if (ModIsEnabled.Value && ShowUnroundedDegs.Value)
                {
                    __result = ClipboardManager.GetRotationDegrees().ToString() + "º";
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Panel_Clipboard), "OnEnable")]
        static class Patch_Panel_Clipboard_OnEnable
        {
            [HarmonyPrefix]
            static void Prefix()
            {
                EditClipboardUI(ModIsEnabled.Value && ShowUnroundedDegs.Value);
            }
        }
    }
}
