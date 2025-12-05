using ItemBrowser.Api.Entries;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using PugMod;
using UnityEngine;

namespace ItemBrowser.Plugins.BuiltinContent.Entries {
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

			if (!Entry.HasSource) {
				descriptionText.Render(API.Localization.GetLocalizedTerm(Entry.Term));
			} else {
				descriptionText.Render(string.Format(
					API.Localization.GetLocalizedTerm(Entry.Term),
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Source.Id, Entry.Source.Variation)
				));	
			}
		}

		private void RenderMoreInfo() {
			if (!Entry.HasSource) {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = Entry.Term,
					color = UserInterfaceUtils.DescriptionColor
				});
			} else {
				MoreInfo.AddLine(new TextAndFormatFields {
					text = Entry.Term,
					formatFields = new[] {
						ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.Source.Id, Entry.Source.Variation)
					},
					dontLocalizeFormatFields = true,
					color = UserInterfaceUtils.DescriptionColor
				});
			}
		}
	}
}