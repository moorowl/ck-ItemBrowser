using ItemBrowser.Api.Entries;
using ItemBrowser.Utilities.DataStructures;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Api {
	public abstract class ItemBrowserPlugin {
		internal LoadedMod AssociatedMod;
		
		public virtual void OnRegister(ItemBrowserRegistry registry) {
			AutomaticallyRegisterFromAssets(registry);
		}
		
		protected virtual void AutomaticallyRegisterFromAssets(ItemBrowserRegistry registry) {
			foreach (var asset in AssociatedMod.Assets) {
				if (asset is not GameObject gameObject)
					return;
			
				if (gameObject.TryGetComponent<ObjectEntryDisplayBase>(out var displayComponent))
					registry.AddEntryDisplay(displayComponent);
				
				if (gameObject.TryGetComponent<ObjectNameAndIconOverride>(out var overrides))
					registry.AddObjectNameAndIconOverride(overrides);
			}
		}
	}
}