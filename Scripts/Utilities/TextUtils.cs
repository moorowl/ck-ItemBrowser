using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class TextUtils {
		public static Color DescriptionColor => Manager.text.GetRarityColor(Rarity.Poor);
		
		public static string GetInputGlyph(string id) {
			var prefersJoystick = Manager.input.IsAnyGamepadConnected() && !Manager.input.singleplayerInputModule.PrefersKeyboardAndMouse();
			return Manager.ui.GetShortCutString(id, prefersJoystick) ?? "?";
		}

		public static string FormatChance(float chance) {
			return (chance * 100f).ToString("0.##");
		}

		public static string FormatAmountOrRollsRange((int Min, int Max) amount) {
			return amount.Min != amount.Max ? $"{amount.Min}-{amount.Max}" : amount.Min.ToString();
		}
		
		public static string FormatDuration(float duration) {
			return duration.ToString("F0");
		}
	}
}