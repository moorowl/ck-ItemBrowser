using ItemBrowser.Utilities;
using ItemBrowser.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class ArchaeologistDropsDisplay : ObjectEntryDisplay<ArchaeologistDrops> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private PugText chanceText;

		public override void RenderSelf() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation
			});
			
			chanceText.Render(TextUtils.FormatChance(Entry.Chance) + "%");
		}
	}
}