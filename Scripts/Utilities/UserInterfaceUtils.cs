using UnityEngine;

namespace ItemBrowser.Utilities {
	public static class UserInterfaceUtils {
		public static void PlayMenuOpenSound(float pitchIncrease = 0f) {
			AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.6f + pitchIncrease, false, 1f, 0f);
		}
		
		public static void PlayMenuCloseSound(float pitchIncrease = 0f) {
			AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.4f + pitchIncrease, false, 1f, 0f);
		}
		
		public static void PlayItemTwitchSound(Transform origin) {
			AudioManager.Sfx(SfxID.twitch, origin.position, 0.1f, 0.55f, 0.1f, reuse: true);
		}
	}
}