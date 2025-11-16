using UnityEngine;

namespace ItemBrowser.Browser {
	public class ClearSearchButton : BasicButton {
		[SerializeField]
		private TextInputField search;

		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = "ItemBrowser:ClearSearch"
			};
		}

		protected override void LateUpdate() {
			canBeClicked = search.GetInputText().Length > 0;

			base.LateUpdate();
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			search.SetInputText("");
		}
	}
}