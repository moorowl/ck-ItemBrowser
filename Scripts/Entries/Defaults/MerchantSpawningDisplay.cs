using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class MerchantSpawningDisplay : ObjectEntryDisplay<MerchantSpawning> {
		[SerializeField]
		private BasicItemSlot merchantSlot;
		[SerializeField]
		private BasicItemSlot idolSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			merchantSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Merchant
			});
			idolSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Idol
			});
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/MerchantSpawning_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Idol),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Merchant)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}