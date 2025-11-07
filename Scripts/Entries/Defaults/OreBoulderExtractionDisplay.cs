using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class OreBoulderExtractionDisplay : ObjectEntryDisplay<OreBoulderExtraction> {
		[SerializeField]
		private BasicItemSlot oreBoulderSlot;
		[SerializeField]
		private BasicItemSlot resultSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			oreBoulderSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.OreBoulder
			});
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result
			}, Entry.TotalOre);
		}
		
		private void RenderMoreInfo() {
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