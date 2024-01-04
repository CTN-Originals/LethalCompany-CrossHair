using UnityEngine;
using TMPro;
using HarmonyLib;

namespace CrossHair.Patches 
{
	[HarmonyPatch(typeof(HUDManager))]
	internal class HUDManagerPatch 
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		private static void Start(ref HUDManager __instance) {
			//> cursor path: System/UI/Canvas/PlayerCursor/Cursor
			//> Reference obj: Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText (1)
			GameObject referenceText = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText (1)");
			GameObject crossHair = GameObject.Instantiate(referenceText);
			crossHair.name = "CrossHair";

			Plugin.crossHair = crossHair;
			Transform parent = __instance.PTTIcon.transform.parent.parent.parent.Find("PlayerCursor").Find("Cursor").transform;

			TextMeshProUGUI text = crossHair.GetComponent<TextMeshProUGUI>();

			RectTransform rect = text.rectTransform;
			rect.SetParent(parent, false);
			rect.anchoredPosition = new Vector2(0, 0);
			rect.localPosition = new Vector3(0, 0, 0);
			rect.offsetMin = new Vector2(-500, -500);
			rect.offsetMax = new Vector2(500, 500);
			
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
			text.enabled = true;

			if (Plugin.CrossHairShadow.Value != true) { return; }

			GameObject shadow = GameObject.Instantiate(crossHair, parent);
			Plugin.crossHairShadow = shadow;
			TextMeshProUGUI shadowText = shadow.GetComponent<TextMeshProUGUI>();
			shadow.name = "CrossHairShadow";
			shadowText.fontSize = Plugin.CrossHairSize.Value;
			shadowText.color = new Color32(byte.MinValue, byte.MinValue, byte.MinValue, 100);
			shadowText.rectTransform.localPosition = new Vector3(2, -2, 0);

			rect.SetAsLastSibling();
		}


		[HarmonyPatch("AddChatMessage")]
		[HarmonyPostfix]
		private static void AddChatMessage(ref HUDManager __instance) {
			string message = __instance.lastChatMessage;
			// Debug.Log("Chat message: " + message);
			if (message.StartsWith("/update")) {
				if (Plugin.crossHair) UpdateCrossHairValues(Plugin.crossHair.GetComponent<TextMeshProUGUI>());
				if (Plugin.crossHairShadow) UpdateCrossHairValues(Plugin.crossHairShadow.GetComponent<TextMeshProUGUI>(), false);
			}
		}

		public static void UpdateCrossHairValues(TextMeshProUGUI element, bool color = true) {
			Debug.Log("Updating crosshair values");

			element.text = Plugin.CrossHairText.Value;
			element.fontSize = Plugin.CrossHairSize.Value;
			if (color) {
				element.color = new Color32(
					(byte)Plugin.CrossHairColor_RED.Value,
					(byte)Plugin.CrossHairColor_GREEN.Value,
					(byte)Plugin.CrossHairColor_BLUE.Value,
					(byte)Plugin.CrossHairColor_ALPHA.Value
				);
			}
		}
	}
}