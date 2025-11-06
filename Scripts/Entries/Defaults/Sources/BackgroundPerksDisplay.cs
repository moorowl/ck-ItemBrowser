using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults.Sources {
	public class BackgroundPerksDisplay : ObjectEntryDisplay<BackgroundPerks> {
		[SerializeField]
		private BasicButton background;
		[SerializeField]
		private BasicItemSlot resultSlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = ObjectData.objectID,
				variation = ObjectData.variation,
				amount = Entry.Amount
			});
			background.optionalTitle.mTerm = $"Roles/{Entry.Background}";
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/BackgroundPerks_0",
				formatFields = new[] {
					background.optionalTitle.mTerm
				},
				color = TextUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/BackgroundPerks_1",
				formatFields = new[] {
					Entry.Amount.ToString()
				},
				dontLocalizeFormatFields = true,
				color = TextUtils.DescriptionColor
			});
		}
	}
}