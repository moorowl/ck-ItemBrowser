using UnityEngine;

namespace ItemBrowser.Browser {
	public class ItemBrowserWindow : MonoBehaviour {
		private bool _hasBeenShownBefore;
		private bool? _wasShowing;
		public bool IsShowing {
			get => gameObject.activeSelf;
			set {
				gameObject.SetActive(value);

				if (_wasShowing != value) {
					if (value) {
						OnShow(!_hasBeenShownBefore);
						_hasBeenShownBefore = true;
					} else {
						OnHide();
					}

					_wasShowing = value;
				}
			}
		}
		
		protected virtual void OnShow(bool isFirstTimeShowing) { }

		protected virtual void OnHide() { }
	}
}