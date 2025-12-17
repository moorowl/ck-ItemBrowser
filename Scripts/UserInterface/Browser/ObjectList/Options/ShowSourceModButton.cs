using System.Collections.Generic;

namespace ItemBrowser.UserInterface.Browser {
	public class ShowSourceModButton : BasicButton {
		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (!canBeClicked)
				return;
			
			Options.ShowSourceMod = !Options.ShowSourceMod;
			Options.Save();
		}
		
		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = "ItemBrowser:Options/ShowSourceMod"
			};
		}

		public override List<TextAndFormatFields> GetHoverDescription() {
			return new List<TextAndFormatFields> {
				new() {
					text = Options.ShowSourceMod ? "ItemBrowser:Options/Enabled" : "ItemBrowser:Options/Disabled"
				}
			};
		}
	}
}