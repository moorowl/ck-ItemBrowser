using ItemBrowser.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class MiscellaneousDisplay : ObjectEntryDisplay<Miscellaneous> {
		[SerializeField]
		private BasicItemSlot resultSlot;
		[SerializeField]
		private PugText descriptionText;
		
		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation
			}, Entry.Result.Amount);

			descriptionText.Render(Entry.Term);
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = Entry.Term,
				color = TextUtils.DescriptionColor
			});
		}
	}
}