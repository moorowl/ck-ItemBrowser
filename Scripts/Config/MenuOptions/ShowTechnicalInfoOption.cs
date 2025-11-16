using System.Collections.Generic;

namespace ItemBrowser.Config.MenuOptions {
	public class ShowTechnicalInfoOption : CyclingOption<bool> {
		protected override List<bool> Options => new() {
			false,
			true
		};
		protected override bool CurrentOption {
			get => ConfigFile.ShowTechnicalInfo;
			set => ConfigFile.ShowTechnicalInfo = value;
		}
		
		protected override void UpdateText() {
			valueText.Render(CurrentOption ? "on" : "off");
		}
	}
}