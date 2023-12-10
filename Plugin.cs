using BepInEx;
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

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
			harmony.PatchAll();
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
			((Transform)rect).SetParent(parent, false);
			rect.anchoredPosition = new Vector2(0, 0);
			rect.localPosition = new Vector3(0, 0, 0);

			text.text = "-  +  -";
			text.fontSize = 40f;
			text.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 50);
			text.alignment = TextAlignmentOptions.Center;
			text.font = __instance.controlTipLines[0].font;
			text.overflowMode = (TextOverflowModes)0;
			text.enabled = true;

			GameObject shadow = GameObject.Instantiate(crossHair, parent);
			TextMeshProUGUI shadowText = shadow.GetComponent<TextMeshProUGUI>();
			shadow.name = "CrossHairShadow";
			shadowText.color = new Color32(byte.MinValue, byte.MinValue, byte.MinValue, 100);
			shadowText.fontSize = 40f;
			shadowText.rectTransform.localPosition = new Vector3(2, -2, 0);

			rect.SetAsLastSibling();
		}
	}
}
