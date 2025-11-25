using System.Collections.Generic;

namespace ItemBrowser.Config.MenuOptions {
	public class CheatModeOption : CyclingOption<bool> {
		protected override List<bool> Options => new() {
			false,
			true
		};
		protected override bool CurrentOption {
			get => ItemBrowser.Options.CheatMode;
			set => ItemBrowser.Options.CheatMode = value;
		}
		
		protected override void UpdateText() {
			valueText.Render(CurrentOption ? "on" : "off");
		}
	}
}