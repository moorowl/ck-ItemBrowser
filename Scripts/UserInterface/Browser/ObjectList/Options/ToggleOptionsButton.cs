using System.Collections.Generic;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class ToggleOptionsButton : BasicButton {
		[SerializeField]
		private OptionsPanel optionsPanel;
		[SerializeField]
		private Sprite ascendingSprite;
		[SerializeField]
		private Sprite descendingSprite;
		[SerializeField]
		private List<SpriteRenderer> spritesToUpdate;
		
		private bool _previousState;
		
		protected override void LateUpdate() {
			base.LateUpdate();

			if (optionsPanel.IsToggled == _previousState)
				return;
			
			var newSprite = optionsPanel.IsToggled ? descendingSprite : ascendingSprite;
			foreach (var sr in spritesToUpdate)
				sr.sprite = newSprite;
				
			_previousState = optionsPanel.IsToggled;
		}

		public override TextAndFormatFields GetHoverTitle() {
			return new TextAndFormatFields {
				text = optionsPanel.IsToggled ? "ItemBrowser:HideOptions" : "ItemBrowser:ShowOptions"
			};
		}

		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);
			
			optionsPanel.IsToggled = !optionsPanel.IsToggled;
		}
	}
}