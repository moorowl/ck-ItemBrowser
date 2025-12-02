using ItemBrowser.Utilities;
using Pug.UnityExtensions;
using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class SearchBar : TextInputField {
		private const float DoubleClickThreshold = 0.5f;
		
		private float _lastLeftClicked;

		public override void OnLeftClicked(bool mod1, bool mod2) {
			if (UserInterfaceUtils.IsUsingMouseAndKeyboard)
				base.OnLeftClicked(mod1, mod2);
		}

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
			
			if (!UserInterfaceUtils.IsUsingMouseAndKeyboard && inputIsActive)
				Deactivate(true);
		}
		
		public override UIelement GetAdjacentUIElement(Direction.Id dir, Vector3 currentPosition) {
			return SnapPoint.TryFindNextSnapPoint(this, dir)?.AttachedElement;
		}
	}
}