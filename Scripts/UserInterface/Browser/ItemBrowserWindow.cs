using UnityEngine;

namespace ItemBrowser.UserInterface.Browser {
	public class ItemBrowserWindow : MonoBehaviour {
		private bool _hasBeenShownBefore;
		private bool _isShowing;
		public bool IsShowing {
			get => gameObject.activeSelf;
			set {
				gameObject.SetActive(value);
				if (_isShowing == value)
					return;
				
				if (value)
					Show();
				else
					Hide();
			}
		}

		private void Show() {
			OnShow(!_hasBeenShownBefore);
			
			foreach (var child in GetComponentsInChildren<ItemBrowserWindow>()) {
				if (child == this)
					continue;
				
				if (child.IsShowing)
					child.Show();
			}

			_isShowing = true;
			_hasBeenShownBefore = true;
		}

		private void Hide() {
			OnHide();
			
			foreach (var child in GetComponentsInChildren<ItemBrowserWindow>()) {
				if (child == this)
					continue;
				
				if (!child.IsShowing)
					child.Hide();
			}

			_isShowing = false;
		}
		
		protected virtual void OnShow(bool isFirstTimeShowing) { }

		protected virtual void OnHide() { }
	}
}