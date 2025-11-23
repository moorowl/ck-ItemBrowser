using ItemBrowser.Utilities;
using ItemBrowser.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
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
				objectID = Entry.Result
			}, Entry.Stock);

			var buyCost = ObjectUtils.GetValue(Entry.Result, 0, true);
			costText.Render(buyCost.ToString());
			
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Merchant_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.MerchantType),
					buyCost.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			if (Entry.Requirement != MerchantItemRequirement.None) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = $"ItemBrowser:MoreInfo/Merchant_2_{Entry.Requirement}",
					color = UserInterfaceUtils.DescriptionColor
				});
			}
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Merchant_1",
				formatFields = new[] {
					Entry.Stock.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}