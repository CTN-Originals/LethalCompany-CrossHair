using UnityEngine;
using TMPro;
using HarmonyLib;
using CrossHair.Utilities;
using System.Collections.Generic;

namespace CrossHair.Patches 
{
	[HarmonyPatch(typeof(HUDManager))]
	internal class HUDManagerPatch 
	{
		public static Transform CrossHair;
		public static TextMeshProUGUI CrossHairTMP;
		public static float CrossHairAlpha = 200;
		public static Transform CrossHairShadow;
		public static float CrossHairShadowAlpha = 100;

		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		private static void Start(ref HUDManager __instance) {
			//> cursor path: System/UI/Canvas/PlayerCursor/Cursor
			//> Reference obj: Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText (1)
			// List<string> refPaths = new List<string>() {
			// 	"Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText (1)",
			// 	"Environment/HangarShip/Terminal/Canvas/MainContainer/Scroll View/Viewport/InputField (TMP)/Text Area/Text",
			// };
			// GameObject referenceText = new GameObject();
			// referenceText.AddComponent<TextMeshProUGUI>();
			// foreach (string path in refPaths) {
			// 	referenceText = GameObject.Find(path);
			// 	if (referenceText != null) break;
			// }

			CrossHair = GameObject.Instantiate(new GameObject().AddComponent<TextMeshProUGUI>()).transform;
			Console.LogDebug($"{CrossHair.name}");
			CrossHair.name = "CrossHair";

			Transform parent = __instance.PTTIcon.transform.parent.parent.parent.Find("PlayerCursor").Find("Cursor").transform;

			CrossHairTMP = CrossHair.GetComponent<TextMeshProUGUI>();

			RectTransform rect = CrossHairTMP.rectTransform;
			rect.SetParent(parent, false);
			rect.anchoredPosition = new Vector2(0, 0);
			rect.localPosition = new Vector3(0, 0, 0);
			rect.offsetMin = new Vector2(-500, -500);
			rect.offsetMax = new Vector2(500, 500);
			
			string hexColor = Plugin.CrossHairColor.Value;
			if (hexColor.Length != 6) { hexColor = HexFormatException($"character amount: \"{hexColor}\""); }

			int argb = 0xffffff;
			try { argb = int.Parse(hexColor.Replace("#", ""), System.Globalization.NumberStyles.HexNumber); }
			catch (System.FormatException) { argb = int.Parse(HexFormatException($"color: \"{hexColor}\""), System.Globalization.NumberStyles.HexNumber); }
			System.Drawing.Color clr = System.Drawing.Color.FromArgb(argb);

			CrossHairAlpha = (byte)(Plugin.CrossHairOpacity.Value * 255 / 100); //? convert 0 - 100 to 0 - 255
			CrossHairShadowAlpha = (byte)(CrossHairAlpha * 50 / 100); //? Calculate shadow alpha as 50% of the crosshair alpha from 0-100 to 0-255

			CrossHairTMP.text = Plugin.CrossHairText.Value;
			CrossHairTMP.fontSize = Plugin.CrossHairSize.Value;
			CrossHairTMP.color = new Color32(clr.R, clr.G, clr.B, (byte)Mathf.RoundToInt(CrossHairAlpha));

			Console.LogDebug($"CrossHairColor: ({clr.R}, {clr.G}, {clr.B}, {CrossHairAlpha})");

			CrossHairTMP.alignment = TextAlignmentOptions.Center;
			CrossHairTMP.font = __instance.controlTipLines[0].font;
			CrossHairTMP.enabled = true;

			if (Plugin.CrossHairShadow.Value != true) { return; }

			CrossHairShadow = GameObject.Instantiate(CrossHair, parent);
			TextMeshProUGUI shadowText = CrossHairShadow.GetComponent<TextMeshProUGUI>();
			CrossHairShadow.name = "CrossHairShadow";
			shadowText.fontSize = Plugin.CrossHairSize.Value;
			shadowText.color = new Color32(byte.MinValue, byte.MinValue, byte.MinValue, (byte)CrossHairShadowAlpha);
			shadowText.rectTransform.localPosition = new Vector3(2, -2, 0);

			rect.SetAsLastSibling();
		}

		/// <summary>
		/// Set the alpha of the crosshair
		/// </summary>
		/// <param name="target">Target alpha value (1f = 100%)</param>
		public static void SetCrossHairAlphaPercent(float target) {
			if (!CrossHair) { return; }
			CrossHair.GetComponent<TextMeshProUGUI>().alpha = target * (CrossHairAlpha / 255f);

			if (!CrossHairShadow) { return; }
			CrossHairShadow.GetComponent<TextMeshProUGUI>().alpha = target * (CrossHairShadowAlpha / 255f);
		}

		private static string HexFormatException(string message = "color") {
			Console.LogMessage($"Invalid hex {message}, using default color (ffffff)");

			Plugin.CrossHairColor.Value = "ffffff";
			Plugin.Config.Save();
			return "ffffff";
		}
	}
}