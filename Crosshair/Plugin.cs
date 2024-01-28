using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

using CrossHair.Utilities;
using System.Collections;

namespace CrossHair
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
		private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

		public static new ConfigFile Config = new ConfigFile(Paths.ConfigPath + "\\" + PluginInfo.PLUGIN_GUID + ".cfg", true);

		public static ConfigEntry<string> CrossHairText;
		public static ConfigEntry<float> CrossHairSize;
		public static ConfigEntry<bool> CrossHairShadow;

		public static ConfigEntry<string> CrossHairColor;
		public static ConfigEntry<int> CrossHairOpacity;
		public static ConfigEntry<bool> CrossHairFading;

		public static ManualLogSource CLog;

        private void Awake() {
			CLog = Logger;
            CLog.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			this.ConfigFile();
			harmony.PatchAll();
		}

		private void ConfigFile() {
			CrossHairText = Config.Bind("!General", "CrossHairText", "-  +  -", "Text to display as crosshair (use \\n for new line)");
			CrossHairSize = Config.Bind("!General", "CrossHairSize", 50f, "Size of the crosshair");
			CrossHairShadow = Config.Bind("!General", "CrossHairShadow", true, "Whether to display a shadow behind the crosshair");

			CrossHairColor = Config.Bind("Appearance", "CrossHairColor", "ffffff", "Color of the crosshair in hexadecimal (Do not include the #)");
			CrossHairOpacity = Config.Bind("Appearance", "CrossHairOpacity", 80, "Opacity of the crosshair (0 to 100)%");
			CrossHairFading = Config.Bind("Appearance", "CrossHairFading", true, "Whether the crosshair should fade in and out in specific situations");

			Console.LogMessage($"CrossHairText: {CrossHairText.Value}");
			Console.LogMessage($"CrossHairSize: {CrossHairSize.Value}");
			Console.LogMessage($"CrossHairShadow: {CrossHairShadow.Value}");

			Console.LogMessage($"CrossHairColor: {CrossHairColor.Value}");
			Console.LogMessage($"CrossHairOpacity: {CrossHairOpacity.Value}");
			Console.LogMessage($"CrossHairFading: {CrossHairFading.Value}");

			Config.Save();
		}
	}
}


