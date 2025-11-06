using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class OreBoulderExtractionDisplay : ObjectEntryDisplay<OreBoulderExtraction> {
		[SerializeField]
		private BasicItemSlot oreBoulderSlot;
		[SerializeField]
		private BasicItemSlot resultSlot;

		public override void RenderSelf() {
			//RenderBody();
			//RenderMoreInfo();
			
			oreBoulderSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.OreBoulder
			});
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation,
				amount = Entry.TotalOre
			});
			
			// Drilled from
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/OreBoulderExtraction_0",
				formatFields = new[] {
					ObjectUtils.GetUnlocalizedDisplayName(Entry.OreBoulder)
				},
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			// x ore total
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/OreBoulderExtraction_1",
				formatFields = new[] {
					Entry.TotalOre.ToString()
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
		}
	}
}