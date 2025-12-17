using System.Collections.Generic;

namespace ItemBrowser.UserInterface.Browser {
	public class CheatModeButton : BasicButton {
		public static bool CanBeToggled => Manager.saves.IsCreativeModeCharacter() || Manager.main.player.adminPrivileges >= 1;
		
		protected override void LateUpdate() {
			canBeClicked = CanBeToggled;
			
			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (!canBeClicked)
				return;
			
			Options.CheatMode = !Options.CheatMode;
			Options.Save();
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = "ItemBrowser:Options/CheatMode"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			var lines = new List<TextAndFormatFields> {
				new() {
					text = Options.CheatMode && CanBeToggled ? "ItemBrowser:Options/Enabled" : "ItemBrowser:Options/Disabled"
				}
			};

			if (!CanBeToggled) {
				lines.Add(new TextAndFormatFields {
					text = "ItemBrowser:Options/CheatModeCantBeToggled",
					color = Manager.ui.brokenColor
				});
			}
			
			return lines;
		}
	}
}