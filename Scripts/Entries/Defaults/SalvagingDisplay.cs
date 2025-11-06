using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class SalvagingDisplay : ObjectEntryDisplay<Salvaging> {
		[SerializeField] 
		private BasicItemSlot resultSlot;
		[SerializeField]
		private BasicItemSlot sourceSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation
			}, Entry.Amount);
			sourceSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Salvaged
			});
		}

		private void RenderMoreInfo() {
			// Salvaged from
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Salvaging_0",
				formatFields = new[] {
					ObjectUtils.GetUnlocalizedDisplayName(Entry.Salvaged)
				},
				color = TextUtils.DescriptionColor
			});
			if (Entry.Amount.Min != Entry.Amount.Max) {
				// Drops x-x
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Salvaging_1_Durability",
					formatFields = new[] {
						Entry.Amount.Min.ToString(),
						Entry.Amount.Max.ToString()
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
			} else {
				// Always drops x
				MoreInfo.AddLine(new TextAndFormatFields {
					text = "ItemBrowser:MoreInfo/Salvaging_1",
					formatFields = new[] {
						Entry.Amount.Max.ToString()
					},
					dontLocalizeFormatFields = true,
					color = TextUtils.DescriptionColor
				});
			}
		}
	}
}