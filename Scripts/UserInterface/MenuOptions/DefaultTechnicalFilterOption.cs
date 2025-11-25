using System.Collections.Generic;

namespace ItemBrowser.Config.MenuOptions {
	public class DefaultTechnicalFilterOption : CyclingOption<bool> {
		protected override List<bool> Options => new() {
			false,
			true
		};
		protected override bool CurrentOption {
			get => ItemBrowser.Options.DefaultTechnicalFilter;
			set => ItemBrowser.Options.DefaultTechnicalFilter = value;
		}
		
		protected override void UpdateText() {
			valueText.Render(CurrentOption ? "on" : "off");
		}
	}
}