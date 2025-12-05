using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
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
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.OreBoulder)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			// x ore total
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/OreBoulderExtraction_1",
				formatFields = new[] {
					Entry.TotalOre.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}