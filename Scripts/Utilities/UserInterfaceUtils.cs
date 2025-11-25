using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class UserInterfaceUtils {
		public static bool IsUsingMouse => Manager.input.SystemIsUsingMouse();
		public static bool IsUsingKeyboard => Manager.input.SystemIsUsingKeyboard();
		public static bool IsUsingMouseAndKeyboard => Manager.input.SystemPrefersKeyboardAndMouse();
		public static bool IsUsingMouseOrKeyboard => IsUsingMouse || IsUsingKeyboard;
		
		public static Color DescriptionColor => Manager.text.GetRarityColor(Rarity.Poor);
		
		public static string GetInputGlyph(string binding) {
			var prefersJoystick = Manager.input.IsAnyGamepadConnected() && !IsUsingMouseOrKeyboard;
			return prefersJoystick ? Manager.ui.GetShortCutString(binding, true, true) : Manager.ui.GetShortCutString(binding, false);
		}

		public static void AppendButtonHint(List<TextAndFormatFields> lines, string term, string binding) {
			var glyph = GetInputGlyph(binding);
			if (glyph == null)
				return;
			
			if (lines.Count > 0)
				lines[^1].paddingBeneath = 0.125f;
			
			lines.Add(new TextAndFormatFields {
				text = term,
				formatFields = new[] {
					glyph
				},
				dontLocalizeFormatFields = true,
				color = Color.white * 0.95f
			});
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

		public static void SelectAndMoveMouseTo(UIelement element) {
			Manager.ui.DeselectAnySelectedUIElement();
			element.Select();
			Manager.ui.mouse.PlaceMousePositionOnSelectedUIElementWhenControlledByJoystick();
		}
		
		public static void PlayMenuOpenSound(float pitchIncrease = 0f) {
			AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.6f + pitchIncrease, false, 1f, 0f);
		}
		
		public static void PlayMenuCloseSound(float pitchIncrease = 0f) {
			AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.4f + pitchIncrease, false, 1f, 0f);
		}
		
		public static void PlayItemTwitchSound(Transform origin) {
			AudioManager.Sfx(SfxID.twitch, origin.position, 0.1f, 0.55f, 0.1f, reuse: true);
		}
	}
}