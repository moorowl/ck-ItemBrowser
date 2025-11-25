using System;
using ItemBrowser.UserInterface.Browser;
using ItemBrowser.Utilities;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class BackgroundPerksDisplay : ObjectEntryDisplay<BackgroundPerks> {
		private static readonly Lazy<SkillIconsTable> SkillIconsTable = new(() => Resources.Load<SkillIconsTable>("SkillIconsTable"));
		
		[SerializeField]
		private BasicButton background;
		[SerializeField]
		private SpriteRenderer backgroundIcon;
		[SerializeField]
		private BasicItemSlot resultSlot;
		
		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			resultSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.Result.Id,
				variation = Entry.Result.Variation,
				amount = Entry.Result.Amount
			});
			background.optionalTitle.mTerm = $"Roles/{Entry.Background}";
			backgroundIcon.sprite = SkillIconsTable.Value.GetIcon(Entry.BackgroundSkill).icon;
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/BackgroundPerks_0",
				formatFields = new[] {
					background.optionalTitle.mTerm
				},
				color = UserInterfaceUtils.DescriptionColor
			});
			MoreInfo.AddPadding();
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/BackgroundPerks_1",
				formatFields = new[] {
					Entry.Result.Amount.ToString()
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});
		}
	}
}