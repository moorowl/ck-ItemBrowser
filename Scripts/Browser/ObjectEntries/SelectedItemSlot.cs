using UnityEngine;

namespace ItemBrowser.Browser {
	public class SelectedItemSlot : BasicItemSlot {
		[SerializeField]
		private ItemBrowserUI itemBrowserUI;

		public void SetObjectData(ObjectDataCD objectData) {
			DisplayedObject = new DisplayedObject.Static(objectData);
		}

		/*public override void OnLeftClicked(bool mod1, bool mod2) {
			AudioManager.SfxUI(Manager.audio.InspectorFriendlySfxIDToSfxID(SfxUnityInspectorFriendlyID.FIXME_menu_select), 0.6f, false, 1f, 0f);
			itemBrowserUI.ShowItemList();
		}*/
	}
}