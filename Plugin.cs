﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrossHair
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
		private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

		public static ConfigEntry<string> CrossHairText;
		public static ConfigEntry<float> CrossHairSize;
		public static ConfigEntry<int> CrossHairColor_RED;
		public static ConfigEntry<int> CrossHairColor_GREEN;
		public static ConfigEntry<int> CrossHairColor_BLUE;
		public static ConfigEntry<int> CrossHairColor_ALPHA;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			this.ConfigFile();
			harmony.PatchAll();
        }

		private void ConfigFile() {
			CrossHairText = Config.Bind("General", "CrossHairText", "-  +  -", "Text to display as crosshair (use \\n for new line)");
			Logger.LogInfo($"CrossHairText: {CrossHairText.Value}");

			CrossHairSize = Config.Bind("General", "CrossHairSize", 40f, "Size of the crosshair");
			Logger.LogInfo($"CrossHairSize: {CrossHairSize.Value}");

			CrossHairColor_RED = Config.Bind("Color", "CrossHairColor_RED", 255, "Red value of the crosshair");
			CrossHairColor_GREEN = Config.Bind("Color", "CrossHairColor_GREEN", 255, "Green value of the crosshair");
			CrossHairColor_BLUE = Config.Bind("Color", "CrossHairColor_BLUE", 255, "Blue value of the crosshair");
			CrossHairColor_ALPHA = Config.Bind("Color", "CrossHairColor_ALPHA", 50, "Alpha value of the crosshair");

			Logger.LogInfo($"CrossHairColor: ({CrossHairColor_RED.Value}, {CrossHairColor_GREEN.Value}, {CrossHairColor_BLUE.Value}, {CrossHairColor_ALPHA.Value})");
		}
    }


	[HarmonyPatch(typeof(HUDManager))]
	internal class HUDManagerPatch
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		private static void Start(ref HUDManager __instance) {
			// cursor path: System/UI/Canvas/PlayerCursor/Cursor 
			GameObject crossHair = new GameObject("CrossHair");
			Transform parent = __instance.PTTIcon.transform.parent.parent.parent.Find("PlayerCursor").Find("Cursor").transform;

			crossHair.AddComponent<RectTransform>();
			TextMeshProUGUI text = crossHair.AddComponent<TextMeshProUGUI>();
			RectTransform rect = text.rectTransform;
			rect.SetParent(parent, false);
			rect.anchoredPosition = new Vector2(0, 0);
			rect.localPosition = new Vector3(0, 0, 0);

			text.text = Plugin.CrossHairText.Value;
			text.fontSize = Plugin.CrossHairSize.Value;
			text.color = new Color32(
				(byte)Plugin.CrossHairColor_RED.Value,
				(byte)Plugin.CrossHairColor_GREEN.Value,
				(byte)Plugin.CrossHairColor_BLUE.Value,
				(byte)Plugin.CrossHairColor_ALPHA.Value
			);

			text.alignment = TextAlignmentOptions.Center;
			text.font = __instance.controlTipLines[0].font;
			text.overflowMode = 0;
			text.enabled = true;

			GameObject shadow = GameObject.Instantiate(crossHair, parent);
			TextMeshProUGUI shadowText = shadow.GetComponent<TextMeshProUGUI>();
			shadow.name = "CrossHairShadow";
			shadowText.fontSize = Plugin.CrossHairSize.Value;
			shadowText.color = new Color32(byte.MinValue, byte.MinValue, byte.MinValue, 100);
			shadowText.rectTransform.localPosition = new Vector3(2, -2, 0);

			rect.SetAsLastSibling();
		}
	}
}