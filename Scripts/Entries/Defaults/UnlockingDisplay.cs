using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class UnlockingDisplay : ObjectEntryDisplay<Unlocking> {
		[SerializeField]
		private BasicItemSlot outputSlot;
		[SerializeField]
		private BasicItemSlot inputSlot;
		[SerializeField]
		private BasicItemSlot keySlot;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			outputSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.OutputObject.Id,
				variation = Entry.OutputObject.Variation
			});
			inputSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.InputObject.Id,
				variation = Entry.InputObject.Variation
			});
			keySlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Key.Id,
				variation = Entry.Key.Variation
			});
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Unlocking_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Key.Id, Entry.Key.Variation),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.InputObject.Id, Entry.InputObject.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/Unlocking_1",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.OutputObject.Id, Entry.OutputObject.Variation)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}