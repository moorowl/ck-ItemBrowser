using System.Collections.Generic;

namespace ItemBrowser.Config.MenuOptions {
	public class CheatModeOption : CyclingOption<bool> {
		protected override List<bool> Options => new() {
			false,
			true
		};
		protected override bool CurrentOption {
			get => Main.Config.CheatMode;
			set => Main.Config.CheatMode = value;
		}
		
		protected override void UpdateText() {
			valueText.Render(CurrentOption ? "on" : "off");
		}
	}
}