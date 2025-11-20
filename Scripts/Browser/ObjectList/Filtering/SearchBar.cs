using UnityEngine;

namespace ItemBrowser.Browser {
	public class SearchBar : TextInputField {
		private const float DoubleClickThreshold = 0.5f;
		
		private float _lastLeftClicked;

		protected override void LateUpdate() {
			base.LateUpdate();

			var input = Manager.input.singleplayerInputModule;
			if (!selectedMarker.activeSelf || !input.WasButtonPressedDownThisFrame(PlayerInput.InputType.UI_INTERACT, true))
				return;

			if (Time.time <= _lastLeftClicked + DoubleClickThreshold) {
				ResetText();
				_lastLeftClicked = 0f;
			} else {
				_lastLeftClicked = Time.time;
			}
		}
	}
}