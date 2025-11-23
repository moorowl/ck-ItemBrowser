using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class VendingMachineDisplay : ObjectEntryDisplay<VendingMachine> {
		[SerializeField]
		private BasicItemSlot vendingMachineSlot;
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private PugText costText;

		public override void RenderSelf() {
			var buyCost = ObjectUtils.GetValue(Entry.Result, 0, true);
			
			RenderBody(buyCost);
			RenderMoreInfo(buyCost);
		}

		private void RenderBody(int buyCost) {
			vendingMachineSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Vendor
			});
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result
			}, Entry.Stock);
			costText.Render(buyCost.ToString());
		}

		private void RenderMoreInfo(int buyCost) {
			// Purchased from
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/VendingMachine_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayName(Entry.Vendor),
					buyCost.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}