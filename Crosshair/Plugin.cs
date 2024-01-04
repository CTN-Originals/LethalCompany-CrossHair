using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

using CrossHair.Utils;

namespace CrossHair
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
		private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

		public static ConfigEntry<string> CrossHairText;
		public static ConfigEntry<float> CrossHairSize;
		public static ConfigEntry<bool> CrossHairShadow;

		public static ConfigEntry<int> CrossHairColor_RED;
		public static ConfigEntry<int> CrossHairColor_GREEN;
		public static ConfigEntry<int> CrossHairColor_BLUE;
		public static ConfigEntry<int> CrossHairColor_ALPHA;

		public static GameObject crossHair;
		public static GameObject crossHairShadow;

		public static Plugin Instance;

		public static ManualLogSource CLog;

        private void Awake()
        {
			if (Instance == null) {Instance = this;}
			
			CLog = Logger;

            // Plugin startup logic
            CLog.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			this.ConfigFile();
			harmony.PatchAll();
        }

		private void ConfigFile() {
			CrossHairText = Config.Bind("General", "CrossHairText", "-  +  -", "Text to display as crosshair (use \\n for new line)");
			Console.LogInfo($"CrossHairText: {CrossHairText.Value}");
			CrossHairSize = Config.Bind("General", "CrossHairSize", 40f, "Size of the crosshair");
			Console.LogInfo($"CrossHairSize: {CrossHairSize.Value}");
			CrossHairShadow = Config.Bind("General", "CrossHairShadow", true, "Whether to display a shadow behind the crosshair");
			Console.LogInfo($"CrossHairShadow: {CrossHairShadow.Value}");

			CrossHairColor_RED = Config.Bind("Color", "CrossHairColor_RED", 255, "Red value of the crosshair");
			CrossHairColor_GREEN = Config.Bind("Color", "CrossHairColor_GREEN", 255, "Green value of the crosshair");
			CrossHairColor_BLUE = Config.Bind("Color", "CrossHairColor_BLUE", 255, "Blue value of the crosshair");
			CrossHairColor_ALPHA = Config.Bind("Color", "CrossHairColor_ALPHA", 50, "Alpha value of the crosshair");
			Console.LogInfo($"CrossHairColor: ({CrossHairColor_RED.Value}, {CrossHairColor_GREEN.Value}, {CrossHairColor_BLUE.Value}, {CrossHairColor_ALPHA.Value})");
		}
	}
}


