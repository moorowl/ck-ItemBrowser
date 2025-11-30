using System.Linq;
using ItemBrowser.Utilities;
using ItemBrowser.UserInterface.Browser;
using UnityEngine;

namespace ItemBrowser.Entries.Defaults {
	public class JewelryCrafterDisplay : ObjectEntryDisplay<JewelryCrafter> {
		[SerializeField]
		private BasicItemSlot unpolishedSlot;
		[SerializeField]
		private BasicItemSlot polishedSlot;
		[SerializeField]
		private PugText chanceText;

		public override void RenderSelf() {
			RenderBody();
			RenderMoreInfo();
		}

		private void RenderBody() {
			unpolishedSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.UnpolishedVersion
			});
			polishedSlot.DisplayedObject = new DisplayedObject.Static(new ObjectDataCD {
				objectID = Entry.PolishedVersion
			});
			chanceText.Render($"{UserInterfaceUtils.FormatChance(Entry.Chance.Min)}-{UserInterfaceUtils.FormatChance(Entry.Chance.Max)}%");
		}

		private void RenderMoreInfo() {
			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/JewelryCrafter_0",
				formatFields = new[] {
					ObjectUtils.GetLocalizedDisplayNameOrDefault(Entry.UnpolishedVersion)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});

			MoreInfo.AddLine(new TextAndFormatFields {
				text = "ItemBrowser:MoreInfo/JewelryCrafter_1",
				formatFields = new[] {
					UserInterfaceUtils.FormatChance(Entry.Chance.Min),
					UserInterfaceUtils.FormatChance(Entry.Chance.Max)
				},
				dontLocalizeFormatFields = true,
				color = UserInterfaceUtils.DescriptionColor
			});	
		}
	}
}