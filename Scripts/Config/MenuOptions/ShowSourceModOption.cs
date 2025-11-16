using System.Collections.Generic;

namespace ItemBrowser.Config.MenuOptions {
	public class ShowSourceModOption : CyclingOption<bool> {
		protected override List<bool> Options => new() {
			false,
			true
		};
		protected override bool CurrentOption {
			get => ConfigFile.ShowSourceMod;
			set => ConfigFile.ShowSourceMod = value;
		}
		
		protected override void UpdateText() {
			valueText.Render(CurrentOption ? "on" : "off");
		}
	}
}