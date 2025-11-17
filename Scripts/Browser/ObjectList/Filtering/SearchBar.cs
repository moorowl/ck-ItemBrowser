using UnityEngine;

namespace ItemBrowser.Browser {
	public class SearchBar : TextInputField {
		private const float DoubleClickThreshold = 0.5f;
		
		private float _lastLeftClicked;
		
		public override void OnLeftClicked(bool mod1, bool mod2) {
			base.OnLeftClicked(mod1, mod2);

			if (Time.time < _lastLeftClicked + DoubleClickThreshold)
				ResetText();
			
			_lastLeftClicked = Time.time;
		}
	}
}