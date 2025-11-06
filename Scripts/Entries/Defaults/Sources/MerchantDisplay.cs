using ItemBrowser.Utilities;
using ItemBrowser.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class MerchantDisplay : ObjectEntryDisplay<Merchant> {
		[SerializeField]
		private BasicItemSlot merchantSlot;
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private PugText costText;

		public override void RenderSelf() {
			merchantSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.MerchantType
			});
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation,
				amount = Entry.Stock
			});

			var buyCost = ObjectUtils.GetValue(ObjectData.objectID, ObjectData.variation, true);
			costText.Render(buyCost.ToString());
			
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Merchant_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayName(Entry.MerchantType),
					buyCost.ToString()
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
			if (Entry.Requirement != MerchantItemRequirement.None) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser:MoreInfo/Merchant_2_{Entry.Requirement}",
					color = TextUtils.DescriptionColor
				});
			}
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Merchant_1",
				formatFields = new[] {
					Entry.Stock.ToString()
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
		}
	}
}